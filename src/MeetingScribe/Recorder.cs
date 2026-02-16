using System;
using NAudio.Wave;

namespace MeetingScribe;

public sealed class Recorder
{
  private WaveInEvent? _waveIn;
  private WaveFileWriter? _writer;
  private string? _path;

  public void Start(string wavPath)
  {
    if (_waveIn != null) throw new InvalidOperationException("Already recording");

    _path = wavPath;
    _waveIn = new WaveInEvent
    {
      WaveFormat = new WaveFormat(16000, 1)
    };

    _writer = new WaveFileWriter(wavPath, _waveIn.WaveFormat);
    _waveIn.DataAvailable += (_, a) => _writer?.Write(a.Buffer, 0, a.BytesRecorded);
    _waveIn.RecordingStopped += (_, __) =>
    {
      _writer?.Dispose();
      _writer = null;
      _waveIn?.Dispose();
      _waveIn = null;
    };

    _waveIn.StartRecording();
  }

  public string Stop()
  {
    var path = _path ?? throw new InvalidOperationException("Not recording");
    _waveIn?.StopRecording();
    _path = null;
    return path;
  }
}
