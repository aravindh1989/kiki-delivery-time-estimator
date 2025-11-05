using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KikiDeliveryTimeEstimator.Console.Models
{
	public class Package
	{
		public string PackageId { get; set; } = string.Empty;
		public double Weight { get; set; }
		public double Distance { get; set; }
		public string OfferCode { get; set; } = string.Empty;

		// Results set by scheduler
		public double EstimatedDeliveryTimeHours { get; set; } = -1;
		public string? AssignedVehicleId { get; set; }
	}
}
