using MH.UI.Controls;
using MH.UI.Dialogs;
using MH.UI.Tree;
using MH.Utils;
using MH.Utils.Localization;
using MH.Utils.Tree;
using PictureManager.Common.Localization;
using PictureManager.Common.Utils;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PictureManager.Common.Features.Folder;

public sealed class FolderTreeCategory : TreeCategory<FolderM> {
  internal static class _loc {
    private const string _prefix = "Folder.";
    public const string Title = _prefix + "TreeCategory.Title";
    public const string DropCopyTitle = _prefix + "Drop.dlg.Copy.Title";
    public const string DropCopyMessage = _prefix + "Drop.dlg.Copy.Message";
    public const string DropMoveTitle = _prefix + "Drop.dlg.Move.Title";
    public const string DropMoveMessage = _prefix + "Drop.dlg.Move.Message";

    static _loc() {
      LocalizationManager.Register(typeof(_loc).Assembly, [
        (Title, "Folders"),
        (DropCopyTitle, "Copy folder"),
        (DropCopyMessage, "Do you really want to copy folder\n'{0}' to '{1}'?"),
        (DropMoveTitle, "Move folder"),
        (DropMoveMessage, "Do you really want to move folder\n'{0}' to '{1}'?")
      ]);
    }

    public static void Register() { }
  }

  static FolderTreeCategory() { _loc.Register(); }

  public FolderTreeCategory(FolderR r, TreeView treeView) :
    base(treeView, Res.IconFolder, L.Get(_loc.Title), (int)Category.Folders, r) {
    CanMoveItem = true;
    CanCopyItem = true;
    UseTreeDelete = true;
  }

  public override bool CanDrop(object? src, ITreeItem? dest) {
    switch (src) {
      case FolderM srcData: {
        // Folder
        if (dest is FolderM destData
            && !destData.HasThisParent(srcData)
            && !ReferenceEquals(srcData, destData)
            && destData.IsAccessible
            && !ReferenceEquals(srcData.Parent, destData))
          return true;

        break;
      }
      case string[] dragged: {
        // MediaItems
        if (Core.VM.MediaItem.Views.Current == null) break;

        var selected = Core.VM.MediaItem.Views.Current.Selected.Items
          .Select(p => p.FilePath)
          .OrderBy(p => p)
          .ToArray();

        if (selected.SequenceEqual(dragged.OrderBy(x => x))
            && dest is FolderM { IsAccessible: true })
          return true;

        break;
      }
    }

    return false;
  }

  public override async Task OnDrop(object src, ITreeItem dest, bool aboveDest, bool copy) {
    if (dest is not FolderM destFolder) return;
    var mode = copy ? FileOperationMode.Copy : FileOperationMode.Move;

    switch (src) {
      case FolderM srcData: // Folder
        if (await Dialog.ShowAsync(new MessageDialog(
            L.Get(copy ? _loc.DropCopyTitle : _loc.DropMoveTitle),
            L.Format(copy ? _loc.DropCopyMessage : _loc.DropMoveMessage, srcData.Name, dest.Name),
            MH.UI.Res.IconQuestion,
            true)) != 1)
          return;

        await CopyMoveU.CopyMoveFolder(srcData, destFolder, mode);

        break;

      case string[]: // MediaItems
        if (!await Core.VM.MediaItem.CopyMoveSelectedToFolder(destFolder, copy)) return;
        break;
    }

    Core.VM.MainWindow.TreeViewCategories.MarkUsedKeywordsAndPeople();
  }

  public void AddDrives() {
    foreach (var drive in Drives.SerialNumbers) {
      var di = new DriveInfo(drive.Key);

      // add Drive to the database and to the tree if not already there
      if (Items.Cast<DriveM>().SingleOrDefault(x => x.SerialNumber.Equals(drive.Value, StringComparison.OrdinalIgnoreCase)) is
          not { } item) {
        item = Core.R.Folder.AddDrive(this, drive.Key, drive.Value);
      }

      item.IsAccessible = di.IsReady;
      item.Icon = FolderS.GetDriveIcon(di.DriveType);
      item.Name = drive.Key;

      // add placeholder so the Drive can be expanded
      if (di.IsReady && item.Items.Count == 0)
        item.Items.Add(FolderS.FolderPlaceHolder);
    }
  }

  public void ScrollTo(FolderM? folder) {
    if (folder == null || !Core.S.Viewer.CanViewerSee(folder)) return;
    folder.IsExpanded = true;
    Core.VM.MainWindow.TreeViewCategories.Select(TreeView);
    TreeView.ScrollTo(folder);
  }
}