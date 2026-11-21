using MH.Utils;
using MH.Utils.BaseClasses;
using MH.Utils.Interfaces;
using MH.Utils.Localization;
using MH.Utils.Tree;
using PictureManager.Common.Features.CategoryGroup;
using PictureManager.Common.Features.Folder;
using PictureManager.Common.Features.Keyword;
using PictureManager.Common.Localization;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using static MH.Utils.DragDropHelper;

namespace PictureManager.Common.Features.Viewer;

public sealed class ViewerVM : ObservableObject {
  internal static class _loc {
    private const string _prefix = "Viewer.";
    public const string TxtIncludedFolders = _prefix + "Detail.txt.IncludedFolders";
    public const string AddIncludedFolder = _prefix + "AddIncludedFolder.cmd";
    public const string RemoveIncludedFolder = _prefix + "Detail.RemoveIncludedFolder.cmd";
    public const string TxtExcludedFolders = _prefix + "Detail.txt.ExcludedFolders";
    public const string AddExcludedFolder = _prefix + "AddExcludedFolder.cmd";
    public const string RemoveExcludedFolder = _prefix + "Detail.RemoveExcludedFolder.cmd";
    public const string TxtExcludedKeywords = _prefix + "Detail.txt.ExcludedKeywords";
    public const string AddExcludedKeyword = _prefix + "AddExcludedKeyword.cmd";
    public const string RemoveExcludedKeyword = _prefix + "Detail.RemoveExcludedKeyword.cmd";
    public const string TxtCategoryGroups = _prefix + "Detail.txt.CategoryGroups";
    public const string TabName = _prefix + "Detail.TabName";
    public const string TxtToolBarAll = _prefix + "ToolBar.txt.All";

    static _loc() {
      LocalizationManager.Register(typeof(_loc).Assembly, [
        (TxtIncludedFolders, "Included Folders"),
        (AddIncludedFolder, "Add to included"),
        (RemoveIncludedFolder, "Remove"),
        (TxtExcludedFolders, "Excluded Folders"),
        (AddExcludedFolder, "Add to excluded"),
        (RemoveExcludedFolder, "Remove"),
        (TxtExcludedKeywords, "Excluded Keywords"),
        (AddExcludedKeyword, "Add to excluded"),
        (RemoveExcludedKeyword, "Remove"),
        (TxtCategoryGroups, "Category Groups"),
        (TabName, "Viewer"),
        (TxtToolBarAll, "All")
      ]);
    }

    public static void Register() { }
  }

  static ViewerVM() { _loc.Register(); }

  private readonly ViewerR _r;
  private ViewerM _selected = null!;

  public ExtObservableCollection<ITreeItem> All => _r.Tree.Items;
  public ViewerM Selected { get => _selected; set { _selected = value; OnPropertyChanged(); } }
  public ObservableCollection<IListItem> CategoryGroups { get; } = [];

  public CanDragFunc CanDragFolder { get; set; }
  public CanDropFunc CanDropFolderIncluded { get; }
  public DoDropAction DoDropFolderIncluded { get; }
  public CanDropFunc CanDropFolderExcluded { get; }
  public DoDropAction DoDropFolderExcluded { get; }
  public CanDropFunc CanDropKeyword { get; }
  public DoDropAction DoDropKeyword { get; }

  public RelayCommand UpdateExcludedCategoryGroupsCommand { get; }
  public static RelayCommand<ViewerM> ChangeCurrentCommand { get; set; } = null!;
  public static RelayCommand<FolderM> AddIncludedFolderCommand { get; set; } = null!;
  public static RelayCommand<FolderM> RemoveIncludedFolderCommand { get; set; } = null!;
  public static RelayCommand<FolderM> AddExcludedFolderCommand { get; set; } = null!;
  public static RelayCommand<FolderM> RemoveExcludedFolderCommand { get; set; } = null!;
  public static RelayCommand<KeywordM> AddExcludedKeywordCommand { get; set; } = null!;
  public static RelayCommand<KeywordM> RemoveExcludedKeywordCommand { get; set; } = null!;

  public string TxtIncludedFolders { get; } = L.Get(_loc.TxtIncludedFolders);
  public string TxtExcludedFolders { get; } = L.Get(_loc.TxtExcludedFolders);
  public string TxtExcludedKeywords { get; } = L.Get(_loc.TxtExcludedKeywords);
  public string TxtCategoryGroups { get; } = L.Get(_loc.TxtCategoryGroups);
  public string TxtToolBarAll { get; } = L.Get(_loc.TxtToolBarAll);

