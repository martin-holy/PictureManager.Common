using MH.UI.Dialogs;
using MH.Utils.Extensions;
using MH.Utils.Localization;
using PictureManager.Common.Localization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace PictureManager.Common.Features.MediaItem.Image;

public sealed class CompressImagesDialog : ParallelProgressDialog<ImageM> {
  internal static class _loc {
    private const string _prefix = "Image.Compress.dlg.";
    public const string Title = _prefix + "Title";
    public const string BtnCompress = _prefix + "btn.Compress";
    public const string TxtJpegQualityLevelText = _prefix + "txt.JpegQualityLevel";
    public const string TxtOriginal = _prefix + "txt.Original";
    public const string TxtCompressed = _prefix + "txt.Compressed";

    static _loc() {
      LocalizationManager.Register(typeof(_loc).Assembly, [
        (Title, "Compress images"),
        (BtnCompress, "Compress"),
        (TxtJpegQualityLevelText, "JPG quality level: {0}"),
        (TxtOriginal, "Original"),
        (TxtCompressed, "Compressed")
      ]);
    }

    public static void Register() { }
  }

  static CompressImagesDialog() { _loc.Register(); }

  private readonly ImageS _imageS;
  private int _jpegQualityLevel;
  private long _totalSourceSize;
  private long _totalCompressedSize;

  public int JpegQualityLevel {
    get => _jpegQualityLevel;
    set {
      _jpegQualityLevel = value;
      OnPropertyChanged();
      OnPropertyChanged(nameof(TxtJpegQualityLevel));
    }
  }
  public string TotalSourceSize => IOExtensions.FileSizeToString(_totalSourceSize);
  public string TotalCompressedSize => IOExtensions.FileSizeToString(_totalCompressedSize);

  public string TxtJpegQualityLevel => L.Format(_loc.TxtJpegQualityLevelText, _jpegQualityLevel);
  public string TxtOriginal { get; } = L.Get(_loc.TxtOriginal);
  public string TxtCompressed { get; } = L.Get(_loc.TxtCompressed);

  public CompressImagesDialog(ImageM[] items, ImageS imageS, int quality) : base(
    L.Get(_loc.Title), MH.UI.Res.IconImage, items, MH.UI.Res.IconImage, L.Get(_loc.BtnCompress), autoClose: false) {

    _imageS = imageS;
    _jpegQualityLevel = quality;
  }

  protected override void _customProgress(object? args) {
    if (args is not (long[] and [var sourceSize, var compressedSize])) return;
    _setSize(_totalSourceSize + sourceSize, _totalCompressedSize + compressedSize);
  }

  protected override bool _doBefore() {
    _setSize(0, 0);
    return true;
  }

  private void _setSize(long source, long compressed) {
    _totalSourceSize = source;
    _totalCompressedSize = compressed;
    OnPropertyChanged(nameof(TotalSourceSize));
    OnPropertyChanged(nameof(TotalCompressedSize));
  }

  protected override Task _do(ImageM image, CancellationToken token) {
    var sourceSize = new FileInfo(image.FilePath).Length;
    var bSuccess = _imageS.TryEncodeJpeg(image, _jpegQualityLevel);
    var compressedSize = bSuccess ? new FileInfo(image.FilePath).Length : sourceSize;

    _reportProgress(image.FileName, new[] { sourceSize, compressedSize });

    return Task.CompletedTask;
  }
}