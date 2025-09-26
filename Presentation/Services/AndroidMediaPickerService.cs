using Android.Content;
using Android.Provider;
using MoneyTracker.Presentation.Services.Interfaces;

namespace MoneyTracker.Presentation.Services;

public class AndroidMediaPickerService : IMediaPickerService
{
    private readonly Func<Activity> _activityProvider;
    private TaskCompletionSource<Stream?>? _currentTask;

    public AndroidMediaPickerService(Func<Activity> activityProvider)
    {
        _activityProvider = activityProvider;
    }

    public Task<Stream?> TakePhotoAsync()
    {
        _currentTask = new TaskCompletionSource<Stream?>();

        var activity = _activityProvider();
        var intent = new Intent(MediaStore.ActionImageCapture);

        if (intent.ResolveActivity(activity.PackageManager!) != null)
        {
            activity.StartActivityForResult(intent, RequestCodes.TakePhoto);
        }
        else
        {
            _currentTask.SetResult(null);
        }

        return _currentTask.Task;
    }

    public Task<Stream?> PickPhotoAsync()
    {
        _currentTask = new TaskCompletionSource<Stream?>();

        var activity = _activityProvider();
        var intent = new Intent(Intent.ActionPick, MediaStore.Images.Media.ExternalContentUri);
        activity.StartActivityForResult(intent, RequestCodes.PickPhoto);

        return _currentTask.Task;
    }

    public Task<Stream?> TakeVideoAsync()
    {
        _currentTask = new TaskCompletionSource<Stream?>();

        var activity = _activityProvider();
        var intent = new Intent(MediaStore.ActionVideoCapture);

        if (intent.ResolveActivity(activity.PackageManager!) != null)
        {
            activity.StartActivityForResult(intent, RequestCodes.TakeVideo);
        }
        else
        {
            _currentTask.SetResult(null);
        }

        return _currentTask.Task;
    }

    public Task<Stream?> PickVideoAsync()
    {
        _currentTask = new TaskCompletionSource<Stream?>();

        var activity = _activityProvider();
        var intent = new Intent(Intent.ActionPick, MediaStore.Video.Media.ExternalContentUri);
        activity.StartActivityForResult(intent, RequestCodes.PickVideo);

        return _currentTask.Task;
    }

    public Task<IEnumerable<Stream>?> PickMultiplePhotosAsync()
    {
        // Implementation for multiple photo selection
        throw new NotImplementedException("Multiple photo selection not implemented yet");
    }

    // This would be called from your Activity's OnActivityResult
    public void OnActivityResult(int requestCode, Result resultCode, Intent? data)
    {
        if (_currentTask == null) return;

        if (resultCode == Result.Ok && data != null)
        {
            try
            {
                var uri = data.Data;
                if (uri != null)
                {
                    var activity = _activityProvider();
                    var stream = activity.ContentResolver?.OpenInputStream(uri);
                    _currentTask.SetResult(stream);
                }
                else
                {
                    _currentTask.SetResult(null);
                }
            }
            catch (Exception ex)
            {
                _currentTask.SetException(ex);
            }
        }
        else
        {
            _currentTask.SetResult(null);
        }

        _currentTask = null;
    }

    private static class RequestCodes
    {
        public const int TakePhoto = 1001;
        public const int PickPhoto = 1002;
        public const int TakeVideo = 1003;
        public const int PickVideo = 1004;
    }
}