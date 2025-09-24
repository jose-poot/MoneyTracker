using CommunityToolkit.Mvvm.Messaging.Messages;

namespace MoneyTracker.Presentation.Messages;

public sealed class NavigateBackMessage : ValueChangedMessage<bool>
{
    public NavigateBackMessage() : base(true) { }
}