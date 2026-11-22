using MH.UI.Tree;
using MH.Utils.Localization;
using PictureManager.Common.Localization;

namespace PictureManager.Common.Features.GeoName;

public sealed class GeoNameTreeCategory(GeoNameR r)
  : TreeCategory<GeoNameM>(new(), Res.IconLocationCheckin, L.Get(_loc.Title), (int)Category.GeoNames, r) {

  internal static class _loc {
    private const string _prefix = "GeoNames.";
    public const string Title = _prefix + "Tree.Title";

    static _loc() {
      LocalizationManager.Register(typeof(_loc).Assembly, [
        (Title, "GeoNames")
      ]);
    }

    public static void Register() { }
  }

  static GeoNameTreeCategory() { _loc.Register(); }

  protected override void _onItemSelected(object o) =>
    _ = Core.VM.ToggleDialog.Toggle(o as GeoNameM);
}