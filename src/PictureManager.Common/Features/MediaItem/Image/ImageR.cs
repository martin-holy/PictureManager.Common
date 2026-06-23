using MH.Utils.DB.Repositories;
using PictureManager.Common.Features.Folder;

namespace PictureManager.Common.Features.MediaItem.Image;

public sealed class ImageR : Repository<ImageM> {
  private readonly CoreR _coreR;

  public ImageDS DataSource { get; }

  public ImageR(CoreR coreR) {
    _coreR = coreR;
    DataSource = new(_coreR, this);
  }

  public override int GetNextId() =>
    _coreR.MediaItem.GetNextId();

  public ImageM ItemCreate(FolderM folder, string fileName) =>
    ItemCreate(new(GetNextId(), folder, fileName));

  protected override void _onItemDeleted(object sender, ImageM item) {
    _coreR.MediaItem.OnItemDeletedCommon(item);
  }
}