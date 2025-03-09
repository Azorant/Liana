using System.Text.RegularExpressions;
using Discord;
using Microsoft.Recognizers.Text;
using Microsoft.Recognizers.Text.DateTime;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Liana.Models;

public static class Parser
{
    public static string SerializeConfig(GuildConfig config)
    {
        var serializer = new SerializerBuilder()
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitDefaults | DefaultValuesHandling.OmitNull)
            .EnsureRoundtrip()
            .Build();
        return serializer.Serialize(config);
    }

    public static GuildConfig DeserializeConfig(string yaml)
    {
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .Build();
        return deserializer.Deserialize<GuildConfig?>(yaml) ?? new GuildConfig();
    }

    public static string SerializeEmote(IEmote raw)
    {
        return raw switch
        {
            Emoji emoji => emoji.ToString(),
            Emote emote => emote.ToString().Replace("<:", "").Replace("<", "").Replace(">", ""),
            _ => raw.Name
        };
    }

    public static IEmote DeserializeEmote(string raw)
    {
        if (raw.Contains(':')) return Emote.Parse($"{(raw.StartsWith("a:") ? "<" : "<:")}{raw}>");
        return Emoji.Parse(raw);
    }

    public static DateTime? ParseTime(string text, DateTime? refTime = null)
    {
        refTime ??= DateTime.UtcNow;
        text = Regex.Replace(Regex.Replace(text.ToLowerInvariant(), @"(?<=\d)\s*m\s*(?!\w)", "min"), @"(?<=\d)\s*s\s*(?!\w)", "sec");
        var results = DateTimeRecognizer.RecognizeDateTime(text,
            Culture.English, refTime: refTime);
        if (
            results.Count == 0 ||
            results.First().Resolution.Count == 0 ||
            results.First().Resolution.First().Value is not List<Dictionary<string, string>> resolved ||
            resolved.Count == 0
        ) return null;

        var result = resolved.First();

        return result["type"] switch
        {
            "datetime" => DateTime.Parse(result["value"]),
            "duration" => refTime.Value.AddSeconds(int.Parse(result["value"])),
            _ => null
        };
    }

    public static bool TryParseTime(string text, DateTime? refTime, out DateTime time)
    {
        var parsed = ParseTime(text, refTime);
        if (parsed == null)
        {
            time = default;
            return false;
        }
        time = parsed.Value;
        return true;
    }
  
}