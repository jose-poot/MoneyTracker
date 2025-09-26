namespace MoneyTracker.Presentation.Services.Interfaces;

public interface IMediaPickerService
{
    Task<Stream?> TakePhotoAsync();
    Task<Stream?> PickPhotoAsync();
    Task<Stream?> TakeVideoAsync();
    Task<Stream?> PickVideoAsync();
    Task<IEnumerable<Stream>?> PickMultiplePhotosAsync();
}