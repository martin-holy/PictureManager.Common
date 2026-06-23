using MH.Utils;
using MH.Utils.BaseClasses;
using MH.Utils.DB.Repositories;
using MH.Utils.Extensions;
using MH.Utils.Interfaces;
using PictureManager.Common.Features.CategoryGroup;
using PictureManager.Common.Features.Keyword;
using PictureManager.Common.Features.Segment;
using PictureManager.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PictureManager.Common.Features.Person;

public sealed class PersonR : TreeRepository<PersonM> {
  private readonly CoreR _coreR;

  public const string UnknownPersonNamePrefix = "P -";

  public PersonTreeCategory Tree { get; }
  public PersonDS DataSource { get; }
  public event EventHandler<PersonM[]> PersonsKeywordsChangedEvent = delegate { };

  public PersonR(CoreR coreR, CategoryGroupR cgR) {
    _coreR = coreR;
    Tree = new(this, cgR);
    DataSource = new(coreR, this);
  }

  public IEnumerable<PersonM> GetAll() {
    foreach (var cg in Tree.Items.OfType<CategoryGroupM>())
      foreach (var personM in cg.Items.Cast<PersonM>())
        yield return personM;

    foreach (var personM in Tree.Items.OfType<PersonM>())
      yield return personM;
  }

  public IEnumerable<PersonM> GetBy(KeywordM keyword, bool recursive) =>
    All.GetBy(keyword, recursive);

  public override PersonM ItemCreate(ITreeItem parent, string name) =>
    TreeItemCreate(new(GetNextId(), name) { Parent = parent });

  public PersonM ItemCreateUnknown() {
    var id = SimpleDB.GetNextRecycledId(All.Select(x => x.Id).ToHashSet()) ?? GetNextId();

    return TreeItemCreate(new(id, $"{UnknownPersonNamePrefix}{id}") {
      Parent = Tree.UnknownGroup,
      IsUnknown = true
    });
  }

  protected override void _onItemRenamed(PersonM item) {
    if (item.IsUnknown) item.IsUnknown = false;
  }

  protected override void _onItemDeleted(object sender, PersonM item) {
    item.Parent?.Items.Remove(item);
    item.Parent = null;
    item.Segment = null;
    item.TopSegments = null;
    item.Keywords = null;
  }

  public PersonM? GetPerson(string name, bool create) =>
    All.SingleOrDefault(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
    ?? (create ? ItemCreate(Tree, name) : null);

  public void OnSegmentPersonChanged(SegmentM segment, PersonM? oldPerson, PersonM? newPerson) {
    if (newPerson != null) {
      newPerson.Segment ??= segment;
      newPerson.Segments = newPerson.Segments.Toggle(segment, true);
    }

    if (oldPerson == null) return;
    oldPerson.Segments = oldPerson.Segments.Toggle(segment, true);

    if (oldPerson.TopSegments?.Contains(segment) == true) {
      oldPerson.ToggleTopSegment(segment);
      IsModified = true;
    }

    if (ReferenceEquals(oldPerson.Segment, segment))
      oldPerson.Segment = oldPerson.TopSegments?.FirstOrDefault()
                          ?? oldPerson.Segments?.FirstOrDefault();
  }

  public void OnSegmentsPersonChanged(SegmentM[] segments, PersonM? person, PersonM[] people) {
    // delete unknown people without segments
    var toDelete = person == null
      ? people
      : people.Where(x => !ReferenceEquals(x, person)).ToArray();

    // WARNING Segments.All contains only segments from available drives!
    // When the drive is mounted, not found people will be recreated.
    foreach (var ptd in toDelete)
      if (ptd.IsUnknown && !_coreR.Segment.All.Any(x => ReferenceEquals(x.Person, ptd)))
        ItemDelete(ptd);
  }

  public void RemoveKeyword(KeywordM keyword) =>
    ToggleKeyword(All.Where(x => x.Keywords?.Contains(keyword) == true).ToArray(), keyword);

  public void ToggleKeyword(PersonM[] people, KeywordM keyword) =>
    keyword.Toggle(people, _ => IsModified = true, () => PersonsKeywordsChangedEvent(this, people));

  public void ToggleKeywords(PersonM person, IEnumerable<KeywordM> keywords) {
    foreach (var keyword in keywords)
      person.Keywords = person.Keywords.Toggle(keyword);

    IsModified = true;
    PersonsKeywordsChangedEvent(this, [person]);
  }

  public void MoveGroupItemsToRoot(CategoryGroupM group) {
    if (group.Category != Category.People) return;
    foreach (var item in group.Items.ToArray())
      ItemMove(item, Tree, false);
  }
}