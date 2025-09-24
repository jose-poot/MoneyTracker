using CommunityToolkit.Mvvm.Messaging.Messages;
using MoneyTracker.Application.DTOs;

namespace MoneyTracker.Presentation.Messages;

public sealed class TransactionUpdatedMessage : ValueChangedMessage<TransactionDto>
{
    public TransactionUpdatedMessage(TransactionDto value) : base(value) { }
}