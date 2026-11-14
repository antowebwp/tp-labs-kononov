namespace Lab2;

public sealed class Car : Vehicle
{
    public int Seats { get; }

    public Car(string model, decimal fuelConsumption, decimal fuelPrice, int seats)
        : base(model, fuelConsumption, fuelPrice)
    {
        if (seats <= 0)
            throw new ArgumentOutOfRangeException(nameof(seats));
        Seats = seats;
    }

    public override decimal CalculateTripCost(decimal distanceKm) => CalculateFuelCost(distanceKm);
    public override string GetCapacityDescription() => $"мест с водителем: {Seats}";
    public override string ToString() => $"Легковой автомобиль — {base.ToString()}";
}
