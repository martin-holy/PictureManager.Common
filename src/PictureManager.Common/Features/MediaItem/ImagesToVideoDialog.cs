using MH.UI.Controls;
using MH.UI.Dialogs;
using MH.Utils;
using MH.Utils.BaseClasses;
using MH.Utils.Disposables;
using MH.Utils.Extensions;
using MH.Utils.Imaging;
using MH.Utils.Localization;
using PictureManager.Common.Features.Common;
using PictureManager.Common.Features.Folder;
using PictureManager.Common.Features.MediaItem.Image;
using PictureManager.Common.Localization;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PictureManager.Common.Features.MediaItem;

public sealed class ImagesToVideoDialog : Dialog {
  internal static class _loc {
    private const string _prefix = "Images.ToVideo.dlg.";
    public const string Title = _prefix + "Title";
    public const string TxtSpeed = _prefix + "txt.Speed";
    public const string TxtQuality = _prefix + "txt.Quality";
    public const string TxtHeight = _prefix + "txt.Height";
    public const string BtnCreate = _prefix + "btn.Create";
    public const string ErrFfmpegTitle = _prefix + "err.FFMPEG.dlg.Title";
    public const string ErrFfmpegMessage = _prefix + "err.FFMPEG.dlg.Message";

    static _loc() {
      LocalizationManager.Register(typeof(_loc).Assembly, [
        (Title, "Images to Video"),
        (TxtSpeed, "{0:0.00}s per Image"),
        (TxtQuality, "Quality: {0}"),
        (TxtHeight, "Height:"),
        (BtnCreate, "Create Video"),
        (ErrFfmpegTitle, "FFMPEG not found"),
        (ErrFfmpegMessage , "FFMPEG was not found. Install it and set the path in the settings.")
      ]);
    }

    public static void Register() { }
  }

  static ImagesToVideoDialog() { _loc.Register(); }

  private bool _isBusy;
  private readonly ImageM[] _items;
  private Process? _process;
  private readonly string _inputListPath;
  private readonly string _outputFilePath;
  private readonly string _outputFileName;
  private readonly FolderM _outputFolder;
  private readonly Func<FolderM, string, Task> _onVideoCreated;
  private readonly BindingScope _bindingScope = new();
  private ImagesToVideoSettings _imgToVidSettings = Core.Settings.ImagesToVideo;

  public bool IsBusy { get => _isBusy; set { _isBusy = value; OnPropertyChanged(); } }

  public string TxtSpeed => L.Format(_loc.TxtSpeed, _imgToVidSettings.Speed);
  public string TxtQuality => L.Format(_loc.TxtQuality, _imgToVidSettings.Quality);
  public string TxtHeight { get; } = L.Get(_loc.TxtHeight);

  public ImagesToVideoDialog(ImageM[] items, Func<FolderM, string, Task> onVideoCreated) :
    base(L.Get(_loc.Title), MH.UI.Res.IconMovieClapper) {
    Buttons = [
      new(new AsyncRelayCommand(_createVideo, () => !IsBusy, MH.UI.Res.IconMovieClapper, L.Get(_loc.BtnCreate)), true),
      new(CloseCommand, false, true)
    ];

    _imgToVidSettings.Bind(nameof(ImagesToVideoSettings.Speed), x => x.Speed,
      _ => OnPropertyChanged(nameof(TxtSpeed))).DisposeWith(_bindingScope);
    _imgToVidSettings.Bind(nameof(ImagesToVideoSettings.Quality), x => x.Quality,
      _ => OnPropertyChanged(nameof(TxtQuality))).DisposeWith(_bindingScope);

    _items = items;
    var firstMi = _items.First();
    var fileName = IOExtensions.GetNewFileName(firstMi.Folder.FullPath, firstMi.FileName + ".mp4");

    _inputListPath = Path.GetTempPath() + "PictureManagerImagesToVideo.list";
    _outputFilePath = IOExtensions.PathCombine(firstMi.Folder.FullPath, fileName);
    _outputFileName = fileName;
    _outputFolder = firstMi.Folder;
    _onVideoCreated = onVideoCreated;
  }

  protected override Task _onResultChanged(int result) {
    if (result == 0 && _process != null) {
      _process.Kill();
      _deleteFile(_inputListPath);
      _deleteFile(_outputFilePath);
    }

    _bindingScope.Dispose();

    return Task.CompletedTask;
  }

  // create input list of items for FFMPEG
  private bool _createTempListFile() {
    try {
      using var sw = new StreamWriter(_inputListPath, false, new UTF8Encoding(false), 65536);
      foreach (var item in _items)
        sw.WriteLine($"file '{item.FilePath}'");

      return true;
    }
    catch (Exception ex) {
      Log.Error(ex);
      return false;
    }
  }

  private static void _deleteFile(string path) {
    try {
      if (File.Exists(path))
        File.Delete(path);
    }
    catch (Exception ex) {
      Log.Error(ex);
    }
  }

  public Task CreateVideoAsync() {
    var mi = _items.First();

    // Scale
    var height = (double)_imgToVidSettings.Height;
    var width = mi.Orientation is Orientation.Rotate270 or Orientation.Rotate90
      ? Math.Round(mi.Height / (mi.Width / height), 0)
      : Math.Round(mi.Width / (mi.Height / height), 0);
    if (width % 2 != 0) width++;
    var scale = $"{width}x{height}";

    // Rotate
    var rotation = mi.Orientation switch {
      Orientation.Rotate180 => "transpose=clock,transpose=clock,",
      Orientation.Rotate270 => "transpose=clock:passthrough=portrait,",
      Orientation.Rotate90 => "transpose=cclock:passthrough=portrait,",
      _ => string.Empty
    };

    var speedStr = _imgToVidSettings.Speed.ToString(CultureInfo.InvariantCulture);
    var args = $"-y -r 1/{speedStr} -f concat -safe 0 -i \"{_inputListPath}\" -c:v libx264 -r 25 -preset medium -crf {_imgToVidSettings.Quality} -vf \"{rotation}scale={scale},format=yuv420p\" \"{_outputFilePath}\"";
    var tcs = new TaskCompletionSource<bool>();

    _process = new() {
      EnableRaisingEvents = true,
      StartInfo = new() {
        Arguments = args,
        FileName = Core.Settings.Common.FfmpegPath,
        UseShellExecute = false,
        CreateNoWindow = false
      }
    };

    _process.Exited += (_, _) => {
      tcs.TrySetResult(true);
      _process.Dispose();
    };

    _process.Start();
    return tcs.Task;
  }

  private async Task _createVideo(CancellationToken token) {
    Core.Settings.Save();

    // check for FFMPEG
    if (!File.Exists(Core.Settings.Common.FfmpegPath)) {
      await ShowAsync(new MessageDialog(
        L.Get(_loc.ErrFfmpegTitle),
        L.Get(_loc.ErrFfmpegMessage),
        Res.IconInformation,
        false));
      Result = 0;
      return;
    }

    IsBusy = true;

    if (_createTempListFile()) {
      await CreateVideoAsync();
      _deleteFile(_inputListPath);
      await _onVideoCreated(_outputFolder, _outputFileName);
    }

    Result = 1;
  }
}