using MH.Utils;
using MH.Utils.Extensions;
using MH.Utils.Imaging;
using MH.Utils.Imaging.Exif;
using MH.Utils.Imaging.Jpeg;
using MH.Utils.Imaging.Xmp;
using PictureManager.Common.Features.Person;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace PictureManager.Common.Features.MediaItem.Image;

public sealed class ImageS(ImageR r) {
  private static readonly XNamespace _nsPM = "https://github.com/martin-holy/PictureManager/";
  private static readonly XNamespace _nsGeoNames = "GeoNames"; // old ns used before
  private static readonly XName _nCompressionQuality = _nsPM + "CompressionQuality";

  private static bool _customXmpNsPrefixAdded;

  public static event EventHandler<MediaItemM>? OnMetadataWrittenEvent;

  public bool TryEncodeJpeg(ImageM img, int quality) {
    try {
      var filePath = img.FilePath;
      var metadata = new ImageMetadata(filePath, JpegMetadataLoad.Xmp);
      if (metadata.Jpeg.Xmp.Doc?.GetInt(_nCompressionQuality) == quality) return true;

      using var encoded = new MemoryStream();
      ImagingU.EncodeJpegTo(encoded, filePath, quality);

      encoded.Position = 0;
      metadata = new ImageMetadata(encoded, JpegMetadataLoad.All);
      _ensureCustomXmpNamespacePrefix();
      metadata.Jpeg.Xmp.EnsureDoc().SetProperty(_nCompressionQuality, quality.ToString(), XmpValueStyle.Attribute);

      _writeMetadata(img, metadata);

      var originalCreationTime = new FileInfo(filePath).CreationTime;

      encoded.Position = 0;
      if (!metadata.Write(encoded, filePath)) throw new("Error writing metadata");

      _tryRestoreCreationTime(filePath, originalCreationTime);

      img.IsOnlyInDb = false;
    }
    catch (Exception ex) {
      Log.Error(ex, $"Metadata will be saved just in database. {img.FilePath}");
      img.IsOnlyInDb = true;
    }

    r.IsModified = true;
    return !img.IsOnlyInDb;
  }

  public bool TryWriteMetadata(ImageM img) {
    try {
      if (!_writeMetadata(img)) throw new("Error writing metadata");
      img.IsOnlyInDb = false;
    }
    catch (Exception ex) {
      Log.Error(ex, $"Metadata will be saved just in database. {img.FilePath}");
      img.IsOnlyInDb = true;
    }

    r.IsModified = true;
    return !img.IsOnlyInDb;
  }

  private static bool _writeMetadata(ImageM img) {
    var filePath = img.FilePath;
    var metadata = new ImageMetadata(filePath, JpegMetadataLoad.All);

    _writeMetadata(img, metadata);

    if (!metadata.IsModified) return true;

    var originalCreationTime = new FileInfo(filePath).CreationTime;

    var success = metadata.Write(filePath);

    _tryRestoreCreationTime(filePath, originalCreationTime);

    OnMetadataWrittenEvent?.Invoke(null, img);

    return success;
  }

  private static void _ensureCustomXmpNamespacePrefix() {
    if (_customXmpNsPrefixAdded) return;
    XmpNs.SetPrefix(_nsPM, "PM");
    _customXmpNsPrefixAdded = true;
  }

  private static void _tryRestoreCreationTime(string filePath, DateTime creationTime) {
    try {
      // BUG System.UnauthorizedAccessException: Access to the path '...' is denied. Operation not permitted
      if (OperatingSystem.IsWindows())
        new FileInfo(filePath).CreationTime = creationTime;
    }
    catch (Exception ex) {
      Log.Error(ex, "Can't preserve file original creation time.");
    }
  }

  private static void _writeMetadata(ImageM img, ImageMetadata metadata) {
    _writePeople(metadata, img);
    metadata.Rating = img.Rating;
    metadata.Comment = img.Comment;
    metadata.Keywords = img.Keywords?.Select(k => k.FullName).ToArray();
    metadata.Orientation = img.Orientation.ToExifOrientation();

    _ensureCustomXmpNamespacePrefix();
    var doc = metadata.Jpeg.Xmp.EnsureDoc();
    doc.SetProperty(_nsGeoNames + "GeoNameId", null); // remove old location
    doc.SetProperty(_nsPM + "GeoNameId", img.GeoLocation?.GeoName?.Id.ToString(), XmpValueStyle.Attribute);
  }

  private static void _writePeople(ImageMetadata metadata, ImageM img) {
    var people = _getPeopleSegmentsKeywords(img);
    var existing = metadata.People;

    if (people == null) {
      existing?.Clear();
      return;
    }

    var used = new List<XElement>();
    existing ??= metadata.Jpeg.Xmp.EnsurePeople();

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

  private static List<Tuple<PersonM?, string?, string[]?>>? _getPeopleSegmentsKeywords(ImageM img) {
    var people = img.People;
    var segments = img.Segments;

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

  public static int? GetGeoNameId(ImageMetadata metadata) =>
    metadata.Jpeg.Xmp.Doc is not { } doc
      ? null
      : doc.GetInt(_nsPM + "GeoNameId") ??
        doc.GetInt(_nsGeoNames + "GeoNameId"); // this is old namespace I used before

  public static void ResizeJpeg(ImageM img, string dest, int px, bool withMetadata, bool withThumbnail, int quality) {
    var metadata = new ImageMetadata(img.FilePath, JpegMetadataLoad.Size);
    var dateTaken = _getDateTaken(img.FileName, metadata);

    ImagingU.GetScaledSizeToPx(px, metadata.Width, metadata.Height, out var width, out var height);

    using var encoded = new MemoryStream();
    ImagingU.EncodeJpegTo(encoded, img.FilePath, quality, withMetadata, withThumbnail, width, height);

    encoded.Position = 0;
    metadata = new ImageMetadata(encoded, JpegMetadataLoad.All);
    metadata.UpdateDimensions();
    _ensureCustomXmpNamespacePrefix();
    metadata.Jpeg.Xmp.EnsureDoc().SetProperty(_nCompressionQuality, quality.ToString(), XmpValueStyle.Attribute);

    if (withMetadata)
      _writeMetadata(img, metadata);
    else
      metadata.Orientation = img.Orientation.ToExifOrientation();

    encoded.Position = 0;
    metadata.Write(encoded, dest);

    if (dateTaken != null) {
      try {
        new FileInfo(img.FilePath).LastWriteTime = dateTaken.Value;
      }
      catch (Exception ex) {
        Log.Error(ex, "Can't update LastWriteTime to DateTaken.");
      }
    }
  }

  private static DateTime? _getDateTaken(string fileName, ImageMetadata metadata) {
    var date = DateTime.MinValue;

    var match = Regex.Match(fileName, "[0-9]{8}_[0-9]{6}");
    if (match.Success)
      DateTime.TryParseExact(match.Value, "yyyyMMdd_HHmmss", CultureInfo.InvariantCulture, DateTimeStyles.None, out date);

    if (date != DateTime.MinValue) return date;

    return metadata.DateTimeOriginal;
  }
}