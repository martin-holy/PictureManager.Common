namespace PictureManager.Common.Localization;

public static class Registration {
  public static void RegisterAll() {
    Features.MediaItem.ReloadMetadataDialog.RegisterLocalization();
  }
}