using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig;

namespace PDFReader {

    /// <summary>
    /// Main entry point for the PDF Flight Plan Reader application.
    /// Orchestrates the process of reading a PDF file, extracting flight plan
    /// and crew briefing data, linking the data, and presenting the results to the console.
    /// </summary>
    class Program {

        /// <summary>
        /// The main execution method for the application.
        /// </summary>
        /// <param name="args">Command line arguments. Expects the path to the PDF file as the first argument.</param>
        static void Main(string[] args) {
            Console.WriteLine("--- PDF Flight Plan Reader Initializing ---");
            string pdfFilePath;

            if (args.Length == 0) {
                Console.WriteLine("No PDF file path provided via command line.");
                Console.Write("Please enter the full path to the PDF file: ");
                pdfFilePath = Console.ReadLine();

                // If still nothing entered, exit
                if (string.IsNullOrWhiteSpace(pdfFilePath)) {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Error: No input received.");
                    Console.ResetColor();
                    Console.WriteLine("\nPress any key to exit.");
                    Console.ReadKey();
                    return;
                }
            } else {
                pdfFilePath = args[0];
                Console.WriteLine($"File path provided: {pdfFilePath}");
            }

            // Validate File existence
            if (!File.Exists(pdfFilePath)) {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error: The file was not found at the specified path: {pdfFilePath}");
                Console.ResetColor();
                Console.WriteLine("\nPress any key to exit.");
                Console.ReadKey();
                return;
            }

            // Extract
            Console.WriteLine("PDF file found. Beginning with processing");
            var extractor = new FlightPlanExtractor();
            var linker = new FlightPlanLinker();
            var presenter = new FlightPlanConsolePresenter();
            Console.WriteLine("Components initialized.");

            Console.WriteLine("\n--- Starting Extraction Phase ---");
            (List<FlightLegData> extractedLegs, List<CrewBriefingData> extractedBriefings, List<string> extractionErrors) =
                extractor.FindAndExtractFlightData(pdfFilePath);

            Console.WriteLine($"Extraction Phase Complete. Found {extractedLegs.Count} potential legs, " +
                $"{extractedBriefings.Count} potential briefings.");

            if (extractionErrors.Any()) {
                Console.WriteLine($"Extraction encountered {extractionErrors.Count} errors/warnings.");
            }

            // Link
            Console.WriteLine("\n--- Starting Linking Phase ---");
            List<string> linkingErrors = new List<string>();
            List<FlightPlan> linkedFlightPlans = linker.LinkDataByIdentifier(extractedLegs, extractedBriefings, linkingErrors);
            Console.WriteLine($"Linking Phase Complete. Created {linkedFlightPlans.Count} flight plan entries.");
            if (linkingErrors.Any()) {
                Console.WriteLine($"Linking encountered {linkingErrors.Count} errors/warnings.");
            }

            // Present results
            List<string> allErrors = extractionErrors.Concat(linkingErrors).ToList();
            presenter.DisplayErrors(allErrors);
            presenter.DisplaySeparator();
            presenter.DisplaySummary(linkedFlightPlans);
            presenter.DisplaySeparator();

            Console.WriteLine("\n--- Detailed Flight Plan Output ---");
            if (linkedFlightPlans.Any()) {
                foreach (var flightPlan in linkedFlightPlans) {
                    presenter.DisplayFlightPlan(flightPlan);
                }
            } else {
                Console.WriteLine("No flight plans available to display details.");
            }
            Console.WriteLine("--- End of Detailed Output ---");

            Console.WriteLine("\n--- Processing Complete ---");
            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();
        }
    }
}
