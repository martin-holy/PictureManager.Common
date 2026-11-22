using MH.UI.Controls;
using MH.Utils;
using MH.Utils.BaseClasses;
using MH.Utils.Extensions;
using MH.Utils.Localization;
using PictureManager.Common.Features.Folder;
using PictureManager.Common.Features.MediaItem;
using PictureManager.Common.Localization;
using PictureManager.Common.Utils;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PictureManager.Common.Features.Common;

public sealed class FileOperationCollisionDialog : Dialog {
  internal static class _loc {
    private const string _prefix = "MediaItems.FileOperationCollision.dlg.";
    public const string Title = _prefix + "Title";
    public const string BtnRename = _prefix + "btn.Rename";
    public const string BtnReplace = _prefix + "btn.Replace";
    public const string BtnSkip = _prefix + "btn.Skip";
    public const string ErrEmptyFileName = _prefix + "err.EmptyFileName";
    public const string TxtDimensions = _prefix + "txt.Dimensions";
    public const string TxtFileSize = _prefix + "txt.FileSize";
    public const string TxtFileName = _prefix + "txt.FileName";
    public const string TxtModified = _prefix + "txt.Modified";

    static _loc() {
      LocalizationManager.Register(typeof(_loc).Assembly, [
        (Title, "File already exists"),
        (BtnRename, "Rename"),
        (BtnReplace, "Replace"),
        (BtnSkip, "Skip"),
        (ErrEmptyFileName, "New file name is empty!"),
        (TxtDimensions, "Dimensions:"),
        (TxtFileSize, "File Size:"),
        (TxtFileName, "File name:"),
        (TxtModified, "Modified:")
      ]);
    }

    public static void Register() { }
  }

  static FileOperationCollisionDialog() { _loc.Register(); }

  private readonly FolderM _destFolder;

  private string _error = string.Empty;
  private string _fileName;
  private FileCollisionInfo _dstFile;

  public string Error { get => _error; set { _error = value; OnPropertyChanged(); } }
  public string FileName { get => _fileName; set { _fileName = value; OnPropertyChanged(); } }
  public FileCollisionInfo SrcFile { get; }
  public FileCollisionInfo DstFile { get => _dstFile; set { _dstFile = value; OnPropertyChanged(); } }

  public string TxtDimensions { get; } = L.Get(_loc.TxtDimensions);
  public string TxtFileSize { get; } = L.Get(_loc.TxtFileSize);
  public string TxtFileName { get; } = L.Get(_loc.TxtFileName);
  public string TxtModified { get; } = L.Get(_loc.TxtModified);

  public FileOperationCollisionDialog(FolderM srcFolder, FolderM dstFolder, RealMediaItemM? srcMi, RealMediaItemM? dstMi, string fileName) : base(L.Get(_loc.Title), Res.IconImageMultiple) {
    _destFolder = dstFolder;
    SrcFile = new(srcMi, IOExtensions.PathCombine(srcFolder.FullPath, fileName));
    _dstFile = new(dstMi, IOExtensions.PathCombine(dstFolder.FullPath, fileName));
    _fileName = fileName;

    Buttons = [
      new(new AsyncRelayCommand(_rename, null, L.Get(_loc.BtnRename))),
      new(new RelayCommand(() => Result = (int)CollisionResult.Replace, null, L.Get(_loc.BtnReplace))),
      new(new RelayCommand(() => Result = (int)CollisionResult.Skip, null, L.Get(_loc.BtnSkip)))
    ]; 
  }

  public static async Task<(CollisionResult, string, RealMediaItemM?)> Open(FolderM src, FolderM dest, RealMediaItemM? srcMi, RealMediaItemM? dstMi, string fileName, RealMediaItemM? replacedMi) {
    var result = CollisionResult.Skip;
    var outFileName = fileName;
    var outReplacedMi = replacedMi;

    _initThumb(srcMi);
    _initThumb(dstMi);

    await Tasks.RunOnUiThread(async () => {
      var cd = new FileOperationCollisionDialog(src, dest, srcMi, dstMi, outFileName);
      result = (CollisionResult)await ShowAsync(cd);
      outFileName = cd.FileName;
      outReplacedMi = cd.DstFile.MediaItem;
    });

    return new(result, outFileName, outReplacedMi);
  }

  private static void _initThumb(RealMediaItemM? mi) {
    if (mi == null) return;
    mi.SetInfoBox();
    mi.SetThumbSize();
  }

  private static async Task<RealMediaItemM?> _getMediaItem(FolderM folder, string fileName) {
    var mi = folder.MediaItems.GetByFileName(fileName)
             ?? await CopyMoveU.CreateMediaItemAndReadMetadata(folder, fileName);

    _initThumb(mi);
    return mi;
  }

  private async Task _rename(CancellationToken token) {
    Error = string.Empty;

    if (string.IsNullOrEmpty(FileName)) {
      Error = L.Get(_loc.ErrEmptyFileName);
      return;
    }

    if (Path.GetInvalidFileNameChars().Any(FileName.Contains)) {
      Error = L.Get(MediaItemVM._loc.RenameErrInvalidCharacters);
      return;
    }

    var newFilePath = Path.Combine(_destFolder.FullPath, FileName);
    if (File.Exists(newFilePath)) {
      DstFile = new(await _getMediaItem(_destFolder, FileName), newFilePath);
      return;
    }

    Result = (int)CollisionResult.Rename;
  }
}

public sealed class FileCollisionInfo {
  public RealMediaItemM? MediaItem { get; }
  public string Path { get; } = string.Empty;
  public string Size { get; } = string.Empty;
  public string LastWrite { get; } = string.Empty;

  public FileCollisionInfo(RealMediaItemM? mediaItem, string filePath) {
    MediaItem = mediaItem;

    try {
      var fi = new FileInfo(filePath);
      Path = fi.FullName;
      Size = fi.Length.ToString("0 B");
      LastWrite = fi.LastWriteTime.ToString("G");
    }
    catch (Exception ex) {
      MH.Utils.Log.Error(ex);
    }
  }
}