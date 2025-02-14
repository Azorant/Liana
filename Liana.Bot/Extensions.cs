using System.Text.RegularExpressions;

namespace Liana.Bot;

public static class Extensions
{
    public static string ReplaceRegex(this string input, string pattern, string replacement) => Regex.Replace(input, pattern, replacement);
}