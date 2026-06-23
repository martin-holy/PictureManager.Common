using MH.Utils.DB.Repositories;
using PictureManager.Common.Features.Folder;
using System.Linq;

namespace PictureManager.Common.Features.FavoriteFolder;

public sealed class FavoriteFolderR : TreeRepository<FavoriteFolderM> {
  public FavoriteFolderTreeCategory Tree { get; }
  public FavoriteFolderDS DataSource { get; }

  public FavoriteFolderR(CoreR coreR) {
    Tree = new(this);
    DataSource = new(coreR, this);
  }

  public void ItemCreate(FolderM folder) =>
    TreeItemCreate(new(GetNextId(), folder.Name, folder) { Parent = Tree });

  public void ItemDeleteByFolder(FolderM folder) {
    if (All.SingleOrDefault(x => ReferenceEquals(x.Folder, folder)) is { } ff)
      ItemDelete(ff);
  }

  protected override void _onItemDeleted(object sender, FavoriteFolderM item) {
    item.Parent!.Items.Remove(item);
  }
}