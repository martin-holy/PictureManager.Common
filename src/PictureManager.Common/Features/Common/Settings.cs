using MH.UI.BaseClasses;
using MH.UI.Controls;
using MH.Utils;
using MH.Utils.BaseClasses;
using MH.Utils.Localization;
using PictureManager.Common.Localization;
using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace PictureManager.Common.Features.Common;

public sealed class Settings : UserSettings {
  internal static class _loc {
    private const string _prefix = "Settings.";
    public const string CommonTitle = _prefix + "Common.Title";
    public const string GeoNameTitle = _prefix + "GeoName.Title";
    public const string MediaItemTitle = _prefix + "MediaItem.Title";
    public const string SegmentTitle = _prefix + "Segment.Title";
    public const string MediaViewerTitle = _prefix + "MediaViewer.Title";

    static _loc() {
      LocalizationManager.Register(typeof(_loc).Assembly, [
        (CommonTitle, "Common"),
        (GeoNameTitle, "GeoName"),
        (MediaItemTitle, "MediaItem"),
        (SegmentTitle, "Segment"),
        (MediaViewerTitle, "MediaViewer")
      ]);
    }

    public static void Register() { }
  }

  static Settings() { _loc.Register(); }

  public CommonSettings Common { get; }
  public GeoNameSettings GeoName { get; }
  public ImagesToVideoSettings ImagesToVideo { get; }
  public MediaItemSettings MediaItem { get; }
  public SegmentSettings Segment { get; }
  public MediaViewerSettings MediaViewer { get; }

  public Settings(string filePath, CommonSettings common, GeoNameSettings geoName, ImagesToVideoSettings imagesToVideo, MediaItemSettings mediaItem, SegmentSettings segment, MediaViewerSettings mediaViewer) : base(filePath) {
    Common = common;
    GeoName = geoName;
    ImagesToVideo = imagesToVideo;
    MediaItem = mediaItem;
    Segment = segment;
    MediaViewer = mediaViewer;
  }

  public void Localize() {
    Groups = [
      new(Res.IconSettings, L.Get(_loc.CommonTitle), Common),
      new(Res.IconLocationCheckin, L.Get(_loc.GeoNameTitle), GeoName),
      new(Res.IconImageMultiple, L.Get(_loc.MediaItemTitle), MediaItem),
      new(Res.IconSegment, L.Get(_loc.SegmentTitle), Segment),
      new(Res.IconImageMultiple, L.Get(_loc.MediaItemTitle), MediaViewer)
    ];

    _watchForChanges();
    Common._localize();
    GeoName._localize();
    MediaItem._localize();
    Segment._localize();
    MediaViewer._localize();
  }

  public static Settings Load(string filePath) {
    if (!File.Exists(filePath)) return _createNew(filePath);
    try {
      using var doc = JsonDocument.Parse(File.ReadAllText(filePath));
      var root = doc.RootElement;
      var ctx = SettingsJsonContext.Default;
      var common = _deserializeGroup(root, "Common", ctx.CommonSettings) ?? new();
      var geoName = _deserializeGroup(root, "GeoName", ctx.GeoNameSettings) ?? new();
      var imagesToVideo = _deserializeGroup(root, "ImagesToVideo", ctx.ImagesToVideoSettings) ?? new();
      var mediaItem = _deserializeGroup(root, "MediaItem", ctx.MediaItemSettings) ?? new();
      var segment = _deserializeGroup(root, "Segment", ctx.SegmentSettings) ?? new();
      var mediaViewer = _deserializeGroup(root, "MediaViewer", ctx.MediaViewerSettings) ?? new();

      return new Settings(filePath, common, geoName, imagesToVideo, mediaItem, segment, mediaViewer);
    }
    catch (Exception ex) {
      Log.Error(ex);
      return _createNew(filePath);
    }
  }

  private static T? _deserializeGroup<T>(JsonElement root, string name, JsonTypeInfo<T> typeInfo) {
    if (root.TryGetProperty(name, out var elm))
      return JsonSerializer.Deserialize(elm.GetRawText(), typeInfo);
    return default;
  }

