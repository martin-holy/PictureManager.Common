using MH.Utils.DB;
using MH.Utils.DB.DataSources;
using System;
using System.Collections.Generic;

namespace PictureManager.Common.Features.MediaItem;

public sealed class MediaItemDS(CoreR coreR, MediaItemR repository)
  : CsvRepositoryDataSource<MediaItemM, MediaItemR, NoLinkInfo>(coreR.DB, string.Empty, 0, repository) {

  private readonly CoreR _coreR = coreR;

  public override MediaItemM? GetById(int id, bool nullable = false) {
    if (_coreR.Image.DataSource.AllDict.TryGetValue(id, out var img)) return img;
    if (_coreR.Video.DataSource.AllDict.TryGetValue(id, out var vid)) return vid;
    if (_coreR.VideoClip.DataSource.AllDict.TryGetValue(id, out var vc)) return vc;
    if (_coreR.VideoImage.DataSource.AllDict.TryGetValue(id, out var vi)) return vi;
    return null;
  }

  public List<MediaItemM>? Link(ReadOnlySpan<char> csv) {
    if (csv.IsEmpty) return null;

    List<MediaItemM> items = [];

    CsvParser.ParseInts(csv, (items, this), static (state, id) => {
      if (state.Item2.GetById(id) is { } item)
        state.items.Add(item);
    });

    return items.Count == 0 ? null : items;
  }
}