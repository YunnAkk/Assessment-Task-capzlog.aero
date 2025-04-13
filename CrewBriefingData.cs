using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDFReader {
    public class CrewBriefingData {
        public int? PaxBusiness { get; set; }
        public int? PaxEconomy { get; set; }
        public int? Dow { get; set; }
        public float? Doi { get; set; }   
        public List<CrewMember> CrewMembers { get; set; } = new List<CrewMember>();

        public override string ToString() {
            var sb = new StringBuilder();

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
    }
}
