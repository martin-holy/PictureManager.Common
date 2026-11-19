using MH.UI.Controls;
using MH.Utils;
using MH.Utils.EventsArgs;
using MH.Utils.Localization;
using PictureManager.Common.Features.Common;
using PictureManager.Common.Localization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PictureManager.Common.Features.MediaItem.Video;

public class VideoItemCollectionView : CollectionView<VideoItemM> {
  private static class _loc {
    private const string _prefix = "VideoItem.CollectionView.";
    public const string Title = _prefix + "Title";
    public const string SortFieldTime = _prefix + "SortField.Time";

    static _loc() {
      LocalizationManager.Register(typeof(_loc).Assembly, [
        (Title, "Video Items"),
        (SortFieldTime, "Time")
      ]);
    }

    public static void Register() { }
  }

  static VideoItemCollectionView() {
    _loc.Register();

    _sortFields = [
      new SortField<VideoItemM>("Time", L.Get(_loc.SortFieldTime), x => x.TimeStart, StringComparer.CurrentCultureIgnoreCase)
    ];
  }

  public static void RegisterLocalization() => _loc.Register();

  private static readonly IReadOnlyList<SortField<VideoItemM>> _sortFields;

  public Selecting<VideoItemM> Selected { get; } = new();

  public VideoItemCollectionView() : base(Res.IconImageMultiple, L.Get(_loc.Title), [ViewMode.ThumbBig]) {
    DefaultSortField = _sortFields[0];
  }

  public override int GetItemSize(ViewMode viewMode, VideoItemM item, bool getWidth) =>
    (int)((getWidth ? item.ThumbWidth : item.ThumbHeight) * Core.Settings.MediaItem.VideoItemThumbScale);

  public override IEnumerable<GroupByItem<VideoItemM>> GetGroupByItems(IEnumerable<VideoItemM> source) {
    var src = source.ToArray();
    var top = new List<GroupByItem<VideoItemM>>();
    top.AddRange(GroupByItems.GetFolders(src));
    top.Add(GroupByItems.GetDatesInGroup(src));
    top.AddRange(GroupByItems.GetGeoNames(src));
    top.Add(GroupByItems.GetKeywordsInGroup(src));
    top.Add(GroupByItems.GetPeopleInGroup(src));

    return top;
  }

  public override IEnumerable<SortField<VideoItemM>> GetSortFields() => _sortFields;

  public override int SortCompare(VideoItemM itemA, VideoItemM itemB) =>
    itemA.TimeStart - itemB.TimeStart;

  protected override void _onItemSelected(SelectionEventArgs<VideoItemM> e) =>
    Selected.Select(e.Items, e.Item, e.IsCtrlOn, e.IsShiftOn);

  public override string GetItemTemplateName(ViewMode viewMode) => "PM.DT.VideoItem.Thumb";
}