using MH.UI.Controls;
using MH.Utils.EventsArgs;
using MH.Utils.Localization;
using PictureManager.Common.Features.Common;
using PictureManager.Common.Localization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PictureManager.Common.Features.Segment;

public class SegmentCollectionView : CollectionView<SegmentM> {
  private static class _loc {
    private const string _prefix = "Segment.CollectionView.";
    public const string Title = _prefix + "Title";
    public const string SortFieldFileName = _prefix + "SortField.FileName";

    static _loc() {
      LocalizationManager.Register(typeof(_loc).Assembly, [
        (Title, "Segments"),
        (SortFieldFileName, "File name")
      ]);
    }

    public static void Register() { }
  }

  static SegmentCollectionView() {
    _loc.Register();

    _sortFields = [
      new SortField<SegmentM>("File name", L.Get(_loc.SortFieldFileName), x => x.MediaItem.FileName, StringComparer.CurrentCultureIgnoreCase)
    ];
  }

  public static void RegisterLocalization() { }

  private static readonly IReadOnlyList<SortField<SegmentM>> _sortFields;

  public SegmentCollectionView() : base(Res.IconSegment, L.Get(_loc.Title), [ViewMode.ThumbSmall]) {
    DefaultSortField = _sortFields[0];
  }

  public override IEnumerable<GroupByItem<SegmentM>> GetGroupByItems(IEnumerable<SegmentM> source) {
    var src = source.ToArray();
    var top = new List<GroupByItem<SegmentM>>();
    top.AddRange(GroupByItems.GetFolders(src));
    top.AddRange(GroupByItems.GetGeoNames(src));
    top.Add(GroupByItems.GetKeywordsInGroup(src));
    top.Add(GroupByItems.GetPeopleInGroup(src));
    top.Add(GroupByItems.GetSegmentSizesInGroup(src));

    return top;
  }

  public override int GetItemSize(ViewMode viewMode, SegmentM item, bool getWidth) =>
    SegmentVM.SegmentUiFullWidth;

  public override IEnumerable<SortField<SegmentM>> GetSortFields() => _sortFields;

  public override int SortCompare(SegmentM itemA, SegmentM itemB) =>
    string.Compare(itemA.MediaItem.FileName, itemB.MediaItem.FileName, StringComparison.CurrentCultureIgnoreCase);

  protected override void _onItemSelected(SelectionEventArgs<SegmentM> e) =>
    Core.S.Segment.Select(e.Items, e.Item, e.IsCtrlOn, e.IsShiftOn);

  protected override void _onItemOpened(SegmentM item) =>
    Core.S.Segment.ViewMediaItemsWithSegment(this, item);

  public override string GetItemTemplateName(ViewMode viewMode) => "PM.DT.Segment";
}