using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDFReader {

    /// <summary>
    /// Represents a complete flight plan by aggregating flight leg data and crew briefing data,
    /// typically linked by a common identifier. Implements <see cref="IErrorReporter"/>
    /// to capture errors related to the aggregation or validation process.
    /// </summary>
    /// <remarks>
    /// This class acts as a container, combining <see cref="FlightLegData"/> and <see cref="CrewBriefingData"/>.
    /// It provides a status indicating whether all required data components are present and if any errors occurred.
    /// </remarks>
    public class FlightPlan : IErrorReporter {
        public FlightLegData? FlightData { get; set; }
        public CrewBriefingData? CrewData { get; set; }
        public string FlightIdentifier { get; set; } = "N/A";
        public List<string> Errors { get; } = new List<string>();

        /// <summary>
        /// Gets the current status of this flight plan, indicating whether it's complete,
        /// missing data, or has errors. See <see cref="FlightPlanStatus"/>.
        /// </summary>
        /// <value>
        /// <see cref="FlightPlanStatus.MissingFlightData"/> if <see cref="FlightData"/> is null.
        /// <see cref="FlightPlanStatus.MissingCrewData"/> if <see cref="CrewData"/> is null (and FlightData is not null).
        /// <see cref="FlightPlanStatus.HasErrors"/> if the <see cref="Errors"/> list for this FlightPlan instance contains any entries.
        /// <see cref="FlightPlanStatus.Complete"/> if both <see cref="FlightData"/> and <see cref="CrewData"/> are present and there are no errors in this instance's <see cref="Errors"/> list.
        /// </value>
        public FlightPlanStatus Status {
            get {
                if (FlightData == null)
                    return FlightPlanStatus.MissingFlightData;

                if (CrewData == null)
                    return FlightPlanStatus.MissingCrewData;

                if (this.Errors.Any())
                    return FlightPlanStatus.HasErrors;

                return FlightPlanStatus.Complete;
            }
        }

        // <summary>
        /// Initializes a new instance of the <see cref="FlightPlan"/> class, associating
        /// flight leg data and crew briefing data using a common identifier.
        /// </summary>
        /// <param name="identifier">The identifier (e.g., flight number and date) used to link the data. If null, defaults to "N/A".</param>
        /// <param name="flightData">The matched <see cref="FlightLegData"/>. Can be null if not found.</param>
        /// <param name="crewData">The matched <see cref="CrewBriefingData"/>. Can be null if not found.</param>
        public FlightPlan(string identifier, FlightLegData flightData, CrewBriefingData crewData) {
            this.FlightIdentifier = identifier ?? "N/A";
            this.FlightData = flightData;
            this.CrewData = crewData;
        }

        /// <summary>
        /// Determines whether this flight plan instance has both flight data and crew data assigned.
        /// </summary>
        /// <returns><c>true</c> if both <see cref="FlightData"/> and <see cref="CrewData"/> are non-null; otherwise, <c>false</c>.</returns>
        /// <remarks>This is a simplified check and does not consider the <see cref="Status"/> or any errors.</remarks>
        public bool IsComplete() {
            return FlightData != null && CrewData != null;
        }

        /// <summary>
        /// Generates a comprehensive string representation of the flight plan, including its status,
        /// identifier, contained flight data, contained crew data, and any specific linking errors.
        /// </summary>
        /// <returns>A multi-line formatted string summarizing the entire flight plan.</returns>
        public override string ToString() {
            var sb = new StringBuilder();

            sb.AppendLine($"=========================================");
            sb.AppendLine($"=== FLIGHT PLAN: {FlightIdentifier ?? "N/A"} ===");
            sb.AppendLine($"Status: {Status}");
            sb.AppendLine($"=========================================");
            sb.AppendLine();

            if (FlightData == null) {
                sb.AppendLine("--- FLIGHT DATA MISSING ---");
            } else {
                sb.AppendLine("--- FLIGHT DATA ---");
                sb.AppendLine(FlightData.ToString());
            }
            sb.AppendLine();


            if (CrewData == null) {
                sb.AppendLine("--- CREW DATA MISSING ---");
            } else {
                sb.AppendLine("--- CREW DATA ---");
                sb.AppendLine(CrewData.ToString());
            }
            sb.AppendLine();

            // Display only errors added directly to this FlightPlan object            
            if (this.Errors.Any()) {
                sb.AppendLine("--- FLIGHT PLAN LINKING ERRORS ---");
                foreach (var error in this.Errors) {
                    sb.AppendLine($"- {error}");
                }
                sb.AppendLine("------------------------------------------");
                sb.AppendLine();
            }

            sb.AppendLine($"=== End of Flight Plan: {FlightIdentifier} ===");
            sb.AppendLine($"=========================================");
            return sb.ToString();
        }

        /// <summary>
        /// Adds an error message to the <see cref="Errors"/> collection.
        /// Implements the <see cref="IErrorReporter.AddError"/> contract.
        /// </summary>
        /// <param name="error">The error message string to add.</param>
        public void AddError(string error) {
            Errors.Add($"[FlightPlan Error] {error}");
        }
    }

    /// <summary>
    /// Represents the possible status values for a <see cref="FlightPlan"/> instance,
    /// indicating its completeness and error state.
    /// </summary>
    public enum FlightPlanStatus {
        Complete,
        MissingFlightData,
        MissingCrewData,
        HasErrors
    }
}
