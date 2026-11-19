using MH.UI.Controls;
using MH.Utils;
using MH.Utils.BaseClasses;
using MH.Utils.Localization;
using PictureManager.Common.Localization;

namespace PictureManager.Common.Features.Common;

public sealed class LogDialog : Dialog {
  private static class _loc {
    private const string _prefix = "App.Log.";
    public const string Title = _prefix + "dlg.Title";
    public const string Clear = _prefix + "dlg.btn.Clear";

    static _loc() {
      LocalizationManager.Register(typeof(_loc).Assembly, [
        (Title, "Log"),
        (Clear, "Clear")
      ]);
    }

    public static void Register() { }
  }

  static LogDialog() { _loc.Register(); }
  public static void RegisterLocalization() { }

  public LogDialog() : base(L.Get(_loc.Title), MH.UI.Res.IconSort) {
    Buttons = [
      new(new RelayCommand(() => { Log.Items.Clear(); Result = 1; }, null, L.Get(_loc.Clear)), true),
      new(CloseCommand, false, true)
    ];
  }
}