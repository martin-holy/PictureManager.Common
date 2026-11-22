using MH.UI.Controls;
using MH.Utils;
using MH.Utils.BaseClasses;
using MH.Utils.Localization;
using PictureManager.Common.Features.MediaItem.Image;
using PictureManager.Common.Features.MediaItem.Video;
using PictureManager.Common.Localization;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PictureManager.Common.Features.MediaItem;

public enum SlideshowState { On, Paused, Stopped }

public sealed class SlideshowVM : ObservableObject {
  internal static class _loc {
    private const string _prefix = "MediaItems.Slideshow.";
    public const string Start = _prefix + "Start.cmd";
    public const string Stop = _prefix + "Stop.cmd";
    public const string TxtInterval = _prefix + "ToolBar.txt.Interval";
    public const string TxtPlayPanoramicImages = _prefix + "ToolBar.txt.PlayPanoramicImages";

    static _loc() {
      LocalizationManager.Register(typeof(_loc).Assembly, [
        (Start, "Start slideshow"),
        (Stop, "Stop slideshow"),
        (TxtInterval, "Slideshow interval"),
        (TxtPlayPanoramicImages, "Play panoramic images")
      ]);
    }

    public static void Register() { }
  }

  static SlideshowVM() { _loc.Register(); }

  private ZoomAndPan? _zoomAndPan;
  private bool _isTimerOn;
  private int _repeatCount;
  private readonly MediaViewerVM _mediaViewer;
  private MediaItemM? _current;

  private bool _playPanoramicImages = true;
  private int _interval = 3;
  private SlideshowState _state = SlideshowState.Stopped;

  public bool PlayPanoramicImages { get => _playPanoramicImages; set { _playPanoramicImages = value; OnPropertyChanged(); } }
  public int Interval { get => _interval; set { _interval = value; OnPropertyChanged(); } }
  public SlideshowState State { get => _state; set { _state = value; OnPropertyChanged(); } }

  public string TxtInterval { get; } = L.Get(_loc.TxtInterval);
  public string TxtPlayPanoramicImages { get; } = L.Get(_loc.TxtPlayPanoramicImages);

  public RelayCommand StartCommand { get; }
  public RelayCommand StopCommand { get; }

  public SlideshowVM(MediaViewerVM mediaViewer) {
    _mediaViewer = mediaViewer;
    StartCommand = new(_slideshow, MH.UI.Res.IconPlay, L.Get(_loc.Start));
    StopCommand = new(_slideshow, MH.UI.Res.IconStop, L.Get(_loc.Stop));
  }

  /// <summary>
  /// Slideshow is now only for WPF
  /// </summary>
  public void Init(ZoomAndPan zoomAndPan) {
    _zoomAndPan = zoomAndPan;
    zoomAndPan.AnimationEndedEvent += _onZoomAndPanAnimationEnded;
    zoomAndPan.ContentMouseDownEvent += _onZoomAndPanContentMouseDown;
  }

  public void OnCurrentChanged(MediaItemM? current) {
    _current = current;
    if (_state != SlideshowState.Stopped)
      _next(true);
  }

  private void _onZoomAndPanAnimationEnded(object? sender, EventArgs e) {
    if (_state != SlideshowState.Paused) return;
    State = SlideshowState.On;
    _next(false);
  }

  private void _onZoomAndPanContentMouseDown(object? sender, EventArgs e) =>
    Stop();

  public void OnPlayerMediaEnded(object? sender, EventArgs e) {
    if (_state != SlideshowState.Paused) return;

    if (_repeatCount == 0) {
      var rc = _interval / (((MediaPlayer)sender!).TimelineMaximum / 1000);
      _repeatCount = (int)rc;
      if (rc - _repeatCount > 0) _repeatCount++;
    }

    _repeatCount--;

    if (_repeatCount <= 0) {
      State = SlideshowState.On;
      _next(false);
    }
  }

  public bool Stop() {
    if (_state == SlideshowState.Stopped) return false;
    State = SlideshowState.Stopped;
    _zoomAndPan?.StopAnimation();
    return true;
  }

  private void _next(bool delay) {
    if (!delay) {
      if (!_mediaViewer.Next()) Stop();
      return;
    }

    switch (_current) {
      case ImageM:
        if (_playPanoramicImages && _zoomAndPan != null && _zoomAndPan.IsContentPanoramic() && _zoomAndPan.CanStartAnimation()) {
          State = SlideshowState.Paused;
          _zoomAndPan.StartAnimation(_interval * 1000);
        }
        else if (!_isTimerOn) {
          _isTimerOn = true;

          Task.Run(() => {
            Thread.Sleep(_interval * 1000);
            Tasks.RunOnUiThread(() => {
              _isTimerOn = false;

              if (_state != SlideshowState.Stopped)
                _next(false);
            });
          });
        }
        break;
      case VideoM:
        _repeatCount = 0;
        State = SlideshowState.Paused;
        break;
    }
  }

  private void _slideshow() {
    if (Stop()) return;
    State = SlideshowState.On;
    _next(true);
  }
}