  private static Settings _createNew(string filePath) =>
    new(filePath, new(), new(), new(), new(), new(), new());

  protected override string Serialize() =>
    JsonSerializer.Serialize(this, SettingsJsonContext.Default.Settings);
}

public sealed class CommonSettings : ObservableObject {
  internal static class _loc {
    private const string _prefix = "App.Settings.Common.";
    public const string TxtCachePath = _prefix + "txt.CachePath";
    public const string TxtFfmpegPath = _prefix + "txt.FfmpegPath";
    public const string TxtJpegQuality = _prefix + "txt.JpegQuality";
    public const string TxtLanguages = _prefix + "txt.Languages";

    static _loc() {
      LocalizationManager.Register(typeof(_loc).Assembly, [
        (TxtCachePath, "Cache path:"),
        (TxtFfmpegPath, "FFMPEG path:"),
        (TxtJpegQuality, "Jpeg quality: {0}"),
        (TxtLanguages, "Languages:")
      ]);
    }

    public static void Register() { }
  }

  static CommonSettings() { _loc.Register();  }

  private string _cachePath = @":\Temp\PictureManagerCache";
  private string[] _directorySelectFolders = [];
  private string _ffmpegPath = string.Empty;
  private int _jpegQuality = 80;
  private string _languages = "en";
  
  public string CachePath { get => _cachePath; set => _onCachePathChange(value); }
  public string[] DirectorySelectFolders { get => _directorySelectFolders; set { _directorySelectFolders = value; OnPropertyChanged(); } }
  public string FfmpegPath { get => _ffmpegPath; set { _ffmpegPath = value; OnPropertyChanged(); } }
  public int JpegQuality { get => _jpegQuality; set { _jpegQuality = value; OnPropertyChanged(); } }
  public string Languages { get => _languages; set { _languages = value; OnPropertyChanged(); } }

  [JsonIgnore]
  public string TxtCachePath { get; private set; } = _loc.TxtCachePath;
  [JsonIgnore]
  public string TxtFfmpegPath { get; private set; } = _loc.TxtFfmpegPath;
  [JsonIgnore]
  public string TxtJpegQuality { get; private set; } = _loc.TxtJpegQuality;
  [JsonIgnore]
  public string TxtLanguages { get; private set; } = _loc.TxtLanguages;

  internal void _localize() {
    TxtCachePath = L.Get(_loc.TxtCachePath);
    TxtFfmpegPath = L.Get(_loc.TxtFfmpegPath);
    TxtLanguages = L.Get(_loc.TxtLanguages);

    this.Bind(nameof(JpegQuality), x => x.JpegQuality, q => {
      TxtJpegQuality = L.Format(_loc.TxtJpegQuality, q);
      OnPropertyChanged(nameof(TxtJpegQuality));
    });
  }

  private void _onCachePathChange(string value) {
    if (value.Length < 4 || !value.StartsWith(":\\") || value.EndsWith("\\")) return;
    _cachePath = value;
    OnPropertyChanged(nameof(CachePath));
  }

  public void AddDirectorySelectFolder(string dirPath) {
    if (DirectorySelectFolders.Contains(dirPath)) return;
    var list = DirectorySelectFolders.ToList();
    list.Insert(0, dirPath);
    DirectorySelectFolders = list.ToArray();
    Core.Settings.Save();
  }
}

public sealed class GeoNameSettings : ObservableObject {
  internal static class _loc {
    private const string _prefix = "App.Settings.GeoName.";
    public const string TxtLoadFromWeb = _prefix + "txt.LoadFromWeb";
    public const string TxtUserName = _prefix + "txt.UserName";

    static _loc() {
      LocalizationManager.Register(typeof(_loc).Assembly, [
        (TxtLoadFromWeb, "Load from web:"),
        (TxtUserName, "User name:")
      ]);
    }

    public static void Register() { }
  }

  static GeoNameSettings() { _loc.Register(); }

  private bool _loadFromWeb;
  private string _userName = string.Empty;

