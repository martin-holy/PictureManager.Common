using MH.Utils.DB.Repositories;
using PictureManager.Common.Features.Folder;

namespace PictureManager.Common.Features.MediaItem.Video;

public sealed class VideoR : Repository<VideoM> {
  public static VideoM Dummy = new(0, FolderR.Dummy, string.Empty);
  private readonly CoreR _coreR;

  public VideoDS DataSource { get; }

  public VideoR(CoreR coreR) {
    _coreR = coreR;
    DataSource = new(coreR, this);
  }

  public override int GetNextId() =>
    _coreR.MediaItem.GetNextId();

  public VideoM ItemCreate(FolderM folder, string fileName) =>
    ItemCreate(new(GetNextId(), folder, fileName));

  protected override void _onItemDeleted(object sender, VideoM item) {
    _coreR.VideoClip.ItemsDelete(item.VideoClips?.ToArray());
    _coreR.VideoImage.ItemsDelete(item.VideoImages?.ToArray());
    _coreR.MediaItem.OnItemDeletedCommon(item);
  }
}