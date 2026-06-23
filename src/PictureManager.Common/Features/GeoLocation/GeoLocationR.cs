using MH.Utils.DB.Repositories;
using PictureManager.Common.Features.GeoName;
using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace PictureManager.Common.Features.GeoLocation;

public sealed class GeoLocationR : Repository<GeoLocationM> {
  private readonly CoreR _coreR;

  public GeoLocationDS DataSource { get; }

  public GeoLocationR(CoreR coreR) {
    _coreR = coreR;
    DataSource = new(coreR, this);
  }

  public GeoLocationM ItemCreate(double? lat, double? lng, GeoNameM? g) =>
    ItemCreate(new(GetNextId()) {
      Lat = lat,
      Lng = lng,
      GeoName = g
    });

  public async Task<GeoLocationM?> GetOrCreate(double? lat, double? lng, int? gnId, GeoNameM? gn, bool online = true) {
    if (lat == null && lng == null && gnId == null && gn == null) return null;
    lat = lat == null ? null : Math.Round((double)lat, 5);
    lng = lng == null ? null : Math.Round((double)lng, 5);

    if (gnId != null) {
      gn = _coreR.GeoName.All.SingleOrDefault(x => x.GetHashCode() == gnId);
      if (gn == null && online)
        gn = await _coreR.GeoName.CreateGeoNameHierarchy((int)gnId);
    }

    GeoLocationM? gl;
    if (lat != null && lng != null) {
      gl = All.FirstOrDefault(x =>
        x.Lat != null &&
        x.Lng != null &&
        Math.Abs((double)x.Lat - (double)lat) < 0.00001 &&
        Math.Abs((double)x.Lng - (double)lng) < 0.00001);

      if (gl?.GeoName != null) gn = gl.GeoName;

      if (gn == null && online)
        gn = await _coreR.GeoName.CreateGeoNameHierarchy((double)lat, (double)lng);

      if (gl != null && gl.GeoName == null && gn != null) {
        gl.GeoName = gn;
        _raiseItemUpdated(gl);
        IsModified = true;
      }
    }
    else {
      if (gn == null) return null;
      gl = All.SingleOrDefault(x => ReferenceEquals(x.GeoName, gn) && x.Lat == null && x.Lng == null);
    }

    return gl ?? ItemCreate(lat, lng, gn);
  }

  public void RemoveGeoName(GeoNameM geoName) {
    var all = All.Where(x => ReferenceEquals(x.GeoName, geoName)).ToArray();
    var toDelete = all.Where(x => x.Lat == null && x.Lng == null).ToArray();

    foreach (var gl in all.Except(toDelete)) {
      gl.GeoName = null;
      _raiseItemUpdated(gl);
    }

    ItemsDelete(toDelete);
    IsModified = true;
  }
}