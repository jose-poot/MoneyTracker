using CommunityToolkit.Mvvm.Messaging.Messages;

namespace MoneyTracker.Presentation.Messages;

public sealed class ShowErrorMessage : ValueChangedMessage<string>
{
    public ShowErrorMessage(string error) : base(error) { }
}