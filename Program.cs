using MultiFormatKnowledgeAssistant.Knowledge;
using MultiFormatKnowledgeAssistant.Retrieval;
using MultiFormatKnowledgeAssistant.Services;

Console.Title = "Multi-Format Knowledge Assistant";

string documentsFolder = Path.Combine(
    AppContext.BaseDirectory, "Documents");

Directory.CreateDirectory(documentsFolder);

var chunks = DocumentLoader.LoadFolder(documentsFolder);
var retriever = new KeywordRetriever(chunks);

using var aiService = new OllamaRagService();

Console.WriteLine("====================================");
Console.WriteLine("   MULTI-FORMAT KNOWLEDGE ASSISTANT");
Console.WriteLine("   Visual Studio 2026");
Console.WriteLine("   Ollama + Qwen 3.5");
Console.WriteLine("====================================");

Console.WriteLine($"Loaded {chunks.Count} chunks.");
Console.WriteLine("Commands: sources, reload, exit");

while (true)
{
    Console.WriteLine();
    Console.Write("Question: ");

    string? question = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(question))
        continue;

    string command = question.Trim().ToLowerInvariant();

    if (command == "exit")
        break;

    if (command == "sources")
    {
        foreach (string source in chunks
            .Select(c => c.Source)
            .Distinct())
        {
            Console.WriteLine(source);
        }

        continue;
    }

    if (command == "reload")
    {
        chunks = DocumentLoader.LoadFolder(documentsFolder);
        retriever = new KeywordRetriever(chunks);

        Console.WriteLine(
            $"Reloaded {chunks.Count} chunks.");

        continue;
    }

    try
    {
        var results = retriever.Search(
            question, maxResults: 3);

        if (results.Count == 0)
        {
            Console.WriteLine(
                "No relevant information was found.");
            continue;
        }

        Console.WriteLine();
        Console.WriteLine("Retrieved Sources:");

        foreach (var chunk in results)
        {
            Console.WriteLine(
                $"- {chunk.Source} (Part {chunk.ChunkNumber})");
        }

        Console.WriteLine();
        Console.WriteLine("Qwen is preparing an answer...");

        string answer =
            await aiService.AnswerAsync(question, results);

        Console.WriteLine();
        Console.WriteLine("Assistant:");
        Console.WriteLine(answer);
    }
    catch (HttpRequestException ex)
    {
        Console.WriteLine(
            "Ollama API error: " + ex.Message);
    }
    catch (OperationCanceledException)
    {
        Console.WriteLine("The request timed out.");
    }
    catch (Exception ex)
    {
        Console.WriteLine("Error: " + ex.Message);
    }
}

Console.WriteLine("Goodbye!");

