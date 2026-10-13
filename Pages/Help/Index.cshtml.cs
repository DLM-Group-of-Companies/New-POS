using System.Net;
using System.Text.RegularExpressions;
using Ganss.Xss;
using Markdig;
using Markdig.Extensions.AutoIdentifiers;
using Microsoft.AspNetCore.Hosting;

namespace NLI_POS.Pages.Help;

public sealed class IndexModel : Microsoft.AspNetCore.Mvc.RazorPages.PageModel
{
    private static readonly Regex HeadingPattern = new(
        "<h([2-6])\\b([^>]*)>(.*?)</h[2-6]>",
        RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);
    private static readonly Regex IdPattern = new(
        "\\bid=\"(?<id>[^\"]+)\"",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex TagPattern = new(
        "<[^>]+>",
        RegexOptions.Singleline | RegexOptions.Compiled);
    private static readonly Regex ChapterSlugPattern = new(
        "^[a-z0-9]+(?:-[a-z0-9]+)*$",
        RegexOptions.Compiled);

    private readonly IWebHostEnvironment _environment;
    private readonly MarkdownPipeline _markdownPipeline;

    public IndexModel(IWebHostEnvironment environment)
    {
        _environment = environment;
        _markdownPipeline = new MarkdownPipelineBuilder()
            .DisableHtml()
            .UseAutoIdentifiers(AutoIdentifierOptions.GitHub)
            .Build();
    }

    public IReadOnlyList<HelpChapter> Chapters { get; private set; } = [];
    public HelpChapter? SelectedChapter { get; private set; }
    public HelpChapter? PreviousChapter { get; private set; }
    public HelpChapter? NextChapter { get; private set; }
    public string? SearchQuery { get; private set; }
    public bool ChapterNotFound { get; private set; }

    public void OnGet(string? chapterSlug, string? q)
    {
        SearchQuery = q?.Trim();

        var allChapters = LoadChapters()
            .Where(chapter => chapter.Roles.Count == 0 || chapter.Roles.Any(User.IsInRole))
            .OrderBy(chapter => chapter.Order)
            .ThenBy(chapter => chapter.Title, StringComparer.OrdinalIgnoreCase)
            .ToList();

        Chapters = string.IsNullOrWhiteSpace(SearchQuery)
            ? allChapters
            : allChapters
                .Where(chapter =>
                    chapter.Title.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase) ||
                    chapter.Content.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase))
                .ToList();

        if (string.IsNullOrWhiteSpace(chapterSlug))
        {
            SelectedChapter = Chapters.FirstOrDefault();
        }
        else
        {
            SelectedChapter = Chapters.FirstOrDefault(chapter =>
                string.Equals(chapter.Slug, chapterSlug, StringComparison.OrdinalIgnoreCase));
            ChapterNotFound = SelectedChapter is null;
        }

        if (SelectedChapter is null)
        {
            return;
        }