  public bool LoadFromWeb { get => _loadFromWeb; set { _loadFromWeb = value; OnPropertyChanged(); } }
  public string UserName { get => _userName; set { _userName = value; OnPropertyChanged(); } }

  [JsonIgnore]
  public string TxtLoadFromWeb { get; private set; } = _loc.TxtLoadFromWeb;
  [JsonIgnore]
  public string TxtUserName { get; private set; } = _loc.TxtUserName;

  internal void _localize() {
    TxtLoadFromWeb = L.Get(_loc.TxtLoadFromWeb);
    TxtUserName = L.Get(_loc.TxtUserName);
  }
}

public sealed class ImagesToVideoSettings : ObservableObject {
  private int _height = 1080;
  private int _quality = 27;
  private double _speed = 0.25;

  public int Height { get => _height; set { _height = value; OnPropertyChanged(); } }
  public int Quality { get => _quality; set { _quality = value; OnPropertyChanged(); } }
  public double Speed { get => _speed; set { _speed = value; OnPropertyChanged(); } }
}

public sealed class MediaItemSettings : ObservableObject {
  internal static class _loc {
    private const string _prefix = "App.Settings.MediaItem.";
    public const string TxtMediaItemThumbScale = _prefix + "txt.MediaItemThumbScale";
    public const string TxtScrollExactlyToMediaItem = _prefix + "txt.ScrollExactlyToMediaItem";
    public const string TxtSortField = _prefix + "txt.SortField";
    public const string TxtSortOrder = _prefix + "txt.SortOrder";
    public const string TxtThumbSize = _prefix + "txt.ThumbSize";
    public const string TxtVideoItemThumbScale = _prefix + "txt.VideoItemThumbScale";
    public const string TxtVideoMetadataFullRewriteThreshold = _prefix + "txt.VideoMetadataFullRewriteThreshold";

    static _loc() {
      LocalizationManager.Register(typeof(_loc).Assembly, [
        (TxtMediaItemThumbScale, "Media item thumb scale: {0:G2}"),
        (TxtScrollExactlyToMediaItem, "Scroll exactly to MediaItem in thumbnails:"),
        (TxtSortField, "Sort field:"),
        (TxtSortOrder, "Sort order:"),
        (TxtThumbSize, "Thumbnail size: {0}"),
        (TxtVideoItemThumbScale, "Video item thumbnail scale: {0:G3}"),
        (TxtVideoMetadataFullRewriteThreshold, "Allow full rewrite of video files smaller than: {0} MB")
      ]);
    }

    public static void Register() { }
  }

  static MediaItemSettings() { _loc.Register(); }

  private double _mediaItemThumbScale = 0.8;
  private bool _scrollExactlyToMediaItem;
  private string _sortField = "File name";
  private CollectionView.SortOrder _sortOrder = CollectionView.SortOrder.Ascending;
  private int _thumbSize = 400;
  private double _videoItemThumbScale = 0.28;
  private int _videoMetadataFullRewriteThreshold = 50;
  
  public double MediaItemThumbScale { get => _mediaItemThumbScale; set { _mediaItemThumbScale = value; OnPropertyChanged(); } }
  public bool ScrollExactlyToMediaItem { get => _scrollExactlyToMediaItem; set { _scrollExactlyToMediaItem = value; OnPropertyChanged(); } }
  public string SortField { get => _sortField; set { _sortField = value; OnPropertyChanged(); } }
  public CollectionView.SortOrder SortOrder { get => _sortOrder; set { _sortOrder = value; OnPropertyChanged(); } }
  public int ThumbSize { get => _thumbSize; set { _thumbSize = value; OnPropertyChanged(); } }
  public double VideoItemThumbScale { get => _videoItemThumbScale; set { _videoItemThumbScale = value; OnPropertyChanged(); } }
  public int VideoMetadataFullRewriteThreshold { get => _videoMetadataFullRewriteThreshold; set { _videoMetadataFullRewriteThreshold = value; OnPropertyChanged(); } }

