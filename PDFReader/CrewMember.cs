using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDFReader {

    /// <summary>
    /// Represents a single crew member identified in a crew briefing document.
    /// </summary>
    public class CrewMember {
        public string? Name { get; set; }
        public string? Function { get; set; }

        /// <summary>
        /// Returns a string representation of the <see cref="CrewMember"/> object,
        /// typically formatted as "Name, Function".
        /// Properties with null values are indicated as "N/A".
        /// </summary>
        /// <returns>A formatted string containing the crew member's name and function.</returns>
        public override string ToString() {
            return $"{Name ?? "N/A"}, {Function ?? "N/A"}";
        }
    }
}
