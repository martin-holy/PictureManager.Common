using MH.UI.Controls;
using MH.Utils;
using MH.Utils.BaseClasses;
using MH.Utils.Localization;
using PictureManager.Common.Localization;

namespace PictureManager.Common.Features.Common;

public sealed class LogDialog : Dialog {
  private static class _loc {
    private const string _prefix = "App.Log.dlg.";
    public const string Title = _prefix + "Title";
    public const string Clear = _prefix + "btn.Clear";
    public const string TxtWrapText = _prefix + "txt.WrapText";

    static _loc() {
      LocalizationManager.Register(typeof(_loc).Assembly, [
        (Title, "Log"),
        (Clear, "Clear"),
        (TxtWrapText, "Wrap text")
      ]);
    }

    public static void Register() { }
  }

  static LogDialog() { _loc.Register(); }
  public static void RegisterLocalization() { }

  public string TxtWrapText { get; } = L.Get(_loc.TxtWrapText);

  public LogDialog() : base(L.Get(_loc.Title), MH.UI.Res.IconSort) {
    Buttons = [
      new(new RelayCommand(() => { Log.Items.Clear(); Result = 1; }, null, L.Get(_loc.Clear)), true),
      new(CloseCommand, false, true)
    ];
  }
}