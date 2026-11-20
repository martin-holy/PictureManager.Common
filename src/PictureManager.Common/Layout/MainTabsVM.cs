using MH.UI.Controls;
using MH.Utils.Localization;
using PictureManager.Common.Localization;

namespace PictureManager.Common.Layout;

public sealed class MainTabsSlotVM;

public sealed class MainTabsVM : TabControl {
  internal static class _loc {
    private const string _prefix = "MainTabs.";
    public const string NoTabsText = _prefix + "NoTabsText";

    static _loc() {
      LocalizationManager.Register(typeof(_loc).Assembly, [
        (NoTabsText, "Main tabs")
      ]);
    }

    public static void Register() { }
  }

  static MainTabsVM() { _loc.Register(); }

  public MainTabsVM() : base(new(Dock.Left) { StartSlot = new MainTabsSlotVM(), RotationAngle = 270, JustifyTabSize = true }) {
    CanCloseTabs = true;
    NoTabsText = L.Get(_loc.NoTabsText);
  }
}