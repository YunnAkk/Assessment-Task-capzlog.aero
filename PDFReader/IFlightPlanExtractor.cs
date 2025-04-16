using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDFReader {
    /// <summary>
    /// Defines a contract for extracting flight plan related data
    /// (both flight leg details and crew briefing information) from a PDF document.
    /// </summary>
    public interface IFlightPlanExtractor {

        /// <summary>
        /// Opens and processes the specified PDF file to find and extract flight leg data
        /// and crew briefing data.
        /// </summary>
        /// <param name="pdfFilePath">The full path to the PDF file to be processed.</param>
        /// <returns>
        /// A tuple containing:
        /// <list type="bullet">
        /// <item><term>Data</term><description>A <see cref="List{T}"/> of <see cref="FlightLegData"/> objects extracted from the PDF.</description></item>
        /// <item><term>CrewData</term><description>A <see cref="List{T}"/> of <see cref="CrewBriefingData"/> objects extracted from the PDF.</description></item>
        /// <item><term>Errors</term><description>A <see cref="List{T}"/> of <see cref="string"/> containing any error messages generated during the file reading or data extraction process.</description></item>
        /// </list>
        /// The lists within the tuple may be empty if no corresponding data is found or if errors prevent extraction,
        /// but the tuple itself should be returned.
        /// </returns>
        /// <exception cref="System.IO.FileNotFoundException">Thrown if the specified <paramref name="pdfFilePath"/> does not exist.</exception>
        /// <exception cref="System.Exception">May throw other exceptions related to file access permissions, PDF parsing errors (e.g., invalid format, password protected), or internal processing issues.</exception>
        public (List<FlightLegData> Data, List<CrewBriefingData> CrewData, List<string> Errors) FindAndExtractFlightData(string pdfFilePath);
    }
}
