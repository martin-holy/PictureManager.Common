using MH.UI.Tree;
using MH.Utils.Localization;
using MH.Utils.Tree;
using PictureManager.Common.Features.CategoryGroup;
using PictureManager.Common.Localization;
using System.Collections.Generic;
using System.Linq;

namespace PictureManager.Common.Features.Person;

public sealed class PersonTreeCategory : TreeCategory<PersonM, CategoryGroupM> {
  internal static class _loc {
    private const string _prefix = "Person.";
    public const string Title = _prefix + "TreeCategory.Title";
    public const string UnknownGroupName = _prefix + "UnknownGroupName";

    static _loc() {
      LocalizationManager.Register(typeof(_loc).Assembly, [
        (Title, "People"),
        (UnknownGroupName, "Unknown")
      ]);
    }

    public static void Register() { }
  }

  static PersonTreeCategory() { _loc.Register(); }

  private CategoryGroupM? _unknownGroup;

  public CategoryGroupM UnknownGroup {
    get =>
      _unknownGroup ??=
        Items
          .OfType<CategoryGroupM>()
          .SingleOrDefault(x => x.Name.Equals(L.Get(_loc.UnknownGroupName)))
        ?? Core.R.CategoryGroup.ItemCreate(this, L.Get(_loc.UnknownGroupName));
    set => _unknownGroup = value;
  }

  public PersonTreeCategory(PersonR r, CategoryGroupR groupR)
    : base(new(), Res.IconPeopleMultiple, L.Get(_loc.Title), (int)Category.People, r, groupR) {
    CanMoveItem = true;
    ScrollToAfterCreate = true;
  }

  protected override void _onItemSelected(object o) {
    switch (o) {
      case PersonM p:
        _ = Core.VM.ToggleDialog.Toggle(p);
        break;
      case PersonTreeCategory:
        Core.VM.OpenPeopleView();
        break;
    }
  }

  public override IEnumerable<ITreeItem> GroupGetItemsToMove(ITreeGroup group) =>
    Core.S.Person.Selected.Items.Where(x => !ReferenceEquals(x.Parent, group));

  public override bool GroupAnyItemsToMove(ITreeGroup group) =>
    Core.S.Person.Selected.Items.Where(x => !ReferenceEquals(x.Parent, group)).Any();
}