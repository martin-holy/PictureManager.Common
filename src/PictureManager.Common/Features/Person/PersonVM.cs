using MH.Utils;
using MH.Utils.BaseClasses;
using MH.Utils.Localization;
using PictureManager.Common.Features.CategoryGroup;
using PictureManager.Common.Features.Keyword;
using PictureManager.Common.Features.Segment;
using PictureManager.Common.Localization;
using System.Linq;
using System.Threading.Tasks;

namespace PictureManager.Common.Features.Person;

public sealed class PersonVM {
  internal static class _loc {
    private const string _prefix = "Persons.ToolsTabs.";
    public const string LoadByGroup = _prefix + "LoadByGroup.cmd";
    public const string LoadByKeyword = _prefix + "LoadByKeyword.cmd";

    static _loc() {
      LocalizationManager.Register(typeof(_loc).Assembly, [
        (LoadByGroup, "Load People"),
        (LoadByKeyword, "Load People")
      ]);
    }

    public static void Register() { }
  }

  static PersonVM() { _loc.Register(); }

  private readonly CoreVM _coreVM;
  private readonly PersonR _r;

  public static int PersonTileSegmentWidth { get; set; }
  public static int PersonTileTextWidth { get; set; }
  public static int PersonListWidth { get; set; }
  public static int PersonListHeight { get; set; }
  public static AsyncRelayCommand<CategoryGroupM> LoadByCategoryGroupCommand { get; set; } = null!;
  public static AsyncRelayCommand<KeywordM> LoadByKeywordCommand { get; set; } = null!;

  public PersonVM(CoreVM coreVM, PersonR r) {
    _coreVM = coreVM;
    _r = r;
    LoadByCategoryGroupCommand = new((x, _) => _loadBy(x!), x => x != null, Res.IconPeopleMultiple, L.Get(_loc.LoadByGroup));
    LoadByKeywordCommand = new((x, _) => _loadBy(x!), x => x != null, Res.IconPeopleMultiple, L.Get(_loc.LoadByKeyword));
  }

  private Task _loadBy(CategoryGroupM cg) =>
    _coreVM.MainWindow.ToolsTabs.OpenPeopleTab(cg.Items.OfType<PersonM>().ToArray());

  private Task _loadBy(KeywordM k) =>
    _coreVM.MainWindow.ToolsTabs.OpenPeopleTab(_r.GetBy(k, Keyboard.IsShiftOn()).ToArray());

  public static void SetPersonUiSize(double scale) {
    PersonTileSegmentWidth = SegmentVM.SegmentUiSize + 2;
    PersonTileTextWidth = (int)(173 / scale);
    PersonListWidth = (int)(200 / scale);
    PersonListHeight = (int)(38 / scale);
  }
}