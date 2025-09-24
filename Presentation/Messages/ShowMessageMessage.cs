using CommunityToolkit.Mvvm.Messaging.Messages;

namespace MoneyTracker.Presentation.Messages;

public sealed class ShowMessageMessage : ValueChangedMessage<string>
{
    public ShowMessageMessage(string text) : base(text) { }
}