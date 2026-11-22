using MH.UI.Dialogs;
using MH.Utils.Localization;
using PictureManager.Common.Localization;
using System.Threading;
using System.Threading.Tasks;

namespace PictureManager.Common.Features.MediaItem;

public sealed class ReloadMetadataDialog : ParallelProgressDialog<RealMediaItemM> {
  internal static class _loc {
    private const string _prefix = "MediaItems.ReloadMetadata.";
    public const string ProgressTitle = _prefix + "Progress.dlg.Title";
    public const string Title = _prefix + "dlg.Title";
    public const string Message = _prefix + "dlg.Message";

    static _loc() {
      LocalizationManager.Register(typeof(_loc).Assembly, [
        (ProgressTitle, "Reloading metadata..."),
        (Title, "Reload metadata from files"),
        (Message + ".one", "Do you really want to reload metadata for {0} file?"),
        (Message + ".other", "Do you really want to reload metadata for {0} files?")
      ]);
    }

    public static void Register() { }
  }

  static ReloadMetadataDialog() { _loc.Register(); }

  private readonly MediaItemS _mediaItemS;

  public ReloadMetadataDialog(RealMediaItemM[] items, MediaItemS mediaItemS) :
    base(L.Get(_loc.ProgressTitle), MH.UI.Res.IconImage, items) {
    _mediaItemS = mediaItemS;
    _autoStart = true;
  }

  protected override Task _do(RealMediaItemM item, CancellationToken token) {
    _reportProgress(item.FilePath);
    return _mediaItemS.ReloadMetadata(item);
  }

  protected override void _doAfter() =>
    _mediaItemS.OnMetadataReloaded(_items);

  public static async Task Open(RealMediaItemM[] items, MediaItemS mediaItemS) {
    if (items.Length == 0 || await ShowAsync(new MessageDialog(
      L.Get(_loc.Title),
      L.FormatPlural(_loc.Message, items.Length),
      MH.UI.Res.IconQuestion,
      true)) != 1) return;

    await ShowAsync(new ReloadMetadataDialog(items, mediaItemS));
  }
}