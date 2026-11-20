using MH.Utils.BaseClasses;
using MH.Utils.Localization;
using PictureManager.Common.Localization;
using System;
using System.IO;
using System.Text;

namespace PictureManager.Common.Features.WhatIsNew;

public class WhatIsNewVM {
  internal static class _loc {
    private const string _prefix = "WhatIsNew.";
    public const string Open = _prefix + "Open.cmd";
    public const string TabName = _prefix + "TabName";

    static _loc() {
      LocalizationManager.Register(typeof(_loc).Assembly, [
        (Open, "What's new"),
        (TabName, "What's new")
      ]);
    }

    public static void Register() { }
  }

  static WhatIsNewVM() {
    _loc.Register();

    OpenCommand = new(_open, null, L.Get(_loc.Open));
  }

  private static WhatIsNewVM? _inst;

  public static string Content { get; set; } = string.Empty;

  public static RelayCommand OpenCommand { get; }

  private static void _open() {
    try {
      _inst ??= new();
      using var sr = new StreamReader("changelog.txt", Encoding.UTF8);
      Content = sr.ReadToEnd();
      Core.VM.MainTabs.Activate(Res.IconInformation, L.Get(_loc.TabName), _inst);
    }
    catch (Exception) {
      // ignored
    }
  }
}