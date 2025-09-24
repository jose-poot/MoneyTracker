using CommunityToolkit.Mvvm.Messaging.Messages;

namespace MoneyTracker.Presentation.Messages;

public sealed class NavigateToAddTransactionMessage : ValueChangedMessage<bool>
{
    public NavigateToAddTransactionMessage() : base(true) { }
}