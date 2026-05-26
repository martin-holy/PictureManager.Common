using MH.Utils;
using MH.Utils.BaseClasses;
using MH.Utils.Interfaces;
using PictureManager.Common.Features.CategoryGroup;
using PictureManager.Common.Features.Folder;
using PictureManager.Common.Features.Keyword;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using static MH.Utils.DragDropHelper;

namespace PictureManager.Common.Features.Viewer;

public sealed class ViewerVM : ObservableObject {
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
  public static RelayCommand<FolderM> AddExcludedFolderCommand { get; set; } = null!;
  public static RelayCommand<KeywordM> AddExcludedKeywordCommand { get; set; } = null!;

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
    AddIncludedFolderCommand = new(x => _r.AddFolder(Selected, x!, true), x => x != null && Selected != null, Res.IconFolder, "Add to included");
    AddExcludedFolderCommand = new(x => _r.AddFolder(Selected, x!, false), x => x != null && Selected != null, Res.IconFolder, "Add to excluded");
    AddExcludedKeywordCommand = new(x => _r.AddKeyword(Selected, x!), x => x != null && Selected != null, Res.IconTagLabel, "Add to excluded");
  }

  private void _updateExcludedCategoryGroups() {
    Selected.ExcludedCategoryGroups.Clear();
    foreach (var cg in CategoryGroups.Where(x => !x.IsSelected))
      Selected.ExcludedCategoryGroups.Add((CategoryGroupM)cg.Data!);

    _r.IsModified = true;
  }

  public void OpenDetail(ViewerM? viewer) {
    if (viewer == null) return;
    Core.VM.MainTabs.Activate(Res.IconEye, "Viewer", this);
    Selected = viewer;

    var groups = Core.R.CategoryGroup.All
      .OrderBy(x => x.Category)
      .ThenBy(x => x.Name)
      .Select(x => new ListItem(x))
      .ToArray();

    CategoryGroups.Clear();
    foreach (var cg in groups)
      CategoryGroups.Add(cg);

    foreach (var licg in CategoryGroups)
      licg.IsSelected = !Selected.ExcludedCategoryGroups.Contains((CategoryGroupM)licg.Data!);
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