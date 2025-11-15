namespace Spice86.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Spice86.Core.Emulator.Devices.Video;
using Spice86.ViewModels.PropertiesMappers;
using Spice86.ViewModels.Services;
using Spice86.ViewModels.ValueViewModels.Debugging;

using System.Text.Json;

/// <summary>
/// Represents video card view model.
/// </summary>
public partial class VideoCardViewModel : ViewModelBase, IEmulatorObjectViewModel {
    [ObservableProperty]
    private VideoCardInfo _videoCard = new();
    private readonly IVgaRenderer _vgaRenderer;
    private readonly IVideoState _videoState;
    private readonly IHostStorageProvider _storageProvider;

    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>
    /// <param name="vgaRenderer">The vga renderer.</param>
    /// <param name="videoState">The video state.</param>
    /// <param name="storageProvider">The storage provider.</param>
    public VideoCardViewModel(IVgaRenderer vgaRenderer, IVideoState videoState,
        IHostStorageProvider storageProvider) {
        _vgaRenderer = vgaRenderer;
        _videoState = videoState;
        _storageProvider = storageProvider;
    }

    /// <summary>
    /// Gets or sets is visible.
    /// </summary>
    public bool IsVisible { get; set; }

    /// <summary>
    /// Performs the save video card info operation.
    /// </summary>
    /// <returns>The result of the operation.</returns>
    [RelayCommand]
    public async Task SaveVideoCardInfo() {
        await _storageProvider.SaveVideoCardInfoFile(JsonSerializer.Serialize(VideoCard));
    }

    /// <summary>
    /// Updates values.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The e.</param>
    public void UpdateValues(object? sender, EventArgs e) {
        if (!IsVisible) {
            return;
        }
        VisitVgaRenderer(_vgaRenderer);
        VisitVideoState(_videoState);
    }

    private void VisitVgaRenderer(IVgaRenderer vgaRenderer) {
        vgaRenderer.CopyToVideoCardInfo(VideoCard);
    }

    private void VisitVideoState(IVideoState videoState) {
        videoState.CopyToVideoCardInfo(VideoCard);
    }
}