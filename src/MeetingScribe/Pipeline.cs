using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace MeetingScribe;

public static class Pipeline
{
  // Placeholder: you will provide the whisper.cpp binary + model path later.
  // In CI we can download these.
  public static async Task<string> TranscribeOfflineAsync(string wavPath)
  {
    // TODO: integrate whisper.cpp invocation (whisper-cli / main.exe) on Windows.
    // For now, return a placeholder so the UI flow is testable.
    await Task.Delay(300);
    return $"[offline transcription placeholder]\nFile: {wavPath}\n\nNext: wire whisper.cpp (tiny/base/small).";
  }

  public static async Task<string> SummarizeAsync(string transcript)
  {
    var client = new SummaryClient();
    return await client.SummarizeAsync(transcript);
  }
}
