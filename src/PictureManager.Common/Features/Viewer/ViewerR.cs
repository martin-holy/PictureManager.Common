using MH.Utils.DB.Repositories;
using MH.Utils.Extensions;
using MH.Utils.Interfaces;
using PictureManager.Common.Features.Folder;
using PictureManager.Common.Features.Keyword;

namespace PictureManager.Common.Features.Viewer;

public sealed class ViewerR : TreeRepository<ViewerM> {
  public ViewerTreeCategory Tree { get; }
  public ViewerDS DataSource { get; }

  public ViewerR(CoreR coreR) {
    Tree = new(this);
    DataSource = new(coreR, this);
  }

  public override ViewerM ItemCreate(ITreeItem parent, string name) =>
    TreeItemCreate(new(GetNextId(), name, parent));

  protected override void _onItemDeleted(object sender, ViewerM item) {
    item.Parent!.Items.Remove(item);
    item.Parent = null;
    item.IncludedFolders.Clear();
    item.ExcludedFolders.Clear();
    item.ExcludedKeywords.Clear();
  }

  public void AddFolder(ViewerM viewer, FolderM folder, bool included) {
    (included ? viewer.IncludedFolders : viewer.ExcludedFolders).SetInOrder(folder, x => x.FullPath);
    IsModified = true;
  }

  public void RemoveFolder(ViewerM viewer, FolderM folder, bool included) {
    (included ? viewer.IncludedFolders : viewer.ExcludedFolders).Remove(folder);
    IsModified = true;
  }

  public void AddKeyword(ViewerM viewer, KeywordM keyword) {
    viewer.ExcludedKeywords.SetInOrder(keyword, x => x.FullName);
    IsModified = true;
  }

  public void RemoveKeyword(ViewerM viewer, KeywordM keyword) {
    viewer.ExcludedKeywords.Remove(keyword);
    IsModified = true;
  }
}