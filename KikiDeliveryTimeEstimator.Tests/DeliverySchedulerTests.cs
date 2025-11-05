using KikiDeliveryTimeEstimator.Console.Models;
using KikiDeliveryTimeEstimator.Console.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KikiDeliveryTimeEstimator.Tests
{
	public class DeliverySchedulerTests
	{
		[Fact]
		public void SampleScenario_PerVehicleSpecs_MatchesExpectedTimings()
		{
			var packages = new List<Package>
		{
			new Package { PackageId = "PKG1", Weight = 50, Distance = 30 },
			new Package { PackageId = "PKG2", Weight = 75, Distance = 125 },
			new Package { PackageId = "PKG3", Weight = 175, Distance = 100 },
			new Package { PackageId = "PKG4", Weight = 110, Distance = 60 },
			new Package { PackageId = "PKG5", Weight = 155, Distance = 95 }
		};

			var vehicles = new List<Vehicle>
		{
			new Vehicle("V1", capacityKg: 200, speedKmph: 70),
			new Vehicle("V2", capacityKg: 200, speedKmph: 70)
		};

			var scheduler = new DeliveryScheduler();
			var assignments = scheduler.EstimateDeliveryTimes(packages, vehicles, verbose: false);

			// All packages assigned and have times
			Assert.All(packages, p => Assert.True(p.EstimatedDeliveryTimeHours > 0 && !string.IsNullOrEmpty(p.AssignedVehicleId)));

			// Check some expected rounded values (allow small eps)
			double eps = 0.02;
			Assert.InRange(packages.Single(p => p.PackageId == "PKG2").EstimatedDeliveryTimeHours, 1.78 - eps, 1.78 + eps);
			Assert.InRange(packages.Single(p => p.PackageId == "PKG3").EstimatedDeliveryTimeHours, 1.42 - eps, 1.42 + eps);
			Assert.InRange(packages.Single(p => p.PackageId == "PKG4").EstimatedDeliveryTimeHours, 0.85 - eps, 0.86 + eps);
			Assert.InRange(packages.Single(p => p.PackageId == "PKG5").EstimatedDeliveryTimeHours, 4.19 - eps, 4.19 + eps);
			Assert.InRange(packages.Single(p => p.PackageId == "PKG1").EstimatedDeliveryTimeHours, 3.98 - eps, 3.99 + eps);
		}
	}
}


