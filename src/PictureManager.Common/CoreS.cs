using System.Linq;
using MH.Utils;
using MH.Utils.BaseClasses;
using MH.Utils.Localization;
using PictureManager.Common.Features.Folder;
using PictureManager.Common.Features.MediaItem;
using PictureManager.Common.Features.Person;
using PictureManager.Common.Features.Segment;
using PictureManager.Common.Features.Viewer;
using PictureManager.Common.Localization;

namespace PictureManager.Common;

public class CoreS(CoreR coreR) : ObservableObject {
  private static class _loc {
    private const string _prefix = "App.";
    public const string WarnNewDriveDetectedTitle = _prefix + "warn.NewDriveDetected.Title";
    public const string WarnNewDriveDetectedMessage = _prefix + "warn.NewDriveDetected.Message";

    static _loc() {
      LocalizationManager.Register(typeof(_loc).Assembly, [
        (WarnNewDriveDetectedTitle, "A new drive {0} was detected"),
        (WarnNewDriveDetectedMessage, "A new drive was detected and it won't be visible until it is added to the included folders of the current viewer or until the viewer is changed to All.")
      ]);
    }

    public static void Register() { }
  }

  static CoreS() { _loc.Register(); }
  public static void RegisterLocalization() { }

  public FolderS Folder { get; } = new();
  public MediaItemS MediaItem { get; } = new(coreR.MediaItem, coreR.Image, coreR.Video);
  public PersonS Person { get; } = new(coreR.Person);
  public SegmentS Segment { get; } = new(coreR.Segment);
  public ViewerS Viewer { get; } = new(coreR);

  public void AttachEvents() {
    coreR.Folder.ItemCreatedEvent += OnFolderCreated;
    coreR.Person.ItemDeletedEvent += OnPersonDeleted;
    coreR.Segment.ItemDeletedEvent += OnSegmentDeleted;
    coreR.Segment.SegmentsPersonChangedEvent += OnSegmentsPersonChanged;
  }

  private void OnFolderCreated(object? sender, FolderM item) {
    if (item is not DriveM drive || !coreR.Viewer.All.Any(x => x.IsDefault)) return;
    Log.Warning(L.Format(_loc.WarnNewDriveDetectedTitle, drive.Name), L.Get(_loc.WarnNewDriveDetectedMessage));
  }

  private void OnPersonDeleted(object? sender, PersonM item) {
    Person.Selected.Set(item, false);
  }

  private void OnSegmentDeleted(object? sender, SegmentM item) {
    Segment.Selected.Set(item, false);
  }

  private void OnSegmentsPersonChanged(object? sender, (SegmentM[], PersonM?, PersonM[]) e) {
    Segment.Selected.DeselectAll();
  }
}