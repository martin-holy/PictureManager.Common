using MH.Utils.Localization;
using System;
using System.Collections.Generic;

namespace PictureManager.Common.Utils.Localization;

internal static class L {
  public static readonly Dictionary<string, string> DefaultTranslations =
    new(StringComparer.Ordinal) {
      ["App.ExportTranslations"] = "Export translations",

      ["Image.Compress.dlg.btn.Compress"] = "Compress",
      ["Image.Compress.dlg.Title"] = "Compress images",

      ["Image.ComputeImageHashes.dlg.Title"] = "Computing Hashes ...",

      ["Image.ImagesToVideo.dlg.btn.Create"] = "Create Video",
      ["Image.ImagesToVideo.dlg.Title"] = "Images to Video",
      ["Image.ImagesToVideo.err.FFMPEG.dlg.Message"] = "FFMPEG was not found. Install it and set the path in the settings.",
      ["Image.ImagesToVideo.err.FFMPEG.dlg.Title"] = "FFMPEG not found",

      ["Image.Resize.dlg.btn.Resize"] = "Resize",
      ["Image.Resize.dlg.Title"] = "Resize images",
      ["Image.Resize.OpenFolderBrowser.cmd"] = "Select folder",

      ["MediaItem.CollectionView.SortField.CreationTime"] = "Created",
      ["MediaItem.CollectionView.SortField.FileName"] = "File name",
      ["MediaItem.CollectionView.SortField.LastWriteTime"] = "Modified",
      ["MediaItem.CollectionView.Title"] = "Media Items",

      ["MediaItem.Comment.cmd"] = "Comment",
      ["MediaItem.Comment.dlg.err.TooLong"] = "Comment is too long!",
      ["MediaItem.Comment.dlg.Message"] = "Add a comment",
      ["MediaItem.Comment.dlg.Title"] = "Comment",

      ["MediaItem.CopySelectedToFolder.cmd"] = "Copy",
      ["MediaItem.CopySelectedToFolder.dlg.Message.one"] = "Do you really want to copy {0} media item to\n'{1}'?",
      ["MediaItem.CopySelectedToFolder.dlg.Message.other"] = "Do you really want to copy {0} media items to\n'{1}'?",
      ["MediaItem.CopySelectedToFolder.dlg.Title"] = "Copy media items",

      ["MediaItem.Delete.cmd"] = "Delete",
      ["MediaItem.Delete.dlg.Message.one"] = "Do you really want to delete {0} item?",
      ["MediaItem.Delete.dlg.Message.other"] = "Do you really want to delete {0} items?",
      ["MediaItem.Delete.dlg.Title"] = "Delete Confirmation",

      ["MediaItem.err.MetadataSavedJustInDB"] = "Metadata will be saved just in database.",
      ["MediaItem.err.MetadataWriting"] = "Error writing metadata",
      ["MediaItem.err.UdateLastWriteTime"] = "Can't update LastWriteTime to DateTaken.",

      ["MediaItem.LoadByGeoName.cmd"] = "Load Media items",
      ["MediaItem.LoadByKeyword.cmd"] = "Load Media items",

      ["MediaItem.LoadByPeopleOrSegments.cmd"] = "Load Media items with selected People or Segments",
      ["MediaItem.LoadByPeopleOrSegments.dlg.btn.People"] = "People",
      ["MediaItem.LoadByPeopleOrSegments.dlg.btn.Segments"] = "Segments",
      ["MediaItem.LoadByPeopleOrSegments.dlg.Message"] = "Do you want to load Media items from selected People or Segments?",
      ["MediaItem.LoadByPeopleOrSegments.dlg.Title"] = "Load Media items",
      
      ["MediaItem.LoadByPerson.cmd"] = "Load Media items",

      ["MediaItem.MoveSelectedToFolder.cmd"] = "Move",
      ["MediaItem.MoveSelectedToFolder.dlg.Message.one"] = "Do you really want to move {0} media item to\n'{1}'?",
      ["MediaItem.MoveSelectedToFolder.dlg.Message.other"] = "Do you really want to move {0} media items to\n'{1}'?",
      ["MediaItem.MoveSelectedToFolder.dlg.Title"] = "Move media items",

      ["MediaItem.Rename.cmd"] = "Rename",
      ["MediaItem.Rename.dlg.err.FileExists"] = "New file name already exists!",
      ["MediaItem.Rename.dlg.err.InvalidCharacters"] = "New file name contains invalid character!",
      ["MediaItem.Rename.dlg.Message"] = "Add a new name",
      ["MediaItem.Rename.dlg.Title"] = "Rename",

      ["MediaItem.SaveMetadata.dlg.Title"] = "Saving metadata to files...",
      ["MediaItem.SaveMetadata.Open.dlg.Message.one"] = "Do you really want to save metadata to {0} file?",
      ["MediaItem.SaveMetadata.Open.dlg.Message.other"] = "Do you really want to save metadata to {0} files?",
      ["MediaItem.SaveMetadata.Open.dlg.Title"] = "Save metadata to files",

      ["MediaItem.ViewSelected.cmd"] = "View selected",

      ["VideoItem.CollectionView.SortField.Time"] = "Time",
      ["VideoItem.CollectionView.Title"] = "Video Items"
    };

  static L() {
    LocalizationManager.Register(typeof(L).Assembly, DefaultTranslations);
  }

  public static string Get(string key) =>
    LocalizationManager.Get(typeof(L).Assembly, key);

  public static string Format(string key, params object?[] args) =>
    LocalizationManager.Format(typeof(L).Assembly, key, args);

  public static string FormatPlural(string key, int count, params object?[] args) =>
    LocalizationManager.FormatPlural(typeof(L).Assembly, key, count, args);
}