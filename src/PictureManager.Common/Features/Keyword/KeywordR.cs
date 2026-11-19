using MH.Utils.DB.Repositories;
using MH.Utils.Localization;
using MH.Utils.Tree;
using PictureManager.Common.Features.CategoryGroup;
using PictureManager.Common.Localization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PictureManager.Common.Features.Keyword;

public sealed class KeywordR : TreeRepository<KeywordM> {
  private static class _loc {
    private const string _prefix = "Keyword.";
    public const string ErrEmptyName = _prefix + "err.EmptyName";
    public const string ErrItemExists = _prefix + "err.ItemExists";

    static _loc() {
      LocalizationManager.Register(typeof(_loc).Assembly, [
        (ErrEmptyName, "The name is empty!"),
        (ErrItemExists, "{0} item already exists!")
      ]);
    }

    public static void Register() { }
  }

  static KeywordR() { _loc.Register(); }
  public static void RegisterLocalization() { }

  public KeywordTreeCategory Tree { get; }
  public KeywordDS DataSource { get; }

  public KeywordR(CoreR coreR, CategoryGroupR cgR) {
    Tree = new(this, cgR);
    DataSource = new(coreR, this);
  }

  public override KeywordM ItemCreate(ITreeItem parent, string name) =>
    TreeItemCreate(new(GetNextId(), name, parent));

  public override string? ValidateNewItemName(ITreeItem parent, string? name) {
    if (string.IsNullOrEmpty(name)) return L.Get(_loc.ErrEmptyName);
    return parent.Items.OfType<KeywordM>().Any(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
      ? L.Format(_loc.ErrItemExists, name)
      : null;
  }

  public KeywordM? GetByFullPath(string? fullPath, IEnumerable<ITreeItem>? src = null, ITreeItem? rootForNew = null) {
    if (string.IsNullOrEmpty(fullPath)) return null;
    src ??= All.Where(x => x.Parent is not KeywordM);
    rootForNew ??= Tree.AutoAddedGroup;
    var last = Array.Empty<ITreeItem>();

    foreach (var path in fullPath.Split('/')) {
      var found = src.Where(x => x.Name.Equals(path, StringComparison.OrdinalIgnoreCase)).ToArray();

      last = found.Length switch {
        0 => [ItemCreate(rootForNew, path)],
        1 => [found[0]],
        _ => found
      };

      src = last.SelectMany(x => x.Items);
      rootForNew = GetFirst(last);
    }

    return GetFirst(last) as KeywordM;

    ITreeItem GetFirst(ITreeItem[] items) =>
      items.FirstOrDefault(x => !x.HasThisParent(Tree.AutoAddedGroup)) ?? items.First();
  }

  public void MoveGroupItemsToRoot(CategoryGroupM group) {
    if (group.Category != Category.Keywords) return;
    foreach (var item in group.Items.ToArray())
      ItemMove(item, Tree, false);
  }
}