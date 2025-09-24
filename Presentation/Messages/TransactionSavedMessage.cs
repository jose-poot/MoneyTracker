using CommunityToolkit.Mvvm.Messaging.Messages;
using MoneyTracker.Application.DTOs;

namespace MoneyTracker.Presentation.Messages;

public sealed class TransactionSavedMessage : ValueChangedMessage<TransactionDto>
{
    public TransactionSavedMessage(TransactionDto value) : base(value) { }
}