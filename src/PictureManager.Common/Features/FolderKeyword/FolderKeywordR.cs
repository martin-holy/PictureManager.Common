using MH.Utils.DB.Repositories;
using MH.Utils.Extensions;
using MH.Utils.Interfaces;
using PictureManager.Common.Features.Folder;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PictureManager.Common.Features.FolderKeyword;

public class FolderKeywordR : TreeRepository<FolderM> {
  public FolderKeywordTreeView Tree { get; }
  public FolderKeywordDS DataSource { get; }

  public static readonly FolderKeywordM FolderKeywordPlaceHolder = new(string.Empty, null);
  public List<FolderKeywordM> All2 { get; } = [];

  public FolderKeywordR(CoreR coreR) {
    Tree = new(this);
    DataSource = new(coreR, this);
  }

  public void LoadIfContains(FolderM? folder) {
    if (folder != null && (All.Contains(folder) || folder.FolderKeyword != null))
      Reload();
  }

  public void Reload() {
    foreach (var fk in All2) {
      fk.Folders.Clear();
      fk.Items.Clear();
    }

    Tree.Category.Items.Clear();
    All2.Clear();

    foreach (var folder in All)
      _loadRecursive(folder, Tree.Category);

    foreach (var fk in All2.Where(fk => fk.Folders.All(x => !Core.S.Viewer.CanViewerSee(x))))
      fk.IsHidden = true;
  }

  private void _loadRecursive(ITreeItem folder, ITreeItem fkRoot) {
    foreach (var f in folder.Items.OfType<FolderM>()) {
      var fk = _getForFolder(f, fkRoot);
      _linkWithFolder(f, fk);
      _loadRecursive(f, fk);
    }
  }

  private FolderKeywordM _getForFolder(FolderM folder, ITreeItem fkRoot) {
    var fk = fkRoot.Items.Cast<FolderKeywordM>()
      .SingleOrDefault(x => x.Name.Equals(folder.Name, StringComparison.OrdinalIgnoreCase));

    if (fk == null) {
      // remove placeholder
      if (Tree.Category.Items.Count == 1 && ReferenceEquals(FolderKeywordPlaceHolder, Tree.Category.Items[0]))
        Tree.Category.Items.Clear();

      fk = new(folder.Name, fkRoot);
      fkRoot.Items.SetInOrder(fk, x => ((FolderKeywordM)x).Name);
      All2.Add(fk);
    }

    return fk;
  }

  private static void _linkWithFolder(FolderM f, FolderKeywordM fk) {
    f.FolderKeyword = fk;
    fk.Folders.Add(f);
  }

  public void LinkFolderWithFolderKeyword(FolderM folder, FolderKeywordM folderKeyword) =>
    _linkWithFolder(folder, _getForFolder(folder, folderKeyword));

  public void SetAsFolderKeyword(FolderM folder) {
    All.Add(folder);
    IsModified = true;
    Reload();
  }

  protected override void _onItemDeleted(object sender, FolderM item) =>
    Reload();
}