        RenderChapter(SelectedChapter);
        var selectedIndex = Chapters.ToList().FindIndex(chapter =>
            string.Equals(chapter.Slug, SelectedChapter.Slug, StringComparison.Ordinal));
        PreviousChapter = selectedIndex > 0 ? Chapters[selectedIndex - 1] : null;
        NextChapter = selectedIndex + 1 < Chapters.Count ? Chapters[selectedIndex + 1] : null;
    }

    private List<HelpChapter> LoadChapters()
    {
        var contentDirectory = Path.Combine(_environment.ContentRootPath, "Help", "content");
        if (!Directory.Exists(contentDirectory))
        {
            throw new DirectoryNotFoundException($"Help chapter directory was not found: {contentDirectory}");
        }

        var paths = Directory.EnumerateFiles(contentDirectory, "*.md", SearchOption.TopDirectoryOnly)
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (paths.Count == 0)
        {
            throw new InvalidDataException($"No Help chapters were found in {contentDirectory}.");
        }

        return paths.Select(ParseChapter).ToList();
    }

    private static HelpChapter ParseChapter(string path)
    {
        var text = System.IO.File.ReadAllText(path);
        var lines = text.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n');
        if (lines.Length < 3 || lines[0].Trim() != "---")
        {
            throw new InvalidDataException($"Help chapter is missing front matter: {path}");
        }

        var frontMatterEnd = Array.FindIndex(lines, 1, line => line.Trim() == "---");
        if (frontMatterEnd < 0)
        {
            throw new InvalidDataException($"Help chapter front matter is not closed: {path}");
        }

        string? title = null;
        int? order = null;
        var roles = new List<string>();
        var readingRoles = false;

        for (var index = 1; index < frontMatterEnd; index++)
        {
            var line = lines[index].Trim();
            if (line.Length == 0)
            {
                continue;
            }

            if (line.StartsWith("- ", StringComparison.Ordinal) && readingRoles)
            {
                roles.Add(Unquote(line[2..].Trim()));
                continue;
            }

            readingRoles = false;
            var separator = line.IndexOf(':');
            if (separator < 0)
            {
                throw new InvalidDataException($"Invalid front matter in Help chapter: {path}");
            }

            var key = line[..separator].Trim();
            var value = line[(separator + 1)..].Trim();

            switch (key)
            {
                case "title":
                    title = Unquote(value);
                    break;
                case "order":
                    if (!int.TryParse(value, out var parsedOrder))
                    {
                        throw new InvalidDataException($"Help chapter has an invalid order: {path}");
                    }

                    order = parsedOrder;
                    break;
                case "roles":
                    readingRoles = value.Length == 0;
                    if (value.StartsWith('[') && value.EndsWith(']'))
                    {
                        roles.AddRange(value[1..^1]
                            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                            .Select(Unquote));
                    }
                    else if (value.Length > 0)
                    {
                        roles.Add(Unquote(value));
                    }

                    break;
            }
        }

        var filename = Path.GetFileNameWithoutExtension(path);
        var slug = Regex.Replace(filename, "^\\d+-", string.Empty);
        if (string.IsNullOrWhiteSpace(title) || order is null || !ChapterSlugPattern.IsMatch(slug))
        {
            throw new InvalidDataException($"Help chapter has invalid title, order, or filename: {path}");
        }

        var content = string.Join('\n', lines.Skip(frontMatterEnd + 1)).Trim();
        return new HelpChapter(title, order.Value, slug, content, roles);
    }

    private void RenderChapter(HelpChapter chapter)
    {
        var html = Markdown.ToHtml(chapter.Content, _markdownPipeline);
        var sanitizer = new HtmlSanitizer();
        sanitizer.AllowedAttributes.Add("id");
        sanitizer.AllowedAttributes.Add("alt");
        sanitizer.AllowedAttributes.Add("src");
        sanitizer.AllowedAttributes.Add("title");

        chapter.RenderedContent = sanitizer.Sanitize(html);
        chapter.Headings = HeadingPattern.Matches(chapter.RenderedContent)
            .Select(match =>
            {
                var idMatch = IdPattern.Match(match.Groups[2].Value);
                var title = WebUtility.HtmlDecode(TagPattern.Replace(match.Groups[3].Value, string.Empty)).Trim();
                return idMatch.Success && title.Length > 0
                    ? new HelpHeading(title, idMatch.Groups["id"].Value)
                    : null;
            })
            .Where(heading => heading is not null)
            .Cast<HelpHeading>()
            .ToList();
    }

    private static string Unquote(string value)
    {
        return value.Length >= 2 && value[0] == value[^1] && (value[0] == '"' || value[0] == '\'')
            ? value[1..^1]
            : value;
    }

    public sealed class HelpChapter(string title, int order, string slug, string content, IReadOnlyList<string> roles)
    {
        public string Title { get; } = title;
        public int Order { get; } = order;
        public string Slug { get; } = slug;
        public string Content { get; } = content;
        public IReadOnlyList<string> Roles { get; } = roles;
        public string RenderedContent { get; set; } = string.Empty;
        public IReadOnlyList<HelpHeading> Headings { get; set; } = [];
    }

    public sealed record HelpHeading(string Title, string Id);
}
