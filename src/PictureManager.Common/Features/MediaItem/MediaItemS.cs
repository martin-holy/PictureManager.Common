using MH.Utils;
using MH.Utils.BaseClasses;
using MH.Utils.Imaging;
using MH.Utils.Imaging.Exif;
using MH.Utils.Imaging.Jpeg;
using MH.Utils.Imaging.Xmp;
using PictureManager.Common.Features.Folder;
using PictureManager.Common.Features.MediaItem.Image;
using PictureManager.Common.Features.MediaItem.Video;
using PictureManager.Common.Features.Person;
using PictureManager.Common.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace PictureManager.Common.Features.MediaItem;

public sealed class MediaItemS(MediaItemR r) : ObservableObject {
  public static readonly XNamespace NsPM = "https://github.com/martin-holy/PictureManager/";

  private static readonly XNamespace _nsGeoNames = "GeoNames"; // old ns used before
  private static bool _customXmpNsPrefixAdded;

  public static event EventHandler<MediaItemM>? OnMetadataWrittenEvent;

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
    ReadMetadata(mim);

    return Tasks.RunOnUiThread(async () => {
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

  public static void ReadMetadata(MediaItemMetadata mim) {
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
    var filePath = mim.MediaItem.FilePath;
    using var stream = File.OpenRead(filePath);
    var metadata = new VideoMetadata(stream);

    mim.Width = metadata.Width;
    mim.Height = metadata.Height;

    // only dimensions are required
    try {
      mim.Orientation = metadata.Orientation switch {
        90 => Orientation.Rotate90,
        180 => Orientation.Rotate180,
        270 => Orientation.Rotate270,
        _ => Orientation.Normal,
      };

      mim.Rating = metadata.Rating ?? 0;
      mim.Comment = StringUtils.NormalizeComment(metadata.Comment);
      mim.Keywords = metadata.Keywords;
      mim.PeopleSegmentsKeywords = _readPeopleSegmentsKeywords(metadata.People);
      mim.GeoNameId = GetGeoNameId(metadata.IsoBmff.Xmp);

      // TODO implement GPS reading for video files
      /*if (metadata.GpsCoordinate is { } gps) {
        mim.Lat = gps.Latitude;
        mim.Lng = gps.Longitude;
      }*/
    }
    catch (Exception ex) {
      Log.Error(ex, $"Can't read video metadata. ({filePath})");
    }

    mim.Success = true;
  }

  private static void _readImageMetadata(MediaItemMetadata mim) {
    var metadata = new ImageMetadata(mim.MediaItem.FilePath, JpegMetadataLoad.Size);

    mim.Width = metadata.Width;
    mim.Height = metadata.Height;

    // only dimensions are required
    try {
      mim.Rating = metadata.Rating ?? 0;
      mim.Comment = StringUtils.NormalizeComment(metadata.Comment);
      mim.Orientation = metadata.Orientation.ToMsOrientation() ?? Orientation.Normal;
      mim.Keywords = metadata.Keywords;
      mim.PeopleSegmentsKeywords = _readPeopleSegmentsKeywords(metadata.People);
      mim.GeoNameId = GetGeoNameId(metadata.Jpeg.Xmp);

      if (metadata.GpsCoordinate is { } gps) {
        mim.Lat = gps.Latitude;
        mim.Lng = gps.Longitude;
      }
    }
    catch (Exception ex) {
      Log.Error(ex, mim.MediaItem.FilePath);
    }

    mim.Success = true;
  }

  private static List<Tuple<string?, List<Tuple<string, string[]?>>>>? _readPeopleSegmentsKeywords(MpRegionCollection? people) {
    if (people == null || people.Count == 0) return null;

    var output = new List<Tuple<string?, List<Tuple<string, string[]?>>>>();

    foreach (var region in people) {
      var name = region.PersonDisplayName;
      var rect = region.Rectangle;

      if (string.IsNullOrEmpty(name) && string.IsNullOrEmpty(rect))
        continue;

      var person = string.IsNullOrEmpty(name)
        ? null
        : output.FirstOrDefault(x => string.Equals(x.Item1, name, StringComparison.OrdinalIgnoreCase));

      if (person == null) {
        person = new(name, []);
        output.Add(person);
      }

      if (string.IsNullOrEmpty(rect)) continue;

      var keywords = region.Element
        .GetXmpArray(XmpNs.MpReg + "RectangleKeywords")?
        .Select(e => e.Value.Trim())
        .Where(v => v.Length > 0)
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .ToArray();

      person.Item2.Add(new(rect, keywords?.Length > 0 ? keywords : null));
    }

    return output.Count > 0 ? output : null;
  }

  public static void WritePeople(XmpMetadata xmp, MediaItemM mi) {
    var people = _getPeopleSegmentsKeywords(mi);
    var existing = xmp.GetPeople();

    if (people == null) {
      existing?.Clear();
      return;
    }

    var used = new List<XElement>();
    existing ??= xmp.EnsurePeople();

    foreach (var (person, rect, keywords) in people) {
      MpRegion? region = null;
      var name = person?.Name;

      // Named region → match by PersonDisplayName
      if (!string.IsNullOrWhiteSpace(name))
        region = existing
          .Where(x => x.PersonDisplayName == name && !used.Contains(x.Element))
          .Select(x => x)
          .FirstOrDefault();

      // Anonymous region → match by Rectangle
      if (region == null && name == null && rect != null)
        region = existing
          .Where(x => x.PersonDisplayName == null && x.Rectangle == rect && !used.Contains(x.Element))
          .Select(x => x)
          .FirstOrDefault();

      region ??= existing.Add(name);
      region.Rectangle = rect;
      region.Element.SetXmpArray(XmpNs.MpReg + "RectangleKeywords", keywords, XmpArrayType.Bag);
      used.Add(region.Element);
    }

    foreach (var eRegion in existing.Where(x => !used.Contains(x.Element)).ToArray())
      existing.Remove(eRegion);
  }

  private static List<Tuple<PersonM?, string?, string[]?>>? _getPeopleSegmentsKeywords(MediaItemM mi) {
    var people = mi.People;
    var segments = mi.Segments;

    if (people == null && segments == null) return null;

    var output = new List<Tuple<PersonM?, string?, string[]?>>();
    var set = new HashSet<PersonM>();

    if (segments != null)
      foreach (var segment in segments) {
        if (segment.Person != null)
          set.Add(segment.Person);

        output.Add(new(
          segment.Person,
          segment.ToMsRect(),
          segment.Keywords?.Select(k => k.FullName).ToArray()));
      }

    if (people != null)
      foreach (var person in people)
        if (!set.Contains(person))
          output.Add(new(person, null, null));

    return output;
  }

  public static void EnsureCustomXmpNamespacePrefix() {
    if (_customXmpNsPrefixAdded) return;
    XmpNs.SetPrefix(NsPM, "PM");
    _customXmpNsPrefixAdded = true;
  }

  public static void TryRestoreCreationTime(string filePath, DateTime creationTime) {
    try {
      // BUG System.UnauthorizedAccessException: Access to the path '...' is denied. Operation not permitted
      if (OperatingSystem.IsWindows())
        new FileInfo(filePath).CreationTime = creationTime;
    }
    catch (Exception ex) {
      Log.Error(ex, "Can't preserve file original creation time.");
    }
  }

  public static int? GetGeoNameId(XmpMetadata metadata) =>
    metadata.Doc is not { } doc
      ? null
      : doc.GetInt(NsPM + "GeoNameId") ??
        doc.GetInt(_nsGeoNames + "GeoNameId"); // this is old namespace I used before

  public static void WriteGeoNameId(XmpMetadata xmp, MediaItemM mi) {
    EnsureCustomXmpNamespacePrefix();
    var doc = xmp.EnsureDoc();
    doc.SetProperty(_nsGeoNames + "GeoNameId", null); // remove old location
    doc.SetProperty(NsPM + "GeoNameId", mi.GeoLocation?.GeoName?.Id.ToString(), XmpValueStyle.Attribute);
  }

  public static void RaiseMetadataWritten(MediaItemM mi) =>
    OnMetadataWrittenEvent?.Invoke(null, mi);
}