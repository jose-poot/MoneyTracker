namespace MoneyTracker.Presentation.Services.Interfaces;

public interface IDialogService
{
    Task ShowErrorAsync(string message);
    Task ShowInfoAsync(string title, string message);
    Task<bool> ShowConfirmAsync(string title, string message,
        string confirmText = "Confirm", string cancelText = "Cancel");
    Task<string?> ShowPromptAsync(string title, string message,
        string placeholder = "", string initialValue = "");
    void ShowToast(string message);
}