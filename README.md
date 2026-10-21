# Multi-Format Knowledge Assistant

A C# console application that loads knowledge from multiple document formats, finds relevant passages using keyword-based retrieval, and answers questions with a local Ollama LLM.

This project is a practical example of a retrieval-augmented generation (RAG) workflow in .NET, using a small document corpus stored in the `Documents` folder.

## Features

- Loads documents from a folder
- Supports mixed file types such as:
  - CSV
  - Markdown
  - TXT
- Splits content into searchable chunks
- Retrieves the most relevant chunks by keyword scoring
- Sends the retrieved context to Ollama for grounded answers
- Runs as a simple command-line app

## Project Structure

- `Program.cs` – console loop, user commands, retrieval flow, and answer display
- `Documents/` – sample knowledge files used by the app
- `Knowledge/` – project folder for knowledge-related supporting code
- `Retrieval/KeywordRetriever.cs` – lightweight keyword scoring retriever
- `Services/OllamaRagService.cs` – Ollama chat integration for grounded responses
- `Models/` – data models used throughout the application

## How it Works

1. The app creates a `Documents` folder under the output directory.
2. It loads all available knowledge files from that folder.
3. The content is chunked and indexed.
4. A user asks a question in the console.
5. The keyword retriever finds the most relevant chunks.
6. Those chunks are passed to Ollama as context.
7. The model answers using only the supplied document content.

## Included Example Documents

The repository includes a small sample knowledge set:

- `Documents/products.csv`
- `Documents/programming-notes.md`
- `Documents/refund-policy.txt`

These files are copied to the output directory when the project is built so the app can use them immediately.

## Prerequisites

- .NET 10 SDK
- Ollama installed and running locally
- The model `qwen3.5:9b` available in Ollama

## Setup

1. Install and start Ollama.
2. Pull the model used by the app:

```bash
ollama pull qwen3.5:9b
```

3. Restore and build the project:

```bash
dotnet restore
dotnet build
```

4. Run the app:

```bash
dotnet run
```

## Usage

When the app starts, it prints a simple prompt:

```text
Question:
```

You can ask questions such as:

- What is the refund policy?
- What products are available?
- What programming notes are in the knowledge base?

The app also supports these commands:

- `sources` – list the files used as sources
- `reload` – reload documents from the folder
- `exit` – close the application

## Notes

- The app uses a local Ollama endpoint at `http://localhost:11434/`.
- The model configuration is currently set to `qwen3.5:9b`.
- The retrieval logic is intentionally lightweight and keyword-based rather than vector-based.

## License

This repository does not currently include a license file. Review the repository settings or add one if you want to define distribution terms.

## Summary

This project demonstrates a simple but complete RAG-style knowledge assistant built in C#. It is useful for learning how local document retrieval and LLM-based answer generation can be combined in a .NET application.
