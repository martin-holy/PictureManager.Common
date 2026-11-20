using MH.UI.Tree;
using MH.Utils.Localization;
using PictureManager.Common.Localization;

namespace PictureManager.Common.Features.Viewer;

public sealed class ViewerTreeCategory(ViewerR r)
  : TreeCategory<ViewerM>(new(), Res.IconEye, L.Get(_loc.Title), (int)Category.Viewers, r) {

  internal static class _loc {
    private const string _prefix = "Viewer.";
    public const string Title = _prefix + "TreeCategory.Title";

    static _loc() {
      LocalizationManager.Register(typeof(_loc).Assembly, [
        (Title, "Viewers"),
      ]);
    }

    public static void Register() { }
  }

  static ViewerTreeCategory() { _loc.Register(); }

  protected override void _onItemSelected(object o) =>
    Core.VM.Viewer.OpenDetail(o as ViewerM);
}