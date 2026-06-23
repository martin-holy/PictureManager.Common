using MH.Utils.DB.Repositories;
using PictureManager.Common.Features.Keyword;
using PictureManager.Common.Features.MediaItem;
using PictureManager.Common.Features.Person;
using PictureManager.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PictureManager.Common.Features.Segment;

public sealed class SegmentR : Repository<SegmentM> {
  public SegmentDS DataSource { get; }
  public List<SegmentM> Drawer { get; set; } = [];
  public event EventHandler<(SegmentM, PersonM?, PersonM?)>? SegmentPersonChangedEvent;
  public event EventHandler<(SegmentM[], PersonM?, PersonM[])>? SegmentsPersonChangedEvent;
  public event EventHandler<SegmentM[]>? SegmentsKeywordsChangedEvent;

  public SegmentR(CoreR coreR) {
    DataSource = new(coreR, this);
  }

  public SegmentM ItemCreate(double x, double y, int size, MediaItemM mediaItem) =>
    ItemCreate(new(GetNextId(), x, y, size, mediaItem));

  public SegmentM ItemCopy(SegmentM item, MediaItemM mediaItem) =>
    ItemCreate(new(GetNextId(), item.X, item.Y, item.Size, mediaItem) {
      Person = item.Person,
      Keywords = item.Keywords?.ToList()
    });

  protected override void _onItemDeleted(object sender, SegmentM item) {
    var path = item.FilePathCache;
    if (File.Exists(path)) File.Delete(path);
  }

  public IEnumerable<SegmentM> GetBy(KeywordM keyword, bool recursive) =>
    All.GetBy(keyword, recursive);

  public IEnumerable<SegmentM> GetBy(PersonM person) =>
    All.Where(x => ReferenceEquals(x.Person, person));

  public void RemovePerson(PersonM person) {
    var segments = All.Where(x => ReferenceEquals(x.Person, person)).ToArray();
    if (segments.Length == 0) return;
    foreach (var segment in segments) {
      segment.Person = null;
      IsModified = true;
    }

    _raiseSegmentsPersonChanged((segments, null, [person]));
  }

  public void RemoveKeyword(KeywordM keyword) =>
    ToggleKeyword(All.Where(x => x.Keywords?.Contains(keyword) == true).ToArray(), keyword);

  public void ToggleKeyword(SegmentM[] segments, KeywordM keyword) =>
    keyword.Toggle(segments, _ => IsModified = true, () => _raiseSegmentsKeywordsChanged(segments));

  public void ChangePerson(PersonM? person, SegmentM[] segments, PersonM[] people) {
    foreach (var segment in segments)
      _changePerson(segment, person);

    _raiseSegmentsPersonChanged((segments, person, people));
  }

  private void _changePerson(SegmentM segment, PersonM? person) {
    var oldPerson = segment.Person;
    segment.Person = person;
    IsModified = true;
    _raiseSegmentPersonChanged((segment, oldPerson, person));
  }

  private void _raiseSegmentPersonChanged((SegmentM, PersonM?, PersonM?) args) =>
    SegmentPersonChangedEvent?.Invoke(this, args);

  private void _raiseSegmentsPersonChanged((SegmentM[], PersonM?, PersonM[]) args) =>
    SegmentsPersonChangedEvent?.Invoke(this, args);

  private void _raiseSegmentsKeywordsChanged(SegmentM[] args) =>
    SegmentsKeywordsChangedEvent?.Invoke(this, args);
}