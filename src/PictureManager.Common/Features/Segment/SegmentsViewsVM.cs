using MH.UI.Controls;
using MH.UI.Dialogs;
using MH.Utils;
using MH.Utils.BaseClasses;
using MH.Utils.Extensions;
using MH.Utils.Interfaces;
using MH.Utils.Localization;
using PictureManager.Common.Features.Common;
using PictureManager.Common.Features.MediaItem;
using PictureManager.Common.Features.Person;
using PictureManager.Common.Localization;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace PictureManager.Common.Features.Segment;

public sealed class SegmentsViewsVM : ObservableObject {
  internal static class _loc {
    private const string _prefix = "Segment.View.";
    public const string LoadFromTitle = _prefix + "LoadFrom.dlg.Title";
    public const string LoadFromMessage = _prefix + "LoadFrom.dlg.Message";
    public const string LoadFromMediaItems = _prefix + "LoadFrom.dlg.btn.MediaItems";
    public const string LoadFromPeople = _prefix + "LoadFrom.dlg.btn.People";
    public const string LoadFromSegments = _prefix + "LoadFrom.dlg.btn.Segments";
    public const string TabName = _prefix + "TabName";
    public const string TabNameInner = _prefix + "TabNameInner";

    static _loc() {
      LocalizationManager.Register(typeof(_loc).Assembly, [
        (LoadFromTitle, "Segments"),
        (LoadFromMessage, "Load segments from ..."),
        (LoadFromMediaItems, "Media items"),
        (LoadFromPeople, "People"),
        (LoadFromSegments, "Segments"),
        (TabName, "Segments"),
        (TabNameInner, "Segments {0}")
      ]);
    }

    public static void Register() { }
  }

  static SegmentsViewsVM() { _loc.Register(); }

  private readonly SegmentS _segmentS;
  private readonly Dictionary<SegmentsViewVM, SegmentM[]> _views = [];
  private SegmentsViewVM? _current;

  public SegmentsViewsTabsVM Tabs { get; } = new();
  public SegmentsViewVM? Current { get => _current; private set => _setCurrent(value); }

  public SegmentsViewsVM(SegmentS segmentS) {
    _segmentS = segmentS;
    Tabs.PropertyChanged += _onTabsPropertyChanged;
    Tabs.TabClosedEvent += _onTabsTabClosed;
  }

  private void _setCurrent(SegmentsViewVM? value) {
    SegmentM[] oldSelected = [];
    SegmentM[] newSelected = [];

    if (_current != null) {
      oldSelected = _current.Root.Source.Where(x => x.IsSelected).ToArray();

      if (_views.ContainsKey(_current))
        _views[_current] = oldSelected;
    }

    _current = value;

    if (_current != null)
      newSelected = _views[_current];

    _segmentS.Selected.Set(oldSelected.Except(newSelected), false);
    _segmentS.Selected.Set(newSelected, true);
    _segmentS.OnPropertyChanged(nameof(SegmentS.CanSetAsSamePerson));

    OnPropertyChanged(nameof(Current));
  }

  private void _onTabsPropertyChanged(object? sender, PropertyChangedEventArgs e) {
    if (e.Is(nameof(Tabs.Selected)) && Tabs.Selected?.Data is SegmentsViewVM view)
      Current = view;
  }

  private void _onTabsTabClosed(object? sender, IListItem data) {
    if (data.Data is not SegmentsViewVM view) return;
    _views.Remove(view);
    if (!ReferenceEquals(view, Current)) return;
    Current = null;
  }

  public void RemoveSegments(IList<SegmentM> items) {
    var arr = items.ToArray();
    foreach (var view in _views.Keys)
      view.Remove(arr);
  }

  public void Close() {
    foreach (var tab in Tabs.Tabs.ToArray())
      Tabs.Close(tab);
  }

  private SegmentsViewVM _addViewIfNotActive(string tabName) {
    if (Tabs.Selected?.Data is not SegmentsViewVM view)
      return _addView(tabName);
    
    if (!string.IsNullOrEmpty(tabName))
      Tabs.Selected.Name += ", " + tabName;

    return view;
  }

  public SegmentsViewVM AddEmptyView() =>
    _addView(string.Empty);

  private SegmentsViewVM _addView(string tabName) {
    var view = new SegmentsViewVM();
    _views.Add(view, []);
    Current = view;
    Tabs.Activate(Res.IconSegment, tabName, view);
    return view;
  }

  public void UpdateViews(SegmentM[] segments) {
    foreach (var view in _views.Keys)
      view.Update(segments);
  }

  public static Task<int> GetSegmentsToLoadUserInput() {
    var md = new MessageDialog(L.Get(_loc.LoadFromTitle), L.Get(_loc.LoadFromMessage), Res.IconSegment, true);

    md.Buttons = [
      new(md.SetResult(1, MH.UI.Res.IconImage, L.Get(_loc.LoadFromMediaItems)), true),
      new(md.SetResult(2, Res.IconPeople, L.Get(_loc.LoadFromPeople))),
      new(md.SetResult(3, Res.IconSegment, L.Get(_loc.LoadFromSegments)))
    ];

    return Dialog.ShowAsync(md);
  }

  public static IEnumerable<SegmentM> GetSegments(int mode) {
    switch (mode) {
      case 1:
        var items = Core.VM.MainWindow.IsInViewMode
          ? Core.VM.MediaItem.Current != null ? [Core.VM.MediaItem.Current] : []
          : Core.VM.MediaItem.Views.Current?.GetSelectedOrAll().ToArray() ?? [];

        return items.Concat(items.GetVideoItems()).GetSegments();
      case 2:
        var people = Core.S.Person.Selected.Items.ToHashSet();

        return Core.R.Segment.All
          .Where(x => x.Person != null && people.Contains(x.Person))
          .OrderBy(x => x.MediaItem.FileName);
      case 3:
        return Core.S.Segment.Selected.Items;
      default:
        return [];
    }
  }

  public void OnSegmentsPersonChanged(SegmentM[] segments) {
    foreach (var view in _views.Keys)
      view.OnSegmentsPersonChanged(segments);
  }

  public void OnPersonsChanged(PersonM[] items) {
    foreach (var view in _views.Keys)
      view.CvPeople.Update(items);
  }

  public void OnPersonDeleted(PersonM item) {
    foreach (var view in _views.Keys)
      view.CvPeople.Remove(item);
  }

  public void Load(SegmentM[] items, string tabTitle) {
    if (string.IsNullOrEmpty(tabTitle))
      tabTitle = L.Format(_loc.TabNameInner, _views.Count + 1);

    var and = Keyboard.IsCtrlOn() && Current != null;
    var source = (and ? items.Union(Current!.Root.Source) : items).ToList();
    var view = and ? _addViewIfNotActive(tabTitle) : _addView(tabTitle);
    var groupByItems = new[] {
      GroupByItems.GetPeopleInGroup(source),
      GroupByItems.GetKeywordsInGroup(source)
    };

    view.Reload(source, GroupMode.ThenByRecursive, groupByItems, true, true);
    view.ReloadPeople();
  }
}