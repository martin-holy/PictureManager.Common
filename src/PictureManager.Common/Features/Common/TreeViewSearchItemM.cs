using MH.UI.Tree;
using MH.Utils.BaseClasses;
using MH.Utils.Tree;

namespace PictureManager.Common.Features.Common;

public sealed class TreeViewSearchItemM(string icon, string name, ITreeItem data, string? toolTip, TreeCategory category)
  : ListItem(icon, name, data) {
  public string? ToolTip { get; } = toolTip;
  public TreeCategory Category { get; } = category;
}