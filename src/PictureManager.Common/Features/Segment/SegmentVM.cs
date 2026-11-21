using MH.UI.Controls;
using MH.UI.Dialogs;
using MH.Utils;
using MH.Utils.BaseClasses;
using MH.Utils.Extensions;
using MH.Utils.Localization;
using MH.Utils.Tree;
using PictureManager.Common.Features.Keyword;
using PictureManager.Common.Features.Person;
using PictureManager.Common.Interfaces;
using PictureManager.Common.Localization;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PictureManager.Common.Features.Segment;

public sealed class SegmentVM : ObservableObject {
  internal static class _loc {
    private const string _prefix = "Segments.";
    public const string AddEmptyView = _prefix + "AddEmptyView.cmd";
    public const string LoadByKeyword = _prefix + "LoadByKeyword.cmd";
    public const string LoadByPerson = _prefix + "LoadByPerson.cmd";
    public const string AddToPersonTopSegments = _prefix + "AddToPersonTopSegments.cmd";
    public const string RemoveFromPersonTopSegments = _prefix + "RemoveFromPersonTopSegments.cmd";
    public const string SetAsSamePerson = _prefix + "SetAsSamePerson.cmd";
    public const string SetAsUnknown = _prefix + "SetAsUnknown.cmd";
    public const string SetAsUnknownTitle = _prefix + "SetAsUnknown.dlg.Title";
    public const string SetAsUnknownMessage = _prefix + "SetAsUnknown.dlg.Message";
    public const string Delete = _prefix + "Delete.cmd";
    public const string DeleteTitle = _prefix + "Delete.dlg.Title";
    public const string DeleteMessage = _prefix + "Delete.dlg.Message";

    static _loc() {
      LocalizationManager.Register(typeof(_loc).Assembly, [
        (AddEmptyView, "Add Segments View Tab"),
        (LoadByKeyword, "Load Segments"),
        (LoadByPerson, "Load Segments"),
        (AddToPersonTopSegments, "Add selected to Top segments"),
        (RemoveFromPersonTopSegments, "Remove selected from Top segments"),
        (SetAsSamePerson, "Set selected as same person"),
        (SetAsUnknown, "Set selected as Unknown"),
        (SetAsUnknownTitle, "Set as unknown"),
        (SetAsUnknownMessage + ".one", "Do you want to set {0} segment as unknown?"),
        (SetAsUnknownMessage + ".other", "Do you want to set {0} segments as unknown?"),
        (Delete, "Delete selected"),
        (DeleteTitle, "Delete Confirmation"),
        (DeleteMessage + ".one", "Do you really want to delete {0} item?"),
        (DeleteMessage + ".other", "Do you really want to delete {0} items?")
      ]);
    }

    public static void Register() { }
  }

  static SegmentVM() { _loc.Register(); }

  private readonly CoreVM _coreVM;
  private readonly SegmentS _s;
  private readonly SegmentR _r;

  public static int SegmentSize { get; set; } = 100;
  public static int SegmentUiSize { get; set; }
  public static int SegmentUiFullWidth { get; set; }
  public static IImageSourceConverter<SegmentM> ThumbConverter { get; set; } = null!;

  public SegmentsViewsVM Views { get; }
  public SegmentRectVM Rect { get; }

  public static RelayCommand AddEmptyViewCommand { get; set; } = null!;
  public static AsyncRelayCommand<KeywordM> LoadByKeywordCommand { get; set; } = null!;
  public static AsyncRelayCommand<PersonM> LoadByPersonCommand { get; set; } = null!;
  public static RelayCommand<PersonM> AddSelectedToPersonsTopSegmentsCommand { get; set; } = null!;
  public static RelayCommand<PersonM> RemoveSelectedFromPersonsTopSegmentsCommand { get; set; } = null!;
  public static AsyncRelayCommand SetSelectedAsSamePersonCommand { get; set; } = null!;
  public static AsyncRelayCommand SetSelectedAsUnknownCommand { get; set; } = null!;
  public static AsyncRelayCommand DeleteSelectedCommand { get; set; } = null!;

