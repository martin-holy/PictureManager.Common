using MH.UI.Controls;
using MH.UI.Dialogs;
using MH.Utils.Extensions;
using MH.Utils.Localization;
using PictureManager.Common.Features.CategoryGroup;
using PictureManager.Common.Features.MediaItem;
using PictureManager.Common.Localization;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PictureManager.Common.Features.Person;

public sealed class PeopleToolsTabVM : PersonCollectionView {
  internal static class _loc {
    private const string _prefix = "Person.PeopleToolsTab.";
    public const string GetFromTitle = _prefix + "GetFrom.dlg.Title";
    public const string GetFromMessage = _prefix + "GetFrom.dlg.Message";
    public const string GetFromBtnThumbnails = _prefix + "GetFrom.dlg.btn.Thumbnails";
    public const string GetFromBtnMediaViewer = _prefix + "GetFrom.dlg.btn.MediaViewer";
    public const string GetFromBtnAllPeople = _prefix + "GetFrom.dlg.btn.AllPeople";

    static _loc() {
      LocalizationManager.Register(typeof(_loc).Assembly, [
        (GetFromTitle, "Reload People"),
        (GetFromMessage, "From which source do you want to load the people?"),
        (GetFromBtnThumbnails, "Thumbnails"),
        (GetFromBtnMediaViewer, "Media Viewer"),
        (GetFromBtnAllPeople, "All people")
      ]);
    }

    public static void Register() { }
  }

  static PeopleToolsTabVM() { _loc.Register(); }

  private static async Task<IEnumerable<PersonM>> _getPeople() {
    var md = new MessageDialog(
      L.Get(_loc.GetFromTitle),
      L.Get(_loc.GetFromMessage),
      Res.IconPeople,
      true);

    md.Buttons = [
      new(md.SetResult(1, null, L.Get(_loc.GetFromBtnThumbnails)), true),
      new(md.SetResult(2, null, L.Get(_loc.GetFromBtnMediaViewer))),
      new(md.SetResult(3, null, L.Get(_loc.GetFromBtnAllPeople)))
    ];

    var result = await Dialog.ShowAsync(md);
    if (result < 1) return [];

    return result switch {
      1 => Core.VM.MediaItem.Views.Current?.GetSelectedOrAll().GetPeople() ?? [],
      2 => Core.VM.MediaViewer.MediaItems.GetPeople(),
      3 => Core.R.Person.All.Where(x => x.Parent is not CategoryGroupM { IsHidden: true }),
      _ => []
    };
  }

  public async Task Reload(PersonM[]? people) {
    var src = (people ?? await _getPeople()).EmptyIfNull().ToList();
    Reload(src, GroupMode.ThenByRecursive, null, true, true);
  }
}