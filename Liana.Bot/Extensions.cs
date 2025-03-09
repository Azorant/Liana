using System.Text.RegularExpressions;
using Discord;

namespace Liana.Bot;

public static class Extensions
{
    public static string ReplaceRegex(this string input, string pattern, string replacement) => Regex.Replace(input, pattern, replacement);
    public static bool IsEqual<T>(this List<T> list, List<T> compare) => list.All(compare.Contains) && list.Count == compare.Count;
    public static string Sanitize(this string input, bool proper = false) => proper
        ? Format.Sanitize(input)
        : input
            .ReplaceRegex("~", "\u200B~")
            .ReplaceRegex("\\*", "\u200B*")
            .ReplaceRegex("_", "\u200B_")
            .ReplaceRegex("`", "\u02CB")
            .ReplaceRegex("\\|", "\u200B|");
    public static List<string> Missing(this ChannelPermissions permissions, params ChannelPermission[] toCheck) =>
        toCheck.Where(p => !permissions.Has(p)).Select(p => Enum.GetName(p)!).ToList();
    public static DateTime SpecifyUtc(this DateTime date) => DateTime.SpecifyKind(date, DateTimeKind.Utc);
}