using MH.Utils.DB;
using MH.Utils.DB.DataSources;
using PictureManager.Common.Features.Folder;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PictureManager.Common.Features.FavoriteFolder;

/// <summary>
/// DB fields: ID|Folder|Title
/// </summary>
public sealed class FavoriteFolderDS : CsvTreeDataSource<FavoriteFolderM, FavoriteFolderR, int> {
  private readonly CoreR _coreR;

  public FavoriteFolderDS(CoreR coreR, FavoriteFolderR repo) : base(coreR.DB, "FavoriteFolders", 3, repo) {
    _coreR = coreR;
    IsDriveRelated = true;
  }

  protected override Dictionary<string, IEnumerable<FavoriteFolderM>> _getAsDriveRelated() =>
    CoreR.GetAsDriveRelated(Repository.Tree.Items.Cast<FavoriteFolderM>(), x => x.Folder);

  protected override (FavoriteFolderM item, int linkInfo) _fromCsv(ReadOnlySpan<char> csv) {
    int start = 0;
    int field = 0;

    int id = 0;
    int folderId = 0;
    string title = string.Empty;

    for (int i = 0; i <= csv.Length; i++) {
      if (i == csv.Length || csv[i] == '|') {
        var slice = csv[start..i];

        switch (field) {
          case 0: id = CsvParser.ParseInt(slice); break;
          case 1: folderId = CsvParser.ParseInt(slice); break;
          case 2: title = slice.ToString(); break;
        }

        field++;
        start = i + 1;
      }
    }

    _validateFieldsCount(field, csv);

    var item = new FavoriteFolderM(id, title, FolderR.Dummy);

    return new(item, folderId);
  }

  protected override string _toCsv(FavoriteFolderM ff) =>
    string.Join("|",
      ff.GetHashCode().ToString(),
      ff.Folder.GetHashCode().ToString(),
      ff.Name);

  public override void LinkReferences() {
    Repository.Tree.Items.Clear();

    foreach (var (ff, folderId) in _allLinkInfo) {
      ff.Folder = _coreR.Folder.DataSource.GetById(folderId)!;
      ff.Parent = Repository.Tree;
      Repository.Tree.Items.Add(ff);
    }
  }
}