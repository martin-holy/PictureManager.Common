namespace PictureManager.Common.Localization;

public static class Registration {
  public static void RegisterAll() {
    CoreVM.RegisterLocalization();
    Features.CategoryGroup.CategoryGroupR.RegisterLocalization();
    Features.Common.AboutDialog.RegisterLocalization();
    Features.Common.FileOperationCollisionDialog.RegisterLocalization();
    Features.Common.GroupByItems.RegisterLocalization();
    Features.Common.LogDialog.RegisterLocalization();
    Features.Common.Settings.RegisterLocalization();
    Features.Common.TreeViewSearchVM.RegisterLocalization();
    Features.FavoriteFolder.FavoriteFolderTreeCategory.RegisterLocalization();
    Features.Folder.FolderBrowserDialog.RegisterLocalization();
    Features.Folder.FolderR.RegisterLocalization();
    Features.Folder.FolderTreeCategory.RegisterLocalization();
    Features.FolderKeyword.FolderKeywordsDialog.RegisterLocalization();
    Features.FolderKeyword.FolderKeywordTreeCategory.RegisterLocalization();
    Features.GeoLocation.GetGeoNamesFromWebDialog.RegisterLocalization();
    Features.GeoLocation.ReadGeoLocationFromFilesDialog.RegisterLocalization();
    Features.GeoName.GeoNameR.RegisterLocalization();
    Features.GeoName.GeoNameTreeCategory.RegisterLocalization();
    Features.GeoName.GeoNameVM.RegisterLocalization();
    Features.Keyword.KeywordDS.RegisterLocalization();
    Features.Keyword.KeywordR.RegisterLocalization();
    Features.Keyword.KeywordTreeCategory.RegisterLocalization();
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
    Features.Person.MergePeopleDialog.RegisterLocalization();
    Features.Person.PeopleToolsTabVM.RegisterLocalization();
    Features.Person.PersonCollectionView.RegisterLocalization();
    Features.Person.PersonDetailVM.RegisterLocalization();
  }
}