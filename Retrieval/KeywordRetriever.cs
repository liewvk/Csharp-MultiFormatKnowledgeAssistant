using System.Text.RegularExpressions;
using MultiFormatKnowledgeAssistant.Models;

namespace MultiFormatKnowledgeAssistant.Retrieval;

public class KeywordRetriever
{
    private readonly List<DocumentChunk> _chunks;

    private static readonly HashSet<string> StopWords = new()
    {
        "the", "is", "a", "an", "and", "or", "to",
        "of", "in", "for", "on", "with", "what",
        "how", "can", "i", "my", "me", "do",
        "does", "about", "are", "our", "you"
    };

    public KeywordRetriever(List<DocumentChunk> chunks)
    {
        _chunks = chunks;
    }

    private static HashSet<string> Words(string text)
    {
        return Regex.Matches(
                text.ToLowerInvariant(), @"[a-z0-9]+")
            .Select(m => m.Value)
            .Where(w => w.Length > 2 && !StopWords.Contains(w))
            .ToHashSet();
    }

    public List<DocumentChunk> Search(
        string question,
        int maxResults = 3)
    {
        var keywords = Words(question);

        return _chunks
            .Select(c => new
            {
                Chunk = c,
                Score = Score(c, keywords)
            })
            .Where(x => x.Score > 0)
            .OrderByDescending(x => x.Score)
            .Take(maxResults)
            .Select(x => x.Chunk)
            .ToList();
    }

    private static int Score(
        DocumentChunk chunk,
        HashSet<string> keywords)
    {
        var titleWords = Words(chunk.Title);
        var contentWords = Words(chunk.Content);

        int score = 0;

        foreach (string word in keywords)
        {
            if (titleWords.Contains(word))
                score += 3;

            if (contentWords.Contains(word))
                score++;
        }

        return score;
    }
}
