using Android.Content;
using MoneyTracker.Presentation.Services.Interfaces;

namespace MoneyTracker.Presentation.Services;

public partial class AndroidDialogService : IDialogService
{
    private readonly Func<Context> _contextProvider;

    public AndroidDialogService(Func<Context> contextProvider)
    {
        _contextProvider = contextProvider;
    }

    public Task ShowErrorAsync(string message)
    {
        var tcs = new TaskCompletionSource<bool>();
        var context = _contextProvider();

        new AndroidX.AppCompat.App.AlertDialog.Builder(context)
            .SetTitle("Error")
            .SetMessage(message)
            .SetPositiveButton("OK", (s, e) => tcs.SetResult(true))
            .SetOnCancelListener(new DialogCancelListener(() => tcs.SetResult(false)))
            .Show();

        return tcs.Task;
    }

    public Task ShowInfoAsync(string title, string message)
    {
        var tcs = new TaskCompletionSource<bool>();
        var context = _contextProvider();

        new AndroidX.AppCompat.App.AlertDialog.Builder(context)
            .SetTitle(title)
            .SetMessage(message)
            .SetPositiveButton("OK", (s, e) => tcs.SetResult(true))
            .Show();

        return tcs.Task;
    }

    public Task<bool> ShowConfirmAsync(string title, string message,
        string confirmText = "Confirmar", string cancelText = "Cancelar")
    {
        var tcs = new TaskCompletionSource<bool>();
        var context = _contextProvider();

        new AndroidX.AppCompat.App.AlertDialog.Builder(context)
            .SetTitle(title)
            .SetMessage(message)
            .SetPositiveButton(confirmText, (s, e) => tcs.SetResult(true))
            .SetNegativeButton(cancelText, (s, e) => tcs.SetResult(false))
            .SetOnCancelListener(new DialogCancelListener(() => tcs.SetResult(false)))
            .Show();

        return tcs.Task;
    }

    public Task<string?> ShowPromptAsync(string title, string message,
        string placeholder = "", string initialValue = "")
    {
        var tcs = new TaskCompletionSource<string?>();
        var context = _contextProvider();

        var editText = new EditText(context)
        {
            Hint = placeholder,
            Text = initialValue
        };

        new AndroidX.AppCompat.App.AlertDialog.Builder(context)
            .SetTitle(title)
            .SetMessage(message)
            .SetView(editText)
            .SetPositiveButton("OK", (s, e) => tcs.SetResult(editText.Text))
            .SetNegativeButton("Cancelar", (s, e) => tcs.SetResult(null))
            .SetOnCancelListener(new DialogCancelListener(() => tcs.SetResult(null)))
            .Show();

        return tcs.Task;
    }

    public void ShowToast(string message)
    {
        var context = _contextProvider();
        Toast.MakeText(context, message, ToastLength.Short)?.Show();
    }
}