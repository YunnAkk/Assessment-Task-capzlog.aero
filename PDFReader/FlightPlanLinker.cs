using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDFReader {

    /// <summary>
    /// Provides functionality to link extracted <see cref="FlightLegData"/> with corresponding
    /// <see cref="CrewBriefingData"/> based on shared identifiers.
    /// </summary>
    /// <remarks>
    /// This class encapsulates the logic for matching data extracted from potentially separate
    /// parts of a document or different documents (like a flight plan PDF and a crew briefing PDF)
    /// into unified <see cref="FlightPlan"/> objects.
    /// </remarks>
    public class FlightPlanLinker {

        /// <summary>
        /// Links <see cref="FlightLegData"/> with <see cref="CrewBriefingData"/> using a case-insensitive match
        /// on their respective <c>Identifier</c> properties.
        /// </summary>
        /// <param name="extractedLegs">Source list of <see cref="FlightLegData"/>. Items require a non-empty <see cref="FlightLegData.Identifier"/> to be linkable.</param>
        /// <param name="extractedBriefings">Source list of <see cref="CrewBriefingData"/>. Items require a non-empty <see cref="CrewBriefingData.Identifier"/> to be linkable.</param>
        /// <param name="linkingErrors">An initialized list where linking errors (e.g., duplicates, orphans, missing identifiers) are added.</param>
        /// <returns>
        /// A list containing successfully linked <see cref="FlightPlan"/> objects. Returns an empty list if no matches occur or inputs are invalid.
        /// Orphaned items (legs without briefings, or vice-versa) are reported as errors in <paramref name="linkingErrors"/>, not returned.
        /// </returns>
        /// <remarks>
        /// <para><b>Behavior and Warnings:</b></para>
        /// <list type="bullet">
        /// <item><description><b>Date Ambiguity Warning:</b> Linking only by identifier can mismatch data if identifiers repeat across dates.</description></item>
        /// <item><description>Duplicate Identifiers: Logs an error and uses the first encountered leg/briefing for a given identifier.</description></item>
        /// <item><description>Orphaned/Missing Identifiers: Items without a match or without an identifier generate errors in <paramref name="linkingErrors"/> and are not included in the result.</description></item>
        /// <item><description>Error Propagation: Errors from orphaned input items are added to <paramref name="linkingErrors"/>.</description></item>
        /// </list>
        /// </remarks>
        public List<FlightPlan> LinkDataByIdentifier(List<FlightLegData> extractedLegs,
            List<CrewBriefingData> extractedBriefings,
            List<string> linkingErrors) {
            Console.WriteLine("\n --- Starting Linking Phase ---");
            List<FlightPlan> linkedFlightPlans = new List<FlightPlan>();

            if (extractedLegs == null || !extractedLegs.Any()) {
                linkingErrors.Add("Linking Error: Cannot link data as no valid flight leg data (with Identifier) was provided.");
                Console.WriteLine("Linking skipped: No valid flight leg data.");
                if (extractedBriefings != null) {
                    foreach (var briefing in extractedBriefings.Where(b => !string.IsNullOrEmpty(b.Identifier))) {
                        linkingErrors.Add($"Crew Briefing: Identifier '{briefing.Identifier}' found, " +
                            $"but no flight legs were available for matching.");
                    }
                }
                return linkedFlightPlans;
            }

            if (extractedBriefings == null || !extractedBriefings.Any()) {
                linkingErrors.Add("Linking Error: No valid crew briefing data (with Identifier) was provided.");
                Console.WriteLine("Linking Warning: No valid crew briefing data.");
                foreach (var leg in extractedLegs.Where(l => !string.IsNullOrEmpty(l.Identifier))) {
                    linkingErrors.Add($"Flight Leg: Identifier '{leg.Identifier}' found, " +
                        $"but no crew briefings were available for matching.");
                }
                return linkedFlightPlans;
            }

            // Dictionary Setup
            Dictionary<string, FlightLegData> flightLegsByIdentifier = 
                new Dictionary<string, FlightLegData>(StringComparer.OrdinalIgnoreCase);
            Console.WriteLine("Building flight leg lookup dictionary by Identifier...");
            foreach (var leg in extractedLegs) {
                string key = leg.Identifier!;
                if (!flightLegsByIdentifier.TryAdd(key, leg)) {
                    // Handle duplicate
                    string error = $"Duplicate Flight Leg Identifier: A flight leg with identifier '{key}' already exists. Ignoring this duplicate instance. Using the first one found.";
                    Console.WriteLine($"Warning: {error}");
                    linkingErrors.Add(error);
                    linkingErrors.AddRange(leg.Errors.Select(e => $"[Duplicate Leg {key} Error] {e}"));
                } else {
                    Console.WriteLine($"Added leg with identifier '{key}' to dictionary.");
                }
            }
            Console.WriteLine($"Dictionary built with {flightLegsByIdentifier.Count} unique flight leg identifiers.");

            // Matching Phase
            Console.WriteLine("Matching crew briefings to flight legs by Identifier.");
            foreach (var briefing in extractedBriefings) {
                string key = briefing.Identifier!;
                Console.WriteLine($"  Attempting to match briefing with identifier '{key}'");
                if (flightLegsByIdentifier.TryGetValue(key, out FlightLegData? matchingLeg)) {
                    var flightPlan = new FlightPlan(key, matchingLeg, briefing);
                    linkedFlightPlans.Add(flightPlan);
                    Console.WriteLine($"Linked data for identifier: {key}");

                    // Remove matched leg from the dictionary
                    flightLegsByIdentifier.Remove(key);
                } else {
                    string error = $"Crew Briefing data found for identifier '{key}', " +
                        $"but no matching Flight Leg data was found";
                    Console.WriteLine($"Failed: {error}");
                    linkingErrors.Add(error);
                    linkingErrors.AddRange(briefing.Errors.Select(e => $"[Briefing {key} Error] {e}"));
                }
            }

            if (flightLegsByIdentifier.Any()) {
                Console.WriteLine("Identifying unassigned flight legs (by Identifier)");
                foreach (var kvp in flightLegsByIdentifier) {
                    string error = $"Flight Leg data found for identifier '{kvp.Key}', " +
                        $"but no matching Crew Briefing data was found or it was invalid.";
                    Console.WriteLine($"{error}");
                    linkingErrors.Add(error);
                    linkingErrors.AddRange(kvp.Value.Errors.Select(e => $"[Orphaned Leg {kvp.Key} Error] {e}"));
                }
            } else {
                Console.WriteLine("No unassigned flight legs found.");
            }

            Console.WriteLine($"--- Linking Phase Complete ---");
            Console.WriteLine($"Created {linkedFlightPlans.Count} flight plans");
            return linkedFlightPlans;
        }
   
    }
}
