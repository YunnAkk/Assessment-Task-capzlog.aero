using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDFReader {

    /// <summary>
    /// Responsible for formatting and displaying flight plan data, summaries, and errors
    /// to the standard console output.
    /// </summary>
    /// <remarks>
    /// This class provides methods to present information derived from <see cref="FlightPlan"/> objects
    /// in a user-readable format on the console, including separators, error lists,
    /// processing summaries, and detailed views of individual flight plans.
    /// </remarks>
    public class FlightPlanConsolePresenter {

        /// <summary>
        /// Displays a standard visual separator line
        /// to the console, useful for structuring output.
        /// </summary>
        public void DisplaySeparator() {
            Console.WriteLine("--------------------------------------------------");
        }

        /// <summary>
        /// Displays a list of errors to the console, typically formatted for emphasis.
        /// </summary>
        /// <param name="errors">The list of error messages to display.</param>
        public void DisplayErrors(List<string> errors) {
            if (errors == null || !errors.Any()) {
                return; // No errors
            }

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\n--- PROCESSING ERRORS ---");
            foreach (var error in errors) {
                Console.WriteLine($"- {error}");
            }
            Console.WriteLine("--- END ERRORS ---");
            Console.ResetColor();
        }

        /// <summary>
        /// Displays a list of error messages to the console. If the list is null or empty,
        /// nothing is displayed. Errors are typically highlighted
        /// </summary>
        /// <param name="errors">A <see cref="List{T}"/> of <see cref="string"/> containing the error messages to display.</param>
        public void DisplaySummary(List<FlightPlan> flightPlans) {
            if (flightPlans == null) {
                Console.WriteLine("Processing Summary: No flight plans were processed.");
                return;
            }

            int totalCount = flightPlans.Count;
            int completeCount = flightPlans.Count(fp => fp.IsComplete());
            int incompleteCount = totalCount - completeCount;
            int missingLegDataCount = flightPlans.Count(fp => fp.Status == FlightPlanStatus.MissingFlightData);
            int missingCrewDataCount = flightPlans.Count(fp => fp.Status == FlightPlanStatus.MissingCrewData);
            int linkingErrorCount = flightPlans.Count(fp => fp.Status == FlightPlanStatus.HasErrors);

            Console.WriteLine("\n--- PROCESSING SUMMARY ---");
            Console.WriteLine($"Total Flight Plans Processed: {totalCount}");
            Console.WriteLine($" - Complete (Leg + Crew): {completeCount}");
            Console.WriteLine($" - Incomplete: {incompleteCount}");

            if (incompleteCount > 0 || linkingErrorCount > 0) {
                Console.WriteLine($" - Missing Flight Data: {missingLegDataCount}");
                Console.WriteLine($" - Missing Crew Data: {missingCrewDataCount}");
                Console.WriteLine($" - With Linking Errors: {linkingErrorCount}");
            }
            Console.WriteLine("--- END SUMMARY ---");
        }

        /// <summary>
        /// Displays a summary of processing results based on a list of <see cref="FlightPlan"/> objects.
        /// Shows total count, complete count, incomplete count, and breakdown of incomplete reasons
        /// </summary>
        /// <param name="flightPlans">The <see cref="List{T}"/> of <see cref="FlightPlan"/> objects that were processed.</param>
        public void DisplayFlightPlan(FlightPlan flightPlan) {
            if (flightPlan == null) {
                Console.WriteLine("flight plan missing");
                return;
            }
            Console.WriteLine(flightPlan.ToString());
            DisplaySeparator();
        }
    }
}
