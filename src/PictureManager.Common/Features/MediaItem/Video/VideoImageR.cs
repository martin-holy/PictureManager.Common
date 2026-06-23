using MH.Utils.DB.Repositories;

namespace PictureManager.Common.Features.MediaItem.Video;

public sealed class VideoImageR : Repository<VideoImageM> {
  private readonly CoreR _coreR;

  public VideoImageDS DataSource { get; }

  public VideoImageR(CoreR coreR) {
    _coreR = coreR;
    DataSource = new(coreR, this);
  }

  public override int GetNextId() =>
    _coreR.MediaItem.GetNextId();

  public VideoImageM CustomItemCreate(VideoM video, int timeStart) =>
    ItemCreate(new(GetNextId(), video, timeStart));

  protected override void _onItemCreated(object sender, VideoImageM item) =>
    item.Video.Toggle(item);

  protected override void _onItemDeleted(object sender, VideoImageM item) {
    _coreR.MediaItem.OnItemDeletedCommon(item);
    item.Video.Toggle(item);
  }
}