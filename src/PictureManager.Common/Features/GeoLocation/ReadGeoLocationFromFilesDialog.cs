using MH.UI.Dialogs;
using MH.Utils.Imaging;
using MH.Utils.Imaging.Jpeg;
using MH.Utils.Localization;
using PictureManager.Common.Features.MediaItem;
using PictureManager.Common.Features.MediaItem.Image;
using PictureManager.Common.Localization;
using System.Threading;
using System.Threading.Tasks;

namespace PictureManager.Common.Features.GeoLocation;

public sealed class ReadGeoLocationFromFilesDialog : ProgressDialog<ImageM> {
  internal static class _loc {
    private const string _prefix = "GeoLocation.";
    public const string Title = _prefix + "ReadGeoLocationFromFiles.dlg.Title";

    static _loc() {
      LocalizationManager.Register(typeof(_loc).Assembly, [
        (Title, "Reading GeoLocations from files ...")
      ]);
    }

    public static void Register() { }
  }

  static ReadGeoLocationFromFilesDialog() { _loc.Register(); }

  public ReadGeoLocationFromFilesDialog(ImageM[] items) :
    base(L.Get(_loc.Title), Res.IconLocationCheckin, items) {
    RunSync = true;
    _autoStart = true;
  }

  protected override Task _do(ImageM item, CancellationToken token) {
    _reportProgress(item.FileName);

    var mim = new MediaItemMetadata(item);
    var metadata = new ImageMetadata(item.FilePath, JpegMetadataLoad.All);

    // TODO add video support
    mim.GeoNameId = MediaItemS.GetGeoNameId(metadata.Jpeg.Xmp);

    if (metadata.GpsCoordinate is { } gps) {
      mim.Lat = gps.Latitude;
      mim.Lng = gps.Longitude;

      return mim.FindGeoLocation(false);
    }

    return Task.CompletedTask;
  }
}