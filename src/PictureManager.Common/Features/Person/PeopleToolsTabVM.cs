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
  internal new static class _loc {
    private const string _prefix = "Persons.ToolsTabs.LoadFrom.dlg.";
    public const string LoadFromTitle = _prefix + "Title";
    public const string LoadFromMessage = _prefix + "Message";
    public const string BtnLoadFromThumbnails = _prefix + "btn.Thumbnails";
    public const string BtnLoadFromMediaViewer = _prefix + "btn.MediaViewer";
    public const string BtnLoadFromAllPeople = _prefix + "btn.AllPeople";

    static _loc() {
      LocalizationManager.Register(typeof(_loc).Assembly, [
        (LoadFromTitle, "Reload People"),
        (LoadFromMessage, "From which source do you want to load the people?"),
        (BtnLoadFromThumbnails, "Thumbnails"),
        (BtnLoadFromMediaViewer, "Media Viewer"),
        (BtnLoadFromAllPeople, "All people")
      ]);
    }

    public static void Register() { }
  }

  static PeopleToolsTabVM() { _loc.Register(); }

  private static async Task<IEnumerable<PersonM>> _getPeople() {
    var md = new MessageDialog(
      L.Get(_loc.LoadFromTitle),
      L.Get(_loc.LoadFromMessage),
      Res.IconPeople,
      true);

    md.Buttons = [
      new(md.SetResult(1, null, L.Get(_loc.BtnLoadFromThumbnails)), true),
      new(md.SetResult(2, null, L.Get(_loc.BtnLoadFromMediaViewer))),
      new(md.SetResult(3, null, L.Get(_loc.BtnLoadFromAllPeople)))
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