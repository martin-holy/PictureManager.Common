using MH.Utils;
using MH.Utils.BaseClasses;
using MH.Utils.Imaging;
using MH.Utils.Imaging.Exif;
using MH.Utils.Imaging.Jpeg;
using MH.Utils.Imaging.Xmp;
using PictureManager.Common.Features.Folder;
using PictureManager.Common.Features.MediaItem.Image;
using PictureManager.Common.Features.MediaItem.Video;
using PictureManager.Common.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PictureManager.Common.Features.MediaItem;

public sealed class MediaItemS(MediaItemR r) : ObservableObject {
  public static Action<MediaItemMetadata, bool> ReadMetadata { get; set; } = null!;
  public static Func<string, string, object[]?> GetVideoMetadata { get; set; } = null!;

  public void DeleteFromDrive(MediaItemM[] items) =>
    r.ItemsDeleteFromDrive(items);

  public bool Exists(MediaItemM? mi) {
    if (mi == null || File.Exists(mi.FilePath)) return true;
    r.ItemsDelete(new[] { mi });
    return false;
  }

  public void OnMetadataReloaded(RealMediaItemM[] items) {
    r.RaiseMetadataChanged(items.Cast<MediaItemM>().ToArray());
    r.RaiseOrientationChanged(items);
  }

  public Task ReloadMetadata(RealMediaItemM mi) {
    var mim = new MediaItemMetadata(mi);
    if (mi is not VideoM) ReadMetadata(mim, false);

    return Tasks.RunOnUiThread(async () => {
      if (mi is VideoM) ReadMetadata(mim, false);
      if (mim.Success) await mim.FindRefs();
      r.Modify(mi);
      mi.IsOnlyInDb = false;
    });
  }

  public void Rename(RealMediaItemM mi, string newFileName) =>
    r.ItemRename(mi, newFileName);

  public void SetComment(MediaItemM mi, string? comment) {
    mi.Comment = comment;
    mi.SetInfoBox(true);
    mi.OnPropertyChanged(nameof(mi.Comment));
    r.Modify(mi);
  }

  public async Task<MediaItemM?> GetMediaItem(FolderM folder, string fileName) {
    var mi = folder.MediaItems.SingleOrDefault(x => x.FileName.Equals(fileName, StringComparison.Ordinal));
    return mi != null ? mi : await CopyMoveU.CreateMediaItemAndReadMetadata(folder, fileName);
  }

  public static void CreateImageThumbnail(MediaItemM mi) =>
    Core.P.CreateImageThumbnail(
      mi.FilePath,
      mi.FilePathCache,
      Core.Settings.MediaItem.ThumbSize,
      Core.Settings.Common.JpegQuality);

  // TODO remove the gpsOnly param later
  public static void ReadMetadata2(MediaItemMetadata mim, bool gpsOnly = false) {
    mim.Success = false;
    try {
      switch (mim.MediaItem) {
        case VideoM: _readVideoMetadata(mim); break;
        case ImageM: _readImageMetadata(mim); break;
      }
    }
    catch (Exception ex) {
      Log.Error(ex, mim.MediaItem.FilePath);
    }
  }

  private static void _readVideoMetadata(MediaItemMetadata mim) {
    if (GetVideoMetadata(mim.MediaItem.Folder.FullPath, mim.MediaItem.FileName) is not { } data) {
      mim.Success = false;
      Log.Error("Can't read video metadata", mim.MediaItem.FilePath);
      return;
    }

    mim.Height = (int)data[0];
    mim.Width = (int)data[1];
    mim.Orientation = (int)data[2] switch {
      90 => Orientation.Rotate90,
      180 => Orientation.Rotate180,
      270 => Orientation.Rotate270,
      _ => Orientation.Normal,
    };

    mim.Success = true;
  }

  private static void _readImageMetadata(MediaItemMetadata mim) {
    var metadata = new ImageMetadata(mim.MediaItem.FilePath, JpegMetadataLoad.All);

    mim.Width = metadata.Width;
    mim.Height = metadata.Height;
    mim.Rating = metadata.Rating ?? 0;
    mim.Comment = StringUtils.NormalizeComment(metadata.Comment);
    mim.Orientation = metadata.Orientation.ToMsOrientation() ?? Orientation.Normal;
    mim.Keywords = metadata.Keywords;
    mim.PeopleSegmentsKeywords = _readPeopleSegmentsKeywords(metadata.People);
    mim.GeoNameId = ImageS.GetGeoNameId(metadata);

    if (metadata.GpsCoordinate is { } gps) {
      mim.Lat = gps.Latitude;
      mim.Lng = gps.Longitude;
    }

    mim.Success = true;
  }

  private static List<Tuple<string, List<Tuple<string, string[]?>>>>? _readPeopleSegmentsKeywords(MpRegionCollection? people) {
    if (people == null || people.Count == 0) return null;

    var output = new List<Tuple<string, List<Tuple<string, string[]?>>>>();

    foreach (var region in people) {
      var name = region.PersonDisplayName;
      var rect = region.Rectangle;

      if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(rect))
        continue;

      var keywords = region.Element
        .GetXmpArray(XmpNs.MpReg + "RectangleKeywords")?
        .Select(e => e.Value.Trim())
        .Where(v => v.Length > 0)
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .ToArray();

      var person = output.FirstOrDefault(x =>
        string.Equals(x.Item1, name, StringComparison.OrdinalIgnoreCase));

      if (person == null) {
        person = new(name, []);
        output.Add(person);
      }

      person.Item2.Add(new(rect, keywords?.Length > 0 ? keywords : null));
    }

    return output.Count > 0 ? output : null;
  }
}