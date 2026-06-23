using MH.Utils;
using MH.Utils.DB;
using MH.Utils.DB.DataSources;
using MH.Utils.Extensions;
using PictureManager.Common.Features.Folder;
using System;
using System.Collections.Generic;

namespace PictureManager.Common.Features.MediaItem.Image;

/// <summary>
/// DB fields: ID|Folder|FileName|Width|Height|Orientation|Rating|Comment|People|Keywords|IsOnlyInDb
/// </summary>
public sealed class ImageDS : CsvRepositoryDataSource<ImageM, ImageR, ImageLinkInfo> {
  private readonly CoreR _coreR;

  public ImageDS(CoreR coreR, ImageR repo) : base(coreR.DB, "Images", 11, repo) {
    _coreR = coreR;
    IsDriveRelated = true;
  }

  protected override Dictionary<string, IEnumerable<ImageM>> _getAsDriveRelated() =>
    CoreR.GetAsDriveRelated(Repository.All, x => x.Folder);

  protected override (ImageM item, ImageLinkInfo linkInfo) _fromCsv(ReadOnlySpan<char> csv) {
    int start = 0;
    int field = 0;

    int id = 0;
    int folderId = 0;
    string fileName = string.Empty;
    int width = 0;
    int height = 0;
    int orientation = 1;
    int rating = 0;
    string? comment = null;
    string personIds = string.Empty;
    string keywordIds = string.Empty;
    bool isOnlyInDb = false;

    for (int i = 0; i <= csv.Length; i++) {
      if (i == csv.Length || csv[i] == '|') {
        var slice = csv[start..i];

        switch (field) {
          case 0: id = CsvParser.ParseInt(slice); break;
          case 1: folderId = CsvParser.ParseInt(slice); break;
          case 2: fileName = slice.ToString(); break;
          case 3: width = CsvParser.ParseIntOrDefault(slice, 0); break;
          case 4: height = CsvParser.ParseIntOrDefault(slice, 0); break;
          case 5: orientation = CsvParser.ParseIntOrDefault(slice, 1); break;
          case 6: rating = CsvParser.ParseIntOrDefault(slice, 0); break;
          case 7: comment = slice.Length == 0 ? null : slice.ToString(); break;
          case 8: personIds = slice.ToString(); break;
          case 9: keywordIds = slice.ToString(); break;
          case 10: isOnlyInDb = slice.Length == 1 && slice[0] == '1'; break;
        }

        field++;
        start = i + 1;
      }
    }

    _validateFieldsCount(field, csv);

    var item = new ImageM(id, FolderR.Dummy, fileName) {
      Width = width,
      Height = height,
      Orientation = (Imaging.Orientation)orientation,
      Rating = rating,
      Comment = comment,
      IsOnlyInDb = isOnlyInDb
    };

    var linkInfo = new ImageLinkInfo(folderId, personIds, keywordIds);

    return new(item, linkInfo);
  }

  protected override string _toCsv(ImageM img) =>
    string.Join("|",
      img.GetHashCode().ToString(),
      img.Folder.GetHashCode().ToString(),
      img.FileName,
      img.Width.ToString(),
      img.Height.ToString(),
      img.Orientation.ToInt().ToString(),
      img.Rating.ToString(),
      img.Comment ?? string.Empty,
      img.People.ToHashCodes().ToCsv(),
      img.Keywords.ToHashCodes().ToCsv(),
      img.IsOnlyInDb ? "1" : string.Empty);

  public override void LinkReferences() {
    foreach (var (mi, li) in _allLinkInfo) {
      mi.Folder = _coreR.Folder.DataSource.GetById(li.FolderId)!;
      mi.Folder.MediaItems.Add(mi);
      mi.People = _coreR.Person.DataSource.Link(li.PersonIds, this);
      mi.Keywords = _coreR.Keyword.DataSource.Link(li.KeywordIds, this);
    }
  }
}

public readonly struct ImageLinkInfo(int folderId, string personIds, string keywordIds) {
  public readonly int FolderId = folderId;
  public readonly string PersonIds = personIds;
  public readonly string KeywordIds = keywordIds;
}