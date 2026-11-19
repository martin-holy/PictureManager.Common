using MH.UI.Dialogs;
using MH.Utils.Localization;
using PictureManager.Common.Localization;
using System.Threading;
using System.Threading.Tasks;

namespace PictureManager.Common.Features.MediaItem;

public sealed class ReloadMetadataDialog : ParallelProgressDialog<RealMediaItemM> {
  private static class _loc {
    private const string _prefix = "MediaItem.ReloadMetadata.";
    public const string Title = _prefix + "dlg.Title";
    public const string OpenTitle = _prefix + "Open.dlg.Title";
    public const string OpenMessage = _prefix + "Open.dlg.Message";

    static _loc() {
      LocalizationManager.Register(typeof(_loc).Assembly, [
        (Title, "Reloading metadata..."),
        (OpenTitle, "Reload metadata from files"),
        (OpenMessage + ".one", "Do you really want to reload metadata for {0} file?"),
        (OpenMessage + ".other", "Do you really want to reload metadata for {0} files?")
      ]);
    }

    public static void Register() { }
  }

  static ReloadMetadataDialog() { _loc.Register(); }
  public static void RegisterLocalization() { }

  private readonly MediaItemS _mediaItemS;

  public ReloadMetadataDialog(RealMediaItemM[] items, MediaItemS mediaItemS) :
    base(L.Get(_loc.Title), MH.UI.Res.IconImage, items) {
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
      L.Get(_loc.OpenTitle),
      L.FormatPlural(_loc.OpenMessage, items.Length),
      MH.UI.Res.IconQuestion,
      true)) != 1) return;

    await ShowAsync(new ReloadMetadataDialog(items, mediaItemS));
  }
}