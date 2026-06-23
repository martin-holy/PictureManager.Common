using MH.Utils.DB;
using MH.Utils.DB.DataSources;
using PictureManager.Common.Features.Folder;
using System;
using System.Collections.Generic;

namespace PictureManager.Common.Features.FolderKeyword;

/// <summary>
/// DB fields: ID
/// </summary>
public sealed class FolderKeywordDS: CsvRepositoryDataSource<FolderM, FolderKeywordR, NoLinkInfo> {
  private readonly CoreR _coreR;

  public FolderKeywordDS(CoreR coreR, FolderKeywordR repo) : base(coreR.DB, "FolderKeywords", 1, repo) {
    _coreR = coreR;
    IsDriveRelated = true;
  }

  protected override Dictionary<string, IEnumerable<FolderM>> _getAsDriveRelated() =>
    CoreR.GetAsDriveRelated(Repository.All, x => x);

  protected override (FolderM item, NoLinkInfo linkInfo) _fromCsv(ReadOnlySpan<char> csv) =>
    (new(CsvParser.ParseInt(csv), string.Empty, null), default);

  protected override string _toCsv(FolderM folder) =>
    folder.GetHashCode().ToString();

  public override void LinkReferences() {
    foreach (var id in AllDict.Keys)
      AllDict[id] = _coreR.Folder.DataSource.GetById(id)!;
  }
}