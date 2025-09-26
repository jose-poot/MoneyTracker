using Android.Content;

namespace MoneyTracker.Presentation.Services;

public partial class AndroidDialogService
{
    private class DialogCancelListener : Java.Lang.Object, IDialogInterfaceOnCancelListener
    {
        private readonly Action _onCancel;

        public DialogCancelListener(Action onCancel)
        {
            _onCancel = onCancel;
        }

        public void OnCancel(IDialogInterface dialog)
        {
            _onCancel();
        }
    }
}