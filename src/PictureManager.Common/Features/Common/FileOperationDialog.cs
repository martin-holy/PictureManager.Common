using MH.UI.Controls;
using MH.Utils;
using MH.Utils.Localization;
using PictureManager.Common.Localization;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PictureManager.Common.Features.Common;

public sealed class FileOperationDialog : Dialog {
  private static class _loc {
    private const string _prefix = "MediaItem.FileOperation.dlg.";
    public const string TxtFrom = _prefix + "txt.From";
    public const string TxtTo = _prefix + "txt.To";
    public const string TxtFile = _prefix + "txt.File";

    static _loc() {
      LocalizationManager.Register(typeof(_loc).Assembly, [
        (TxtFrom, "From:"),
        (TxtTo, "To:"),
        (TxtFile, "File:")
      ]);
    }

    public static void Register() { }
  }

  static FileOperationDialog() { _loc.Register(); }
  public static void RegisterLocalization() { }

  private int _progressValue;
  private bool _isIndeterminate;
  private string _dirFrom = string.Empty;
  private string _dirTo = string.Empty;
  private string _fileName = string.Empty;

  public int ProgressValue { get => _progressValue; set { _progressValue = value; OnPropertyChanged(); } }
  public bool IsIndeterminate { get => _isIndeterminate; set { _isIndeterminate = value; OnPropertyChanged(); } }
  public string DirFrom { get => _dirFrom; set { _dirFrom = value; OnPropertyChanged(); } }
  public string DirTo { get => _dirTo; set { _dirTo = value; OnPropertyChanged(); } }
  public string FileName { get => _fileName; set { _fileName = value; OnPropertyChanged(); } }
  public CancellationTokenSource? WorkCts { get; private set; } = new();
  public object? WorkResult { get; private set; }
  public Task? WorkTask { get; set; }
  public IProgress<object[]> Progress { get; set; }

  public string TxtFrom { get; } = L.Get(_loc.TxtFrom);
  public string TxtTo { get; } = L.Get(_loc.TxtTo);
  public string TxtFile { get; } = L.Get(_loc.TxtFile);

  public FileOperationDialog(string title) : base(title, Res.IconImageMultiple) {
    _progressValue = 0;

    Progress = new Progress<object[]>(e => {
      ProgressValue = (int)e[0];
      DirFrom = (string)e[1];
      DirTo = (string)e[2];
      FileName = (string)e[3];
    });

    Buttons = [new(CancelCommand, false, true)];
  }

  ~FileOperationDialog() {
    if (WorkCts != null) {
      WorkCts.Dispose();
      WorkCts = null;
    }
  }

  public void SetWorkTask<T>(Task<T> work) {
    WorkTask = _handleWorkAsync(work);
  }

  private async Task _handleWorkAsync<T>(Task<T> work) {
    WorkResult = await work;
    await Tasks.RunOnUiThread(() => Result = 1);
  }

  protected override Task _onResultChanged(int result) {
    if (result != 0 || WorkCts == null) return Task.CompletedTask;
    WorkCts.Cancel();
    return WorkTask ?? Task.CompletedTask;
  }
}