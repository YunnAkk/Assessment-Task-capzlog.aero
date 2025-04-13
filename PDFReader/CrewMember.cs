using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDFReader {
    public class CrewMember {
        public string? Name { get; set; }
        public string? Function { get; set; }

        public override string ToString() {
            return $"{Name ?? "N/A"}, {Function ?? "N/A"}";
        }
    }
}
