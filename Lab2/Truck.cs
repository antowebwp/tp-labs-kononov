namespace Lab2;

public sealed class Truck : Vehicle
{
    public decimal PayloadTons { get; }
    public decimal RoadFeePerKm { get; }

    public Truck(string model, decimal fuelConsumption, decimal fuelPrice,
        decimal payloadTons, decimal roadFeePerKm)
        : base(model, fuelConsumption, fuelPrice)
    {
        if (payloadTons <= 0)
            throw new ArgumentOutOfRangeException(nameof(payloadTons));
        if (roadFeePerKm < 0)
            throw new ArgumentOutOfRangeException(nameof(roadFeePerKm));
        PayloadTons = payloadTons;
        RoadFeePerKm = roadFeePerKm;
    }

    public override decimal CalculateTripCost(decimal distanceKm) =>
        CalculateFuelCost(distanceKm) + distanceKm * RoadFeePerKm;

    public override string GetCapacityDescription() => $"грузоподъёмность: {PayloadTons} т";
    public override string ToString() =>
        $"Грузовик — {base.ToString()}, дорожный сбор {RoadFeePerKm} руб./км";
}
