using MH.UI.TreeLogic;
using MH.Utils.DB;
using MH.Utils.DB.DataSources;
using MH.Utils.Extensions;
using MH.Utils.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PictureManager.Common.Features.CategoryGroup;

/// <summary>
/// DB fields: ID|Name|Category|GroupItems
/// </summary>
public sealed class CategoryGroupDS(CoreR coreR, CategoryGroupR repository)
  : CsvTreeDataSource<CategoryGroupM, CategoryGroupR, string>(coreR.DB, "CategoryGroups", 4, repository) {

  public override bool Save() =>
    _saveToSingleFile(Repository.Categories.SelectMany(x => x.Items.OfType<CategoryGroupM>()));

  protected override (CategoryGroupM item, string linkInfo) _fromCsv(ReadOnlySpan<char> csv) {
    int start = 0;
    int field = 0;

    int id = 0;
    string name = string.Empty;
    Category category = 0;
    string groupItemsIds = string.Empty;


    for (int i = 0; i <= csv.Length; i++) {
      if (i == csv.Length || csv[i] == '|') {
        var slice = csv[start..i];

        switch (field) {
          case 0: id = CsvParser.ParseInt(slice); break;
          case 1: name = slice.ToString(); break;
          case 2: category = (Category)CsvParser.ParseInt(slice); break;
          case 3: groupItemsIds = slice.ToString(); break;
        }

        field++;
        start = i + 1;
      }
    }

    _validateFieldsCount(field, csv);

    var item = CategoryGroupR.GetNew(id, name, category);

    return new(item, groupItemsIds);
  }

  protected override string _toCsv(CategoryGroupM cg) =>
    string.Join("|",
      cg.GetHashCode().ToString(),
      cg.Name,
      (int)cg.Category,
      cg.Items.ToHashCodes().ToCsv());
  
   public void LinkGroups<TI>(TreeCategory cat, Dictionary<int, TI> allDict) where TI : class, ITreeItem {
    cat.Items.Clear();

    foreach (var (cg, groupItemsIds) in _allLinkInfo) {
      if (!cat.Id.Equals((int)cg.Category)) continue;

      cg.Parent = cat;
      cg.Parent.Items.Add(cg);

      int id = 0;

      for (int i = 0; i <= groupItemsIds.Length; i++) {
        if (i == groupItemsIds.Length || groupItemsIds[i] == ',') {
          if (allDict.TryGetValue(id, out var item)) {
            item.Parent = cg;
            cg.Items.Add(item);
          }

          id = 0;
          continue;
        }

        id = id * 10 + (groupItemsIds[i] - '0');
      }

      cg.Items.CollectionChanged += Repository.OnGroupItemsCollectionChanged;
    }
  }
}
