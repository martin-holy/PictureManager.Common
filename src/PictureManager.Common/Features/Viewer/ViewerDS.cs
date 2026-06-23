using MH.Utils.DB;
using MH.Utils.DB.DataSources;
using MH.Utils.Extensions;
using System;
using System.Linq;

namespace PictureManager.Common.Features.Viewer;

/// <summary>
/// DB fields: ID|Name|IncludedFolders|ExcludedFolders|ExcludedCategoryGroups|ExcludedKeywords|IsDefault
/// </summary>
public sealed class ViewerDS : CsvTreeDataSource<ViewerM, ViewerR, ViewerLinkInfo> {
  private readonly CoreR _coreR;

  public ViewerDS(CoreR coreR, ViewerR repo) : base(coreR.DB, "Viewers", 7, repo) {
    _coreR = coreR;
  }

  protected override (ViewerM item, ViewerLinkInfo linkInfo) _fromCsv(ReadOnlySpan<char> csv) {
    int start = 0;
    int field = 0;

    int id = 0;
    string name = string.Empty;
    string inFolders = string.Empty;
    string exFolders = string.Empty;
    string exCategoryGroups = string.Empty;
    string exKeywords = string.Empty;
    bool isDefault = false;

    for (int i = 0; i <= csv.Length; i++) {
      if (i == csv.Length || csv[i] == '|') {
        var slice = csv[start..i];

        switch (field) {
          case 0: id = CsvParser.ParseInt(slice); break;
          case 1: name = slice.ToString(); break;
          case 2: inFolders = slice.ToString(); break;
          case 3: exFolders = slice.ToString(); break;
          case 4: exCategoryGroups = slice.ToString(); break;
          case 5: exKeywords = slice.ToString(); break;
          case 6: isDefault = slice.Length == 1 && slice[0] == '1'; break;
        }

        field++;
        start = i + 1;
      }
    }

    _validateFieldsCount(field, csv);

    var item = new ViewerM(id, name, Repository.Tree) { IsDefault = isDefault };
    var linkInfo = new ViewerLinkInfo(inFolders, exFolders, exCategoryGroups, exKeywords);

    return new(item, linkInfo);
  }

  protected override string _toCsv(ViewerM viewer) =>
    string.Join("|",
      viewer.GetHashCode().ToString(),
      viewer.Name,
      viewer.IncludedFolders.ToHashCodes().ToCsv(),
      viewer.ExcludedFolders.ToHashCodes().ToCsv(),
      viewer.ExcludedCategoryGroups.ToHashCodes().ToCsv(),
      viewer.ExcludedKeywords.ToHashCodes().ToCsv(),
      viewer.IsDefault
        ? "1"
        : string.Empty);

  public override void LinkReferences() {
    Repository.Tree.Items.Clear();

    foreach (var (viewer, li) in _allLinkInfo.OrderBy(x => x.Item1.Name)) {
      // reference to IncludedFolders
      CsvParser.ParseInts(li.InFolders, (_coreR.Folder.DataSource.AllDict, viewer.IncludedFolders), static (state, id) => {
        var f = state.AllDict.TryGetValue(id, out var incF)
            ? incF
            : new(id, "?", null);
        state.IncludedFolders.SetInOrder(f, x => x.FullPath);
      });

      // reference to ExcludedFolders
      CsvParser.ParseInts(li.ExFolders, (_coreR.Folder.DataSource.AllDict, viewer.ExcludedFolders), static (state, id) => {
        var f = state.AllDict.TryGetValue(id, out var incF)
            ? incF
            : new(id, "?", null);
        state.ExcludedFolders.SetInOrder(f, x => x.FullPath);
      });

      // ExcludedCategoryGroups
      CsvParser.ParseInts(li.ExCategoryGroups, (_coreR.CategoryGroup.DataSource.AllDict, viewer.ExcludedCategoryGroups), static (state, id) => {
        state.ExcludedCategoryGroups.Add(state.AllDict[id]);
      });

      // ExcKeywords
      CsvParser.ParseInts(li.ExKeywords, (_coreR.Keyword.DataSource.AllDict, viewer.ExcludedKeywords), static (state, id) => {
        state.ExcludedKeywords.Add(state.AllDict[id]);
      });

      // adding Viewer to Viewers
      Repository.Tree.Items.Add(viewer);
    }
  }
}

public readonly struct ViewerLinkInfo(string inFolders, string exFolders, string exCategoryGroups, string exKeywords) {
  public readonly string InFolders = inFolders;
  public readonly string ExFolders = exFolders;
  public readonly string ExCategoryGroups = exCategoryGroups;
  public readonly string ExKeywords = exKeywords;
}