using MoneyTracker.Presentation.Base.Enums;

namespace MoneyTracker.Presentation.Base.Interfaces;

public interface IBindingModeAware
{
    BindingMode Mode { get; set; }
}