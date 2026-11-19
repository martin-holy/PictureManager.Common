using MH.UI.Dialogs;
using MH.Utils.Localization;
using PictureManager.Common.Features.MediaItem.Image;
using PictureManager.Common.Localization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PictureManager.Common.Features.GeoLocation;

public sealed class GetGeoNamesFromWebDialog : ProgressDialog<ImageM> {
  private static class _loc {
    private const string _prefix = "GeoLocation.";
    public const string Title = _prefix + "GetGeoNamesFromWeb.dlg.Title";

    static _loc() {
      LocalizationManager.Register(typeof(_loc).Assembly, [
        (Title, "Getting GeoNames from web ...")
      ]);
    }

    public static void Register() { }
  }

  static GetGeoNamesFromWebDialog() { _loc.Register(); }
  public static void RegisterLocalization() { }

  private readonly CoreR _coreR;

  public GetGeoNamesFromWebDialog(ImageM[] items, CoreR coreR) :
    base(L.Get(_loc.Title), Res.IconLocationCheckin, items) {
    _coreR = coreR;
    RunSync = true;
    _autoStart = true;
  }

  protected override async Task _do(ImageM item, CancellationToken token) {
    _reportProgress(item.FileName);
    if (_coreR.GeoName.ApiLimitExceeded) return;
    var gl = await _coreR.GeoLocation.GetOrCreate(item.GeoLocation?.Lat, item.GeoLocation?.Lng, null, null);
    _coreR.MediaItemGeoLocation.ItemUpdate(new(item, gl));
  }

  public static async Task<bool> Open(ImageM[] items, CoreR coreR) {
    items = items.Where(x => x.GeoLocation is { GeoName: null, Lat: not null, Lng: not null }).ToArray();
    if (items.Length == 0) return false;
    await ShowAsync(new GetGeoNamesFromWebDialog(items, coreR));
    return true;
  }
}