
using KikiDeliveryTimeEstimator.Console.Models;
using KikiDeliveryTimeEstimator.Console.Services;
using System.Globalization;

Console.WriteLine("Kiki Delivery — Cost + Time Estimator (per-vehicle specs)");
Console.WriteLine("Enter input. Example:");
Console.WriteLine("100 5");
Console.WriteLine("PKG1 50 30 OFR001");
Console.WriteLine("...");
Console.WriteLine("2");
Console.WriteLine("V1 70 200");
Console.WriteLine("V2 70 200");
Console.WriteLine();

var header = Console.ReadLine()?.Trim();
if (string.IsNullOrEmpty(header)) return;
var hdrParts = header.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
if (hdrParts.Length < 2) { Console.WriteLine("Invalid header"); return; }
double baseCost = double.Parse(hdrParts[0], CultureInfo.InvariantCulture);
int nPackages = int.Parse(hdrParts[1]);

var packages = new List<Package>();
for (int i = 0; i < nPackages; i++)
{
	var line = Console.ReadLine() ?? "";
	if (string.IsNullOrWhiteSpace(line)) { i--; continue; }
	var parts = line.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
	var pkg = new Package
	{
		PackageId = parts[0],
		Weight = double.Parse(parts[1], CultureInfo.InvariantCulture),
		Distance = double.Parse(parts[2], CultureInfo.InvariantCulture),
		OfferCode = parts.Length >= 4 ? parts[3] : string.Empty
	};
	packages.Add(pkg);
}

// vehicles: first read count
var vcountLine = Console.ReadLine()?.Trim();
if (string.IsNullOrEmpty(vcountLine)) { Console.WriteLine("Missing vehicle count"); return; }
int vcount = int.Parse(vcountLine);
var vehicles = new List<Vehicle>();
for (int i = 0; i < vcount; i++)
{
	var vline = Console.ReadLine() ?? "";
	if (string.IsNullOrWhiteSpace(vline)) { i--; continue; }
	var p = vline.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
	var vid = p[0];
	var speed = double.Parse(p[1], CultureInfo.InvariantCulture);
	var cap = double.Parse(p[2], CultureInfo.InvariantCulture);
	vehicles.Add(new Vehicle(vid, cap, speed));
}

// run scheduler
var scheduler = new DeliveryScheduler();
var assignments = scheduler.EstimateDeliveryTimes(packages, vehicles, verbose: true);

// compute delivery costs using your existing DeliveryCostService (assumed present).
// If not present, quick inline compute:
double ComputeCost(Package pkg) => baseCost + (pkg.Weight * 10.0) + (pkg.Distance * 5.0);

Console.WriteLine();
Console.WriteLine("Final Results (in input order):");
Console.WriteLine("PkgId Discount FinalCost DeliveryTime(hrs) Vehicle");
foreach (var pkg in packages)
{
	// For discount, call existing Offer engine. For demo we show 0 or compute if you integrate.
	double discount = 0; // integrate with IOfferRule engine to compute exact discount
	double cost = ComputeCost(pkg);
	double finalCost = Math.Round(cost - discount, 2);
	Console.WriteLine($"{pkg.PackageId} {discount} {finalCost} {pkg.EstimatedDeliveryTimeHours:F2} {pkg.AssignedVehicleId}");
}
