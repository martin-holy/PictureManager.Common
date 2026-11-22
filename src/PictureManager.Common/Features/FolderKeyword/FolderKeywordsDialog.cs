using MH.UI.Controls;
using MH.UI.Dialogs;
using MH.Utils.BaseClasses;
using MH.Utils.Localization;
using PictureManager.Common.Features.Folder;
using PictureManager.Common.Localization;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace PictureManager.Common.Features.FolderKeyword;

public sealed class FolderKeywordsDialog : Dialog {
  internal static class _loc {
    private const string _prefix = "FolderKeywords.";
    public const string Title = _prefix + "List.dlg.Title";
    public const string Open = _prefix + "List.Open.cmd";
    public const string Remove = _prefix + "List.dlg.btn.Remove";
    public const string RemoveTitle = _prefix + "List.Remove.dlg.Title";
    public const string RemoveMessage = _prefix + "List.Remove.dlg.Message";

    static _loc() {
      LocalizationManager.Register(typeof(_loc).Assembly, [
        (Title, "Folder Keywords"),
        (Open, "Folder Keywords list"),
        (Remove, "Remove"),
        (RemoveTitle, "Remove Confirmation"),
        (RemoveMessage, "Are you sure?")
      ]);
    }

    public static void Register() { }
  }

  static FolderKeywordsDialog() {
    _loc.Register();

    OpenCommand = new(_ => ShowAsync(new FolderKeywordsDialog()), null, L.Get(_loc.Open));
  }

  private FolderM? _selectedFolder;

  public FolderM? SelectedFolder { get => _selectedFolder; set { _selectedFolder = value; OnPropertyChanged(); } }
  public ObservableCollection<FolderM> Items { get; } = [];
  public RelayCommand<FolderM> SelectCommand { get; }

  public static AsyncRelayCommand OpenCommand { get; } = null!;

  public FolderKeywordsDialog() : base(L.Get(_loc.Title), Res.IconFolderPuzzle) {
    SelectCommand = new(x => SelectedFolder = x);
    Buttons = [
      new(new AsyncRelayCommand(_ => _remove(SelectedFolder!), () => SelectedFolder != null, MH.UI.Res.IconXCross, L.Get(_loc.Remove))),
      new(CloseCommand, false, true)
    ];

    foreach (var folder in Core.R.FolderKeyword.All.OrderBy(x => x.FullPath))
      Items.Add(folder);
  }

  private async Task _remove(FolderM folder) {
    if (await ShowAsync(new MessageDialog(L.Get(_loc.RemoveTitle), L.Get(_loc.RemoveMessage), MH.UI.Res.IconQuestion, true)) != 1) return;

    Core.R.FolderKeyword.ItemDelete(folder);
    Items.Remove(folder);
  }
}