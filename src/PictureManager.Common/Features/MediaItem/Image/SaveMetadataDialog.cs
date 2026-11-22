using MH.UI.Dialogs;
using MH.Utils.Localization;
using PictureManager.Common.Localization;
using System.Threading;
using System.Threading.Tasks;

namespace PictureManager.Common.Features.MediaItem.Image;

public sealed class SaveMetadataDialog : ParallelProgressDialog<RealMediaItemM> {
  internal static class _loc {
    private const string _prefix = "MediaItems.SaveMetadata.";
    public const string ProgressTitle = _prefix + "Progress.dlg.Title";
    public const string Title = _prefix + "dlg.Title";
    public const string Message = _prefix + "dlg.Message";

    static _loc() {
      LocalizationManager.Register(typeof(_loc).Assembly, [
        (ProgressTitle, "Saving metadata to files..."),
        (Title, "Save metadata to files"),
        (Message + ".one", "Do you really want to save metadata to {0} file?"),
        (Message + ".other", "Do you really want to save metadata to {0} files?")
      ]);
    }

    public static void Register() { }
  }

  static SaveMetadataDialog() { _loc.Register(); }

  private readonly MediaItemS _mediaItemS;

  public SaveMetadataDialog(RealMediaItemM[] items, MediaItemS mediaItemS) :
    base(L.Get(_loc.ProgressTitle), Res.IconImageMultiple, items) {
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
      L.Get(_loc.Title),
      L.FormatPlural(_loc.Message, items.Length),
      MH.UI.Res.IconQuestion,
      true)) != 1) return;

    var current = mediaItemVM.Current;
    mediaItemVM.Current = null;
    await ShowAsync(new SaveMetadataDialog(items, mediaItemS));
    mediaItemVM.Current = current;
  }
}