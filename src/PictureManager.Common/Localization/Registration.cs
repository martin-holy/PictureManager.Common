namespace PictureManager.Common.Localization;

public static class Registration {
  public static void RegisterAll() {
    CoreVM.RegisterLocalization();
    Features.CategoryGroup.CategoryGroupR.RegisterLocalization();
    Features.Common.AboutDialog.RegisterLocalization();
    Features.Common.FileOperationCollisionDialog.RegisterLocalization();
    Features.Common.GroupByItems.RegisterLocalization();
    Features.Common.LogDialog.RegisterLocalization();
    Features.MediaItem.Image.CompressImagesDialog.RegisterLocalization();
    Features.MediaItem.Image.ComputeImageHashesDialog.RegisterLocalization();
    Features.MediaItem.Image.ImageResizeDialog.RegisterLocalization();
    Features.MediaItem.Image.SaveMetadataDialog.RegisterLocalization();
    Features.MediaItem.ImagesToVideoDialog.RegisterLocalization();
    Features.MediaItem.MediaItemCollectionView.RegisterLocalization();
    Features.MediaItem.MediaItemS.RegisterLocalization();
    Features.MediaItem.MediaItemsFilterVM.RegisterLocalization();
    Features.MediaItem.MediaItemsImport.RegisterLocalization();
    Features.MediaItem.MediaItemsViewsVM.RegisterLocalization();
    Features.MediaItem.MediaItemVM.RegisterLocalization();
    Features.MediaItem.ReloadMetadataDialog.RegisterLocalization();
    Features.MediaItem.ReloadMetadataDialog.RegisterLocalization();
    Features.MediaItem.RotationDialog.RegisterLocalization();
    Features.MediaItem.SlideshowVM.RegisterLocalization();
    Features.MediaItem.Video.VideoItemCollectionView.RegisterLocalization();
  }
}