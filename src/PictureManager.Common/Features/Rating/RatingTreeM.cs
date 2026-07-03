using MH.Utils.Tree;

namespace PictureManager.Common.Features.Rating;

public sealed class RatingTreeM(int value) : TreeItem(Res.IconStar, value.ToString()) {
  public RatingM Rating { get; } = new(value);
}