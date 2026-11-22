using MH.UI.Controls;
using MH.UI.Tree;
using MH.Utils.BaseClasses;
using MH.Utils.Localization;
using PictureManager.Common.Features.Folder;
using PictureManager.Common.Localization;

namespace PictureManager.Common.Features.FolderKeyword;

public sealed class FolderKeywordTreeCategory : TreeCategory<FolderM> {
  internal static class _loc {
    private const string _prefix = "FolderKeywords.";
    public const string Title = _prefix + "Tree.Title";
    public const string Add = _prefix + "Add.cmd";

    static _loc() {
      LocalizationManager.Register(typeof(_loc).Assembly, [
        (Title, "Folder Keywords"),
        (Add, "Set as Folder Keyword")
      ]);
    }

    public static void Register() { }
  }

  static FolderKeywordTreeCategory() { _loc.Register(); }

  public static RelayCommand<FolderM> SetAsFolderKeywordCommand { get; set; } = null!;

  public FolderKeywordTreeCategory(FolderKeywordR r, TreeView treeView) :
    base(treeView, Res.IconFolderPuzzle, L.Get(_loc.Title), (int)Category.FolderKeywords, r) {
    SetAsFolderKeywordCommand = new(x => r.SetAsFolderKeyword(x!), x => x != null, null, L.Get(_loc.Add));
  }
}