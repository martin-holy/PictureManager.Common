using MH.UI.Dialogs;
using MH.Utils.Extensions;
using System.Threading;
using System.Threading.Tasks;

namespace PictureManager.Common.Features.MediaItem.Image;

public sealed class SaveMetadataDialog : ParallelProgressDialog<RealMediaItemM> {
  private readonly MediaItemS _mediaItemS;

  public SaveMetadataDialog(RealMediaItemM[] items, MediaItemS mediaItemS) :
    base("Saving metadata to files...", Res.IconImageMultiple, items) {
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
      "Save metadata to files",
      "Do you really want to save metadata to {0} file{1}?".Plural(items.Length),
      MH.UI.Res.IconQuestion,
      true)) != 1) return;

    var current = mediaItemVM.Current;
    mediaItemVM.Current = null;
    await ShowAsync(new SaveMetadataDialog(items, mediaItemS));
    mediaItemVM.Current = current;
  }
}