  [JsonIgnore]
  public string TxtMediaItemThumbScale { get; private set; } = _loc.TxtMediaItemThumbScale;
  [JsonIgnore]
  public string TxtScrollExactlyToMediaItem { get; private set; } = _loc.TxtScrollExactlyToMediaItem;
  [JsonIgnore]
  public string TxtSortField { get; private set; } = _loc.TxtSortField;
  [JsonIgnore]
  public string TxtSortOrder { get; private set; } = _loc.TxtSortOrder;
  [JsonIgnore]
  public string TxtThumbSize { get; private set; } = _loc.TxtThumbSize;
  [JsonIgnore]
  public string TxtVideoItemThumbScale { get; private set; } = _loc.TxtVideoItemThumbScale;
  [JsonIgnore]
  public string TxtVideoMetadataFullRewriteThreshold { get; private set; } = _loc.TxtVideoMetadataFullRewriteThreshold;

  internal void _localize() {
    TxtScrollExactlyToMediaItem = L.Get(_loc.TxtScrollExactlyToMediaItem);
    TxtSortField = L.Get(_loc.TxtSortField);
    TxtSortOrder = L.Get(_loc.TxtSortOrder);

    this.Bind(nameof(MediaItemThumbScale), x => x.MediaItemThumbScale, x => {
      TxtMediaItemThumbScale = L.Format(_loc.TxtMediaItemThumbScale, x);
      OnPropertyChanged(nameof(TxtMediaItemThumbScale));
    });

    this.Bind(nameof(ThumbSize), x => x.ThumbSize, x => {
      TxtThumbSize = L.Format(_loc.TxtThumbSize, x);
      OnPropertyChanged(nameof(TxtThumbSize));
    });

    this.Bind(nameof(VideoItemThumbScale), x => x.VideoItemThumbScale, x => {
      TxtVideoItemThumbScale = L.Format(_loc.TxtVideoItemThumbScale, x);
      OnPropertyChanged(nameof(TxtVideoItemThumbScale));
    });

    this.Bind(nameof(VideoMetadataFullRewriteThreshold), x => x.VideoMetadataFullRewriteThreshold, x => {
      TxtVideoMetadataFullRewriteThreshold = L.Format(_loc.TxtVideoMetadataFullRewriteThreshold, x);
      OnPropertyChanged(nameof(TxtVideoMetadataFullRewriteThreshold));
    });
  }
}

public sealed class SegmentSettings : ObservableObject {
  internal static class _loc {
    private const string _prefix = "App.Settings.Segment.";
    public const string TxtGroupSize = _prefix + "txt.GroupSize";

    static _loc() {
      LocalizationManager.Register(typeof(_loc).Assembly, [
        (TxtGroupSize, "Group size: {0}")
      ]);
    }

    public static void Register() { }
  }

  static SegmentSettings() { _loc.Register(); }

  private int _groupSize = 250;

  public int GroupSize { get => _groupSize; set { _groupSize = value; OnPropertyChanged(); } }

  [JsonIgnore]
  public string TxtGroupSize { get; private set; } = _loc.TxtGroupSize;

  internal void _localize() {
    this.Bind(nameof(GroupSize), x => x.GroupSize, x => {
      TxtGroupSize = L.Format(_loc.TxtGroupSize, x);
      OnPropertyChanged(nameof(TxtGroupSize));
    });
  }
}

public sealed class MediaViewerSettings : ObservableObject {
  internal static class _loc {
    private const string _prefix = "App.Settings.MediaViewer.";
    public const string TxtExpandToFill = _prefix + "txt.ExpandToFill";

    static _loc() {
      LocalizationManager.Register(typeof(_loc).Assembly, [
        (TxtExpandToFill, "Expand content to fill:")
      ]);
    }

    public static void Register() { }
  }

  static MediaViewerSettings() { _loc.Register(); }

  private bool _expandToFill;

  public bool ExpandToFill { get => _expandToFill; set { _expandToFill = value; OnPropertyChanged(); } }

  [JsonIgnore]
  public string TxtExpandToFill { get; private set; } = _loc.TxtExpandToFill;

  internal void _localize() {
    TxtExpandToFill = L.Get(_loc.TxtExpandToFill);
  }
}