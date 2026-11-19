using MH.UI.Dialogs;
using MH.Utils.Extensions;
using MH.Utils.Localization;
using PictureManager.Common.Localization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace PictureManager.Common.Features.MediaItem.Image;

public sealed class CompressImagesDialog : ParallelProgressDialog<ImageM> {
  private static class _loc {
    private const string _prefix = "Image.Compress.";
    public const string Title = _prefix + "dlg.Title";
    public const string BtnCompress = _prefix + "dlg.btn.Compress";

    static _loc() {
      LocalizationManager.Register(typeof(_loc).Assembly, [
        (Title, "Compress images"),
        (BtnCompress, "Compress")
      ]);
    }

    public static void Register() { }
  }

  static CompressImagesDialog() { _loc.Register(); }
  public static void RegisterLocalization() { }

  private readonly ImageS _imageS;
  private int _jpegQualityLevel;
  private long _totalSourceSize;
  private long _totalCompressedSize;

  public int JpegQualityLevel { get => _jpegQualityLevel; set { _jpegQualityLevel = value; OnPropertyChanged(); } }
  public string TotalSourceSize => IOExtensions.FileSizeToString(_totalSourceSize);
  public string TotalCompressedSize => IOExtensions.FileSizeToString(_totalCompressedSize);

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