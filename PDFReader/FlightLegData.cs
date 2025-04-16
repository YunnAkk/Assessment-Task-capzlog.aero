using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDFReader {

    /// <summary>
    /// Holds data specific to a single flight leg extracted from a flight plan document.
    /// Implements <see cref="IErrorReporter"/> to collect parsing or validation errors.
    /// </summary>
    public class FlightLegData : IErrorReporter {
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
        public double? FuelToDestination { get; set; }
        public int? TimeToAlternate { get; set; }
        public double? FuelToAlternate { get; set; }
        public double? MinFuelRequired { get; set; }
        public string? RouteFirstNavPoint { get; set; }
        public string? RouteLastNavPoint { get; set; }
        public int? GainLoss { get; set; }
        public string? Identifier { get; set; }
        public List<string> Errors { get; } = new List<string>();

        /// <summary>
        /// Returns a multi-line string representation of the <see cref="FlightLegData"/> object,
        /// summarizing key flight details. Properties with null values are indicated as "N/A".
        /// Includes a list of any errors recorded.
        /// </summary>
        /// <returns>A formatted string containing the flight leg data summary and any errors.</returns>
        public override string ToString() {
            var sb = new StringBuilder();

            sb.AppendLine($"Date: {Date ?? "N/A"}");
            sb.AppendLine($"Flight: {FlightNumber ?? "N/A"}/{AtcCallsign ?? "N/A"} ({Registration ?? "N/A"})");
            sb.AppendLine($"From: {DepartureIcao ?? "N/A"} To: {ArrivalIcao ?? "N/A"}");
            sb.AppendLine($"Dep: {FormatTime(DepartureTime)}Z Arr: {FormatTime(ArrivalTime)}Z");
            sb.AppendLine($"Alternates: 1st: {FirstAlternate ?? "N/A"}, 2nd: {SecondaryAlternate ?? "N/A"}");
            sb.AppendLine($"Time to Dest: {FormatDuration(TimeToDestination)}, Fuel to Dest: {FuelToDestination?.ToString() ?? "N/A"}");
            sb.AppendLine($"Time to Alt: {FormatDuration(TimeToAlternate)}, Fuel to Alt: {FuelToAlternate?.ToString() ?? "N/A"}");
            sb.AppendLine($"Min Fuel Req: {MinFuelRequired?.ToString() ?? "N/A"}");
            sb.AppendLine($"Route Waypoints: 1st: {RouteFirstNavPoint ?? "N/A"}, Last: {RouteLastNavPoint ?? "N/A"}");
            sb.AppendLine($"ZFM: {ZeroFuelMass?.ToString() ?? "N/A"}");
            sb.AppendLine($"Gain/Loss: {GainLoss?.ToString() ?? "N/A"}");
            sb.Append($"Identifier: {Identifier?.ToString() ?? "N/A"}");

            // Add errors if any
            if (Errors.Count > 0) {
                sb.AppendLine();
                sb.AppendLine("Errors:");
                foreach (var error in Errors) {
                    sb.AppendLine($"- {error}");
                }
            }

            return sb.ToString();
        }
       
        private string FormatTime(int? time) {
            if (time == null) {
                return "N/A";
            }
            int hours = time.Value / 100;
            int minutes = time.Value % 100;

            return $"{hours:00}:{minutes:00}";
        }
        private string FormatDuration(int? minutes) {
            if (minutes == null) {
                return "N/A";
            }
            int hours = minutes.Value / 60;
            int mins = minutes.Value % 60;

            return $"{hours:00}:{mins:00}";
        }

        /// <summary>
        /// Adds an error message to the <see cref="Errors"/> collection.
        /// Implements the <see cref="IErrorReporter.AddError"/> contract.
        /// </summary>
        /// <param name="error">The error message string to add.</param>
        public void AddError(string error) {
            Errors.Add(error);
        }
    }
}
