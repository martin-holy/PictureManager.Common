using MH.Utils.Localization;

namespace PictureManager.Common.Localization;

internal static class L {
  public static string Get(string key) =>
    LocalizationManager.Get(typeof(L).Assembly, key);

  public static string Format(string key, params object?[] args) =>
    LocalizationManager.Format(typeof(L).Assembly, key, args);

  public static string FormatPlural(string key, int count, params object?[] args) =>
    LocalizationManager.FormatPlural(typeof(L).Assembly, key, count, args);
}