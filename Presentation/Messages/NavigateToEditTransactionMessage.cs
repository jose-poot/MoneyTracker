using CommunityToolkit.Mvvm.Messaging.Messages;
using MoneyTracker.Application.DTOs;

namespace MoneyTracker.Presentation.Messages;

public sealed class NavigateToEditTransactionMessage : ValueChangedMessage<TransactionDto>
{
    public NavigateToEditTransactionMessage(TransactionDto value) : base(value) { }
}