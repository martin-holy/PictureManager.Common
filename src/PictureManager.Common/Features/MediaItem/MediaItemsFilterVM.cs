using MH.UI.Interfaces;
using MH.Utils.BaseClasses;
using MH.Utils.Localization;
using PictureManager.Common.Features.MediaItem.Image;
using PictureManager.Common.Features.MediaItem.Video;
using PictureManager.Common.Features.Rating;
using PictureManager.Common.Interfaces;
using PictureManager.Common.Localization;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace PictureManager.Common.Features.MediaItem;

public sealed class MediaItemsFilterVM : ObservableObject, ICollectionViewFilter<MediaItemM> {
  internal static class _loc {
    private const string _prefix = "MediaItems.Filter.";
    public const string Clear = _prefix + "Clear.cmd";
    public const string BtnImage = _prefix + "btn.Image";
    public const string BtnVideo = _prefix + "btn.Video";
    public const string TxtAnd = _prefix + "txt.And";
    public const string TxtOr = _prefix + "txt.Or";
    public const string TxtNot = _prefix + "txt.Not";
    public const string TxtHeight = _prefix + "txt.Height";
    public const string TxtWidth = _prefix + "txt.Width";
    public const string TxtSize = _prefix + "txt.Size";

    static _loc() {
      LocalizationManager.Register(typeof(_loc).Assembly, [
        (Clear, "Clear"),
        (BtnImage, "Image"),
        (BtnVideo, "Video"),
        (TxtAnd, "And"),
        (TxtOr, "Or"),
        (TxtNot, "Not"),
        (TxtHeight, "Height:"),
        (TxtWidth, "Width:"),
        (TxtSize, "Size MPx:")
      ]);
    }

    public static void Register() { }
  }

  static MediaItemsFilterVM() { _loc.Register(); }

  private bool _showImages = true;
  private bool _showVideos = true;

  public bool ShowImages { get => _showImages; set { _showImages = value; _raiseFilterChanged(); OnPropertyChanged(); } }
  public bool ShowVideos { get => _showVideos; set { _showVideos = value; _raiseFilterChanged(); OnPropertyChanged(); } }

  public ObservableCollection<object> FilterAnd { get; } = [];
  public ObservableCollection<object> FilterOr { get; } = [];
  public ObservableCollection<object> FilterNot { get; } = [];
  public SelectionRange Height { get; } = new();
  public SelectionRange Width { get; } = new();
  public SelectionRange Size { get; } = new();

  public string BtnImage { get; } = L.Get(_loc.BtnImage);
  public string BtnVideo { get; } = L.Get(_loc.BtnVideo);
  public string TxtAnd { get; } = L.Get(_loc.TxtAnd);
  public string TxtOr { get; } = L.Get(_loc.TxtOr);
  public string TxtNot { get; } = L.Get(_loc.TxtNot);
  public string TxtHeight { get; } = L.Get(_loc.TxtHeight);
  public string TxtWidth { get; } = L.Get(_loc.TxtWidth);
  public string TxtSize { get; } = L.Get(_loc.TxtSize);

  public event EventHandler FilterChangedEvent = delegate { };

  public RelayCommand ClearCommand { get; }

  public MediaItemsFilterVM() {
    Height.ChangedEvent += delegate { _raiseFilterChanged(); };
    Width.ChangedEvent += delegate { _raiseFilterChanged(); };
    Size.ChangedEvent += delegate { _raiseFilterChanged(); };
    ClearCommand = new(_clear, null, L.Get(_loc.Clear));
  }

  private void _raiseFilterChanged() => FilterChangedEvent(this, EventArgs.Empty);

  private void _clear() {
    FilterAnd.Clear();
    FilterOr.Clear();
    FilterNot.Clear();

    Size.SetFullRange();
    Height.SetFullRange();
    Width.SetFullRange();

    _raiseFilterChanged();
  }

  public void Set(object? item, DisplayFilter displayFilter) {
    if (item == null) return;
    if (FilterAnd.Contains(item) || FilterOr.Contains(item) || FilterNot.Contains(item))
      displayFilter = DisplayFilter.None;

    switch (displayFilter) {
      case DisplayFilter.None:
        FilterAnd.Remove(item);
        FilterOr.Remove(item);
        FilterNot.Remove(item);
        break;
      case DisplayFilter.And:
        FilterAnd.Add(item);
        break;
      case DisplayFilter.Or:
        FilterOr.Add(item);
        break;
      case DisplayFilter.Not:
        FilterNot.Add(item);
        break;
      default:
        throw new ArgumentOutOfRangeException(nameof(displayFilter), displayFilter, null);
    }

    _raiseFilterChanged();
  }

  public bool Filter(MediaItemM mi) {
    // Media Type
    if (!ShowImages && mi is ImageM) return false;
    if (!ShowVideos && mi is VideoM) return false;

    // GeoNames
    if (!_filter(mi.GetGeoNames().ToArray())) return false;

    //Ratings
    var chosenRatings = FilterOr.OfType<RatingM>().Select(x => x.Value).ToArray();
    if (chosenRatings.Any() && !chosenRatings.Contains(mi.Rating)) return false;

    // MediaItemSizes
    if (!Width.IsFullRange && !Width.Fits(mi.Width)
        || !Height.IsFullRange && !Height.Fits(mi.Height)
        || !Size.IsFullRange && !Size.Fits(mi.Width * mi.Height / 1000000.0))
      return false;

    // People
    var miPeople = mi.GetPeople().ToArray();
    if (!_filter(miPeople)) return false;

    // Keywords
    return _filter(mi.GetKeywords().Concat(miPeople.GetKeywords()).Distinct().ToArray());
  }

  private bool _filter<T>(T[] miI) where T : class {
    var notI = FilterNot.OfType<T>().ToArray();
    if (notI.Any() && miI.Any() && notI.Any(fx => miI.Any(x => ReferenceEquals(x, fx)))) return false;
    var andI = FilterAnd.OfType<T>().ToArray();
    if (andI.Any() && (!miI.Any() || !andI.All(fx => miI.Any(x => ReferenceEquals(x, fx))))) return false;
    var orI = FilterOr.OfType<T>().ToArray();
    if (orI.Any() && (!miI.Any() || !orI.Any(fx => miI.Any(x => ReferenceEquals(x, fx))))) return false;

    return true;
  }

  public void UpdateSizeRanges(IList<MediaItemM> limit) {
    var zeroItems = !limit.Any();

    if (zeroItems) {
      Size.Zero();
      Height.Zero();
      Width.Zero();
    }
    else {
      Size.Reset(
        Math.Round(limit.Min(x => x.Width * x.Height) / 1000000.0, 1),
        Math.Round((limit.Max(x => x.Width * x.Height) + 100000) / 1000000.0, 1));
      Height.Reset(limit.Min(x => x.Height), limit.Max(x => x.Height));
      Width.Reset(limit.Min(x => x.Width), limit.Max(x => x.Width));
    }

    _raiseFilterChanged();
  }
}