using MH.Utils.BaseClasses;
using MH.Utils.Localization;
using PictureManager.Common.Layout;
using PictureManager.Common.Localization;

namespace PictureManager.Common.Features.MediaItem;

public sealed class MediaItemToolBarVM(MediaItemsViewsVM mediaItemViews, MainWindowVM mainWindow) : ObservableObject {
  internal static class _loc {
    private const string _prefix = "MediaItem.ToolBar.";
    public const string TxtThumbScale = _prefix + "txt.ThumbScale";
    public const string TxtShowThumbInfo = _prefix + "txt.ShowThumbInfo";
    public const string TxtCompareLimit = _prefix + "txt.CompareLimit";

    static _loc() {
      LocalizationManager.Register(typeof(_loc).Assembly, [
        (TxtThumbScale, "Thumbnail scale"),
        (TxtShowThumbInfo, "Show thumbnail info"),
        (TxtCompareLimit, "Compare limit")
      ]);
    }

    public static void Register() { }
  }

  static MediaItemToolBarVM() { _loc.Register(); }

  public MediaItemsViewsVM MediaItemViews { get; } = mediaItemViews;
  public MainWindowVM MainWindow { get; } = mainWindow;

  public string TxtThumbScale { get; } = L.Get(_loc.TxtThumbScale);
  public string TxtShowThumbInfo { get; } = L.Get(_loc.TxtShowThumbInfo);
  public string TxtCompareLimit { get; } = L.Get(_loc.TxtCompareLimit);
}