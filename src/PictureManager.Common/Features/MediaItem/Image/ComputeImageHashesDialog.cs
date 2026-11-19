using MH.UI.Dialogs;
using MH.Utils.Imaging;
using MH.Utils.Localization;
using PictureManager.Common.Localization;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace PictureManager.Common.Features.MediaItem.Image;

public sealed class ComputeImageHashesDialog : ProgressDialog<MediaItemM> {
  private static class _loc {
    private const string _prefix = "Image.ComputeImageHashes.";
    public const string Title = _prefix + "dlg.Title";

    static _loc() {
      LocalizationManager.Register(typeof(_loc).Assembly, [
        (Title, "Computing Hashes ...")
      ]);
    }

    public static void Register() { }
  }

  static ComputeImageHashesDialog() { _loc.Register(); }
  public static void RegisterLocalization() { }

  private readonly Dictionary<object, long> _hashes;
  private readonly ImagingU.ImageHashFunc _hashMethod;

  public ComputeImageHashesDialog(MediaItemM[] items, Dictionary<object, long> hashes, ImagingU.ImageHashFunc hashMethod) :
    base(L.Get(_loc.Title), Res.IconCompare, items) {
    _hashes = hashes;
    _hashMethod = hashMethod;
    _autoStart = true;
  }

  protected override Task _do(MediaItemM item, CancellationToken token) {
    _reportProgress(item.FilePath);

    if (_hashes.ContainsKey(item)) return Task.CompletedTask;

    if (!File.Exists(item.FilePathCache))
      MediaItemS.CreateImageThumbnail(item);

    _hashes.Add(item, _hashMethod(item.FilePathCache));

    return Task.CompletedTask;
  }
}