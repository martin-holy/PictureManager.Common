using MH.Utils;
using MH.Utils.Imaging;
using System;
using System.IO;
using System.Linq;

namespace PictureManager.Common.Features.MediaItem.Video;

public sealed class VideoS(VideoR r) {
  public bool TryWriteMetadata(VideoM vid) {
    try {
      if (_writeMetadata(vid))
        vid.IsOnlyInDb = false;
    }
    catch (Exception ex) {
      Log.Error(ex, $"Metadata will be saved just in database. {vid.FilePath}");
      vid.IsOnlyInDb = true;
    }

    r.IsModified = true;
    return !vid.IsOnlyInDb;
  }

  private static bool _writeMetadata(VideoM vid) {
    var filePath = vid.FilePath;
    using var metadata = new VideoMetadata(filePath);

    _writeMetadata(vid, metadata);

    if (!metadata.IsModified) return true;

    var fi = new FileInfo(filePath);
    var originalCreationTime = fi.CreationTime;

    var writePlan = metadata.CreateWritePlan();
    var fullRewriteThreshold = Core.Settings.MediaItem.VideoMetadataFullRewriteThreshold * 1024 * 1024;

    if (writePlan.IsFullRewriteRequired && fi.Length > fullRewriteThreshold)
      return false;

    metadata.Write(writePlan);

    MediaItemS.TryRestoreCreationTime(filePath, originalCreationTime);
    MediaItemS.RaiseMetadataWritten(vid);

    return true;
  }

  private static void _writeMetadata(VideoM vid, VideoMetadata metadata) {
    MediaItemS.WritePeople(metadata.IsoBmff.Xmp, vid);
    metadata.Rating = vid.Rating;
    metadata.Comment = vid.Comment;
    metadata.Keywords = vid.Keywords?.Select(k => k.FullName).ToArray();
    MediaItemS.WriteGeoNameId(metadata.IsoBmff.Xmp, vid);
  }
}