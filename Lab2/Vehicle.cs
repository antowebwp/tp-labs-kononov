namespace Lab2;

public abstract class Vehicle : IComparable<Vehicle>
{
    public string Model { get; }
    public decimal FuelConsumption { get; }
    public decimal FuelPrice { get; }

    protected Vehicle(string model, decimal fuelConsumption, decimal fuelPrice)
    {
        if (string.IsNullOrWhiteSpace(model))
            throw new ArgumentException("Укажите модель.", nameof(model));
        if (fuelConsumption <= 0)
            throw new ArgumentOutOfRangeException(nameof(fuelConsumption));
        if (fuelPrice <= 0)
            throw new ArgumentOutOfRangeException(nameof(fuelPrice));
        Model = model.Trim();
        FuelConsumption = fuelConsumption;
        FuelPrice = fuelPrice;
    }

    public abstract decimal CalculateTripCost(decimal distanceKm);
    public abstract string GetCapacityDescription();

    protected decimal CalculateFuelCost(decimal distanceKm)
    {
        if (distanceKm < 0)
            throw new ArgumentOutOfRangeException(nameof(distanceKm));
        return distanceKm / 100m * FuelConsumption * FuelPrice;
    }

    public int CompareTo(Vehicle? other) => other == null
        ? 1
        : StringComparer.Ordinal.Compare(Model, other.Model);

    public override string ToString() =>
        $"{Model}: {GetCapacityDescription()}, расход {FuelConsumption} л/100 км, топливо {FuelPrice} руб./л";
}
