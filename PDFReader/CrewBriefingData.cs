using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDFReader {

    /// <summary>
    /// Represents data extracted from a crew briefing document, such as passenger counts,
    /// weights, crew members, and an identifier. Also provides error reporting capabilities.
    /// </summary>
    /// <remarks>
    /// This class implements <see cref="IErrorReporter"/> to collect errors encountered
    /// during the parsing or processing of the briefing data.
    /// </remarks>
    public class CrewBriefingData : IErrorReporter {
        public int? PaxBusiness { get; set; }
        public int? PaxEconomy { get; set; }
        public int? Dow { get; set; }
        public double? Doi { get; set; }   
        public List<CrewMember> CrewMembers { get; set; } = new List<CrewMember>();
        public string? Identifier { get; set; }
        public List<string> Errors { get; } = new List<string>();

        /// <summary>
        /// Returns a string representation of the <see cref="CrewBriefingData"/> object,
        /// including the identifier, passenger counts, weights, and crew members.
        /// Properties with null values are indicated as "N/A".
        /// </summary>
        /// <returns>A multi-line formatted string containing the briefing data.</returns>
        public override string ToString() {
            var sb = new StringBuilder();

            sb.AppendLine($"Identifier: {Identifier?.ToString() ?? "N/A"}");
            sb.AppendLine($"Number of passengers in business (C) class: {PaxBusiness?.ToString() ?? "N/A"}");
            sb.AppendLine($"Number of passengers in economy (Y) class: {PaxEconomy?.ToString() ?? "N/A"}");
            sb.AppendLine($"Dry operating weight: {Dow?.ToString() ?? "N/A"}");
            sb.AppendLine($"Dry operating index: {Doi?.ToString() ?? "N/A"}");
            sb.AppendLine("Crew Members:");
            sb.Append(GetFormattedCrewList());

            return sb.ToString();
        }
    
        private bool HasCrewMembers() {
            return CrewMembers != null && CrewMembers.Count > 0;
        }

        private string GetFormattedCrewList() {
            if (!HasCrewMembers()) {
                return "No Crew Members assigned.";
            }

            var sb = new StringBuilder();
            foreach (var crew in CrewMembers) {
                sb.AppendLine($"{crew}");
            }
            return sb.ToString();
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
