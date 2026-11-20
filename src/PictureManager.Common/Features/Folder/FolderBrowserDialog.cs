using MH.UI.Controls;
using MH.Utils.BaseClasses;
using MH.Utils.Localization;
using MH.Utils.Tree;
using PictureManager.Common.Localization;
using System;
using System.IO;

namespace PictureManager.Common.Features.Folder;

public sealed class FolderBrowserDialog : Dialog {
  internal static class _loc {
    private const string _prefix = "FolderBrowser.";
    public const string Title = _prefix + "dlg.Title";
    public const string Ok = _prefix + "dlg.btn.Ok";

    static _loc() {
      LocalizationManager.Register(typeof(_loc).Assembly, [
        (Title, "Browse For Folder"),
        (Ok, "Ok")
      ]);
    }

    public static void Register() { }
  }

  static FolderBrowserDialog() { _loc.Register(); }

  private FolderTreeViewItem? _selectedFolder;

  public FolderTreeViewItem? SelectedFolder { get => _selectedFolder; private set { _selectedFolder = value; OnPropertyChanged(); } }
  public TreeView TreeView { get; } = new() { ShowTreeItemSelection = true };

  public new RelayCommand OkCommand { get; }

  public FolderBrowserDialog() : base(L.Get(_loc.Title), Res.IconFolder) {
    TreeView.ItemSelectedEvent += (_, item) => SelectedFolder = item as FolderTreeViewItem;
    OkCommand = new(() => SetResult(this, 1), () => SelectedFolder != null, null, L.Get(_loc.Ok));
    Buttons = [
      new(OkCommand, true),
      new(CloseCommand, false, true)
    ];

    _addDrives();
  }

  private void _addDrives() {
    var drives = Environment.GetLogicalDrives();
    TreeView.RootHolder.Clear();
    TreeView.SelectedTreeItems.DeselectAll();

    foreach (var drive in drives) {
      var di = new DriveInfo(drive);
      if (!di.IsReady) continue;

      var item = new FolderTreeViewItem(null, di.Name.TrimEnd(Path.DirectorySeparatorChar)) {
        Icon = FolderS.GetDriveIcon(di.DriveType)
      };

      // add placeholder so the Drive can be expanded
      item.Items.Add(FolderTreeViewItem.Dummy);

      TreeView.RootHolder.Add(item);
    }
  }
}

public class FolderTreeViewItem : TreeItem {
  public static FolderTreeViewItem Dummy { get; } = new(null, string.Empty);
  public string FullPath => this.GetFullName(Path.DirectorySeparatorChar.ToString(), x => x.Name);

  public FolderTreeViewItem(ITreeItem? parent, string name) : base(Res.IconFolder, name) {
    Parent = parent;
  }

  protected override void _onIsExpandedChanged(bool value) {
    if (value) _loadSubFolders();
    _updateIcon();
  }

  private void _updateIcon() {
    if (Parent != null) // not Drive Folder
      Icon = IsExpanded
        ? Res.IconFolderOpen
        : Res.IconFolder;
  }

  private void _loadSubFolders() {
    var fullPath = FullPath + Path.DirectorySeparatorChar;
    Items.Clear();

    foreach (var dir in Directory.EnumerateDirectories(fullPath)) {
      var folder = new FolderTreeViewItem(this, dir[fullPath.Length..]);

      try {
        // add placeholder so the folder can be expanded
        using var enumerator = Directory.EnumerateDirectories(folder.FullPath).GetEnumerator();
        if (enumerator.MoveNext())
          folder.Items.Add(Dummy);

        // add new Folder to the tree if is Accessible
        Items.Add(folder);
      }
      catch (UnauthorizedAccessException) { }
    }
  }
}