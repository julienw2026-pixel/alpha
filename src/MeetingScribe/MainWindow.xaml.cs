using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

namespace MeetingScribe;

public partial class MainWindow : Window
{
  private readonly Recorder _recorder = new();

  public MainWindow()
  {
    InitializeComponent();
    Status.Text = "Idle";
  }

  private async void BtnStart_Click(object sender, RoutedEventArgs e)
  {
    try
    {
      var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "MeetingScribe");
      Directory.CreateDirectory(dir);
      var wavPath = Path.Combine(dir, $"meeting-{DateTime.Now:yyyyMMdd-HHmmss}.wav");

      _recorder.Start(wavPath);
      BtnStart.IsEnabled = false;
      BtnStop.IsEnabled = true;
      Status.Text = $"Recording: {wavPath}";
    }
    catch (Exception ex)
    {
      MessageBox.Show(ex.ToString(), "Start failed");
    }
  }

  private async void BtnStop_Click(object sender, RoutedEventArgs e)
  {
    try
    {
      var wavPath = _recorder.Stop();
      BtnStart.IsEnabled = true;
      BtnStop.IsEnabled = false;
      Status.Text = "Transcribing…";

      var transcript = await Pipeline.TranscribeOfflineAsync(wavPath);
      TranscriptBox.Text = transcript;

      Status.Text = "Summarizing…";
      var summary = await Pipeline.SummarizeAsync(transcript);
      SummaryBox.Text = summary;

      Status.Text = "Done";
    }
    catch (Exception ex)
    {
      Status.Text = "Error";
      MessageBox.Show(ex.ToString(), "Stop failed");
    }
  }
}
