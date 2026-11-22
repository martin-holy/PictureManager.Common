using MH.UI.Dialogs;
using MH.Utils;
using MH.Utils.BaseClasses;
using MH.Utils.Localization;
using PictureManager.Common.Localization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PictureManager.Common.Features.MediaItem.Image;

public sealed class ImageResizeDialog : ParallelProgressDialog<ImageM> {
  internal static class _loc {
    private const string _prefix = "Images.Resize.dlg.";
    public const string Title = _prefix + "Title";
    public const string BtnResize = _prefix + "btn.Resize";
    public const string OpenFolderBrowser = _prefix + "OpenFolderBrowser.cmd";
    public const string TxtPreserve = _prefix + "txt.Preserve";
    public const string TxtPreserveThumbnail = _prefix + "txt.PreserveThumbnail";
    public const string TxtPreserveMetadata = _prefix + "txt.PreserveMetadata";
    public const string TxtPreserveFolders = _prefix + "txt.PreserveFolders";
    public const string TxtResizeTo = _prefix + "txt.ResizeTo";
    public const string TxtMpx = _prefix + "txt.Mpx";
    public const string TxtSkipIfExists = _prefix + "txt.SkipIfExists";

    static _loc() {
      LocalizationManager.Register(typeof(_loc).Assembly, [
        (Title, "Resize images"),
        (BtnResize, "Resize"),
        (OpenFolderBrowser, "Select folder"),
        (TxtPreserve, "Preserve:"),
        (TxtPreserveThumbnail, "Thumbnail"),
        (TxtPreserveMetadata, "Metadata"),
        (TxtPreserveFolders, "Folders"),
        (TxtResizeTo, "Resize to:"),
        (TxtMpx, "{0:F1} Mpx"),
        (TxtSkipIfExists, "Skip if file exists")
      ]);
    }

    public static void Register() { }
  }

  static ImageResizeDialog() { _loc.Register(); }

  private bool _preserveThumbnail;
  private bool _preserveMetadata;
  private bool _preserveFolders;
  private bool _skipIfExists = true;
  private string? _destDir;
  private double _mpx;
  private double _maxMpx;
  private int _px;
  private Dictionary<ImageM, string>? _pathMap;

  public bool PreserveThumbnail { get => _preserveThumbnail; set { _preserveThumbnail = value; OnPropertyChanged(); } }
  public bool PreserveMetadata { get => _preserveMetadata; set { _preserveMetadata = value; OnPropertyChanged(); } }
  public bool PreserveFolders { get => _preserveFolders; set { _preserveFolders = value; OnPropertyChanged(); } }
  public bool SkipIfExists { get => _skipIfExists; set { _skipIfExists = value; OnPropertyChanged(); } }
  public string? DestDir { get => _destDir; set { _destDir = value; OnPropertyChanged(); } }
  public double Mpx { get => _mpx; set { _mpx = value; OnPropertyChanged(); OnPropertyChanged(nameof(TxtMpx)); } }
  public double MaxMpx { get => _maxMpx; set { _maxMpx = value; OnPropertyChanged(); } }

  public string TxtPreserve { get; } = L.Get(_loc.TxtPreserve);
  public string TxtPreserveThumbnail { get; } = L.Get(_loc.TxtPreserveThumbnail);
  public string TxtPreserveMetadata { get; } = L.Get(_loc.TxtPreserveMetadata);
  public string TxtPreserveFolders { get; } = L.Get(_loc.TxtPreserveFolders);
  public string TxtResizeTo { get; } = L.Get(_loc.TxtResizeTo);
  public string TxtMpx => L.Format(_loc.TxtMpx, Mpx);
  public string TxtSkipIfExists { get; } = L.Get(_loc.TxtSkipIfExists);

  public AsyncRelayCommand OpenFolderBrowserCommand { get; }

  public ImageResizeDialog(ImageM[] items) : base(
    L.Get(_loc.Title), Res.IconImageMultiple, items, null, L.Get(_loc.BtnResize)) {

    OpenFolderBrowserCommand = new(_openFolderBrowser, Res.IconFolder, L.Get(_loc.OpenFolderBrowser));
    _setMaxMpx(items);
  }

  public ImageResizeDialog(ImageM[] items, string destDir) : this(items) {
    Core.Settings.Common.AddDirectorySelectFolder(destDir);
    _destDir = destDir;
  }

  protected override bool _canAction() =>
    !string.IsNullOrEmpty(_destDir);

  protected override bool _doBefore() {
    _px = Convert.ToInt32(Mpx * 1000000);
    _pathMap = _buildPathMap(_items, _destDir!);

    foreach (var dir in _pathMap.Values.Select(Path.GetDirectoryName).Distinct()!)
      Directory.CreateDirectory(dir!);

    return true;
  }

  protected override void _doAfter() {
    base._doAfter();
    _pathMap?.Clear();
  }

  protected override Task _do(ImageM image, CancellationToken token) {
    _reportProgress(image.FileName);

    try {
      var dest = _pathMap![image];

      if (File.Exists(dest)) {
        if (_skipIfExists) return Task.CompletedTask;
        File.Delete(dest);
      }

      ImageS.ResizeJpeg(image, dest, _px, _preserveMetadata, _preserveThumbnail, Core.Settings.Common.JpegQuality);
    }
    catch (Exception ex) {
      Log.Error(ex, image.FilePath);
    }

    return Task.CompletedTask;
  }

  private Dictionary<ImageM, string> _buildPathMap(ImageM[] files, string destinationRoot) {
    var map = new Dictionary<ImageM, string>();

    if (!_preserveFolders) {
      foreach (var file in files)
        map[file] = Path.Combine(destinationRoot, file.FileName);

      return map;
    }

    var commonRoot = _getCommonRoot(files.Select(x => x.FilePath).ToArray());
    foreach (var file in files) {
      var relative = Path.GetRelativePath(commonRoot, file.FilePath);
      map[file] = Path.Combine(destinationRoot, relative);
    }

    return map;
  }

  private static string _getCommonRoot(string[] paths) {
    if (paths.Length == 0) return string.Empty;

    if (paths.Length == 1)
      return Path.GetDirectoryName(Path.GetFullPath(paths[0])) ?? string.Empty;

    var separated = paths
      .Select(x => Path
        .GetFullPath(x)
        .TrimEnd(Path.DirectorySeparatorChar)
        .Split(Path.DirectorySeparatorChar))
      .ToArray();

    var minLen = separated.Min(parts => parts.Length);
    var lastCommon = 0;

    for (var i = 1; i < minLen; i++) // start at index 1 -> skip drive letter
      if (separated.All(parts => parts[i].Equals(separated[0][i], StringComparison.OrdinalIgnoreCase)))
        lastCommon = i;
      else
        break;

    // Join from index 1 to lastCommon to drop drive
    return string.Join(Path.DirectorySeparatorChar, separated[0].Take(lastCommon + 1));
  }

  private void _setMaxMpx(ImageM[] items) {
    var maxPx = 0;
    foreach (var mi in items) {
      var px = mi.Width * mi.Height;
      if (px > maxPx) maxPx = px;
    }

    MaxMpx = Math.Round(maxPx / 1000000.0, 1);
    Mpx = MaxMpx;
  }

  private async Task _openFolderBrowser(CancellationToken token) {
    DestDir = await CoreVM.BrowseForFolder();
  }
}