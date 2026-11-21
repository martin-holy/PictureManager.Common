using MH.Utils.BaseClasses;
using MH.Utils.Localization;
using PictureManager.Common.Localization;

namespace PictureManager.Common.Features.Segment;

public sealed class SegmentToolBarVM : ObservableObject {
  internal static class _loc {
    private const string _prefix = "Segments.ToolBar.";
    public const string TxtShowSegments = _prefix + "txt.ShowSegments";

    static _loc() {
      LocalizationManager.Register(typeof(_loc).Assembly, [
        (TxtShowSegments, "Segments edit")
      ]);
    }

    public static void Register() { }
  }

  static SegmentToolBarVM() { _loc.Register(); }

  public SegmentVM SegmentVM { get; }
  public string TxtShowSegments { get; } = L.Get(_loc.TxtShowSegments);

  public SegmentToolBarVM(SegmentVM segmentVM) {
    SegmentVM = segmentVM;
  }
}