using MH.UI.Controls;
using MH.UI.Dialogs;
using MH.Utils.BaseClasses;
using MH.Utils.Extensions;
using MH.Utils.Localization;
using PictureManager.Common.Features.Folder;
using PictureManager.Common.Features.GeoName;
using PictureManager.Common.Features.Keyword;
using PictureManager.Common.Features.Person;
using PictureManager.Common.Interfaces;
using PictureManager.Common.Localization;
using PictureManager.Common.Utils;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PictureManager.Common.Features.MediaItem;

public sealed class MediaItemVM : ObservableObject {
  internal static class _loc {
    private const string _prefix = "MediaItems.";

    public const string Comment = "MediaItem.Comment.cmd";
    public const string CommentErrTooLong = "MediaItem.Comment.dlg.err.TooLong";
    public const string CommentMessage = "MediaItem.Comment.dlg.Message";
    public const string CommentTitle = "MediaItem.Comment.dlg.Title";
    public const string LoadByGeoName = _prefix + "LoadByGeoName.cmd";
    public const string LoadByKeyword = _prefix + "LoadByKeyword.cmd";
    public const string LoadByPerson = _prefix + "LoadByPerson.cmd";
    public const string LoadByPeopleOrSegments = _prefix + "LoadByPeopleOrSegments.cmd";
    public const string LoadByPeopleOrSegmentsDlgBtnPeople = _prefix + "LoadByPeopleOrSegments.dlg.btn.People";
    public const string LoadByPeopleOrSegmentsDlgBtnSegments = _prefix + "LoadByPeopleOrSegments.dlg.btn.Segments";
    public const string LoadByPeopleOrSegmentsDlgMessage = _prefix + "LoadByPeopleOrSegments.dlg.Message";
    public const string LoadByPeopleOrSegmentsDlgTitle = _prefix + "LoadByPeopleOrSegments.dlg.Title";
    public const string Rename = "MediaItem.Rename.cmd";
    public const string RenameErrFileExists = "MediaItem.Rename.dlg.err.FileExists";
    public const string RenameErrInvalidCharacters = "MediaItem.Rename.dlg.err.InvalidCharacters";
    public const string RenameMessage = "MediaItem.Rename.dlg.Message";
    public const string RenameTitle = "MediaItem.Rename.dlg.Title";
    public const string ViewSelected = _prefix + "ViewSelected.cmd";
    public const string CopyToFolder = _prefix + "CopyToFolder.cmd";
    public const string CopyToFolderMessage = _prefix + "CopyToFolder.dlg.Message";
    public const string CopyToFolderTitle = _prefix + "CopyToFolder.dlg.Title";
    public const string MoveToFolder = _prefix + "MoveToFolder.cmd";
    public const string MoveToFolderMessage = _prefix + "MoveToFolder.dlg.Message";
    public const string MoveToFolderTitle = _prefix + "MoveToFolder.dlg.Title";
    public const string Delete = _prefix + "Delete.cmd";
    public const string DeleteMessage = _prefix + "Delete.dlg.Message";
    public const string DeleteTitle = _prefix + "Delete.dlg.Title";

    static _loc() {
      LocalizationManager.Register(typeof(_loc).Assembly, [
        (Comment, "Comment"),
        (CommentErrTooLong, "Comment is too long!"),
        (CommentMessage, "Add a comment"),
        (CommentTitle, "Comment"),
        (LoadByGeoName, "Load Media items"),
        (LoadByKeyword, "Load Media items"),
        (LoadByPerson, "Load Media items"),
        (LoadByPeopleOrSegments, "Load Media items with selected People or Segments"),
        (LoadByPeopleOrSegmentsDlgBtnPeople, "People"),
        (LoadByPeopleOrSegmentsDlgBtnSegments, "Segments"),
        (LoadByPeopleOrSegmentsDlgMessage, "Do you want to load Media items from selected People or Segments?"),
        (LoadByPeopleOrSegmentsDlgTitle, "Load Media items"),
        (Rename, "Rename"),
        (RenameErrFileExists, "New file name already exists!"),
        (RenameErrInvalidCharacters, "New file name contains invalid character!"),
        (RenameMessage, "Add a new name"),
        (RenameTitle, "Rename"),
        (ViewSelected, "View selected"),
        (CopyToFolder, "Copy"),
        (CopyToFolderMessage + ".one", "Do you really want to copy {0} media item to\n'{1}'?"),
        (CopyToFolderMessage + ".other", "Do you really want to copy {0} media items to\n'{1}'?"),
        (CopyToFolderTitle, "Copy media items"),
        (MoveToFolder, "Move"),
        (MoveToFolderMessage + ".one", "Do you really want to move {0} media item to\n'{1}'?"),
        (MoveToFolderMessage + ".other", "Do you really want to move {0} media items to\n'{1}'?"),
        (MoveToFolderTitle, "Move media items"),
        (Delete, "Delete"),
        (DeleteMessage + ".one", "Do you really want to delete {0} item?"),
        (DeleteMessage + ".other", "Do you really want to delete {0} items?"),
        (DeleteTitle, "Delete Confirmation")
      ]);
    }

