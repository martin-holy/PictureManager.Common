using MH.UI.Dialogs;
using MH.Utils.Imaging;
using MH.Utils.Imaging.Jpeg;
using PictureManager.Common.Features.MediaItem;
using PictureManager.Common.Features.MediaItem.Image;
using System.Threading;
using System.Threading.Tasks;

namespace PictureManager.Common.Features.GeoLocation;

public sealed class ReadGeoLocationFromFilesDialog : ProgressDialog<ImageM> {
  public ReadGeoLocationFromFilesDialog(ImageM[] items) :
    base("Reading GeoLocations from files ...", Res.IconLocationCheckin, items) {
    RunSync = true;
    _autoRun();
  }

  private Task _oldDo(ImageM item, CancellationToken token) {
    _reportProgress(item.FileName);
    var mim = new MediaItemMetadata(item);
    MediaItemS.ReadMetadata(mim, true);
    return mim.Success ? mim.FindGeoLocation(false) : Task.CompletedTask;
  }

  protected override Task _do(ImageM item, CancellationToken token) {
    if (!FF.XPlatformMetadata)
      return _oldDo(item, token);

    _reportProgress(item.FileName);

    var mim = new MediaItemMetadata(item);
    var metadata = new ImageMetadata(item.FilePath, JpegMetadataLoad.All);
    
    mim.GeoNameId = ImageS.GetGeoNameId(metadata);

    if (metadata.GpsCoordinate is { } gps) {
      mim.Lat = gps.Latitude;
      mim.Lng = gps.Longitude;

      return mim.FindGeoLocation(false);
    }

    return Task.CompletedTask;
  }
}