  public ViewerVM(ViewerR r, ViewerS s) {
    _r = r;
    CanDragFolder = source => source is FolderM ? source : null;
    CanDropFolderIncluded = (a, b, c) => _canDropFolder(a, b, c, true);
    CanDropFolderExcluded = (a, b, c) => _canDropFolder(a, b, c, false);
    DoDropFolderIncluded = (a, b) => _doDropFolder(a, b, true);
    DoDropFolderExcluded = (a, b) => _doDropFolder(a, b, false);
    CanDropKeyword = _canDropKeywordMethod;
    DoDropKeyword = _doDropKeywordMethod;

    UpdateExcludedCategoryGroupsCommand = new(_updateExcludedCategoryGroups);
    ChangeCurrentCommand = new(s.ChangeCurrent, Res.IconEye);
    AddIncludedFolderCommand = new(x => _r.AddFolder(Selected, x!, true), x => x != null && Selected != null, Res.IconFolder, L.Get(_loc.AddIncludedFolder));
    RemoveIncludedFolderCommand = new(x => _r.RemoveFolder(Selected, x!, true), x => x != null && Selected != null, Res.IconFolder, L.Get(_loc.RemoveIncludedFolder));
    AddExcludedFolderCommand = new(x => _r.AddFolder(Selected, x!, false), x => x != null && Selected != null, Res.IconFolder, L.Get(_loc.AddExcludedFolder));
    RemoveExcludedFolderCommand = new(x => _r.RemoveFolder(Selected, x!, false), x => x != null && Selected != null, Res.IconFolder, L.Get(_loc.RemoveExcludedFolder));
    AddExcludedKeywordCommand = new(x => _r.AddKeyword(Selected, x!), x => x != null && Selected != null, Res.IconTagLabel, L.Get(_loc.AddExcludedKeyword));
    RemoveExcludedKeywordCommand = new(x => _r.RemoveKeyword(Selected, x!), x => x != null && Selected != null, Res.IconTagLabel, L.Get(_loc.RemoveExcludedKeyword));
  }

  private void _updateExcludedCategoryGroups() {
    Selected.ExcludedCategoryGroups.Clear();
    foreach (var cg in CategoryGroups.Where(x => !x.IsSelected))
      Selected.ExcludedCategoryGroups.Add((CategoryGroupM)cg.Data!);

    _r.IsModified = true;
  }

  public void OpenDetail(ViewerM? viewer) {
    if (viewer == null) return;

    var groups = Core.R.CategoryGroup.All
      .OrderBy(x => x.Category)
      .ThenBy(x => x.Name)
      .Select(x => new ListItem(x))
      .ToArray();

    CategoryGroups.Clear();
    foreach (var cg in groups)
      CategoryGroups.Add(cg);

    foreach (var licg in CategoryGroups)
      licg.IsSelected = !viewer.ExcludedCategoryGroups.Contains((CategoryGroupM)licg.Data!);

    Selected = viewer;
    Core.VM.MainTabs.Activate(Res.IconEye, L.Get(_loc.TabName), this);
  }

  private DragDropEffects _canDropFolder(object? target, object? data, bool haveSameOrigin, bool included) {
    if (data is not FolderM folder)
      return DragDropEffects.None;

    if (!haveSameOrigin)
      return (included
          ? Selected.IncludedFolders
          : Selected.ExcludedFolders)
        .Contains(folder)
          ? DragDropEffects.None
          : DragDropEffects.Copy;

    return ReferenceEquals(folder, target)
      ? DragDropEffects.None
      : DragDropEffects.Move;
  }

  private Task _doDropFolder(object data, bool haveSameOrigin, bool included) {
    if (haveSameOrigin)
      _r.RemoveFolder(Selected, (FolderM)data, included);
    else
      _r.AddFolder(Selected, (FolderM)data, included);

    return Task.CompletedTask;
  }

  private DragDropEffects _canDropKeywordMethod(object? target, object? data, bool haveSameOrigin) {
    if (data is not KeywordM keyword)
      return DragDropEffects.None;

    if (haveSameOrigin)
      return ReferenceEquals(keyword, target)
        ? DragDropEffects.None
        : DragDropEffects.Move;

    return Selected.ExcludedKeywords.Contains(keyword)
      ? DragDropEffects.None
      : DragDropEffects.Copy;
  }

  private Task _doDropKeywordMethod(object data, bool haveSameOrigin) {
    if (haveSameOrigin)
      _r.RemoveKeyword(Selected, (KeywordM)data);
    else
      _r.AddKeyword(Selected, (KeywordM)data);

    return Task.CompletedTask;
  }
}