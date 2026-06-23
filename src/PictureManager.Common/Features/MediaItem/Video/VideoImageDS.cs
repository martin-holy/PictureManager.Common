using MH.Utils.DB;
using MH.Utils.DB.DataSources;
using MH.Utils.Extensions;
using System;
using System.Collections.Generic;

namespace PictureManager.Common.Features.MediaItem.Video;

/// <summary>
/// DB fields: ID|MediaItem|Time|Rating|Comment|People|Keywords
/// </summary>
public sealed class VideoImageDS : CsvRepositoryDataSource<VideoImageM, VideoImageR, VideoImageLinkInfo> {
  private readonly CoreR _coreR;

  public VideoImageDS(CoreR coreR, VideoImageR repo) : base(coreR.DB, "VideoImages", 7, repo) {
    IsDriveRelated = true;
    _coreR = coreR;
  }

  protected override Dictionary<string, IEnumerable<VideoImageM>> _getAsDriveRelated() =>
    CoreR.GetAsDriveRelated(Repository.All, x => x.Folder);

  protected override (VideoImageM item, VideoImageLinkInfo linkInfo) _fromCsv(ReadOnlySpan<char> csv) {
    int start = 0;
    int field = 0;

    int id = 0;
    int videoId = 0;
    int time = 0;
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
          case 2: time = CsvParser.ParseInt(slice); break;
          case 3: rating = CsvParser.ParseInt(slice); break;
          case 4: comment = slice.IsEmpty ? null : slice.ToString(); break;
          case 5: personIds = slice.ToString(); break;
          case 6: keywordIds = slice.ToString(); break;
        }

        field++;
        start = i + 1;
      }
    }

    _validateFieldsCount(field, csv);


    var item = new VideoImageM(id, VideoR.Dummy, time) {
      Rating = rating,
      Comment = comment
    };

    var linkInfo = new VideoImageLinkInfo(videoId, personIds, keywordIds);

    return new(item, linkInfo);
  }

  protected override string _toCsv(VideoImageM vi) =>
    string.Join("|",
      vi.GetHashCode().ToString(),
      vi.Video.GetHashCode().ToString(),
      vi.TimeStart.ToString(),
      vi.Rating.ToString(),
      vi.Comment ?? string.Empty,
      vi.People.ToHashCodes().ToCsv(),
      vi.Keywords.ToHashCodes().ToCsv());

  public override void LinkReferences() {
    foreach (var (vi, li) in _allLinkInfo) {
      vi.Video = _coreR.Video.DataSource.GetById(li.VideoId)!;
      vi.Video.VideoImages ??= [];
      vi.Video.VideoImages.Add(vi);
      vi.People = _coreR.Person.DataSource.Link(li.PersonIds, this);
      vi.Keywords = _coreR.Keyword.DataSource.Link(li.KeywordIds, this);
    }
  }
}

public readonly struct VideoImageLinkInfo(int videoId, string personIds, string keywordIds) {
  public readonly int VideoId = videoId;
  public readonly string PersonIds = personIds;
  public readonly string KeywordIds = keywordIds;
}