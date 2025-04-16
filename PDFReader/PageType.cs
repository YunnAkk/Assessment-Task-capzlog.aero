using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDFReader {

    /// <summary>
    /// Represents the identified type of content found on a specific page
    /// within a processed PDF document, typically used to distinguish between
    /// flight plan detail pages and crew briefing pages.
    /// </summary>
    public enum PageType {
        FlightPlan,
        CrewBriefing
    }
}
