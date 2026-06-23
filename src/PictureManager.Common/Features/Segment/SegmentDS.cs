using MH.Utils.DB;
using MH.Utils.DB.DataSources;
using MH.Utils.Extensions;
using PictureManager.Common.Features.MediaItem;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PictureManager.Common.Features.Segment;

/// <summary>
/// DB fields: ID|MediaItemId|PersonId|SegmentBox|Keywords
/// </summary>
public sealed class SegmentDS : CsvRepositoryDataSource<SegmentM, SegmentR, SegmentLinkInfo> {
  private readonly CoreR _coreR;
  private readonly List<int> _drawerNotAvailable = [];

  public SegmentDS(CoreR coreR, SegmentR repo) : base(coreR.DB, "Segments", 5, repo) {
    _coreR = coreR;
    IsDriveRelated = true;
  }

  protected override Dictionary<string, IEnumerable<SegmentM>> _getAsDriveRelated() =>
    CoreR.GetAsDriveRelated(Repository.All, x => x.MediaItem.Folder);

  protected override (SegmentM item, SegmentLinkInfo linkInfo) _fromCsv(ReadOnlySpan<char> csv) {
    int start = 0;
    int field = 0;

    int id = 0;
    int mediaItemId = 0;
    int personId = 0;
    ReadOnlySpan<char> segmentBox = [];
    string keywordIds = string.Empty;

    for (int i = 0; i <= csv.Length; i++) {
      if (i == csv.Length || csv[i] == '|') {
        var slice = csv[start..i];

        switch (field) {
          case 0: id = CsvParser.ParseInt(slice); break;
          case 1: mediaItemId = CsvParser.ParseInt(slice); break;
          case 2: personId = CsvParser.ParseInt(slice); break;
          case 3: segmentBox = slice; break;
          case 4: keywordIds = slice.ToString(); break;
        }

        field++;
        start = i + 1;
      }
    }

    _validateFieldsCount(field, csv);

    var item = _createSegment(id, segmentBox);
    var linkInfo = new SegmentLinkInfo(mediaItemId, personId, keywordIds);

    return new(item, linkInfo);
  }

  private static SegmentM _createSegment(int id, ReadOnlySpan<char> csv) {
    int x = 0;
    int y = 0;
    int size = 0;

    int value = 0;
    int field = 0;

    for (int i = 0; i <= csv.Length; i++) {
      if (i == csv.Length || csv[i] == ',') {
        switch (field) {
          case 0: x = value; break;
          case 1: y = value; break;
          case 2: size = value; break;
        }

        value = 0;
        field++;
        continue;
      }

      value = value * 10 + (csv[i] - '0');
    }

    return new SegmentM(id, x, y, size, MediaItemR.Dummy);
  }

  protected override string _toCsv(SegmentM segment) =>
    string.Join("|",
      segment.GetHashCode().ToString(),
      segment.MediaItem.GetHashCode().ToString(),
      segment.Person == null
        ? "0"
        : segment.Person.GetHashCode().ToString(),
      string.Join(",", ((int)segment.X).ToString(), ((int)segment.Y).ToString(), ((int)segment.Size).ToString()),
      segment.Keywords.ToHashCodes().ToCsv());

  protected override Dictionary<string, string>? _propsToCsv() {
    Dictionary<string, string>? props = [];
    props.Add(nameof(SegmentVM.SegmentSize), SegmentVM.SegmentSize.ToString());
    props.Add("SegmentsDrawer", string.Join(",",
      Repository.Drawer
        .Select(x => x.GetHashCode())
        .Concat(_drawerNotAvailable)
        .Select(x => x.ToString())));

    return props;
  }

  public override void LinkReferences() {
    var withoutMediaItem = new List<SegmentM>();

    foreach (var (segment, li) in _allLinkInfo) {
      var mi = _coreR.MediaItem.DataSource.GetById(li.MediaItemId);
      if (mi != null) {
        segment.MediaItem = mi;
        mi.Segments ??= [];
        mi.Segments.Add(segment);

        if (li.PersonId != 0) {
          segment.Person = _coreR.Person.DataSource.GetPerson(li.PersonId, this);
          segment.Person.Segment ??= segment;
          segment.Person.Segments ??= [];
          segment.Person.Segments.Add(segment);
        }
      }
      else
        withoutMediaItem.Add(segment);

      // reference to Keywords
      segment.Keywords = _coreR.Keyword.DataSource.Link(li.KeywordIds, this);
    }

    // in case MediaItem was deleted
    foreach (var segment in withoutMediaItem)
      _ = AllDict.Remove(segment.GetHashCode());
  }

  public override void LinkProps() {
    if (_props == null) return;
    if (_props.TryGetValue(nameof(SegmentVM.SegmentSize), out var segmentSize))
      SegmentVM.SegmentSize = int.Parse(segmentSize);

    if (_props.TryGetValue("SegmentsDrawer", out var segmentsDrawerIds)) {
      Repository.Drawer.Clear();
      _drawerNotAvailable.Clear();

      CsvParser.ParseInts(segmentsDrawerIds, (Repository.Drawer, AllDict, _drawerNotAvailable), static (state, id) => {
        if (state.AllDict.TryGetValue(id, out var segment))
          state.Drawer.Add(segment);
        else
          state._drawerNotAvailable.Add(id);
      });
    }
  }
}

public readonly struct SegmentLinkInfo(int mediaItemId, int personId, string keywordIds) {
  public readonly int MediaItemId = mediaItemId;
  public readonly int PersonId = personId;
  public readonly string KeywordIds = keywordIds;
}