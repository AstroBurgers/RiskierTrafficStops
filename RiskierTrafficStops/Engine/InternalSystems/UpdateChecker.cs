/*
 * Credit to Khorio for this code
 * Much appreciated!
 */

using System.ComponentModel;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using TTask = System.Threading.Tasks.Task;

namespace RiskierTrafficStops.Engine.InternalSystems;

[EditorBrowsable(EditorBrowsableState.Never)]
public class UpdateChecker
{
    private readonly Assembly _assembly;
    private readonly Uri apiUrl;
    private readonly Version currentVersion;

    private Version latestVersion;
    private bool failure;

    private readonly TTask asyncUpdateTask;
    private readonly CancellationTokenSource cts;

    public class UpdateCompletedEventArgs : EventArgs
    {
        public bool Failed { get; private set; }
        public bool UpdateAvailable { get; private set; }
        public Version LatestVersion { get; private set; }

        public UpdateCompletedEventArgs(bool failed, bool updateAvailable, Version latestVersion)
        {
            Failed = failed;
            UpdateAvailable = updateAvailable;
            LatestVersion = latestVersion;
        }
    }

    public event EventHandler<UpdateCompletedEventArgs> OnCompleted;

    public UpdateChecker(int fileId, Assembly assembly)
    {
        if (!Uri.TryCreate($"https://www.lcpdfr.com/applications/downloadsng/interface/api.php?do=checkForUpdates&fileId=44036&textOnly=1", UriKind.Absolute, out apiUrl))
        {
            throw new UriFormatException(nameof(apiUrl));
        }
        _assembly = assembly;

        currentVersion = latestVersion = _assembly.GetName().Version;

        cts = new CancellationTokenSource();
        cts.CancelAfter(30000);

        asyncUpdateTask = TTask.Run(() => CheckForUpdatesAsync(cts.Token));

        GameFiber.StartNew(WaitFiber);
    }

    private void WaitFiber()
    {
        GameFiber.Wait(1000);

        GameFiber.WaitUntil(() => asyncUpdateTask.IsCompleted);

        OnCompleted?.Invoke(this, new UpdateCompletedEventArgs(failure, latestVersion > currentVersion, latestVersion));
    }

    public async TTask CheckForUpdatesAsync(CancellationToken cts)
    {
        try
        {
            string updateText = await DownloadUpdateTextAsync(apiUrl, cts);

            cts.ThrowIfCancellationRequested();

            if (!string.IsNullOrWhiteSpace(updateText))
            {
                if (!Version.TryParse(updateText.Trim('v'), out latestVersion))
                {
                    failure = true;
                }
            }
            else
            {
                failure = true;
            }
        }
        catch (Exception)
        {
            failure = true;
            return;
        }
    }

    private async Task<string> DownloadUpdateTextAsync(Uri url, CancellationToken cts)
    {
        using (HttpClient httpClient = new HttpClient())
        {
            httpClient.Timeout = TimeSpan.FromMilliseconds(30000);

            return await GetStringWithTimeoutAsync(httpClient, url, cts);
        }
    }

    private static void SetTLS()
    {
        ServicePointManager.Expect100Continue = true;
        ServicePointManager.MaxServicePointIdleTime = 2000;
        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls
                                               | SecurityProtocolType.Tls11
                                               | SecurityProtocolType.Tls12
                                               | SecurityProtocolType.Ssl3;
    }

    private static async Task<string> GetStringWithTimeoutAsync(HttpClient client, Uri requestUri, CancellationToken cancellationToken)
    {
        using (var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
        {
            cts.CancelAfter(client.Timeout);

            SetTLS();

            using var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            using (var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cts.Token))
            {
                response.EnsureSuccessStatusCode();

                using (var stream = await response.Content.ReadAsStreamAsync())
                using (var reader = new StreamReader(stream))
                {
                    return await reader.ReadToEndAsync();
                }
            }
        }
    }
}