using MH.UI.Tree;
using MH.Utils.DB.Repositories;
using MH.Utils.Localization;
using MH.Utils.Tree;
using PictureManager.Common.Features.Keyword;
using PictureManager.Common.Features.Person;
using PictureManager.Common.Localization;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace PictureManager.Common.Features.CategoryGroup;

public class CategoryGroupR : TreeRepository<CategoryGroupM> {
  internal static class _loc {
    private const string _prefix = "CategoryGroup.";
    public const string EmptyName = _prefix + "err.EmptyName";
    public const string GroupExists = _prefix + "err.GroupExists";

    static _loc() {
      LocalizationManager.Register(typeof(_loc).Assembly, [
        (EmptyName, "The name is empty!"),
        (GroupExists, "{0} group already exists!")
      ]);
    }

    public static void Register() { }
  }

  static CategoryGroupR() { _loc.Register(); }

  private readonly CoreR _coreR;

  public CategoryGroupDS DataSource { get; }
  public List<ITreeItem> Categories { get; } = [];

  public CategoryGroupR(CoreR coreR) {
    _coreR = coreR;
    DataSource = new(coreR, this);
  }

  public override CategoryGroupM ItemCreate(ITreeItem parent, string name) {
    var cat = (Category)parent.GetParentOf<ITreeCategory>()!.Id;
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
    if (string.IsNullOrEmpty(name)) return L.Get(_loc.EmptyName);
    return parent.Items
      .OfType<CategoryGroupM>()
      .Any(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
        ? L.Format(_loc.GroupExists, name)
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