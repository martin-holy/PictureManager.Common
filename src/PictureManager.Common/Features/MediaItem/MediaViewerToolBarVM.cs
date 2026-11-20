using MH.Utils.BaseClasses;
using MH.Utils.Localization;
using PictureManager.Common.Layout;
using PictureManager.Common.Localization;

namespace PictureManager.Common.Features.MediaItem;

public sealed class MediaViewerToolBarVM(MediaViewerVM mediaViewer, MainWindowVM mainWindow) : ObservableObject {
  internal static class _loc {
    private const string _prefix = "MediaViewer.ToolBar.";
    public const string TxtExpandToFill = _prefix + "txt.ExpandToFill";

    static _loc() {
      LocalizationManager.Register(typeof(_loc).Assembly, [
        (TxtExpandToFill, "Expand to fill")
      ]);
    }

    public static void Register() { }
  }

  static MediaViewerToolBarVM() { _loc.Register(); }

  public MediaViewerVM MediaViewer { get; } = mediaViewer;
  public MainWindowVM MainWindow { get; } = mainWindow;

  public string TxtExpandToFill { get; } = L.Get(_loc.TxtExpandToFill);
}