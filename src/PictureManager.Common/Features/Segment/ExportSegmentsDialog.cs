using MH.UI.Dialogs;
using MH.Utils;
using MH.Utils.BaseClasses;
using MH.Utils.Localization;
using PictureManager.Common.Localization;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace PictureManager.Common.Features.Segment;

public sealed class ExportSegmentsDialog : ParallelProgressDialog<SegmentM> {
  internal static class _loc {
    private const string _prefix = "Segments.Export.dlg.";
    public const string Title = _prefix + "Title";
    public const string BtnExport = _prefix + "btn.Export";
    public const string TxtExportTo = _prefix + "txt.ExportTo";
    public const string OpenFolderBrowser = _prefix + "OpenFolderBrowser.cmd";

    static _loc() {
      LocalizationManager.Register(typeof(_loc).Assembly, [
        (Title, "Export Segments"),
        (BtnExport, "Export"),
        (TxtExportTo, "Export to:"),
        (OpenFolderBrowser, "Select folder")
      ]);
    }

    public static void Register() { }
  }

  static ExportSegmentsDialog() { _loc.Register(); }

  private string? _destDir;

  public string? DestDir { get => _destDir; set { _destDir = value; OnPropertyChanged(); } }
  public string ExportToText { get; } = L.Get(_loc.TxtExportTo);
  public AsyncRelayCommand? OpenFolderBrowserCommand { get; }

  public ExportSegmentsDialog(SegmentM[] items) : base(L.Get(_loc.Title), Res.IconSegment, items, null, L.Get(_loc.BtnExport)) {
    OpenFolderBrowserCommand = new(_openFolderBrowser, Res.IconFolder, L.Get(_loc.OpenFolderBrowser));
  }

  public ExportSegmentsDialog(SegmentM[] items, string destDir) : base(L.Get(_loc.Title), Res.IconSegment, items) {
    _destDir = destDir;
    _autoStart = true;
  }

  protected override bool _canAction() =>
    !string.IsNullOrEmpty(_destDir);

  protected override bool _doBefore() {
    Directory.CreateDirectory(_destDir!);
    return true;
  }

  protected override Task _do(SegmentM segment, CancellationToken token) {
    var fileName = segment.MediaItem.FileName.Replace(".jpg", "_segment_" + segment.Id + ".jpg", StringComparison.OrdinalIgnoreCase);
    _reportProgress(fileName);

    try {
      SegmentS.ExportSegment(segment, Path.Combine(_destDir!, fileName));
    }
    catch (Exception ex) {
      Log.Error(ex, segment.MediaItem.FilePath);
    }

    return Task.CompletedTask;
  }

  private async Task _openFolderBrowser(CancellationToken token) {
    DestDir = await CoreVM.BrowseForFolder();
  }
}