using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDFReader {
    /// <summary>
    /// Holds data specific to a single flight leg extracted from the flight plan.
    /// </summary>
    public class FlightLegData {
        public string? Date { get; set; }
        public string? Registration { get; set; }
        public string? DepartureIcao { get; set; }
        public string? ArrivalIcao { get; set; }
        public string? FirstAlternate { get; set; }
        public string? SecondaryAlternate { get; set; }
        public string? FlightNumber { get; set; }
        public string? AtcCallsign { get; set; }
        public int? DepartureTime { get; set; }
        public int? ArrivalTime { get; set; }
        public int? ZeroFuelMass { get; set; }
        public int? TimeToDestination { get; set; }
        public int? FuelToDestination { get; set; }
        public int? TimeToAlternate { get; set; }
        public int? FuelToAlternate { get; set; }
        public int? MinFuelRequired { get; set; }
        public string? RouteFirstNavPoint { get; set; }
        public string? RouteLastNavPoint { get; set; }
        public int? GainLossMinutes { get; set; }

        /// <summary>
        /// Provides a basic string representation for debugging.
        /// </summary>
        public override string ToString() {
            var sb = new StringBuilder();

            sb.AppendLine($"Date: {Date ?? "N/A"}");
            sb.AppendLine($"Flight: {FlightNumber ?? "N/A"}/{AtcCallsign ?? "N/A"} ({Registration ?? "N/A"})");
            sb.AppendLine($"From: {DepartureIcao ?? "N/A"} To: {ArrivalIcao ?? "N/A"}");
            sb.AppendLine($"Dep: {formatTime(DepartureTime)}Z Arr: {formatTime(ArrivalTime)}Z");
            sb.AppendLine($"Alternates: 1st: {FirstAlternate ?? "N/A"}, 2nd: {SecondaryAlternate ?? "N/A"}");
            sb.AppendLine($"Time to Dest: {TimeToDestination?.ToString() ?? "N/A"}, Fuel to Dest: {FuelToDestination?.ToString() ?? "N/A"}");
            sb.AppendLine($"Time to Alt: {TimeToAlternate?.ToString() ?? "N/A"}, Fuel to Alt: {FuelToAlternate?.ToString() ?? "N/A"}");
            sb.AppendLine($"Min Fuel Req: {MinFuelRequired?.ToString() ?? "N/A"}");
            sb.AppendLine($"Route Waypoints: 1st: {RouteFirstNavPoint ?? "N/A"}, Last: {RouteLastNavPoint ?? "N/A"}");
            sb.AppendLine($"ZFM: {ZeroFuelMass?.ToString() ?? "N/A"}");
            sb.Append($"Gain/Loss: {GainLossMinutes?.ToString() ?? "N/A"}");

            return sb.ToString();
        }

        private string formatTime(int? time) {
            if (time == null) {
                return "N/A";
            }
            int hours = time.Value / 100;
            int minutes = time.Value % 100;

            return $"{hours:00}:{minutes:00}";
        }
    }
}
