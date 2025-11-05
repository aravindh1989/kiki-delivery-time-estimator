using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KikiDeliveryTimeEstimator.Console.Models
{
	public class Vehicle
	{
		public string Id { get; set; }
		public double CapacityKg { get; set; }
		public double SpeedKmph { get; set; }

		// Time when this vehicle becomes available next (hours)
		public double AvailableAt { get; set; } = 0.0;

		public Vehicle(string id, double capacityKg, double speedKmph)
		{
			Id = id;
			CapacityKg = capacityKg;
			SpeedKmph = speedKmph;
			AvailableAt = 0.0;
		}
	}
}
