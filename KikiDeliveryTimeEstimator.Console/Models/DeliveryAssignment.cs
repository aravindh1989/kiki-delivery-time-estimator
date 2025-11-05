using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KikiDeliveryTimeEstimator.Console.Models
{
	public class DeliveryAssignment
	{
		public Vehicle Vehicle { get; set; } = null!;
		public IReadOnlyList<Package> Packages { get; set; } = Array.Empty<Package>();
		public double StartTimeHours { get; set; }
		public double MaxDistanceKm { get; set; }
		public double VehicleReturnTimeHours { get; set; }
	}
}
