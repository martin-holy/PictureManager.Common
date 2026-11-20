using MH.UI.Controls;
using MH.UI.Dialogs;
using MH.Utils.BaseClasses;
using MH.Utils.Localization;
using PictureManager.Common.Localization;
using System.Threading;
using System.Threading.Tasks;

namespace PictureManager.Common.Features.GeoName;

public sealed class GeoNameVM {
  internal static class _loc {
    private const string _prefix = "GeoName.";
    public const string NewFromGps = _prefix + "NewFromGps.cmd";
    public const string NewFromGpsTitle = _prefix + "NewFromGps.dlg.Title";
    public const string NewFromGpsMessage = _prefix + "NewFromGps.dlg.Message";
    public const string ErrNewFromGpsFormat = _prefix + "NewFromGps.dlg.err.Format";

    static _loc() {
      LocalizationManager.Register(typeof(_loc).Assembly, [
        (NewFromGps, "New GeoName from GPS"),
        (NewFromGpsTitle, "GeoName latitude and longitude"),
        (NewFromGpsMessage, "Enter in format: N36.75847,W3.84609"),
        (ErrNewFromGpsFormat, "Incorrect format")
      ]);
    }

    public static void Register() { }
  }

  static GeoNameVM() { _loc.Register(); }

  private readonly GeoNameR _r;

  public static AsyncRelayCommand NewGeoNameFromGpsCommand { get; private set; } = null!;

  internal GeoNameVM(GeoNameR r) {
    _r = r;
    NewGeoNameFromGpsCommand = new(_newGeoNameFromGps, Res.IconLocationCheckin, L.Get(_loc.NewFromGps));
  }

  private async Task _newGeoNameFromGps(CancellationToken token) {
    var inputDialog = new InputDialog(
      L.Get(_loc.NewFromGpsTitle),
      L.Get(_loc.NewFromGpsMessage),
      Res.IconLocationCheckin,
      string.Empty,
      answer => {
        var (a, b) = GeoNameR.ParseLatLng(answer);
        return a == 0 && b == 0
          ? L.Get(_loc.ErrNewFromGpsFormat)
          : string.Empty;
      });

    if (await Dialog.ShowAsync(inputDialog) != 1) return;

    var (lat, lng) = GeoNameR.ParseLatLng(inputDialog.Answer);
    await _r.CreateGeoNameHierarchy(lat, lng);
  }
}