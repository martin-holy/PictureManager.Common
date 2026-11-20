using MH.UI.Controls;
using MH.Utils.BaseClasses;
using MH.Utils.Imaging;
using MH.Utils.Localization;
using PictureManager.Common.Localization;
using System.Threading.Tasks;

namespace PictureManager.Common.Features.MediaItem;

public sealed class RotationDialog : Dialog {
  internal static class _loc {
    private const string _prefix = "MediaItem.Rotation.";
    public const string Title = _prefix + "dlg.Title";

    static _loc() {
      LocalizationManager.Register(typeof(_loc).Assembly, [
        (Title, "Rotation")
      ]);
    }

    public static void Register() { }
  }

  static RotationDialog() { _loc.Register(); }

  private static RotationDialog? _inst;

  public RelayCommand Rotate90Command { get; }
  public RelayCommand Rotate180Command { get; }
  public RelayCommand Rotate270Command { get; }

  public RotationDialog() : base(L.Get(_loc.Title), Res.IconImageMultiple) {
    Rotate90Command = new(() => Result = (int)Orientation.Rotate90, Res.IconRotateLeft);
    Rotate180Command = new(() => Result = (int)Orientation.Rotate180, Res.IconRotateClockwise);
    Rotate270Command = new(() => Result = (int)Orientation.Rotate270, Res.IconRotateRight);
  }

  public static async Task<Orientation> Open() {
    _inst ??= new();
    var result = await ShowAsync(_inst);

    return result == 0
      ? Orientation.Normal
      : (Orientation)result;
  }
}