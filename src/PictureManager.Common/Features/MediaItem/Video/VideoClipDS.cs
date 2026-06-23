using MH.Utils.DB;
using MH.Utils.DB.DataSources;
using MH.Utils.Extensions;
using System;
using System.Collections.Generic;

namespace PictureManager.Common.Features.MediaItem.Video;

/// <summary>
/// DB fields: ID|MediaItem|TimeStart|TimeEnd|Volume|Speed|Rating|Comment|People|Keywords
/// </summary>
public sealed class VideoClipDS : CsvRepositoryDataSource<VideoClipM, VideoClipR, VideoClipLinkInfo> {
  private readonly CoreR _coreR;

  public VideoClipDS(CoreR coreR, VideoClipR repo) : base(coreR.DB, "VideoClips", 10, repo) {
    _coreR = coreR;
    IsDriveRelated = true;
  }

  protected override Dictionary<string, IEnumerable<VideoClipM>> _getAsDriveRelated() =>
    CoreR.GetAsDriveRelated(Repository.All, x => x.Folder);

  protected override (VideoClipM item, VideoClipLinkInfo linkInfo) _fromCsv(ReadOnlySpan<char> csv) {
    int start = 0;
    int field = 0;

    int id = 0;
    int videoId = 0;
    int timeStart = 0;
    int timeEnd = 0;
    int volume = 50;
    int speed = 10;
    int rating = 0;
    string? comment = null;
    string personIds = string.Empty;
    string keywordIds = string.Empty;

    for (int i = 0; i <= csv.Length; i++) {
      if (i == csv.Length || csv[i] == '|') {
        var slice = csv[start..i];

        switch (field) {
          case 0: id = CsvParser.ParseInt(slice); break;
          case 1: videoId = CsvParser.ParseInt(slice); break;
          case 2: timeStart = CsvParser.ParseInt(slice); break;
          case 3: timeEnd = CsvParser.ParseInt(slice); break;
          case 4: volume = CsvParser.ParseInt(slice); break;
          case 5: speed = CsvParser.ParseInt(slice); break;
          case 6: rating = CsvParser.ParseInt(slice); break;
          case 7: comment = slice.IsEmpty ? null : slice.ToString(); break;
          case 8: personIds = slice.ToString(); break;
          case 9: keywordIds = slice.ToString(); break;
        }

        field++;
        start = i + 1;
      }
    }

    _validateFieldsCount(field, csv);


    var item = new VideoClipM(id, VideoR.Dummy, timeStart) {
      TimeEnd = timeEnd,
      Volume = volume / 100.0,
      Speed = speed / 10.0,
      Rating = rating,
      Comment = comment
    };

    var linkInfo = new VideoClipLinkInfo(videoId, personIds, keywordIds);

    return new(item, linkInfo);
  }

  protected override string _toCsv(VideoClipM vc) =>
    string.Join("|",
      vc.GetHashCode().ToString(),
      vc.Video.GetHashCode().ToString(),
      vc.TimeStart.ToString(),
      vc.TimeEnd.ToString(),
      ((int)(vc.Volume * 100)).ToString(),
      ((int)(vc.Speed * 10)).ToString(),
      vc.Rating.ToString(),
      vc.Comment ?? string.Empty,
      vc.People.ToHashCodes().ToCsv(),
      vc.Keywords.ToHashCodes().ToCsv());

  public override void LinkReferences() {
    foreach (var (vc, li) in _allLinkInfo) {
      vc.Video = _coreR.Video.DataSource.GetById(li.VideoId)!;
      vc.Video.VideoClips ??= [];
      vc.Video.VideoClips.Add(vc);
      vc.People = _coreR.Person.DataSource.Link(li.PersonIds, this);
      vc.Keywords = _coreR.Keyword.DataSource.Link(li.KeywordIds, this);
    }
  }
}

public readonly struct VideoClipLinkInfo(int videoId, string personIds, string keywordIds) {
  public readonly int VideoId = videoId;
  public readonly string PersonIds = personIds;
  public readonly string KeywordIds = keywordIds;
}