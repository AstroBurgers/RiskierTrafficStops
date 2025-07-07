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
internal class PluginUpdateChecker
{
    private readonly Uri _apiUrl;
    private readonly Version _currentVersion;

    private Version _latestVersion;
    private bool _failure;

    private readonly TTask _asyncUpdateTask;

    internal class UpdateCompletedEventArgs : EventArgs
    {
        internal bool Failed { get; private set; }
        internal bool UpdateAvailable { get; private set; }
        internal Version LatestVersion { get; private set; }

        internal UpdateCompletedEventArgs(bool failed, bool updateAvailable, Version latestVersion)
        {
            Failed = failed;
            UpdateAvailable = updateAvailable;
            LatestVersion = latestVersion;
        }
    }

    internal event EventHandler<UpdateCompletedEventArgs> OnCompleted;

    internal PluginUpdateChecker(Assembly assembly)
    {
        if (!Uri.TryCreate(
                "https://www.lcpdfr.com/applications/downloadsng/interface/api.php?do=checkForUpdates&fileId=44036&textOnly=1",
                UriKind.Absolute, out _apiUrl))
        {
            throw new UriFormatException(nameof(_apiUrl));
        }

        _currentVersion = _latestVersion = assembly.GetName().Version;

        var cts = new CancellationTokenSource();
        cts.CancelAfter(30000);

        _asyncUpdateTask = TTask.Run(() => CheckForUpdatesAsync(cts.Token));

        GameFiber.StartNew(WaitFiber);
    }

    private void WaitFiber()
    {
        GameFiber.Wait(1000);

        GameFiber.WaitUntil(() => _asyncUpdateTask.IsCompleted);

        OnCompleted?.Invoke(this,
            new UpdateCompletedEventArgs(_failure, _latestVersion > _currentVersion, _latestVersion));
    }

    internal async TTask CheckForUpdatesAsync(CancellationToken cts)
    {
        try
        {
            var updateText = await DownloadUpdateTextAsync(_apiUrl, cts);

            cts.ThrowIfCancellationRequested();

            if (!string.IsNullOrWhiteSpace(updateText))
            {
                if (!Version.TryParse(updateText.Trim('v'), out _latestVersion))
                {
                    _failure = true;
                }
            }
            else
            {
                _failure = true;
            }
        }
        catch (Exception)
        {
            _failure = true;
        }
    }

    private async Task<string> DownloadUpdateTextAsync(Uri url, CancellationToken cts)
    {
        using (var httpClient = new HttpClient())
        {
            httpClient.Timeout = TimeSpan.FromMilliseconds(30000);

            return await GetStringWithTimeoutAsync(httpClient, url, cts);
        }
    }

    private static void SetTls()
    {
        ServicePointManager.Expect100Continue = true;
        ServicePointManager.MaxServicePointIdleTime = 2000;
        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls
                                               | SecurityProtocolType.Tls11
                                               | SecurityProtocolType.Tls12
                                               | SecurityProtocolType.Ssl3;
    }

    private static async Task<string> GetStringWithTimeoutAsync(HttpClient client, Uri requestUri,
        CancellationToken cancellationToken)
    {
        using (var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
        {
            cts.CancelAfter(client.Timeout);

            SetTls();

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