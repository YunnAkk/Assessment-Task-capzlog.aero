using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PDFReader;


namespace PDFReader.Tests;
[TestClass]
public sealed class FlightPlanExtractorTest {
    [TestMethod]
    public void TestExtractor() {
        string pdfFilePath = "C:/Users/Yun/Downloads/Backup/Task 1 Sample File.pdf";
        Console.WriteLine("Flight Plan Extractor Test");
        Console.WriteLine("==========================");

        Console.WriteLine($"Processing file: {pdfFilePath}");
        Console.WriteLine();

        try {
            IFlightPlanExtractor extractor = new FlightPlanExtractor();
            var (flightData, crewData, errors) = extractor.FindAndExtractFlightData(pdfFilePath);

            // Display results
            Console.WriteLine("\nExtraction Results:");
            Console.WriteLine("==================");

            if (errors.Count > 0) {
                Console.WriteLine($"Encountered {errors.Count} errors");
                foreach (var error in errors) {
                    Console.WriteLine($"=== {error} ====");
                }
                Console.WriteLine();
            }

            if (flightData.Count > 0) {
                Console.WriteLine($"Extracted {flightData.Count} flight plans:");
                int count = 1;
                foreach (var flight in flightData) {
                    Console.WriteLine($"\nFlight Plan #{count++}");
                    Console.WriteLine($"Flight Number: {flight.FlightNumber}");
                    Console.WriteLine($"Callsign: {flight.AtcCallsign}");
                }
            }
        } catch (Exception ex) {
            Console.WriteLine($"An unexpected error occurred: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
            Assert.Fail($"Test threw an exception: {ex.Message}");
        }
    }
}