  public SegmentVM(CoreVM coreVM, SegmentS s, SegmentR r) {
    _coreVM = coreVM;
    _s = s;
    _r = r;
    Views = new(_s);
    Rect = new(s.Rect);

    AddEmptyViewCommand = new(_addEmptyView, Res.IconTabPlus, L.Get(_loc.AddEmptyView));
    LoadByKeywordCommand = new((x, _) => _loadBy(x!), x => x != null, Res.IconSegment, L.Get(_loc.LoadByKeyword));
    LoadByPersonCommand = new((x, _) => _loadBy(x!), x => x != null, Res.IconSegment, L.Get(_loc.LoadByPerson));
    AddSelectedToPersonsTopSegmentsCommand = new(_addSelectedToPersonsTopSegments, _canAddSelectedToPersonsTopSegments, Res.IconSegmentAdd, L.Get(_loc.AddToPersonTopSegments));
    RemoveSelectedFromPersonsTopSegmentsCommand = new(_removeSelectedFromPersonsTopSegments, _canRemoveSelectedFromPersonsTopSegments, Res.IconSegmentRemove, L.Get(_loc.RemoveFromPersonTopSegments));
    SetSelectedAsSamePersonCommand = new(_setSelectedAsSamePerson, Res.IconEquals, L.Get(_loc.SetAsSamePerson));
    SetSelectedAsUnknownCommand = new(_setSelectedAsUnknown, _canSetSelectedAsUnknown, Res.IconUnknownSegment, L.Get(_loc.SetAsUnknown));
    DeleteSelectedCommand = new(_deleteSelected, _canDeleteSelected, Res.IconSegmentDelete, L.Get(_loc.Delete));

    _s.Selected.ItemsChangedEvent += _onSelectedSegmentsChanged;
  }

  private void _addEmptyView() {
    _coreVM.MainTabs.Activate(Res.IconSegment, L.Get(SegmentsViewsVM._loc.TabName), Views);
    Views.AddEmptyView();
  }

  private Task _loadBy(KeywordM k) =>
    _coreVM.OpenSegmentsViews(_r.GetBy(k, Keyboard.IsShiftOn()).ToArray(), k.GetFullName("-", x => x.Name));

  private Task _loadBy(PersonM p) =>
    _coreVM.OpenSegmentsViews(_r.GetBy(p).ToArray(), p.Name);

  private void _addSelectedToPersonsTopSegments(PersonM? person) {
    Core.S.Person.AddToTopSegments(person!, _s.Selected.Items);
  }

  private bool _canAddSelectedToPersonsTopSegments(PersonM? person) =>
    person != null && _s.Selected.Items.Any(x => ReferenceEquals(person, x.Person)
      && (person.TopSegments == null || !person.TopSegments.Contains(x)));

  private void _removeSelectedFromPersonsTopSegments(PersonM? person) {
    Core.S.Person.RemoveFromTopSegments(person!, _s.Selected.Items);
  }

  private bool _canRemoveSelectedFromPersonsTopSegments(PersonM? person) =>
     person?.TopSegments?.Any(_s.Selected.Items.Contains) == true;

  private Task _setSelectedAsSamePerson(CancellationToken token) =>
    _s.SetSelectedAsSamePerson(_s.Selected.Items.ToArray());

  private Task _setSelectedAsUnknown(CancellationToken token) =>
    _setAsUnknown(_s.Selected.Items.ToArray(), _r);

  private bool _canSetSelectedAsUnknown() =>
    _s.Selected.Items.Count > 0;

  private static async Task _setAsUnknown(SegmentM[] segments, SegmentR r) {
    var msg = L.FormatPlural(_loc.SetAsUnknownMessage, segments.Length);
    if (await Dialog.ShowAsync(new MessageDialog(L.Get(_loc.SetAsUnknownTitle), msg, MH.UI.Res.IconQuestion, true)) != 1) return;
    r.ChangePerson(null, segments, segments.GetPeople().ToArray());
  }

  private async Task _deleteSelected(CancellationToken token) {
    var items = _s.Selected.Items.ToArray();
    var msg = L.FormatPlural(_loc.DeleteMessage, items.Length);
    if (await Dialog.ShowAsync(new MessageDialog(L.Get(_loc.DeleteTitle), msg, MH.UI.Res.IconQuestion, true)) != 1) return;
    _r.ItemsDelete(items);
  }

  private bool _canDeleteSelected() =>
    _s.Selected.Items.Count > 0;

  private void _onSelectedSegmentsChanged(object? sender, SegmentM[] e) {
    AddSelectedToPersonsTopSegmentsCommand.RaiseCanExecuteChanged();
    RemoveSelectedFromPersonsTopSegmentsCommand.RaiseCanExecuteChanged();
    SetSelectedAsSamePersonCommand.RaiseCanExecuteChanged();
    SetSelectedAsUnknownCommand.RaiseCanExecuteChanged();
    DeleteSelectedCommand.RaiseCanExecuteChanged();
  }

  public static void SetSegmentUiSize(double scale) {
    var size = (int)(SegmentSize / scale);
    SegmentUiSize = size;
    SegmentUiFullWidth = size + 6; // + border, margin
  }
}