    public static void Register() { }
  }

  static MediaItemVM() { _loc.Register(); }

  private readonly CoreVM _coreVM;
  private readonly MediaItemS _s;
  private MediaItemM? _current;
  private int _itemsCount;

  public static IImageSourceConverter<MediaItemM> ThumbConverter { get; set; } = null!;

  public MediaItemM? Current {
    get => _current;
    set {
      _current = value;
      _updateCommands();
      OnPropertyChanged();
      OnPropertyChanged(nameof(CurrentGeoName));
    }
  }
  public GeoNameM? CurrentGeoName => Current?.GeoLocation?.GeoName;
  public MediaItemsViewsVM Views { get; } = new();
  public int ItemsCount { get => _itemsCount; set { _itemsCount = value; OnPropertyChanged(); } }

  public static AsyncRelayCommand CommentCommand { get; set; } = null!;
  public static AsyncRelayCommand DeleteCommand { get; set; } = null!;
  public static AsyncRelayCommand<GeoNameM> LoadByGeoNameCommand { get; set; } = null!;
  public static AsyncRelayCommand<KeywordM> LoadByKeywordCommand { get; set; } = null!;
  public static AsyncRelayCommand<PersonM> LoadByPersonCommand { get; set; } = null!;
  public static AsyncRelayCommand LoadByPeopleOrSegmentsCommand { get; set; } = null!;
  public static AsyncRelayCommand RenameCommand { get; set; } = null!;
  public static RelayCommand ViewSelectedCommand { get; set; } = null!;
  public static AsyncRelayCommand<FolderM> CopySelectedToFolderCommand { get; private set; } = null!;
  public static AsyncRelayCommand<FolderM> MoveSelectedToFolderCommand { get; private set; } = null!;

  public MediaItemVM(CoreVM coreVM, MediaItemS s) {
    _coreVM = coreVM;
    _s = s;
    CommentCommand = new(_ => _comment(Current!), () => Current != null, Res.IconNotification, L.Get(_loc.Comment));
    DeleteCommand = new(_ => Delete(_coreVM.GetActive<MediaItemM>()), () => _coreVM.AnyActive<MediaItemM>(), Res.IconTrash, L.Get(_loc.Delete));
    LoadByGeoNameCommand = new(_loadBy, Res.IconImageMultiple, L.Get(_loc.LoadByGeoName));
    LoadByKeywordCommand = new(_loadBy, Res.IconImageMultiple, L.Get(_loc.LoadByKeyword));
    LoadByPersonCommand = new(_loadBy, Res.IconImageMultiple, L.Get(_loc.LoadByPerson));
    LoadByPeopleOrSegmentsCommand = new(_loadByPeopleOrSegments, Res.IconImageMultiple, L.Get(_loc.LoadByPeopleOrSegments));
    RenameCommand = new(_ => Rename((RealMediaItemM)Current!), () => Current is RealMediaItemM, null, L.Get(_loc.Rename));
    ViewSelectedCommand = new(_viewSelected, _canViewSelected, Res.IconImageMultiple, L.Get(_loc.ViewSelected));
    CopySelectedToFolderCommand = new(_copySelectedToFolder, _canCopyMoveSelectedToFolder, Res.IconCopy, L.Get(_loc.CopyToFolder));
    MoveSelectedToFolderCommand = new(_moveSelectedToFolder, _canCopyMoveSelectedToFolder, Res.IconMove, L.Get(_loc.MoveToFolder));

    Views.CurrentViewSelectionChangedEvent += (_, _) => _updateCommands();
  }

  private void _updateCommands() {
    CommentCommand.RaiseCanExecuteChanged();
    DeleteCommand.RaiseCanExecuteChanged();
    RenameCommand.RaiseCanExecuteChanged();
    ViewSelectedCommand.RaiseCanExecuteChanged();
  }

  private async Task _comment(MediaItemM mi) {
    var commentDialog = new InputDialog(
      L.Get(_loc.CommentTitle),
      L.Get(_loc.CommentMessage),
      Res.IconNotification,
      mi.Comment,
      x => x != null && x.Length > 256
        ? L.Get(_loc.CommentErrTooLong)
        : null);

    if (await Dialog.ShowAsync(commentDialog) == 1)
      _s.SetComment(mi, StringUtils.NormalizeComment(commentDialog.Answer));
  }

