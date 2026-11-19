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
    private const string _prefix = "MediaItem.";

    public const string Comment = _prefix + "Comment.cmd";
    public const string CommentDlgErrTooLong = _prefix + "Comment.dlg.err.TooLong";
    public const string CommentDlgMessage = _prefix + "Comment.dlg.Message";
    public const string CommentDlgTitle = _prefix + "Comment.dlg.Title";
    public const string LoadByGeoName = _prefix + "LoadByGeoName.cmd";
    public const string LoadByKeyword = _prefix + "LoadByKeyword.cmd";
    public const string LoadByPerson = _prefix + "LoadByPerson.cmd";
    public const string LoadByPeopleOrSegments = _prefix + "LoadByPeopleOrSegments.cmd";
    public const string LoadByPeopleOrSegmentsDlgBtnPeople = _prefix + "LoadByPeopleOrSegments.dlg.btn.People";
    public const string LoadByPeopleOrSegmentsDlgBtnSegments = _prefix + "LoadByPeopleOrSegments.dlg.btn.Segments";
    public const string LoadByPeopleOrSegmentsDlgMessage = _prefix + "LoadByPeopleOrSegments.dlg.Message";
    public const string LoadByPeopleOrSegmentsDlgTitle = _prefix + "LoadByPeopleOrSegments.dlg.Title";
    public const string Rename = _prefix + "Rename.cmd";
    public const string RenameDlgErrFileExists = _prefix + "Rename.dlg.err.FileExists";
    public const string RenameDlgErrInvalidCharacters = _prefix + "Rename.dlg.err.InvalidCharacters";
    public const string RenameDlgMessage = _prefix + "Rename.dlg.Message";
    public const string RenameDlgTitle = _prefix + "Rename.dlg.Title";
    public const string ViewSelected = _prefix + "ViewSelected.cmd";
    public const string CopySelectedToFolder = _prefix + "CopySelectedToFolder.cmd";
    public const string CopySelectedToFolderDlgMessage = _prefix + "CopySelectedToFolder.dlg.Message";
    public const string CopySelectedToFolderDlgTitle = _prefix + "CopySelectedToFolder.dlg.Title";
    public const string MoveSelectedToFolder = _prefix + "MoveSelectedToFolder.cmd";
    public const string MoveSelectedToFolderDlgMessage = _prefix + "MoveSelectedToFolder.dlg.Message";
    public const string MoveSelectedToFolderDlgTitle = _prefix + "MoveSelectedToFolder.dlg.Title";
    public const string Delete = _prefix + "Delete.cmd";
    public const string DeleteDlgMessage = _prefix + "Delete.dlg.Message";
    public const string DeleteDlgTitle = _prefix + "Delete.dlg.Title";

    static _loc() {
      LocalizationManager.Register(typeof(_loc).Assembly, [
        (Comment, "Comment"),
        (CommentDlgErrTooLong, "Comment is too long!"),
        (CommentDlgMessage, "Add a comment"),
        (CommentDlgTitle, "Comment"),
        (LoadByGeoName, "Load Media items"),
        (LoadByKeyword, "Load Media items"),
        (LoadByPerson, "Load Media items"),
        (LoadByPeopleOrSegments, "Load Media items with selected People or Segments"),
        (LoadByPeopleOrSegmentsDlgBtnPeople, "People"),
        (LoadByPeopleOrSegmentsDlgBtnSegments, "Segments"),
        (LoadByPeopleOrSegmentsDlgMessage, "Do you want to load Media items from selected People or Segments?"),
        (LoadByPeopleOrSegmentsDlgTitle, "Load Media items"),
        (Rename, "Rename"),
        (RenameDlgErrFileExists, "New file name already exists!"),
        (RenameDlgErrInvalidCharacters, "New file name contains invalid character!"),
        (RenameDlgMessage, "Add a new name"),
        (RenameDlgTitle, "Rename"),
        (ViewSelected, "View selected"),
        (CopySelectedToFolder, "Copy"),
        (CopySelectedToFolderDlgMessage + ".one", "Do you really want to copy {0} media item to\n'{1}'?"),
        (CopySelectedToFolderDlgMessage + ".other", "Do you really want to copy {0} media items to\n'{1}'?"),
        (CopySelectedToFolderDlgTitle, "Copy media items"),
        (MoveSelectedToFolder, "Move"),
        (MoveSelectedToFolderDlgMessage + ".one", "Do you really want to move {0} media item to\n'{1}'?"),
        (MoveSelectedToFolderDlgMessage + ".other", "Do you really want to move {0} media items to\n'{1}'?"),
        (MoveSelectedToFolderDlgTitle, "Move media items"),
        (Delete, "Delete"),
        (DeleteDlgMessage + ".one", "Do you really want to delete {0} item?"),
        (DeleteDlgMessage + ".other", "Do you really want to delete {0} items?"),
        (DeleteDlgTitle, "Delete Confirmation")
      ]);
    }

    public static void Register() { }
  }

  static MediaItemVM() { _loc.Register(); }
  public static void RegisterLocalization() => _loc.Register();

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
    CopySelectedToFolderCommand = new(_copySelectedToFolder, _canCopyMoveSelectedToFolder, Res.IconCopy, L.Get(_loc.CopySelectedToFolder));
    MoveSelectedToFolderCommand = new(_moveSelectedToFolder, _canCopyMoveSelectedToFolder, Res.IconMove, L.Get(_loc.MoveSelectedToFolder));

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
      L.Get(_loc.CommentDlgTitle),
      L.Get(_loc.CommentDlgMessage),
      Res.IconNotification,
      mi.Comment,
      x => x != null && x.Length > 256
        ? L.Get(_loc.CommentDlgErrTooLong)
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
    
    var title = copy ? _loc.CopySelectedToFolderDlgTitle : _loc.MoveSelectedToFolderDlgTitle;
    var message = copy ? _loc.CopySelectedToFolderDlgMessage : _loc.MoveSelectedToFolderDlgMessage;
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
      L.Get(_loc.DeleteDlgTitle),
      L.FormatPlural(_loc.DeleteDlgMessage, items.Length),
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
      L.Get(_loc.RenameDlgTitle),
      L.Get(_loc.RenameDlgMessage),
      Res.IconNotification,
      Path.GetFileNameWithoutExtension(current.FileName),
      answer => {
        var newFileName = answer + ext;

        if (Path.GetInvalidFileNameChars().Any(x => newFileName.IndexOf(x) != -1))
          return L.Get(_loc.RenameDlgErrInvalidCharacters);

        if (File.Exists(IOExtensions.PathCombine(current.Folder.FullPath, newFileName)))
          return L.Get(_loc.RenameDlgErrFileExists);

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