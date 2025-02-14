using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Liana.Models;

public static class ConfigParser
{
    public static string Serialize(GuildConfig config)
    {
        var serializer = new SerializerBuilder()
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .EnsureRoundtrip()
            .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull)
            .Build();
        return serializer.Serialize(config);
    }

    public static GuildConfig Deserialize(string yaml)
    {
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .Build();
        return deserializer.Deserialize<GuildConfig>(yaml);
    }
}