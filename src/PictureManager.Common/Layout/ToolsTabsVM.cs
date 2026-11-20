using MH.UI.Controls;
using MH.Utils.BaseClasses;
using MH.Utils.Interfaces;
using MH.Utils.Localization;
using MH.Utils.Tree;
using PictureManager.Common.Features.Person;
using PictureManager.Common.Localization;
using PictureManager.Common.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PictureManager.Common.Layout;

public sealed class ToolsTabsVM : TabControl {
  internal static class _loc {
    private const string _prefix = "ToolsTabs.";
    public const string NoTabsText = _prefix + "NoTabsText";
    public const string PersonDetailOpen = "Person.Detail.Open.cmd";
    public const string PersonDetailTabName = "Person.Detail.TabName";
    public const string PeopleOpen = _prefix + "People.Open.cmd";
    public const string PeopleTabName = _prefix + "People.TabName";

    static _loc() {
      LocalizationManager.Register(typeof(_loc).Assembly, [
        (NoTabsText, "Tools tabs"),
        (PersonDetailOpen, "Detail"),
        (PersonDetailTabName, "Person"),
        (PeopleOpen, "People"),
        (PeopleTabName, "People")
      ]);
    }

    public static void Register() { }
  }

  static ToolsTabsVM() { _loc.Register(); }

  public PeopleToolsTabVM? PeopleTab { get; private set; }
  public PersonDetailVM? PersonDetailTab { get; private set; }

  public static RelayCommand<PersonM> OpenPersonTabCommand { get; set; } = null!;
  public static AsyncRelayCommand OpenPeopleTabCommand { get; set; } = null!;

  public ToolsTabsVM() : base(new(Dock.Top) { EndSlot = new SlidePanelPinButton(), JustifyTabSize = true }) {
    NoTabsText = L.Get(_loc.NoTabsText);
    OpenPersonTabCommand = new(_openPersonTab, Res.IconInformation, L.Get(_loc.PersonDetailOpen));
    OpenPeopleTabCommand = new(_ => OpenPeopleTab(null), Res.IconPeopleMultiple, L.Get(_loc.PeopleOpen));
  }

  public async Task OpenPeopleTab(PersonM[]? people) {
    PeopleTab ??= new();
    await PeopleTab.Reload(people);
    Activate(Res.IconPeopleMultiple, L.Get(_loc.PeopleTabName), PeopleTab);
  }

  private void _openPersonTab(PersonM? person) {
    PersonDetailTab ??= new(Core.S.Person, Core.S.Segment);
    PersonDetailTab.Reload(person);
    Activate(Res.IconPeople, L.Get(_loc.PersonDetailTabName), PersonDetailTab);
  }

  public override IEnumerable<ITreeItem> ItemMenuFactory(object item) =>
    MenuFactory.GetMenu(((IListItem)item).Data!)
    .Append(new MenuItem(CloseTabCommand, item));
}