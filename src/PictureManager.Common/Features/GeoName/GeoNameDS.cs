using MH.Utils.DB;
using MH.Utils.DB.DataSources;
using System;

namespace PictureManager.Common.Features.GeoName;

/// <summary>
/// DB fields: ID|Name|ToponymName|FCode|Parent
/// </summary>
public sealed class GeoNameDS(CoreR coreR, GeoNameR repository)
  : CsvTreeDataSource<GeoNameM, GeoNameR, int>(coreR.DB, "GeoNames", 5, repository) {
  
  protected override (GeoNameM item, int linkInfo) _fromCsv(ReadOnlySpan<char> csv) {
    int start = 0;
    int field = 0;

    int id = 0;
    string name = string.Empty;
    string toponymName = string.Empty;
    string fCode = string.Empty;
    int parentId = 0;

    for (int i = 0; i <= csv.Length; i++) {
      if (i == csv.Length || csv[i] == '|') {
        var slice = csv[start..i];

        switch (field) {
          case 0: id = CsvParser.ParseInt(slice); break;
          case 1: name = slice.ToString(); break;
          case 2: toponymName = slice.ToString(); break;
          case 3: fCode = slice.ToString(); break;
          case 4: parentId = CsvParser.ParseIntOrDefault(slice, 0); break;
        }

        field++;
        start = i + 1;
      }
    }

    _validateFieldsCount(field, csv);

    var item = new GeoNameM(id, name, toponymName, fCode, null);

    return new(item, parentId);
  }

  protected override string _toCsv(GeoNameM geoName) =>
    string.Join("|",
      geoName.GetHashCode().ToString(),
      geoName.Name,
      geoName.ToponymName,
      geoName.Fcode,
      (geoName.Parent as GeoNameM)?.GetHashCode().ToString());

  public override void LinkReferences() {
    Repository.Tree.Items.Clear();
    _linkTree(Repository.Tree, x => x);
  }
}