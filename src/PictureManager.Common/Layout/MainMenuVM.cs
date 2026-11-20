using MH.UI.Controls;
using MH.Utils.BaseClasses;
using MH.Utils.Extensions;
using MH.Utils.Localization;
using MH.Utils.Tree;
using PictureManager.Common.Features.GeoName;
using PictureManager.Common.Features.MediaItem;
using PictureManager.Common.Features.Segment;
using PictureManager.Common.Features.Viewer;
using PictureManager.Common.Features.WhatIsNew;
using PictureManager.Common.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace PictureManager.Common.Layout;

public class MainMenuVM : TreeView {
  private static class _loc {
    private const string _prefix = "MainMenu.";
    public const string GeoLocation = _prefix + "GeoLocation";
    public const string MediaItems = _prefix + "MediaItems";
    public const string Segments = _prefix + "Segments";
    public const string Viewers = _prefix + "Viewers";
    public const string ViewersAll = _prefix + "ViewersAll";

    static _loc() {
      LocalizationManager.Register(typeof(_loc).Assembly, [
        (GeoLocation, "GeoLocation"),
        (MediaItems, "Media Items"),
        (Segments, "Segments"),
        (Viewers, "Viewers"),
        (ViewersAll, "All")
      ]);
    }

    public static void Register() { }
  }

  static MainMenuVM() { _loc.Register(); }
  public static void RegisterLocalization() { }

  public string Icon { get; } = Res.IconThreeBars;
  public Func<IEnumerable<ITreeItem>> BuildMenu => () => RootHolder;

  public void Build(CoreVM coreVM) {
    RootHolder.Clear();

    var geoLocation = new MenuItem(Res.IconLocationCheckin, L.Get(_loc.GeoLocation), [
      new MenuItem(CoreVM.GetGeoNamesFromWebCommand),
      new MenuItem(GeoNameVM.NewGeoNameFromGpsCommand),
      new MenuItem(CoreVM.ReadGeoLocationFromFilesCommand)]);

    var mediaItem = new MenuItem(Res.IconImageMultiple, L.Get(_loc.MediaItems), [
      new MenuItem(MediaItemsViewsVM.AddViewCommand),
      new MenuItem(MediaItemVM.CommentCommand) { InputGestureText = "Ctrl+K"},
      new MenuItem(CoreVM.CompressImagesCommand),
      new MenuItem(MediaItemsViewsVM.CopyPathsCommand),
      new MenuItem(MediaItemVM.DeleteCommand),
      new MenuItem(CoreVM.ImagesToVideoCommand),
      new MenuItem(MediaItemsViewsVM.RebuildThumbnailsCommand),
      new MenuItem(MediaItemVM.RenameCommand) { InputGestureText = "F2"},
      new MenuItem(CoreVM.ResizeSelectedImagesCommand),
      new MenuItem(CoreVM.ReloadMetadataCommand),
      new MenuItem(CoreVM.RotateMediaItemsCommand) { InputGestureText = "Ctrl+R"},
      new MenuItem(CoreVM.SaveMetadataToFilesCommand),
      new MenuItem(MediaItemsViewsVM.ViewModifiedCommand),
      new MenuItem(MediaItemVM.ViewSelectedCommand)]);

    var segments = new MenuItem(Res.IconSegment, L.Get(_loc.Segments), [
      new MenuItem(SegmentVM.DeleteSelectedCommand),
      new MenuItem(CoreVM.ExportSegmentsCommand),
      new MenuItem(CoreVM.OpenSegmentsViewsCommand),
      new MenuItem(SegmentVM.SetSelectedAsSamePersonCommand),
      new MenuItem(SegmentVM.SetSelectedAsUnknownCommand),
      new MenuItem(SegmentVM.AddEmptyViewCommand),
      new MenuItem(SegmentsDrawerVM.OpenCommand),
      new MenuItem(SegmentsDrawerVM.AddSelectedCommand),
      new MenuItem(SegmentsDrawerVM.RemoveSelectedCommand)
    ]);

    var viewers = new MenuItem(Res.IconEye, L.Get(_loc.Viewers))
      .AddSource(new MenuItem(ViewerVM.ChangeCurrentCommand) { Name = L.Get(_loc.ViewersAll) })
      .AddSource(coreVM.Viewer.All, ViewerVM.ChangeCurrentCommand);

    RootHolder.Add(geoLocation);
    RootHolder.Add(mediaItem);
    RootHolder.Add(segments);
    RootHolder.Add(viewers);
    RootHolder.Add(new MenuItemSeparator());
    RootHolder.Add(new MenuItem(CoreVM.SaveDbCommand));
    RootHolder.Add(new MenuItem(CoreVM.OpenSettingsCommand));
    RootHolder.Add(new MenuItem(CoreVM.ExportTranslationsCommand));
    RootHolder.Add(new MenuItem(WhatIsNewVM.OpenCommand));
    RootHolder.Add(new MenuItem(CoreVM.OpenAboutCommand));
  }

  public void HideMenuItems(ICommand[] commands) {
    foreach (var menuItem in RootHolder.Flatten().OfType<MenuItem>())
      if (commands.Contains(menuItem.Command))
        menuItem.IsHidden = true;
  }
}