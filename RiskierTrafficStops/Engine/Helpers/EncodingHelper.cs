namespace RiskierTrafficStops.Engine.Helpers;

internal class EncodingHelper
{
    internal static string ToBase64Encode(string text)
    {
        if (string.IsNullOrEmpty(text)) {
            return text;
        }
 
        var textBytes = System.Text.Encoding.UTF8.GetBytes(text);
        return Convert.ToBase64String(textBytes);
    }
 
    internal static string ToBase64Decode(string base64EncodedText)
    {
        if (string.IsNullOrEmpty(base64EncodedText)) {
            return base64EncodedText;
        }
 
        var base64EncodedBytes = Convert.FromBase64String(base64EncodedText);
        return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
    }
}