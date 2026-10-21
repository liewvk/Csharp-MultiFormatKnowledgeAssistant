namespace MultiFormatKnowledgeAssistant.Models;

public class DocumentChunk
{
    public string Id { get; set; } =
        Guid.NewGuid().ToString();

    public string Source { get; set; } = "";

    public string Title { get; set; } = "";

    public string Content { get; set; } = "";

    public int ChunkNumber { get; set; }

    public string FileType { get; set; } = "";
}

