using MH.UI.Controls;
using MH.Utils.BaseClasses;
using MH.Utils.Localization;
using PictureManager.Common.Localization;
using System;
using System.Diagnostics;
using System.Reflection;

namespace PictureManager.Common.Features.Common;

public sealed class AboutDialog : Dialog {
  private static class _loc {
    private const string _prefix = "App.About.";
    public const string Title = _prefix + "dlg.Title";

    static _loc() {
      LocalizationManager.Register(typeof(_loc).Assembly, [
        (Title, "About")
      ]);
    }

    public static void Register() { }
  }

  static AboutDialog() { _loc.Register(); }
  public static void RegisterLocalization() => _loc.Register();

  public string Version => $"Version: {Core.UiVersion} (Core: {Core.Version})";
  public string? Copyright => FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).LegalCopyright;
  public string HomePageUrl => "https://github.com/martin-holy/PictureManager";
  public string TelegramUrl => "https://t.me/PictureManager";
  public static Action<string>? OpenUrl { get; set; }

  public RelayCommand OpenHomePageCommand { get; }
  public RelayCommand OpenTelegramCommand { get; }
    
  public AboutDialog() : base(L.Get(_loc.Title), MH.UI.Res.IconImage) {
    OpenHomePageCommand = new(() => OpenUrl?.Invoke(HomePageUrl), null, HomePageUrl);
    OpenTelegramCommand = new(() => OpenUrl?.Invoke(TelegramUrl), null, TelegramUrl);
  }
}