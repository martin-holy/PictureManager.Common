using MH.UI.Tree;
using MH.Utils.Localization;
using PictureManager.Common.Localization;
using System.Linq;

namespace PictureManager.Common.Features.Rating;

public sealed class RatingTreeCategory : TreeCategory {
  internal static class _loc {
    private const string _prefix = "Rating.";
    public const string Title = _prefix + "TreeCategory.Title";

    static _loc() {
      LocalizationManager.Register(typeof(_loc).Assembly, [
        (Title, "Ratings")
      ]);
    }

    public static void Register() { }
  }

  static RatingTreeCategory() { _loc.Register(); }

  public RatingTreeCategory() : base(new(), Res.IconStar, L.Get(_loc.Title), (int)Category.Ratings) {
    _load();
  }

  protected override void _onItemSelected(object o) =>
    _ = Core.VM.ToggleDialog.Toggle(o as RatingTreeM);

  private void _load() {
    for (var i = 0; i < 6; i++)
      Items.Add(new RatingTreeM(i));
  }

  public RatingTreeM GetRatingByValue(int value) =>
    Items.Cast<RatingTreeM>().Single(x => x.Rating.Value == value);
}
