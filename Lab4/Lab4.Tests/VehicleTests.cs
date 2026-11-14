using Lab2;
namespace Lab4.Tests;
public class VehicleTests
{
    [Fact]
    public void Car_HundredKilometers_ReturnsFuelCost()
    {
        var car = new Car("Vesta", 7.5m, 60m, 5);

        decimal cost = car.CalculateTripCost(100);

        Assert.Equal(450m, cost);
    }
    [Fact]
    public void Truck_HundredKilometers_IncludesRoadFee()
    {
        var truck = new Truck("Kamaz", 30m, 70m, 8m, 3m);

        decimal cost = truck.CalculateTripCost(100);

        Assert.Equal(2400m, cost);
    }
    [Fact]
    public void Bus_HundredKilometers_IncludesDriverPay()
    {
        var bus = new Bus("PAZ", 24m, 70m, 42, 15m);

        decimal cost = bus.CalculateTripCost(100);

        Assert.Equal(3180m, cost);
    }
    [Fact]
    public void Car_ZeroDistance_ReturnsZero()
    {
        var car = new Car("Vesta", 7.5m, 60m, 5);

        decimal cost = car.CalculateTripCost(0);

        Assert.Equal(0m, cost);
    }
    [Fact]
    public void Car_NegativeDistance_Throws()
    {
        var car = new Car("Vesta", 7.5m, 60m, 5);

        Action action = () => car.CalculateTripCost(-1);

        Assert.Throws<ArgumentOutOfRangeException>(action);
    }
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Car_InvalidSeats_Throws(int seats)
    {
        string model = "Vesta";

        Action action = () => new Car(model, 7.5m, 60m, seats);

        Assert.Throws<ArgumentOutOfRangeException>(action);
    }
    [Fact]
    public void Vehicle_EmptyModel_Throws()
    {
        string model = " ";

        Action action = () => new Car(model, 7m, 60m, 5);

        Assert.Throws<ArgumentException>(action);
    }
    [Fact]
    public void Truck_NegativeRoadFee_Throws()
    {
        decimal fee = -1;

        Action action = () => new Truck("Kamaz", 30m, 70m, 8m, fee);

        Assert.Throws<ArgumentOutOfRangeException>(action);
    }
    [Fact]
    public void Garage_EmptyCollection_ReturnsZero()
    {
        var garage = new Garage();

        decimal result = garage.GetTotalTripCost(100);

        Assert.Equal(0m, result);
    }
    [Fact]
    public void Garage_AddVehicle_ChangesCollection()
    {
        var garage = new Garage();
        var car = new Car("Vesta", 7m, 60m, 5);

        garage.Add(car);

        Assert.Same(car, Assert.Single(garage.Vehicles));
    }
    [Fact]
    public void Garage_AddNull_ThrowsWithoutChangingState()
    {
        var garage = new Garage();

        Action action = () => garage.Add(null!);

        Assert.Throws<ArgumentNullException>(action);
        Assert.Empty(garage.Vehicles);
    }
    [Fact]
    public void Garage_MixedVehicles_SumsCosts()
    {
        var garage = new Garage();
        garage.Add(new Car("Vesta", 7.5m, 60m, 5));
        garage.Add(new Truck("Kamaz", 30m, 70m, 8m, 3m));

        decimal total = garage.GetTotalTripCost(100);

        Assert.Equal(2850m, total);
    }
    [Fact]
    public void Garage_BudgetBoundary_IncludesEqualCost()
    {
        var garage = new Garage();
        garage.Add(new Car("Vesta", 7.5m, 60m, 5));

        var result = garage.GetAffordableVehicles(100, 450m);

        Assert.Single(result);
    }
    [Fact]
    public void Vehicle_ListSort_OrdersByModel()
    {
        List<Vehicle> vehicles = [new Car("B", 7m, 60m, 5), new Bus("A", 20m, 60m, 30, 10m)];

        vehicles.Sort();

        Assert.Equal(new[] { "A", "B" }, vehicles.Select(vehicle => vehicle.Model));
    }
    [Fact]
    public void Garage_GroupedVehicles_ReturnsCounts()
    {
        var garage = new Garage();
        garage.Add(new Car("A", 7m, 60m, 5));
        garage.Add(new Car("B", 7m, 60m, 5));

        var counts = garage.GetCountsByType();

        Assert.Equal(2, counts["Car"]);
    }
}
