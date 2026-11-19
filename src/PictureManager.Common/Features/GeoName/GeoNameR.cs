using MH.Utils;
using MH.Utils.DB.Repositories;
using MH.Utils.Localization;
using MH.Utils.Tree;
using PictureManager.Common.Localization;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;

namespace PictureManager.Common.Features.GeoName;

public sealed class GeoNameR : TreeRepository<GeoNameM> {
  private static class _loc {
    private const string _prefix = "GeoName.";
    public const string WarnDisabledTitle = _prefix + "warn.LoadFromWebDisabled.Title";
    public const string WarnDisabledMessage = _prefix + "warn.LoadFromWebDisabled.Message";
    public const string ErrUserNameTitle = _prefix + "err.UserName.Title";
    public const string ErrUserNameMessage = _prefix + "err.UserName.Message";
    public const string ErrService = _prefix + "err.Service";

    static _loc() {
      LocalizationManager.Register(typeof(_loc).Assembly, [
        (WarnDisabledTitle, "Load GeoNames from web is disabled."),
        (WarnDisabledMessage, "Enable it in the settings."),
        (ErrUserNameTitle, "GeoNames user name was not set."),
        (ErrUserNameMessage, "Please register at geonames.org and set your user name in the settings."),
        (ErrService, "Error occurred while retrieving GeoName information.")
      ]);
    }

    public static void Register() { }
  }

  static GeoNameR() { _loc.Register(); }
  public static void RegisterLocalization() { }

  private bool _webLoadDisabled;

  public GeoNameTreeCategory Tree { get; }
  public GeoNameDS DataSource { get; }
  public bool ApiLimitExceeded { get; set; }

  public GeoNameR(CoreR coreR) {
    Tree = new(this);
    DataSource = new(coreR, this);
  }

  public GeoNameM ItemCreate(int id, string name, string toponymName, string fCode, ITreeItem parent) =>
    TreeItemCreate(new(id, name, toponymName, fCode, parent));

  public Task<GeoNameM?> CreateGeoNameHierarchy(double lat, double lng) =>
    _createGeoNameHierarchy($"http://api.geonames.org/extendedFindNearby?lat={lat}&lng={lng}".Replace(",", "."));

  public Task<GeoNameM?> CreateGeoNameHierarchy(int id) =>
    _createGeoNameHierarchy($"http://api.geonames.org/hierarchy?geonameId={id}");

  private async Task<GeoNameM?> _createGeoNameHierarchy(string url) {
    if (ApiLimitExceeded) return null;

    if (!Core.Settings.GeoName.LoadFromWeb) {
      if (!_webLoadDisabled) {
        _webLoadDisabled = true;
        Log.Warning(L.Get(_loc.WarnDisabledTitle), L.Get(_loc.WarnDisabledMessage));
      }

      return null;
    }

    if (string.IsNullOrEmpty(Core.Settings.GeoName.UserName)) {
      ApiLimitExceeded = true;
      Log.Error(L.Get(_loc.ErrUserNameTitle), L.Get(_loc.ErrUserNameMessage));
      return null;
    }

    try {
      var root = await Task.Run(() => {
        var xml = new XmlDocument();
        xml.Load($"{url}&username={Core.Settings.GeoName.UserName}");
        return xml.SelectSingleNode("/geonames");
      });

      if (root == null) return null;

      var geoNames = root.SelectNodes("geoname");
      if (geoNames == null) {
        ApiLimitExceeded = true;
        var errorMessage = root.SelectSingleNode("/status")?.Attributes?["message"]?.Value ?? string.Empty;
        Log.Error(L.Get(_loc.ErrService), errorMessage);
        return null;
      }

      GeoNameM? gn = null;
      foreach (XmlNode g in geoNames) {
        var geoNameId = int.Parse(g.SelectSingleNode("geonameId")?.InnerText ?? "0");
        gn = All.SingleOrDefault(x => x.GetHashCode() == geoNameId) ??
             ItemCreate(
               geoNameId,
               g.SelectSingleNode("name")?.InnerText ?? string.Empty,
               g.SelectSingleNode("toponymName")?.InnerText ?? string.Empty,
               g.SelectSingleNode("fcode")?.InnerText ?? string.Empty,
               gn as ITreeItem ?? Tree);
      }

      return gn;
    }
    catch (Exception ex) {
      Log.Error(ex);
      return null;
    }
  }

  public static (double, double) ParseLatLng(string? latLng) {
    if (string.IsNullOrEmpty(latLng)) return (0, 0);
    try {
      var lat = double.Parse(latLng.Split(',')[0].Replace("N", "").Replace("S", "-"), CultureInfo.InvariantCulture);
      var lng = double.Parse(latLng.Split(',')[1].Replace("E", "").Replace("W", "-"), CultureInfo.InvariantCulture);

      return (lat, lng);
    }
    catch {
      return (0, 0);
    }
  }
}