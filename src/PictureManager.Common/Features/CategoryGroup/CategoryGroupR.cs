using MH.UI.Interfaces;
using MH.Utils;
using MH.Utils.DB.Repositories;
using MH.Utils.Interfaces;
using PictureManager.Common.Features.Keyword;
using PictureManager.Common.Features.Person;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace PictureManager.Common.Features.CategoryGroup;

public class CategoryGroupR : TreeRepository<CategoryGroupM> {
  private readonly CoreR _coreR;

  public CategoryGroupDS DataSource { get; }
  public List<ITreeItem> Categories { get; } = [];

  public CategoryGroupR(CoreR coreR) {
    _coreR = coreR;
    DataSource = new(coreR, this);
  }

  public override CategoryGroupM ItemCreate(ITreeItem parent, string name) {
    var cat = (Category)Tree.GetParentOf<ITreeCategory>(parent)!.Id;
    var group = GetNew(GetNextId(), name, cat);
    group.Parent = parent;
    group.Items.CollectionChanged += OnGroupItemsCollectionChanged;

    return TreeItemCreate(group);
  }

  public static CategoryGroupM GetNew(int id, string name, Category cat) =>
    cat switch {
      Category.Keywords => new KeywordCategoryGroupM(id, name, cat, Res.CategoryToIcon(cat)),
      Category.People => new PersonCategoryGroupM(id, name, cat, Res.CategoryToIcon(cat)),
      _ => throw new NotSupportedException()
    };

  public override string? ValidateNewItemName(ITreeItem parent, string? name) {
    if (string.IsNullOrEmpty(name)) return "The name is empty!";
    return parent.Items
      .OfType<CategoryGroupM>()
      .Any(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
        ? $"{name} group already exists!"
        : null;
  }

  protected override void _onItemDeleted(object sender, CategoryGroupM item) {
    item.Parent?.Items.Remove(item);
  }

  public void OnGroupItemsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) {
    if (_coreR.DB.IsReady) IsModified = true;
  }

  public void AddCategory(ITreeItem cat) {
    Categories.Add(cat);
  }
}