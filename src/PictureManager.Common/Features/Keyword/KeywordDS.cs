using MH.Utils.DB;
using MH.Utils.DB.DataSources;
using PictureManager.Common.Features.CategoryGroup;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PictureManager.Common.Features.Keyword;

/// <summary>
/// DB fields: ID|Name|Parent
/// </summary>
public sealed class KeywordDS(CoreR coreR, KeywordR repository)
  : CsvTreeDataSource<KeywordM, KeywordR, int>(coreR.DB, "Keywords", 3, repository) {

  private readonly CoreR _coreR = coreR;
  private const string _notFoundRecordNamePrefix = "Not found ";

  public override bool Save() =>
    _saveToSingleFile(KeywordR.GetAll<KeywordM>(Repository.Tree));

  protected override (KeywordM item, int linkInfo) _fromCsv(ReadOnlySpan<char> csv) {
    int start = 0;
    int field = 0;

    int id = 0;
    string name = string.Empty;
    int parentId = 0;

    for (int i = 0; i <= csv.Length; i++) {
      if (i == csv.Length || csv[i] == '|') {
        var slice = csv[start..i];

        switch (field) {
          case 0: id = CsvParser.ParseInt(slice); break;
          case 1: name = slice.ToString(); break;
          case 2: parentId = CsvParser.ParseIntOrDefault(slice, 0); break;
        }

        field++;
        start = i + 1;
      }
    }

    _validateFieldsCount(field, csv);

    var item = new KeywordM(id, name, null);

    return new(item, parentId);
  }

  protected override string _toCsv(KeywordM keyword) =>
    string.Join("|",
      keyword.GetHashCode().ToString(),
      keyword.Name,
      (keyword.Parent as KeywordM)?.GetHashCode().ToString());

  public override void LinkReferences() {
    _coreR.CategoryGroup.DataSource.LinkGroups(Repository.Tree, AllDict);
    _linkTree(Repository.Tree, x => x);

    // TODO this might belong to the KeywordR
    // group for keywords automatically added from MediaItems metadata
    Repository.Tree.AutoAddedGroup = Repository.Tree.Items
                            .OfType<CategoryGroupM>()
                            .SingleOrDefault(x => x.Name.Equals("Auto Added"))
                          ?? _coreR.CategoryGroup.ItemCreate(Repository.Tree, "Auto Added");
  }

  public List<KeywordM>? Link(string csv, ICsvRepositoryDataSource seeker) =>
    LinkList(csv, _getNotFoundRecord, seeker);

  private KeywordM _getNotFoundRecord(int notFoundId) {
    var id = Repository.GetNextId();
    var item = new KeywordM(id, $"{_notFoundRecordNamePrefix}{id} ({notFoundId})", Repository.Tree);
    item.Parent!.Items.Add(item);
    Repository.IsModified = true;
    return item;
  }
}