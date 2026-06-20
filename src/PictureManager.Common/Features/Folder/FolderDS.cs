using MH.Utils.DB;
using MH.Utils.DB.DataSources;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PictureManager.Common.Features.Folder;

/// <summary>
/// DB fields: ID|Name|Parent
/// </summary>
public sealed class FolderDS : CsvTreeDataSource<FolderM, FolderR2, int> {
  public FolderDS(CoreR coreR, FolderR2 repo) : base(coreR.DB, "Folders", 3, repo) {
    IsDriveRelated = true;
  }

  protected override Dictionary<string, IEnumerable<FolderM>> _getAsDriveRelated() =>
    Repository.Tree.Category.Items.ToDictionary(x => x.Name, FolderR2.GetAll<FolderM>);

  protected override (FolderM item, int linkInfo) _fromCsv(ReadOnlySpan<char> csv) {
    int start = 0;
    int field = 0;

    int id = 0;
    string name = string.Empty;
    int parentId = 0;

    for (int i = 0; i <= csv.Length; i++) {
      if (i == csv.Length || csv[i] == '|') {
        var slice = csv[start..i];

        switch (field) {
          case 0: id = IdParser.Parse(slice); break;
          case 1: name = slice.ToString(); break;
          case 2: parentId = IdParser.Parse(slice, 0); break;
        }

        field++;
        start = i + 1;
      }
    }

    _validateFieldsCount(field, csv);

    var item = parentId == 0
      ? new DriveM(id, name, null, _currentVolumeSerialNumber!)
      : new FolderM(id, name, null);

    return new(item, parentId);
  }

  protected override string _toCsv(FolderM folder) =>
    string.Join("|",
      folder.GetHashCode().ToString(),
      folder.Name,
      (folder.Parent as FolderM)?.GetHashCode().ToString() ?? string.Empty);

  public override void LinkReferences() {
    Repository.Tree.Category.Items.Clear();
    _linkTree(Repository.Tree.Category, x => x);
  }
}