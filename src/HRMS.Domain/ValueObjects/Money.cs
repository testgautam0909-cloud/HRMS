namespace HRMS.Domain.ValueObjects;

public record Money
{
    public decimal Amount { get; init; }
    public string Currency { get; init; } = "INR";

    public Money(decimal amount, string currency = "INR")
    {
        Amount = amount;
        Currency = currency;
    }

    public static Money Zero => new(0);

    public Money Add(Money other)
    {
        return new Money(Amount + other.Amount, Currency);
    }

    public Money Subtract(Money other)
    {
        return new Money(Amount - other.Amount, Currency);
    }

    public Money Multiply(decimal factor)
    {
        return new Money(Math.Round(Amount * factor, 2), Currency);
    }
}
