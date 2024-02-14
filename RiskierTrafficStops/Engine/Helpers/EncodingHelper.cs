namespace RiskierTrafficStops.Engine.Helpers;

public class EncodingHelper
{
    internal static string ToBase64Encode(string text)
    {
        if (String.IsNullOrEmpty(text)) {
            return text;
        }
 
        byte[] textBytes = System.Text.Encoding.UTF8.GetBytes(text);
        return Convert.ToBase64String(textBytes);
    }
 
    internal static string ToBase64Decode(string base64EncodedText)
    {
        if (String.IsNullOrEmpty(base64EncodedText)) {
            return base64EncodedText;
        }
 
        byte[] base64EncodedBytes = Convert.FromBase64String(base64EncodedText);
        return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
    }
}