using MH.UI.Dialogs;
using MH.Utils.Localization;
using PictureManager.Common.Localization;
using System.Threading;
using System.Threading.Tasks;

namespace PictureManager.Common.Features.MediaItem.Image;

public sealed class SaveMetadataDialog : ParallelProgressDialog<RealMediaItemM> {
  private static class _loc {
    private const string _prefix = "MediaItem.SaveMetadata.";
    public const string Title = _prefix + "dlg.Title";
    public const string OpenTitle = _prefix + "Open.dlg.Title";
    public const string OpenMessage = _prefix + "Open.dlg.Message";

    static _loc() {
      LocalizationManager.Register(typeof(_loc).Assembly, [
        (Title, "Saving metadata to files..."),
        (OpenTitle, "Save metadata to files"),
        (OpenMessage + ".one", "Do you really want to save metadata to {0} file?"),
        (OpenMessage + ".other", "Do you really want to save metadata to {0} files?")
      ]);
    }

    public static void Register() { }
  }

  static SaveMetadataDialog() { _loc.Register(); }
  public static void RegisterLocalization() => _loc.Register();

  private readonly MediaItemS _mediaItemS;

  public SaveMetadataDialog(RealMediaItemM[] items, MediaItemS mediaItemS) :
    base(L.Get(_loc.Title), Res.IconImageMultiple, items) {
    _mediaItemS = mediaItemS;
    _autoStart = true;
  }

  protected override Task _do(RealMediaItemM item, CancellationToken token) {
    _reportProgress(item.FilePath);
    _mediaItemS.TryWriteMetadata(item);

    return Task.CompletedTask;
  }

  public static async Task Open(RealMediaItemM[] items, MediaItemS mediaItemS, MediaItemVM mediaItemVM) {
    if (await ShowAsync(new MessageDialog(
      L.Get(_loc.OpenTitle),
      L.FormatPlural(_loc.OpenMessage, items.Length),
      MH.UI.Res.IconQuestion,
      true)) != 1) return;

    var current = mediaItemVM.Current;
    mediaItemVM.Current = null;
    await ShowAsync(new SaveMetadataDialog(items, mediaItemS));
    mediaItemVM.Current = current;
  }
}