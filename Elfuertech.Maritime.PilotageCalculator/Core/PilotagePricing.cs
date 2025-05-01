namespace Elfuertech.Maritime.PilotageCalculator.Core;

public class PilotagePricing
{
    public string CurrencyType { get; set; } = CurrencyCodes.USD;

    private decimal _price;

    public decimal Price
    {
        get => _price;
        set
        {
            _price = value;
            SetNetPrice();
        }
    }

    private readonly int _discountOrHikePercentage;

    private readonly bool _maneuverCancellation;

    public decimal NetPrice { get; private set; }

    public PilotagePricing(bool maneuverCancellation = false)
    {
        _maneuverCancellation = maneuverCancellation;
    }

    public PilotagePricing(
        decimal price,
        int discountOrHikePercentage, 
        bool maneuverCancellation = false)
    {
        _price = price;
        _discountOrHikePercentage = discountOrHikePercentage;
        _maneuverCancellation = maneuverCancellation;
        
        SetNetPrice();
    }

    private void SetNetPrice()
    {
        var discountOrHikeRate = 1m + _discountOrHikePercentage / 100m;
        var newNetPrice = _price * discountOrHikeRate * (_maneuverCancellation ? 0.5m : 1m);
        NetPrice = Math.Round(newNetPrice, 2, MidpointRounding.AwayFromZero);
    }
}