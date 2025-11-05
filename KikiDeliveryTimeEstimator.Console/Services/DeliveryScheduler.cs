using KikiDeliveryTimeEstimator.Console.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KikiDeliveryTimeEstimator.Console.Services
{
	public class DeliveryScheduler
	{
		// threshold to run exact subset search on remaining packages:
		// exact search is exponential, safe up to ~20
		private readonly int _exactSearchThreshold;

		public DeliveryScheduler(int exactSearchThreshold = 20)
		{
			_exactSearchThreshold = exactSearchThreshold;
		}

		/// <summary>
		/// Estimates delivery times for packages by assigning them to vehicles.
		/// Modifies packages in-place (EstimatedDeliveryTimeHours, AssignedVehicleId).
		/// Returns list of assignments (for verbose logging).
		/// </summary>
		public IList<DeliveryAssignment> EstimateDeliveryTimes(
			IList<Package> packages,
			IList<Vehicle> vehicles,
			bool verbose = false)
		{
			if (packages == null) throw new ArgumentNullException(nameof(packages));
			if (vehicles == null || vehicles.Count == 0) throw new ArgumentException("At least one vehicle required", nameof(vehicles));

			// remaining packages (shallow copy)
			var remaining = packages.Where(p => p.EstimatedDeliveryTimeHours < 0).ToList();
			var assignments = new List<DeliveryAssignment>();

			// loop until none remain
			while (remaining.Any())
			{
				// pick next vehicle to become available (min AvailableAt). If tie, earliest index.
				var vehicle = vehicles.OrderBy(v => v.AvailableAt).First();
				double startTime = vehicle.AvailableAt;

				// choose best shipment for this vehicle from remaining packages
				List<Package> chosen;
				if (remaining.Count <= _exactSearchThreshold)
					chosen = ChooseBestShipmentExact(remaining, vehicle.CapacityKg);
				else
					chosen = ChooseBestShipmentGreedy(remaining, vehicle.CapacityKg);

				// If nothing fits (single package might exceed capacity), take the heaviest single and allow overload
				if (!chosen.Any())
				{
					var heavy = remaining.OrderByDescending(p => p.Weight).First();
					chosen = new List<Package> { heavy };
				}

				// compute max distance (farthest destination in that shipment)
				double maxDistance = chosen.Max(p => p.Distance);

				// assign package delivery times and vehicle id
				foreach (var pkg in chosen)
				{
					pkg.AssignedVehicleId = vehicle.Id;
					var drivingTimeHours = pkg.Distance / vehicle.SpeedKmph; // one-way
					pkg.EstimatedDeliveryTimeHours = startTime + drivingTimeHours;
				}

				// update vehicle availability: returns after roundtrip to farthest
				double returnAt = startTime + 2.0 * (maxDistance / vehicle.SpeedKmph);
				vehicle.AvailableAt = returnAt;

				// remove chosen from remaining
				foreach (var c in chosen) remaining.Remove(c);

				var assignment = new DeliveryAssignment
				{
					Vehicle = vehicle,
					Packages = chosen.ToList(),
					StartTimeHours = startTime,
					MaxDistanceKm = maxDistance,
					VehicleReturnTimeHours = returnAt
				};
				assignments.Add(assignment);

				if (verbose)
					PrintVerboseAssignment(assignment);
			}

			return assignments;
		}

		private void PrintVerboseAssignment(DeliveryAssignment a)
		{
			System.Console.WriteLine ($"--- Step: Vehicle {a.Vehicle.Id} departs at {a.StartTimeHours:F2} hrs with {a.Packages.Count} package(s)");
			foreach (var p in a.Packages)
			{
				System.Console.WriteLine($"    -> {p.PackageId} ({p.Weight} kg, {p.Distance} km) delivered at {p.EstimatedDeliveryTimeHours:F2} hrs");
			}
			System.Console.WriteLine($"    Vehicle {a.Vehicle.Id} will return at {a.VehicleReturnTimeHours:F2} hrs (farthest {a.MaxDistanceKm} km)");
		}

		#region Shipment selection (Exact + Greedy)

		// Exact search enumerates all subsets (bitmask) - correct for small n.
		private List<Package> ChooseBestShipmentExact(IList<Package> remaining, double capacityKg)
		{
			int n = remaining.Count;
			List<Package>? best = null;
			int bestCount = -1;
			double bestWeightSum = -1;
			double bestMaxDistance = double.MaxValue;

			int maxMask = 1 << n;
			for (int mask = 1; mask < maxMask; mask++)
			{
				int count = PopCount(mask);
				if (count < bestCount) continue; // prune by count

				double sumW = 0;
				double maxD = 0;
				var subset = new List<Package>(count);
				bool ok = true;
				for (int i = 0; i < n; i++)
				{
					if (((mask >> i) & 1) == 1)
					{
						var p = remaining[i];
						sumW += p.Weight;
						if (sumW > capacityKg) { ok = false; break; } // overweight -> skip
						if (p.Distance > maxD) maxD = p.Distance;
						subset.Add(p);
					}
				}
				if (!ok) continue;

				// tie-break rules:
				// 1) maximize count
				// 2) if same count, maximize total weight
				// 3) if same weight, choose smallest maxDistance (deliver earlier)
				if (count > bestCount
					|| (count == bestCount && sumW > bestWeightSum)
					|| (count == bestCount && Math.Abs(sumW - bestWeightSum) < 1e-9 && maxD < bestMaxDistance))
				{
					best = subset;
					bestCount = count;
					bestWeightSum = sumW;
					bestMaxDistance = maxD;
				}
			}

			return best ?? new List<Package>();
		}

		// Greedy fallback for larger n:
		// Try two heuristics and pick best by the same criteria.
		private List<Package> ChooseBestShipmentGreedy(IList<Package> remaining, double capacityKg)
		{
			// 1) Attempt maximize count: pack by ascending weight (small items first)
			var asc = remaining.OrderBy(p => p.Weight).ToList();
			var packA = new List<Package>();
			double sumA = 0;
			foreach (var p in asc)
			{
				if (sumA + p.Weight <= capacityKg)
				{
					packA.Add(p);
					sumA += p.Weight;
				}
			}

			// 2) Attempt heavier packing: pack by descending weight
			var desc = remaining.OrderByDescending(p => p.Weight).ToList();
			var packB = new List<Package>();
			double sumB = 0;
			foreach (var p in desc)
			{
				if (sumB + p.Weight <= capacityKg)
				{
					packB.Add(p);
					sumB += p.Weight;
				}
			}

			// compare
			if (packA.Count > packB.Count) return packA;
			if (packB.Count > packA.Count) return packB;
			// counts equals -> prefer heavier
			if (Math.Abs(sumB - sumA) > 1e-9) return sumB > sumA ? packB : packA;
			// still tie -> prefer one with smaller max distance (earlier)
			double mdA = packA.Any() ? packA.Max(p => p.Distance) : double.MaxValue;
			double mdB = packB.Any() ? packB.Max(p => p.Distance) : double.MaxValue;
			return mdA <= mdB ? packA : packB;
		}

		private static int PopCount(int x)
		{
			// simple bit count
			int c = 0;
			while (x != 0) { c += x & 1; x >>= 1; }
			return c;
		}

		#endregion
	}
}
