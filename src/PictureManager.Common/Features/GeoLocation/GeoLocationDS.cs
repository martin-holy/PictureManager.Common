using MH.Utils.DB;
using MH.Utils.DB.DataSources;
using MH.Utils.Extensions;
using System;
using System.Globalization;

namespace PictureManager.Common.Features.GeoLocation;

/// <summary>
/// DB fields: ID|Lat|Lng|GeoName
/// </summary>
public sealed class GeoLocationDS(CoreR coreR, GeoLocationR repository)
  : CsvRepositoryDataSource<GeoLocationM, GeoLocationR, int>(coreR.DB, "GeoLocations", 4, repository) {

  protected override (GeoLocationM item, int linkInfo) _fromCsv(ReadOnlySpan<char> csv) {
    int start = 0;
    int field = 0;

    int id = 0;
    double? lat = null;
    double? lng = null;
    int geoNameId = 0;

    for (int i = 0; i <= csv.Length; i++) {
      if (i == csv.Length || csv[i] == '|') {
        var slice = csv[start..i];

        switch (field) {
          case 0: id = CsvParser.ParseInt(slice); break;
          case 1: lat = slice.ToDouble(CultureInfo.InvariantCulture); break;
          case 2: lng = slice.ToDouble(CultureInfo.InvariantCulture); break;
          case 3: geoNameId = CsvParser.ParseIntOrDefault(slice, 0); break;
        }

        field++;
        start = i + 1;
      }
    }

    _validateFieldsCount(field, csv);

    var item = new GeoLocationM(id) { Lat = lat, Lng = lng };

    return new(item, geoNameId);
  }

  protected override string _toCsv(GeoLocationM gl) =>
    string.Join("|",
      gl.GetHashCode().ToString(),
      gl.Lat.ToString(CultureInfo.InvariantCulture),
      gl.Lng.ToString(CultureInfo.InvariantCulture),
      gl.GeoName?.GetHashCode().ToString());

  public override void LinkReferences() {
    foreach (var (gl, geoNameId) in _allLinkInfo)
      gl.GeoName = coreR.GeoName.DataSource.GetById(geoNameId, true);
  }
}