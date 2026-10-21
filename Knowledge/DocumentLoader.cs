using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using HtmlAgilityPack;
using Microsoft.VisualBasic.FileIO;
using MultiFormatKnowledgeAssistant.Models;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using UglyToad.PdfPig;
using UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor;

namespace MultiFormatKnowledgeAssistant.Knowledge;

public static class DocumentLoader
{
    private static readonly HashSet<string> Extensions =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ".txt", ".md", ".pdf", ".docx",
            ".html", ".htm", ".csv"
        };

    public static List<DocumentChunk> LoadFolder(string folder)
    {
        var result = new List<DocumentChunk>();

        if (!Directory.Exists(folder))
            return result;

        foreach (string file in Directory.GetFiles(folder))
        {
            if (!Extensions.Contains(Path.GetExtension(file)))
                continue;

            try
            {
                result.AddRange(LoadFile(file));
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"Could not load {Path.GetFileName(file)}: " +
                    ex.Message);
            }
        }

        return result;
    }

    public static List<DocumentChunk> LoadFile(string path)
    {
        string extension =
            Path.GetExtension(path).ToLowerInvariant();

        if (extension == ".csv")
            return LoadCsv(path);

        string text = extension switch
        {
            ".txt" or ".md" => File.ReadAllText(path),
            ".pdf" => LoadPdf(path),
            ".docx" => LoadWord(path),
            ".html" or ".htm" => LoadHtml(path),
            _ => ""
        };

        return CreateChunks(text, path);
    }

    private static string LoadPdf(string path)
    {
        var text = new StringBuilder();

        using var document = PdfDocument.Open(path);

        foreach (var page in document.GetPages())
        {
            text.AppendLine(
                ContentOrderTextExtractor.GetText(page));
            text.AppendLine();
        }

        return text.ToString();
    }

    private static string LoadWord(string path)
    {
        using var document =
            WordprocessingDocument.Open(path, false);

        var body = document.MainDocumentPart?
            .Document.Body;

        if (body == null)
            return "";

        return string.Join(
            Environment.NewLine,
            body.Descendants<Paragraph>()
                .Select(p => p.InnerText)
                .Where(s => !string.IsNullOrWhiteSpace(s)));
    }

    private static string LoadHtml(string path)
    {
        var document = new HtmlDocument();
        document.Load(path);

        var unwanted = document.DocumentNode.SelectNodes(
            "//script|//style|//noscript");

        if (unwanted != null)
        {
            foreach (var node in unwanted)
                node.Remove();
        }

        string htmlText =
            document.DocumentNode.SelectSingleNode("//body")?
                .InnerText
            ?? document.DocumentNode.InnerText;

        return WebUtility.HtmlDecode(htmlText);
    }

    private static List<DocumentChunk> LoadCsv(string path)
    {
        var chunks = new List<DocumentChunk>();

        using var parser = new TextFieldParser(path);
        parser.TextFieldType = FieldType.Delimited;
        parser.SetDelimiters(",");
        parser.HasFieldsEnclosedInQuotes = true;

        if (parser.EndOfData)
            return chunks;

        string[] headers = parser.ReadFields()
            ?? Array.Empty<string>();

        int row = 0;

        while (!parser.EndOfData)
        {
            string[] values = parser.ReadFields()
                ?? Array.Empty<string>();

            if (values.Length != headers.Length)
                throw new InvalidDataException(
                    "CSV row has an unexpected number of fields.");

            row++;

            string content = string.Join(
                Environment.NewLine,
                headers.Select((header, index) =>
                    $"{header}: {values[index]}"));

            chunks.Add(new DocumentChunk
            {
                Source = Path.GetFileName(path),
                Title = $"CSV Record {row}",
                Content = content,
                ChunkNumber = row,
                FileType = ".csv"
            });
        }

        return chunks;
    }

    private static List<DocumentChunk> CreateChunks(
        string text,
        string path,
        int maxWords = 120)
    {
        var chunks = new List<DocumentChunk>();

        text = Regex.Replace(text, @"[ \t]+", " ");
        text = text.Trim();

        string[] words = text.Split(
            (char[]?)null,
            StringSplitOptions.RemoveEmptyEntries);

        string source = Path.GetFileName(path);

        for (int i = 0; i < words.Length; i += maxWords)
        {
            int number = i / maxWords + 1;

            chunks.Add(new DocumentChunk
            {
                Source = source,
                Title =
                    $"{Path.GetFileNameWithoutExtension(path)} " +
                    $"- Part {number}",
                Content = string.Join(
                    " ", words.Skip(i).Take(maxWords)),
                ChunkNumber = number,
                FileType = Path.GetExtension(path)
            });
        }

        return chunks;
    }
}

