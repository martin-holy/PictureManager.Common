using MH.Utils.DB.Repositories;

namespace PictureManager.Common.Features.MediaItem.Video;

public sealed class VideoClipR : Repository<VideoClipM> {
  private readonly CoreR _coreR;

  public VideoClipDS DataSource { get; }

  public VideoClipR(CoreR coreR) {
    _coreR = coreR;
    DataSource = new(coreR, this);
  }

  public override int GetNextId() =>
    _coreR.MediaItem.GetNextId();

  public VideoClipM CustomItemCreate(VideoM video, int timeStart) =>
    ItemCreate(new(GetNextId(), video, timeStart));

  protected override void _onItemCreated(object sender, VideoClipM item) {
    item.Video.Toggle(item);
  }

  protected override void _onItemDeleted(object sender, VideoClipM item) {
    _coreR.MediaItem.OnItemDeletedCommon(item);
    item.Video.Toggle(item);
  }
}