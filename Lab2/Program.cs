using Lab2;

Console.OutputEncoding = System.Text.Encoding.UTF8;

List<Vehicle> vehicles =
[
    new Car("Лада Веста", 7.5m, 60m, 5),
    new Truck("КамАЗ 5320", 30m, 70m, 8m, 3m),
    new Bus("ПАЗ 3205", 24m, 70m, 42, 15m),
    new Car("Toyota Corolla", 6.5m, 60m, 5),
    new Bus("ЛиАЗ 5292", 32m, 70m, 100, 20m)
];

const decimal distanceKm = 100m;
const decimal budget = 2500m;
var garage = new Garage();

Console.WriteLine($"Автопарк. Поездка на {distanceKm} км.");
foreach (Vehicle vehicle in vehicles)
{
    garage.Add(vehicle);
    Console.WriteLine(vehicle);
    Console.WriteLine($"Вместимость: {vehicle.GetCapacityDescription()}");
    Console.WriteLine($"Стоимость поездки: {vehicle.CalculateTripCost(distanceKm):F2} руб.");
}

Console.WriteLine("\nСортировка по модели:");
vehicles.Sort();
foreach (Vehicle vehicle in vehicles)
    Console.WriteLine(vehicle.Model);

Console.WriteLine($"\nОбщая стоимость поездки автопарка: {garage.GetTotalTripCost(distanceKm):F2} руб.");
Console.WriteLine($"Транспорт с ценой поездки до {budget} руб. по возрастанию стоимости:");
foreach (Vehicle vehicle in garage.GetAffordableVehicles(distanceKm, budget))
    Console.WriteLine($"{vehicle.Model}: {vehicle.CalculateTripCost(distanceKm):F2} руб.");

Console.WriteLine("\nКоличество машин по типам:");
foreach (var group in garage.GetCountsByType())
    Console.WriteLine($"{group.Key}: {group.Value}");
