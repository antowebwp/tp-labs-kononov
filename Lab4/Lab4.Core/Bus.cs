namespace Lab2;

public sealed class Bus : Vehicle
{
    public int PassengerCapacity { get; }
    public decimal DriverPayPerKm { get; }

    public Bus(string model, decimal fuelConsumption, decimal fuelPrice,
        int passengerCapacity, decimal driverPayPerKm)
        : base(model, fuelConsumption, fuelPrice)
    {
        if (passengerCapacity <= 0)
            throw new ArgumentOutOfRangeException(nameof(passengerCapacity));
        if (driverPayPerKm < 0)
            throw new ArgumentOutOfRangeException(nameof(driverPayPerKm));
        PassengerCapacity = passengerCapacity;
        DriverPayPerKm = driverPayPerKm;
    }

    public override decimal CalculateTripCost(decimal distanceKm) =>
        CalculateFuelCost(distanceKm) + distanceKm * DriverPayPerKm;

    public override string GetCapacityDescription() => $"пассажиров: {PassengerCapacity}";
    public override string ToString() =>
        $"Автобус — {base.ToString()}, оплата водителя {DriverPayPerKm} руб./км";
}
