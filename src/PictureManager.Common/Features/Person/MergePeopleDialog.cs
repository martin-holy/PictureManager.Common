using MH.UI.Controls;
using MH.Utils.Localization;
using PictureManager.Common.Features.Common;
using PictureManager.Common.Features.Segment;
using PictureManager.Common.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PictureManager.Common.Features.Person;

public sealed class MergePeopleDialog : Dialog {
  private static class _loc {
    private const string _prefix = "Person.MergePeople.";
    public const string Title = _prefix + "dlg.Title";
    public const string SegmentsViewName = _prefix + "SegmentsView.Name";

    static _loc() {
      LocalizationManager.Register(typeof(_loc).Assembly, [
        (Title, "Select a person for segments"),
        (SegmentsViewName, "Segments to update")
      ]);
    }

    public static void Register() { }
  }

  static MergePeopleDialog() { _loc.Register(); }
  public static void RegisterLocalization() { }

  private static MergePeopleDialog? _inst;
  private readonly SegmentS _segmentS;
  private List<PersonM> _people = null!;
  private SegmentM[] _unknownSelected = null!;
  private SegmentM[] _segmentsToUpdate = null!;
  private PersonM _person = null!;

  public PersonM Person { get => _person; private set { _person = value; OnPropertyChanged(); } }
  public PersonCollectionView PeopleView { get; }
  public SegmentCollectionView SegmentsView { get; }

  public MergePeopleDialog(SegmentS segmentS) : base(L.Get(_loc.Title), Res.IconPeople) {
    _segmentS = segmentS;
    PeopleView = new() { CanOpen = false, IsMultiSelect = false };
    SegmentsView = new() { CanSelect = false, CanOpen = false };
    Buttons = [
      new(OkCommand, true),
      new(CloseCommand, false, true)
    ];
  }

  public static Task<(PersonM, SegmentM[])?> Open(PersonS personS, SegmentS segmentS, PersonM[] people) {
    if (_inst == null) {
      _inst = new(segmentS);
      _inst.PeopleView.ItemSelectedEvent += (_, e) =>
        _inst._setPerson(e.Item);
    }

    return _inst._open(personS, segmentS, people);
  }

  private async Task<(PersonM, SegmentM[])?> _open(PersonS personS, SegmentS segmentS, PersonM[] people) {
    _people = PeopleView.Sort(people.ToList());
    _unknownSelected = segmentS.Selected.Items.Where(x => x.Person == null).ToArray();
    personS.Selected.Select(_people[0]);
    _setPerson(_people[0]);
    PeopleView.Reload(_people, GroupMode.ThenByRecursive, null, true, true);

    if (await ShowAsync(this) != 1) {
      _clear();
      return null;
    }

    personS.MergePeople(Person, people.Where(x => !ReferenceEquals(x, Person)).ToArray());
    _clear();

    return new ValueTuple<PersonM, SegmentM[]>(Person, _segmentsToUpdate);
  }

  private void _setPerson(PersonM person) {
    Person = person;
    _segmentsToUpdate = _getSegmentsToUpdate(Person, _people);
    var groupByItems = GroupByItems.GetPeople(_segmentsToUpdate).ToArray();

    SegmentsView.Name = L.Get(_loc.SegmentsViewName);
    SegmentsView.Reload([.. _segmentsToUpdate], GroupMode.GroupBy, groupByItems, true, true);
  }

  private SegmentM[] _getSegmentsToUpdate(PersonM person, IEnumerable<PersonM> people) {
    var oldPeople = people.Where(x => !ReferenceEquals(x, person)).ToHashSet();
    return _segmentS.Repo.All
      .Where(x => x.Person != null && oldPeople.Contains(x.Person))
      .Concat(_unknownSelected)
      .ToArray();
  }

  private void _clear() {
    PeopleView.Root.Clear();
    SegmentsView.Root.Clear();
  }
}