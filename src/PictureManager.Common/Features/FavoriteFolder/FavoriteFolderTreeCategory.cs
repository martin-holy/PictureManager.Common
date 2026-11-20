using MH.UI.Tree;
using MH.Utils.BaseClasses;
using MH.Utils.Localization;
using PictureManager.Common.Features.Folder;
using PictureManager.Common.Localization;

namespace PictureManager.Common.Features.FavoriteFolder;

public sealed class FavoriteFolderTreeCategory : TreeCategory<FavoriteFolderM> {
  internal static class _loc {
    private const string _prefix = "FavoriteFolder.";
    public const string Title = _prefix + "TreeCategory.Title";
    public const string Add = _prefix + "Add.cmd";

    static _loc() {
      LocalizationManager.Register(typeof(_loc).Assembly, [
        (Title, "Favorites"),
        (Add, "Add to Favorites")
      ]);
    }

    public static void Register() { }
  }

  static FavoriteFolderTreeCategory() { _loc.Register(); }

  public static RelayCommand<FolderM> AddToFavoritesCommand { get; set; } = null!;

  public FavoriteFolderTreeCategory(FavoriteFolderR r) :
    base(new(), Res.IconFolderStar, L.Get(_loc.Title), (int)Category.FavoriteFolders, r) {
    CanMoveItem = true;
    AddToFavoritesCommand = new(x => r.ItemCreate(x!), x => x != null, null, L.Get(_loc.Add));
  }

  protected override void _onItemSelected(object o) =>
    Core.R.Folder.Tree.Category.ScrollTo((o as FavoriteFolderM)?.Folder);
}