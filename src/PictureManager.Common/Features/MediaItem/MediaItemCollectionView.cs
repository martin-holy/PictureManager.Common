using MH.UI.Controls;
using MH.Utils;
using MH.Utils.BaseClasses;
using MH.Utils.EventsArgs;
using MH.Utils.Localization;
using PictureManager.Common.Features.Common;
using PictureManager.Common.Localization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PictureManager.Common.Features.MediaItem;

public class MediaItemCollectionView : CollectionView<MediaItemM> {
  internal static class _loc {
    private const string _prefix = "MediaItem.CollectionView.";
    public const string Title = _prefix + "Title";
    public const string SortFieldFileName = _prefix + "SortField.FileName";
    public const string SortFieldLastWriteTime = _prefix + "SortField.LastWriteTime";
    public const string SortFieldCreationTime = _prefix + "SortField.CreationTime";

    static _loc() {
      LocalizationManager.Register(typeof(_loc).Assembly, [
        (Title, "Media Items"),
        (SortFieldFileName, "File name"),
        (SortFieldLastWriteTime, "Modified"),
        (SortFieldCreationTime, "Created")
      ]);
    }

    public static void Register() { }
  }

  static MediaItemCollectionView() {
    _loc.Register();

    SortFields = [
      new SortField<MediaItemM>("File name", L.Get(_loc.SortFieldFileName), x => x.FileName, StringComparer.CurrentCultureIgnoreCase),
      new SortField<MediaItemM>("Modified", L.Get(_loc.SortFieldLastWriteTime), x => new System.IO.FileInfo(x.FilePath).LastWriteTime),
      new SortField<MediaItemM>("Created", L.Get(_loc.SortFieldCreationTime), x => new System.IO.FileInfo(x.FilePath).CreationTime)
    ];
  }

  private double _thumbScale;

  public static readonly IReadOnlyList<SortField<MediaItemM>> SortFields;

  public double ThumbScale { get => _thumbScale; set { _thumbScale = value; OnPropertyChanged(); } }
  public Selecting<MediaItemM> Selected { get; } = new();

  public RelayCommand ThumbScaleChangedCommand { get; }

  public MediaItemCollectionView(double thumbScale) :
    base(Res.IconImageMultiple, L.Get(_loc.Title), [ViewMode.ThumbBig]) {

    ThumbScale = thumbScale;
    ThumbScaleChangedCommand = new(_onThumbScaleChanged);
    DefaultSortField = SortFields.SingleOrDefault(x => x.Name.Equals(Core.Settings.MediaItem.SortField), SortFields[0]);
    DefaultSortOrder = Core.Settings.MediaItem.SortOrder;
  }

  private void _onThumbScaleChanged() =>
    ReWrapAll();

  public override IEnumerable<GroupByItem<MediaItemM>> GetGroupByItems(IEnumerable<MediaItemM> source) {
    var src = source.ToArray();
    var top = new List<GroupByItem<MediaItemM>>();
    // TODO remove trunk from folders => remove common branch starting from root
    top.AddRange(GroupByItems.GetFolders(src));
    top.Add(GroupByItems.GetDatesInGroup(src));
    top.AddRange(GroupByItems.GetGeoNames(src));
    top.Add(GroupByItems.GetKeywordsInGroup(src));
    top.Add(GroupByItems.GetPeopleInGroup(src));

    return top;
  }

  public override int GetItemSize(ViewMode viewMode, MediaItemM item, bool getWidth) =>
    (int)((getWidth ? item.ThumbWidth : item.ThumbHeight) * ThumbScale);

  public override IEnumerable<SortField<MediaItemM>> GetSortFields() => SortFields;

  public override int SortCompare(MediaItemM itemA, MediaItemM itemB) =>
    string.Compare(itemA.FileName, itemB.FileName, StringComparison.CurrentCultureIgnoreCase);

  protected override void _onItemSelected(SelectionEventArgs<MediaItemM> e) =>
    Selected.Select(e.Items, e.Item, e.IsCtrlOn, e.IsShiftOn);

  public override string GetItemTemplateName(ViewMode viewMode) => "PM.DT.MediaItem.Thumb-Full";
}