  private Task _loadBy(object? o, CancellationToken token) =>
    _coreVM.MediaItem.Views.LoadByTag(o, token);

  private async Task _loadByPeopleOrSegments(CancellationToken token) {
    var md = new MessageDialog(
      L.Get(_loc.LoadByPeopleOrSegmentsDlgTitle),
      L.Get(_loc.LoadByPeopleOrSegmentsDlgMessage),
      Res.IconImageMultiple,
      true);

    md.Buttons = [
      new(md.SetResult(1, Res.IconPeople, L.Get(_loc.LoadByPeopleOrSegmentsDlgBtnPeople)), true),
      new(md.SetResult(2, Res.IconSegment, L.Get(_loc.LoadByPeopleOrSegmentsDlgBtnSegments)))
    ];

    var result = await Dialog.ShowAsync(md);
    if (result < 1) return;

    var items = result switch {
      1 => Core.S.Person.Selected.Items.ToArray(),
      2 => Core.S.Segment.Selected.Items.ToArray(),
      _ => Array.Empty<object>()
    };

    if (items.Length == 0) return;

    await _coreVM.MediaItem.Views.LoadByTag(items, token);
  }

  private void _viewSelected() {
    var items = Views.Current!.Selected.Items.ToList();
    _coreVM.OpenMediaItems(items, items[0]);
  }

  private bool _canViewSelected() =>
    Views.Current?.Selected.Items.Count > 1;

  private Task _copySelectedToFolder(FolderM? folder, CancellationToken token) =>
    CopyMoveSelectedToFolder(folder, true);

  private Task _moveSelectedToFolder(FolderM? folder, CancellationToken token) =>
    CopyMoveSelectedToFolder(folder, false);

  private bool _canCopyMoveSelectedToFolder(FolderM? folder) =>
    folder is { IsAccessible: true } && Views.Current?.Selected.Items.OfType<RealMediaItemM>().Any() == true;

  public async Task<bool> CopyMoveSelectedToFolder(FolderM? folder, bool copy) {
    if (folder == null || Views.Current?.Selected.Items.OfType<RealMediaItemM>().ToArray() is not { Length: > 0 } items) return false;
    
    var title = copy ? _loc.CopyToFolderTitle : _loc.MoveToFolderTitle;
    var message = copy ? _loc.CopyToFolderMessage : _loc.MoveToFolderMessage;
    if (await Dialog.ShowAsync(new MessageDialog(
      L.Get(title),
      L.FormatPlural(message, items.Length, folder.Name),
      MH.UI.Res.IconQuestion,
      true)) != 1)
      return false;

    await CopyMoveU.CopyMoveMediaItems(items, folder, copy ? FileOperationMode.Copy : FileOperationMode.Move);
    return true;
  }

  public async Task<bool> Delete(MediaItemM[] items) {
    if (items.Length == 0 || await Dialog.ShowAsync(new MessageDialog(
      L.Get(_loc.DeleteTitle),
      L.FormatPlural(_loc.DeleteMessage, items.Length),
      MH.UI.Res.IconQuestion,
      true)) != 1) return false;

    _s.DeleteFromDrive(items);
    return true;
  }

  public Task ReloadMetadata(RealMediaItemM[] items) =>
    ReloadMetadataDialog.Open(items, _s);

  public async Task Rename(RealMediaItemM current) {
    var ext = Path.GetExtension(current.FileName);
    var dlg = new InputDialog(
      L.Get(_loc.RenameTitle),
      L.Get(_loc.RenameMessage),
      Res.IconNotification,
      Path.GetFileNameWithoutExtension(current.FileName),
      answer => {
        var newFileName = answer + ext;

        if (Path.GetInvalidFileNameChars().Any(x => newFileName.IndexOf(x) != -1))
          return L.Get(_loc.RenameErrInvalidCharacters);

        if (File.Exists(IOExtensions.PathCombine(current.Folder.FullPath, newFileName)))
          return L.Get(_loc.RenameErrFileExists);

        return string.Empty;
      });

    if (await Dialog.ShowAsync(dlg) != 1) return;
    _s.Rename(current, dlg.Answer + ext);
  }

  public void OnMetadataChanged(MediaItemM[] items) {
    CurrentGeoName?.OnPropertyChanged(nameof(CurrentGeoName.FullName));
    foreach (var mi in items)
      mi.SetInfoBox(true);
  }

  public void OnViewSelected(MediaItemsViewVM? view) {
    Views.SetCurrentView(view);
    Current = Views.Current?.Selected.Items.Count > 0
      ? Views.Current.Selected.Items[0]
      : null;
  }

  public Task ViewMediaItems(MediaItemM[] items, string name, CancellationToken token) =>
    Views.ViewMediaItems(items, name, token);
}