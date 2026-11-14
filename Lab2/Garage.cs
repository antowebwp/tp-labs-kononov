namespace Lab2;

public sealed class Garage
{
    private readonly List<Vehicle> _vehicles = [];

    public IReadOnlyList<Vehicle> Vehicles => _vehicles.AsReadOnly();

    public void Add(Vehicle vehicle)
    {
        ArgumentNullException.ThrowIfNull(vehicle);
        _vehicles.Add(vehicle);
    }

    public decimal GetTotalTripCost(decimal distanceKm)
    {
        if (distanceKm < 0)
            throw new ArgumentOutOfRangeException(nameof(distanceKm));
        return _vehicles.Sum(vehicle => vehicle.CalculateTripCost(distanceKm));
    }

    public List<Vehicle> GetAffordableVehicles(decimal distanceKm, decimal budget)
    {
        if (distanceKm < 0)
            throw new ArgumentOutOfRangeException(nameof(distanceKm));
        if (budget < 0)
            throw new ArgumentOutOfRangeException(nameof(budget));
        return _vehicles.Where(vehicle => vehicle.CalculateTripCost(distanceKm) <= budget)
            .OrderBy(vehicle => vehicle.CalculateTripCost(distanceKm)).ToList();
    }

    public Dictionary<string, int> GetCountsByType() => _vehicles
        .GroupBy(vehicle => vehicle.GetType().Name)
        .ToDictionary(group => group.Key, group => group.Count());
}
