using MH.UI.Controls;
using MH.Utils.EventsArgs;
using MH.Utils.Localization;
using PictureManager.Common.Features.Common;
using PictureManager.Common.Features.Segment;
using PictureManager.Common.Localization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PictureManager.Common.Features.Person;

public class PersonCollectionView : CollectionView<PersonM> {
  private static class _loc {
    private const string _prefix = "Person.CollectionView.";
    public const string Title = _prefix + "Title";
    public const string SortFieldName = _prefix + "SortField.Name";

    static _loc() {
      LocalizationManager.Register(typeof(_loc).Assembly, [
        (Title, "People"),
        (SortFieldName, "Name")
      ]);
    }

    public static void Register() { }
  }

  static PersonCollectionView() {
    _loc.Register();

    _sortFields = [
      new SortField<PersonM>("Name", L.Get(_loc.SortFieldName), x => x.Name, StringComparer.CurrentCultureIgnoreCase)
    ];
  }

  public static void RegisterLocalization() { }

  private static readonly IReadOnlyList<SortField<PersonM>> _sortFields;

  public PersonCollectionView() : base(Res.IconPeopleMultiple, L.Get(_loc.Title), [ViewMode.ThumbSmall, ViewMode.List, ViewMode.Tiles]) {
    DefaultSortField = _sortFields[0];
  }

  public override IEnumerable<GroupByItem<PersonM>> GetGroupByItems(IEnumerable<PersonM> source) {
    var src = source.ToArray();
    var top = new List<GroupByItem<PersonM>>();
    top.Add(GroupByItems.GetPeopleGroupsInGroup(src));
    top.Add(GroupByItems.GetKeywordsInGroup(src));
    top.AddRange(GroupByItems.GetFolders(src));

    return top;
  }

  public override int GetItemSize(ViewMode viewMode, PersonM item, bool getWidth) =>
    viewMode switch {
      ViewMode.List => getWidth ? PersonVM.PersonListWidth : PersonVM.PersonListHeight,
      ViewMode.Tiles => getWidth ? PersonVM.PersonTileTextWidth + SegmentVM.SegmentUiFullWidth : SegmentVM.SegmentUiFullWidth,
      _ => SegmentVM.SegmentUiFullWidth
    };

  public override IEnumerable<SortField<PersonM>> GetSortFields() => _sortFields;

  public override int SortCompare(PersonM itemA, PersonM itemB) =>
    string.Compare(itemA.Name, itemB.Name, StringComparison.CurrentCultureIgnoreCase);

  protected override void _onItemSelected(SelectionEventArgs<PersonM> e) =>
    Core.S.Person.Select(e);

  protected override void _onItemOpened(PersonM item) =>
    Core.S.Segment.ViewMediaItemsWithSegment(this, item.Segment);

  public override string GetItemTemplateName(ViewMode viewMode) =>
    viewMode switch {
      ViewMode.List => "PM.DT.Person.ListItem",
      ViewMode.Tiles => "PM.DT.Person.Tile",
      _ => "PM.DT.Person.Thumb"
    };
}