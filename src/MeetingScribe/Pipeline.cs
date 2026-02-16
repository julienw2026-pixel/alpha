using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace MeetingScribe;

public static class Pipeline
{
  public static async Task<string> TranscribeOfflineAsync(string wavPath)
  {
    if (!File.Exists(wavPath))
      throw new FileNotFoundException("WAV not found", wavPath);

    var baseDir = AppContext.BaseDirectory;
    var toolDir = Path.Combine(baseDir, "tools", "whisper");

    // Packaged by CI: tools/whisper/whisper-cli.exe + models/ggml-*.bin
    var whisperExe = Path.Combine(toolDir, "whisper-cli.exe");
    var model = Path.Combine(toolDir, "models", "ggml-small.bin");

    if (!File.Exists(whisperExe))
      return "Missing whisper-cli.exe (not packaged).";
    if (!File.Exists(model))
      return "Missing model ggml-small.bin (not packaged).";

    var outBase = Path.Combine(Path.GetDirectoryName(wavPath)!, Path.GetFileNameWithoutExtension(wavPath));

    var args = new StringBuilder();
    args.Append("-m \"").Append(model).Append("\" ");
    args.Append("-f \"").Append(wavPath).Append("\" ");
    args.Append("-otxt ");
    args.Append("-of \"").Append(outBase).Append("\" ");
    args.Append("--language auto ");

    var psi = new ProcessStartInfo
    {
      FileName = whisperExe,
      Arguments = args.ToString(),
      UseShellExecute = false,
      RedirectStandardOutput = true,
      RedirectStandardError = true,
      CreateNoWindow = true,
      WorkingDirectory = toolDir
    };

    using var p = Process.Start(psi) ?? throw new Exception("Failed to start whisper-cli");
    var stdout = await p.StandardOutput.ReadToEndAsync();
    var stderr = await p.StandardError.ReadToEndAsync();
    await p.WaitForExitAsync();

    var txt = outBase + ".txt";
    if (File.Exists(txt))
      return await File.ReadAllTextAsync(txt);

    return $"Transcription failed (exit {p.ExitCode}).\n\nSTDOUT:\n{stdout}\n\nSTDERR:\n{stderr}";
  }

  public static async Task<string> SummarizeAsync(string transcript)
  {
    var client = new SummaryClient();
    return await client.SummarizeAsync(transcript);
  }
}
