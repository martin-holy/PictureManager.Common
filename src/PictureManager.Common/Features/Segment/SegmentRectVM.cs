using MH.Utils.BaseClasses;
using MH.Utils.Localization;
using PictureManager.Common.Localization;
using System;
using System.Collections.Generic;

namespace PictureManager.Common.Features.Segment;

public sealed class SegmentRectVM : ObservableObject {
  internal static class _loc {
    private const string _prefix = "Segment.Rect.";
    public const string Delete = _prefix + "Delete.cmd";

    static _loc() {
      LocalizationManager.Register(typeof(_loc).Assembly, [
        (Delete, "Delete")
      ]);
    }

    public static void Register() { }
  }

  static SegmentRectVM() { _loc.Register(); }

  private bool _showOverMediaItem;
  private bool _isEditEnabled;
  private bool _canCreateNew;

  public bool ShowOverMediaItem { get => _showOverMediaItem; set { _showOverMediaItem = value; OnPropertyChanged(); } }
  public bool IsEditEnabled { get => _isEditEnabled; set { _isEditEnabled = value; OnPropertyChanged(); } }
  public bool CanCreateNew { get => _canCreateNew; set { _setIfVary(ref _canCreateNew, value); } }
  public List<Tuple<int, int, int, bool>>? SegmentToolTipRects { get; private set; }

  public static RelayCommand<SegmentM> ReloadSegmentToolTipCommand { get; set; } = null!;
  public static AsyncRelayCommand<SegmentRectM> DeleteCommand { get; set; } = null!;

  public SegmentRectVM(SegmentRectS segmentRectS) {
    ReloadSegmentToolTipCommand = new(_reloadSegmentToolTip);
    DeleteCommand = new((x, _) => segmentRectS.Delete(x!), x => x != null, MH.UI.Res.IconXCross, L.Get(_loc.Delete));
  }

  private void _reloadSegmentToolTip(SegmentM? segment) {
    if (segment?.MediaItem.Segments == null)
      SegmentToolTipRects = null;
    else {
      segment.MediaItem.SetThumbSize();
      segment.MediaItem.SetInfoBox();
      SegmentToolTipRects = SegmentRectS.GetMediaItemSegmentsRects(segment);
    }

    OnPropertyChanged(nameof(SegmentToolTipRects));
  }
}