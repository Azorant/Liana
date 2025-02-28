using Discord;
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
}