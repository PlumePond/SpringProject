using System.Text.RegularExpressions;

namespace SpringProject.Core;

public static class StringUtils
{
    public static string ToSnakeCase(string text)
    {
        return Regex.Replace(text, "(?<=[a-z])([A-Z])", "_$1").ToLower();
    }
}