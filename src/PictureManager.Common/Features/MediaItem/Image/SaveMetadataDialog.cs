using MH.UI.Dialogs;
using PictureManager.Common.Utils.Localization;
using System.Threading;
using System.Threading.Tasks;

namespace PictureManager.Common.Features.MediaItem.Image;

public sealed class SaveMetadataDialog : ParallelProgressDialog<RealMediaItemM> {
  private readonly MediaItemS _mediaItemS;

  public SaveMetadataDialog(RealMediaItemM[] items, MediaItemS mediaItemS) :
    base(L.Get("MediaItem.SaveMetadata.dlg.Title"), Res.IconImageMultiple, items) {
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
      L.Get("MediaItem.SaveMetadata.Open.dlg.Title"),
      L.FormatPlural("MediaItem.SaveMetadata.Open.dlg.Message", items.Length),
      MH.UI.Res.IconQuestion,
      true)) != 1) return;

    var current = mediaItemVM.Current;
    mediaItemVM.Current = null;
    await ShowAsync(new SaveMetadataDialog(items, mediaItemS));
    mediaItemVM.Current = current;
  }
}