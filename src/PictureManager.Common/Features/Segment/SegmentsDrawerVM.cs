using MH.UI.Controls;
using MH.UI.Dialogs;
using MH.Utils;
using MH.Utils.BaseClasses;
using MH.Utils.Localization;
using PictureManager.Common.Features.Common;
using PictureManager.Common.Localization;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static MH.Utils.DragDropHelper;

namespace PictureManager.Common.Features.Segment;

public sealed class SegmentsDrawerVM : SegmentCollectionView {
  internal new static class _loc {
    private const string _prefix = "Segments.Drawer.";
    public const string Open = _prefix + "Open.cmd";
    public const string Add = _prefix + "Add.cmd";
    public const string Remove = _prefix + "Remove.cmd";
    public const string RemoveTitle = _prefix + "Remove.dlg.Title";
    public const string RemoveMessage = _prefix + "Remove.dlg.Message";
    public const string TabName = _prefix + "Segments";

    static _loc() {
      LocalizationManager.Register(typeof(_loc).Assembly, [
        (Open, "Open Segments drawer"),
        (Add, "Add to Segments drawer"),
        (Remove, "Remove from Segments drawer"),
        (RemoveTitle, "Segments Drawer"),
        (RemoveMessage, "Do you want to remove segments from drawer?"),
        (TabName, "Segments")
      ]);
    }

    public static void Register() { }
  }

  static SegmentsDrawerVM() { _loc.Register(); }

  private readonly SegmentS _segmentS;

  public List<SegmentM> Items { get; }
  public CanDragFunc CanDragFunc { get; }
  public CanDropFunc CanDropFunc { get; }
  public DoDropAction DoDropAction { get; }

  public static AsyncRelayCommand AddSelectedCommand { get; set; } = null!;
  public static AsyncRelayCommand RemoveSelectedCommand { get; set; } = null!;
  public static RelayCommand OpenCommand { get; set; } = null!;

  public SegmentsDrawerVM(SegmentS segmentS, List<SegmentM> items) {
    _segmentS = segmentS;
    Items = items;

    CanDragFunc = one => _segmentS.GetOneOrSelected(one as SegmentM);
    CanDropFunc = _canDrop;
    DoDropAction = _doDrop;

    AddSelectedCommand = new(
      _ => _addOrRemove(_segmentS.Selected.Items.ToArray(), true),
      () => _segmentS.Selected.Items.Except(Items).Any(), Res.IconDrawerAdd, L.Get(_loc.Add));
    RemoveSelectedCommand = new(
      _ => _addOrRemove(_segmentS.Selected.Items.Intersect(Items).ToArray(), false),
      () => _segmentS.Selected.Items.Intersect(Items).Any(), Res.IconDrawerRemove, L.Get(_loc.Remove));
    OpenCommand = new(() => _open(Core.VM.MainWindow.ToolsTabs), Res.IconDrawer, L.Get(_loc.Open));
  }

  private DragDropEffects _canDrop(object? target, object? data, bool haveSameOrigin) {
    if (!haveSameOrigin && !Items.Contains(data))
      return DragDropEffects.Copy;
    if (haveSameOrigin && (data as SegmentM[])?.Contains(target as SegmentM) == false)
      return DragDropEffects.Move;
    return DragDropEffects.None;
  }

  private Task _doDrop(object data, bool haveSameOrigin) =>
    _addOrRemove(data as SegmentM[] ?? (data is SegmentM s ? [s] : []), !haveSameOrigin);

  private async Task _addOrRemove(SegmentM[] segments, bool add) {
    if (!add && await Dialog.ShowAsync(new MessageDialog(
        L.Get(_loc.RemoveTitle),
        L.Get(_loc.RemoveMessage),
        MH.UI.Res.IconQuestion,
        true)) != 1)
      return;

    var count = Items.Count;

    if (add) {
      var toAdd = Items.Concat(segments.Except(Items)).ToArray();
      Items.Clear();

      foreach (var segment in toAdd)
        Items.Add(segment);
    }
    else
      foreach (var segment in segments)
        Items.Remove(segment);

    if (count == Items.Count) return;
    _segmentS.Repo.ArePropsModified = true;
    if (add) Insert(segments); else Remove(segments);
  }

  public void RemoveIfContains(SegmentM[] segments) {
    var flag = false;
    foreach (var segment in segments)
      if (Items.Remove(segment))
        flag = true;

    if (!flag) return;
    _segmentS.Repo.ArePropsModified = true;
    Remove(segments);
  }

  private void _open(TabControl tt) {
    var source = Items
      .OrderBy(x => x.MediaItem.Folder.FullPath)
      .ThenBy(x => x.MediaItem.FileName)
      .ToList();
    var groupByItems = GroupByItems.GetFolders(source).ToArray();

    Reload(source, GroupMode.GroupByRecursive, groupByItems, true, false);
    tt.Activate(Res.IconDrawer, L.Get(_loc.TabName), this);
  }
}