using System.Text.RegularExpressions;
using Discord;

namespace Liana.Bot;

public static class Extensions
{
    public static string ReplaceRegex(this string input, string pattern, string replacement) => Regex.Replace(input, pattern, replacement);
    public static Embed Build(this IEmbed embed)
    {
        var builder = new EmbedBuilder();
        if (!string.IsNullOrEmpty(embed.Title)) builder.WithTitle(embed.Title);
        if (!string.IsNullOrEmpty(embed.Description)) builder.WithDescription(embed.Description);
        if (!string.IsNullOrEmpty(embed.Url)) builder.WithUrl(embed.Url);
        if (embed.Author.HasValue) builder.WithAuthor(embed.Author.Value.Name, embed.Author.Value.IconUrl, embed.Author.Value.Url);
        if (embed.Timestamp.HasValue) builder.WithTimestamp(embed.Timestamp.Value);
        if (embed.Color.HasValue) builder.WithColor(embed.Color.Value);
        if (embed.Image.HasValue) builder.WithImageUrl(embed.Image.Value.Url);
        if (embed.Thumbnail.HasValue) builder.WithThumbnailUrl(embed.Thumbnail.Value.Url);
        if (embed.Footer.HasValue) builder.WithFooter(embed.Footer.Value.Text, embed.Footer.Value.IconUrl);
        if (embed.Fields.Length != 0)
            builder.WithFields(embed.Fields
                .Select(x => new EmbedFieldBuilder()
                    .WithName(x.Name)
                    .WithValue(x.Value)
                    .WithIsInline(x.Inline)
                ));

        return builder.Build();
    }
}