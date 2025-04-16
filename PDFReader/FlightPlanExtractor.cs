using System.Globalization;
using System.Text.RegularExpressions;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace PDFReader {

    /// <summary>
    /// Extracts flight plan and crew briefing data from PDF documents using PdfPig.
    /// Implements the <see cref="IFlightPlanExtractor"/> interface.
    /// </summary>
    /// <remarks>
    /// This class relies on identifying specific text labels and patterns within the PDF
    /// to locate and parse relevant data fields. It includes logic to differentiate
    /// between flight plan pages and crew briefing pages.
    /// Constants define thresholds and markers used during page analysis and data extraction.
    /// </remarks>
    public class FlightPlanExtractor : IFlightPlanExtractor {
        private const string PageMarkerText = "Page";
        private const string FirstPageNumberText = "1";
        private const int DefaultWordsToTake = 1;
        private const double TopMarginThresholdFactor = 0.80;
        private const double SameLineThreshold = 0.5;
        private const double MaxGapMultiplier = 3.0;
        private const int ColumnPadding = 2;
        private const int NameColumnExtension = 4;

        private const int MIN_TIME_LENGTH = 3;
        private const int MAX_TIME_LENGTH = 4;
        private const int MAX_HOURS = 23;
        private const int MAX_MINUTES = 59;

        /// <summary>
        /// Processes a PDF file to find and extract flight plan and crew briefing data.
        /// </summary>
        /// <param name="pdfFilePath">The full path to the PDF file to process.</param>
        /// <returns>
        /// A tuple containing:
        /// - <c>Data</c>: A list of <see cref="FlightLegData"/> objects representing the extracted flight plan information.
        /// - <c>CrewData</c>: A list of <see cref="CrewBriefingData"/> objects representing the extracted crew briefing information.
        /// - <c>Errors</c>: A list of strings detailing any errors or warnings encountered during processing.
        /// </returns>
        /// <remarks>
        /// This method orchestrates the entire extraction process:
        /// 1. Validates the existence of the PDF file.
        /// 2. Opens the PDF document using PdfPig.
        /// 3. Scans the document to identify the page numbers for flight plan starts and crew briefings using <see cref="FindPagesOfType"/>.
        /// 4. Iterates through identified flight plan pages, calling <see cref="ExtractDataFromPage"/> for each.
        /// 5. Iterates through identified crew briefing pages, calling <see cref="ExtractCrewDataFromPage"/> for each.
        /// 6. Aggregates all extracted data and any errors encountered.
        /// Returns empty lists for data if the file doesn't exist or no relevant pages are found, including appropriate error messages.
        /// </remarks>
        public (List<FlightLegData> Data, List<CrewBriefingData> CrewData, List<string> Errors) FindAndExtractFlightData(string pdfFilePath) {
            Console.WriteLine($"Start processing for: {pdfFilePath}");
            List<FlightLegData> allFlightData = new List<FlightLegData>();
            List<CrewBriefingData> allCrewData = new List<CrewBriefingData>();
            List<string> globalErrors = new List<string>();

            if (!File.Exists(pdfFilePath)) {
                string error = $"Error: File not found at {pdfFilePath}";
                Console.WriteLine(error);
                globalErrors.Add(error);
                return (allFlightData, allCrewData, globalErrors);
            }

            try {
                using (PdfDocument document = PdfDocument.Open(pdfFilePath)) {
                    int totalPages = document.NumberOfPages;
                    Console.WriteLine($"Document has {totalPages} pages.");

                    var flightPlanStartPages = FindPagesOfType(document, PageType.FlightPlan);
                    var crewBriefingPages = FindPagesOfType(document, PageType.CrewBriefing);
                    Console.WriteLine("\n--- Summary of Identified Pages ---");
                    if (flightPlanStartPages.Any()) {
                        Console.WriteLine($"Identified {flightPlanStartPages.Count} flight plan start pages: {string.Join(", ", flightPlanStartPages)}");
                    }
                    else {
                        string error = "No flight plan start pages were identified.";
                        Console.WriteLine(error);
                        globalErrors.Add(error);
                        return (allFlightData, allCrewData, globalErrors);
                    }

                    if (crewBriefingPages.Any()) {
                        Console.WriteLine($"Identified {crewBriefingPages.Count} crew briefing pages: " +
                            $"{string.Join(", ", crewBriefingPages.Select(p => p.pageNumber))}");
                        foreach (var page in crewBriefingPages.Where(p => !string.IsNullOrEmpty(p.Identifier))) {
                            Console.WriteLine($"  - Page {page.pageNumber}: {page.Identifier}");
                        }
                    }
                    else {
                        string error = "No crew briefing pages were identified.";
                        Console.WriteLine(error);
                        globalErrors.Add(error);
                    }

                    Console.WriteLine("\n--- Starting Flight Plan Data Extraction ---");
                    foreach (var pageInfo in flightPlanStartPages) {
                        Console.WriteLine($"Processing flight plan data extraction for page: {pageInfo.pageNumber}");
                        try {
                            Page pageToExtract = document.GetPage(pageInfo.pageNumber);
                            FlightLegData? extractedData = ExtractDataFromPage(pageToExtract);

                            if (extractedData != null) {
                                allFlightData.Add(extractedData);
                                Console.WriteLine($"Extracted data for flight: \n{extractedData.ToString()}");
                            }
                            else {
                                string error = $"Warning: Failed to extract complete flight plan data from page {pageInfo.pageNumber}.";
                                Console.WriteLine(error);
                                globalErrors.Add(error);
                            }
                        }
                        catch (Exception ex) {
                            string error = $"ERROR extracting flight plan data from page {pageInfo.pageNumber}: {ex.Message}";
                            Console.WriteLine(error);
                            globalErrors.Add(error);
                        }
                    }

                    if (crewBriefingPages.Any()) {
                        Console.WriteLine("\n--- Starting Crew Briefing Data Extraction ---");
                        foreach (var pageInfo in crewBriefingPages) {
                            Console.WriteLine($"Processing crew briefing data extraction for page: {pageInfo.pageNumber}");
                            try {
                                Page pageToExtract = document.GetPage(pageInfo.pageNumber);
                                CrewBriefingData? extractedData = ExtractCrewDataFromPage(pageToExtract, pageInfo.Identifier);
                                if (extractedData != null) {
                                    allCrewData.Add(extractedData);
                                    Console.WriteLine($"Extracted crew data: \n{extractedData.ToString()}");
                                }
                                else {
                                    string error = $"Warning: Failed to extract complete crew data from page {pageInfo.pageNumber}.";
                                    Console.WriteLine(error);
                                    globalErrors.Add(error);
                                }
                            }
                            catch (Exception ex) {
                                string error = $"ERROR extracting crew data from page {pageInfo.pageNumber}: {ex.Message}";
                                Console.WriteLine(error);
                                globalErrors.Add(error);
                            }
                        }
                    }
                }
            }
            catch (Exception ex) {
                string error = $"An error occurred during PDF processing: {ex.Message}";
                Console.WriteLine(error);
                globalErrors.Add(error);
            }
            Console.WriteLine($"\n--- Extraction Complete ---");
            Console.WriteLine($"Extracted data for {allFlightData.Count} flight plans " +
                $"and {allCrewData.Count} crew briefings.");

            return (allFlightData, allCrewData, globalErrors);
        }

        private List<(int pageNumber, string Identifier)> FindPagesOfType(PdfDocument document, PageType pageType) {
            List<(int pageNumber, string Identifier)> matchingPages = new List<(int pageNumber, string Identifier)>();
            int totalPages = document.NumberOfPages;

            Console.WriteLine($"Scanning for {pageType} pages");

            for (int pageNum = 1; pageNum <= totalPages; pageNum++) {
                Page page = document.GetPage(pageNum);

                var (isMatch, identifier) = DeterminePageType(page, pageType);

                if (isMatch) {
                    matchingPages.Add((pageNum, identifier));
                    if (!string.IsNullOrEmpty(identifier)) {
                        Console.WriteLine($"Found {pageType} page {pageNum} with identifier: {identifier}");
                    }
                }
            }
            Console.WriteLine($"Finished scanning for {pageType} pages. Found {matchingPages.Count} pages.");
            return matchingPages;
        }

        private (bool isMatch, string Identifier) DeterminePageType(Page page, PageType pageType) {
            double pageHeight = page.Height;
            double topCoordinateThreshold = pageHeight * TopMarginThresholdFactor;

            // Only get words near the top of the page
            List<Word> wordList = page.GetWords()
                .Where(w => w.BoundingBox.Top > topCoordinateThreshold)
                .ToList();

            switch (pageType) {
                case PageType.FlightPlan:
                    return (IsFlightPlanPage(wordList), string.Empty);
                case PageType.CrewBriefing:
                    string identifier = string.Empty;
                    bool isCrewBriefing = IsCrewBriefingPage(wordList, out identifier);
                    return (isCrewBriefing, identifier);
                default:
                    return (false, string.Empty);
            }
        }

        private bool IsFlightPlanPage(List<Word> wordList) {
            bool foundOfpHeader = false;
            bool foundPage1Marker = false;

            for (int i = 0; i < wordList.Count; i++) {
                Word currentWord = wordList[i];

                // Check for "Operational Flight Plan" Phrase
                if (!foundOfpHeader && currentWord.Text.Equals("Operational", StringComparison.OrdinalIgnoreCase)) {
                    if (i + 2 < wordList.Count &&
                wordList[i + 1].Text.Equals("Flight", StringComparison.OrdinalIgnoreCase) &&
                wordList[i + 2].Text.Equals("Plan", StringComparison.OrdinalIgnoreCase)) {
                        foundOfpHeader = true;
                    }
                }
                if (!foundPage1Marker && currentWord.Text.Equals(PageMarkerText, StringComparison.OrdinalIgnoreCase)) {
                    if (i + 1 < wordList.Count && wordList[i + 1].Text.Equals(FirstPageNumberText)) {
                        Word pageWord = currentWord;
                        Word numberWord = wordList[i + 1];

                        if (numberWord.BoundingBox.Left > pageWord.BoundingBox.Right &&
                            (numberWord.BoundingBox.Left - pageWord.BoundingBox.Right) < (pageWord.BoundingBox.Width * 5)) {
                            foundPage1Marker = true;
                        }
                    }
                }
                if (foundOfpHeader && foundPage1Marker) {
                    return true;
                }
            }
            return false;
        }

        private bool IsCrewBriefingPage(List<Word> wordList, out string flightNumber) {
            bool foundFlightAssignment = false;
            bool foundFlightNumberPattern = false;
            flightNumber = string.Empty;

            // Look for "Flight Assignment" in the header
            for (int i = 0; i < wordList.Count - 1; i++) {
                Word currentWord = wordList[i];

                // Check for "Flight" followed by "Assignment"
                if (!foundFlightAssignment &&
                    currentWord.Text.Equals("Flight", StringComparison.OrdinalIgnoreCase) &&
                    i + 1 < wordList.Count &&
                    wordList[i + 1].Text.Equals("Assignment", StringComparison.OrdinalIgnoreCase)) {
                    foundFlightAssignment = true;
                }

                // Check for flight number
                if (!foundFlightNumberPattern) {
                    var match = System.Text.RegularExpressions.Regex.Match(currentWord.Text, @"[A-Z]{2}\d+/[A-Z]{3}\d+[A-Z]*");
                    if (match.Success) {
                        foundFlightNumberPattern = true;
                        flightNumber = match.Value;
                    }

                }
            }
            return foundFlightAssignment && foundFlightNumberPattern;
        }

        /// <summary>
        /// Extracts flight leg data points from a single PDF page identified as a flight plan.
        /// </summary>
        /// <param name="page">The <see cref="Page"/> object representing the flight plan page.</param>
        /// <returns>A <see cref="FlightLegData"/> object populated with extracted data, or <c>null</c> if essential data (like Flight Number or Callsign) couldn't be found.</returns>
        /// <remarks>
        /// This method uses various helper functions
        /// to locate and parse specific fields based on their preceding labels. Errors encountered during extraction
        /// are added to the returned <see cref="FlightLegData"/> object.
        /// </remarks>
        private FlightLegData? ExtractDataFromPage(Page page) {
            FlightLegData data = new FlightLegData();
            List<Word> words = page.GetWords().ToList();

            // Extraction Logic
            data.Date = FindValueNextToLabel(words, "Date:", data, "Date");
            data.Registration = FindValueNextToLabel(words, "Reg.:", data, "Registration");
            data.DepartureIcao = FindValueNextToLabel(words, "From:", data, "Departure ICAO");
            data.ArrivalIcao = FindValueNextToLabel(words, "To:", data, "Arrival ICAO");
            data.FirstAlternate = FindValueNextToLabel(words, "ALTN1:", data, "First Alternate");
            data.SecondaryAlternate = FindValueNextToLabel(words, "ALTN2:", data, "Secondary Alternate");
            data.FlightNumber = FindValueNextToLabel(words, "FltNr:", data, "Flight Number");
            data.AtcCallsign = FindValueNextToLabel(words, "ATC:", data, "ATC Callsign");

            // Times (STD/STA) - parsing HH:MM -> HHMM integer
            string? stdTimeStr = FindValueNextToLabel(words, "STD:", data, "Standard Departure Time");
            string? staTimeStr = FindValueNextToLabel(words, "STA:", data, "Standard Arrival Time");
            data.DepartureTime = ParseTime(stdTimeStr, data, "Departure Time");
            data.ArrivalTime = ParseTime(staTimeStr, data, "Arrival Time");

            data.ZeroFuelMass = ParseIntBeforeUnit(FindValueNextToLabel(words, "ZFM:", data, "Zero Fuel Mass"), data, "Zero Fuel Mass");
            data.TimeToDestination = ParseTime(FindValueNextToLabel(words, data.ArrivalIcao + ":", data, "Time to Destination"), data, "Time to Destination");
            data.FuelToDestination = ParseDouble(FindValueNextToLabel(words, "TRIP:", data, "Trip Fuel"), data, "Trip Fuel");
            data.TimeToAlternate = ParseTime(FindValueNextToLabel(words, data.FirstAlternate + ":", data, "Time to Alternate"), data, "Time to Alternate");
            data.FuelToAlternate = ParseDouble(FindValueNextToLabel(words, data.FirstAlternate + ":", data, "First Alternate", wordsToTake: 2), data, "First Alternate", extractSecondNumber: true);
            data.MinFuelRequired = ParseDouble(FindValueNextToLabel(words, "MIN:", data, "Minimum Fuel", wordsToTake: 2), data, "Minimum Fuel", extractSecondNumber: true);

            var (firstWaypoint, lastWaypoint) = ExtractRouteWaypoints(words, data);
            data.RouteFirstNavPoint = firstWaypoint;
            data.RouteLastNavPoint = lastWaypoint;

            data.GainLoss = ParseGainLoss(FindValueNextToLabel(words, "Loss:", data, "Gain/Loss Text", wordsToTake: 2), data, "Gain/Loss Text");

            // Set identifier
            if (!string.IsNullOrEmpty(data.FlightNumber) &&
                !string.IsNullOrEmpty(data.AtcCallsign)) {
                data.Identifier = $"{data.FlightNumber}/{data.AtcCallsign}";
            } else {
                string error = "Could not determine identifier";
                Console.WriteLine(error);
                data.AddError(error);
            }

            // Basic validation: extracted at least some core info?
            if (string.IsNullOrWhiteSpace(data.FlightNumber) &&
    string.IsNullOrWhiteSpace(data.AtcCallsign)) {
                string error = $"Warning: Could not extract FlightNumber or Callsign from page {page.Number}. Skipping this page's data.";
                Console.WriteLine(error);
                data.AddError(error);
                return null;
            }
            return data;
        }

        /// <summary>
        /// Extracts crew briefing specific data points (DOW, DOI, passenger counts, crew list) from a single PDF page.
        /// </summary>
        /// <param name="page">The <see cref="Page"/> object representing the crew briefing page.</param>
        /// <returns>A <see cref="CrewBriefingData"/> object populated with extracted data. Returns an object even if some data is missing, but errors will be logged within it.</returns>
        /// <remarks>
        /// This method uses various helper functions
        /// to find and parse relevant information.
        /// </remarks>
        private CrewBriefingData? ExtractCrewDataFromPage(Page page, string preIdentifiedIdentifier = null) {
            CrewBriefingData data = new CrewBriefingData();
            List<Word> words = page.GetWords().ToList();

            // Set the identifier if it was pre-identified during page scanning
            if (!string.IsNullOrEmpty(preIdentifiedIdentifier)) {
                data.Identifier = preIdentifiedIdentifier;
                Console.WriteLine($"Using pre-identified identifier: {preIdentifiedIdentifier}");
            }
            else {
                data.AddError("No flight identifier found for this crew briefing");
            }

            string? dowValueStr = FindValueNextToLabel(words, "DOW:", data, "DOW", 1);
            if (dowValueStr != null) {
                string cleanedDow = Regex.Replace(dowValueStr, @"[^\d]", "");
                if (int.TryParse(cleanedDow, out int dowValue)) {
                    data.Dow = dowValue;
                }
                else {
                    data.AddError($"Failed to parse DOW value '{dowValueStr}' as integer.");
                }
            }

            string? doiValueStr = FindValueNextToLabel(words, "DOI:", data, "DOI", 1);
            if (doiValueStr != null) {
                if (double.TryParse(doiValueStr, NumberStyles.Any, CultureInfo.InvariantCulture, out double doiValue)) {
                    data.Doi = doiValue;
                }
                else {
                    data.AddError($"Failed to parse DOI value '{doiValueStr}' as float.");
                }
            }

            ExtractPassengerCounts(words, data);
            ExtractCrewList(page, words, data);

            return data;
        }

        private string? FindValueNextToLabel(List<Word> words, string label, IErrorReporter errorReporter, string fieldName,
            int wordsToTake = DefaultWordsToTake) {
            for (int i = 0; i < words.Count; i++) {
                if (words[i].Text.Equals(label, StringComparison.OrdinalIgnoreCase)) {
                    Word labelWord = words[i];
                    List<string> valueWords = new List<string>();

                    int wordsFound = 0;
                    for (int j = i + 1; j < words.Count && wordsFound < wordsToTake; j++) {
                        Word potentialValueWord = words[j];

                        // Check if roughly on the same horizontal
                        bool onSameLine = Math.Abs(potentialValueWord.BoundingBox.Bottom - labelWord.BoundingBox.Bottom) < (labelWord.BoundingBox.Height * SameLineThreshold);

                        // Check if right and not too far away
                        bool isCloseEnough;
                        bool isToTheRight = potentialValueWord.BoundingBox.Left > labelWord.BoundingBox.Right;
                        if (isToTheRight) {
                            double gap = potentialValueWord.BoundingBox.Left - labelWord.BoundingBox.Right;
                            double maxAllowedGap = labelWord.BoundingBox.Width * MaxGapMultiplier;
                            bool isWithinGapLimit = gap < maxAllowedGap;

                            isCloseEnough = isWithinGapLimit;
                        }
                        else {
                            isCloseEnough = false;
                        }

                        if (onSameLine && isCloseEnough) {
                            valueWords.Add(potentialValueWord.Text);
                            wordsFound++;
                            labelWord = potentialValueWord;
                        }
                    }

                    if (!valueWords.Any()) {
                        string error = $"Could not find value for {fieldName} after label '{label}'";
                        errorReporter.AddError(error);
                        return null;
                    }

                    return string.Join(" ", valueWords);
                }
            }

            string labelNotFoundError = $"Label '{label}' for {fieldName} not found in document";
            Console.WriteLine(labelNotFoundError);
            errorReporter.AddError(labelNotFoundError);
            return null;
        }

        private int? ParseTime(string? timeStr, FlightLegData data, string fieldName) {
            if (string.IsNullOrWhiteSpace(timeStr)) {
                data.AddError($"Missing value for {fieldName}");
                return null;
            }

            string originalInput = timeStr;
            timeStr = timeStr.Replace(":", "");

            if (int.TryParse(timeStr, out int timeValue) &&
                timeStr.Length >= MIN_TIME_LENGTH && timeStr.Length <= MAX_TIME_LENGTH) {
                int hours = timeValue / 100;
                int minutes = timeValue % 100;


                if (hours >= 0 && hours <= MAX_HOURS && minutes >= 0 && minutes <= MAX_MINUTES) {
                    return timeValue;
                }
                else {
                    string error = $"Warning: Parsed value '{timeValue:D4}' from input '{originalInput}' represents an invalid time (HH must be 00-23, MM must be 00-59).";
                    Console.WriteLine(error);
                    data.AddError(error);
                    return null;
                }
            }
            else {
                // Failed basic parsing (not a number, wrong length, etc.)
                string error = $"Warning: Could not parse time string '{timeStr}' (from input '{originalInput}') due to invalid format or length.";
                data.AddError(error);
                return null;
            }
        }

        private int? ParseIntBeforeUnit(string? valueStr, FlightLegData data, string fieldName) {
            if (string.IsNullOrWhiteSpace(valueStr)) {
                data.AddError($"Missing value for {fieldName}");
                return null;
            }

            var match = Regex.Match(valueStr.Trim(), @"^(\d+)");
            if (match.Success && int.TryParse(match.Groups[1].Value, out int intValue)) {
                return intValue;
            }
            string error = $"Warning: Could not parse integer from '{valueStr}' for {fieldName}";
            Console.WriteLine(error);
            data.AddError(error);
            return null;
        }

        private double? ParseDouble(string? valueStr, FlightLegData data, string fieldName, bool extractSecondNumber = false) {
            if (string.IsNullOrWhiteSpace(valueStr)) {
                string error = $"Missing or empty value for {fieldName}";
                data.AddError(error);
                return null;
            }

            string trimmedValue = valueStr.Trim();
            string numericPart;

            if (extractSecondNumber) {
                string[] parts = trimmedValue.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 2) {
                    numericPart = parts[parts.Length - 1];
                }
                else {
                    string error = $"Warning: Expected two values but found only one " +
                        $"in '{trimmedValue}' for {fieldName}";
                    data.AddError(error);
                    return null;
                }
            }
            else {
                var match = Regex.Match(trimmedValue, @"^(\d+(\.\d+)?|\.\d+)");

                if (match.Success) {
                    numericPart = match.Groups[1].Value;
                }
                else {
                    string error = $"Warning: Could not extract a valid numeric pattern " +
                $"from '{trimmedValue}' for {fieldName}";
                    data.AddError(error);
                    return null;
                }
            }

            if (double.TryParse(numericPart, NumberStyles.Any, CultureInfo.InvariantCulture, out double parsedValue)) {
                if (parsedValue < 0) {
                    string error = $"Warning: Parsed value '{parsedValue}' for {fieldName} " +
                        $"is negative.";
                    data.AddError(error);
                    return null;
                }
                else {
                    return parsedValue;
                }
            }
            else {
                // TryParse failed even though Regex Matched
                string error = $"Warning: Could not parse extracted numeric part '{numericPart}' " +
                    $"as double for {fieldName} (Original: '{trimmedValue}')";
                data.AddError(error);
                return null;
            }
        }

        private int? ParseGainLoss(string? valueStr, FlightLegData data, string fieldName) {
            if (string.IsNullOrWhiteSpace(valueStr)) {
                data.AddError("Missing Gain/Loss value");
                return null;
            }

            string trimmedValue = valueStr.Trim();

            var match = Regex.Match(trimmedValue, @"(\d+)\$\/TON");
            if (!match.Success) {
                data.AddError($"Could not extract numeric value from Gain/Loss: '{trimmedValue}'");
                return null;
            }

            if (!int.TryParse(match.Groups[1].Value, out int numericValue)) {
                data.AddError($"Could not parse numeric value in Gain/Loss: '{match.Groups[1].Value}'");
                return null;
            }

            // determine if GAIN or LOSS
            bool isGain = trimmedValue.Contains("GAIN", StringComparison.OrdinalIgnoreCase);
            bool isLoss = trimmedValue.Contains("LOSS", StringComparison.OrdinalIgnoreCase);

            if (isGain && !isLoss) {
                return numericValue;
            }
            else if (isLoss && !isGain) {
                return -numericValue;
            }
            else {
                // Neither GAIN nor LOSS found
                data.AddError($"Could not determine if Gain/Loss: '{trimmedValue}' is a gain or loss");
                return null;
            }
        }

        private (string? firstWaypoint, string? lastWaypoint) ExtractRouteWaypoints(List<Word> words, FlightLegData data) {
            int toDestIndex = -1;
            int toAltn1Index = -1;

            for (int i = 0; i < words.Count; i++) {
                if (words[i].Text.Equals("To", StringComparison.OrdinalIgnoreCase)) {
                    if (i + 1 < words.Count) {
                        if (words[i + 1].Text.Equals("DEST:", StringComparison.OrdinalIgnoreCase)) {
                            toDestIndex = i + 1;
                        }
                        else if (words[i + 1].Text.Equals("ALTN1:", StringComparison.OrdinalIgnoreCase)) {
                            toAltn1Index = i;  // Index of "To" before "ALTN1:"
                        }
                    }
                }
            }

            // "To DEST:" not found, invalid state
            if (toDestIndex == -1) {
                string error = "Could not find 'To DEST:' label in document";
                data.AddError(error);
                return (null, null);
            }

            // Get first waypoint
            string? firstWaypoint = null;
            if (toDestIndex + 1 < words.Count) {
                firstWaypoint = words[toDestIndex + 1].Text;
            }
            else {
                string error = "No waypoint found after 'To DEST:' label";
                data.AddError(error);
            }

            if (toAltn1Index == -1) {
                string error = "Could not find 'To ALTN1:' label in document";
                data.AddError(error);
                return (firstWaypoint, null);
            }

            string? lastWaypoint = null;
            if (toAltn1Index > toDestIndex + 2) {
                lastWaypoint = words[toAltn1Index - 1].Text;
            }
            else {
                string error = "No waypoint found before 'To ALTN1:' label";
                data.AddError(error);
            }
            return (firstWaypoint, lastWaypoint);
        }


        private void ExtractPassengerCounts(List<Word> words, CrewBriefingData data) {
            Word? paxLabel = words.FirstOrDefault(w => w.Text.Equals("PAX", StringComparison.OrdinalIgnoreCase));
            if (paxLabel == null) {
                data.AddError("Label 'PAX' not found for passenger counts.");
                return;
            }

            Word? cyLabel = words.Where(w => w.Text.Equals("C/Y", StringComparison.OrdinalIgnoreCase) &&
            w.BoundingBox.Top < paxLabel.BoundingBox.Bottom &&
            Math.Abs(w.BoundingBox.Centroid.X - paxLabel.BoundingBox.Centroid.X) < paxLabel.BoundingBox.Width)
                .OrderByDescending(w => w.BoundingBox.Top)
                .FirstOrDefault();

            if (cyLabel == null) {
                data.AddError("Label 'C/Y' not found below 'PAX'.");
                return;
            }


            Word? valueWord = words.Where(w => w.Text.Contains('/') &&
            w.BoundingBox.Top < cyLabel.BoundingBox.Bottom &&
            Math.Abs(w.BoundingBox.Centroid.X - paxLabel.BoundingBox.Centroid.X) < paxLabel.BoundingBox.Width * 1.5)
                .OrderByDescending(w => w.BoundingBox.Top)
                .FirstOrDefault();

            if (valueWord == null) {
                data.AddError("Passenger count value not found below 'C/Y'.");
                return;
            }

            string[] parts = valueWord.Text.Split('/');
            if (parts.Length == 2) {
                if (int.TryParse(parts[0], out int paxC)) {
                    data.PaxBusiness = paxC;
                }
                else {
                    data.AddError($"Failed to parse Business Class passenger count from '{parts[0]}'");
                }

                if (int.TryParse(parts[1], out int paxY)) {
                    data.PaxEconomy = paxY;
                }
                else {
                    data.AddError($"Failed to parse Economy Class passenger count from '{parts[1]}'.");
                }
            }
            else {
                data.AddError($"Passenger count value '{valueWord.Text}' does not have the expected 'X/Y' format.");
            }
        }


        private void ExtractCrewList(Page page, List<Word> words, CrewBriefingData data) {
            // Find headers to define columns and starting point
            Word? crewTitle = words.FirstOrDefault(w => w.Text.Equals("Crew", StringComparison.OrdinalIgnoreCase));
            Word? funcHeader = words.FirstOrDefault(w => w.Text.Equals("Func", StringComparison.OrdinalIgnoreCase));
            Word? tlcHeader = words.FirstOrDefault(w => w.Text.Equals("3LC", StringComparison.OrdinalIgnoreCase));
            Word? nameHeader = words.FirstOrDefault(w => w.Text.Equals("Name", StringComparison.OrdinalIgnoreCase));

            if (funcHeader == null || nameHeader == null) {
                data.AddError("Crew list headers 'Func' or 'Name' not found.");
                return;
            }

            // Column boundaries
            double funcColumnStart = funcHeader.BoundingBox.Left - ColumnPadding;
            double funcColumnEnd = (tlcHeader?.BoundingBox.Left ?? nameHeader.BoundingBox.Left) - ColumnPadding;

            double nameColumnStart = nameHeader.BoundingBox.Left - ColumnPadding;
            double nameColumnEnd = nameHeader.BoundingBox.Right + (nameHeader.BoundingBox.Width * NameColumnExtension);

            double startY = new[] {
                funcHeader.BoundingBox.Bottom,
                nameHeader.BoundingBox.Bottom
            }.Min();

            Word? observerHeader = words.FirstOrDefault(w => w.Text.Equals("Observer", StringComparison.OrdinalIgnoreCase));
            double endY = observerHeader?.BoundingBox.Top ?? 0;

            // Process lines
            var lines = words.Where(w => w.BoundingBox.Bottom < startY && w.BoundingBox.Top > endY)
                .GroupBy(w => Math.Round(w.BoundingBox.Centroid.Y, 0))
                .OrderByDescending(g => g.Key);

            foreach (var lineGroup in lines) {
                List<Word> lineWords = lineGroup.OrderBy(w => w.BoundingBox.Left).ToList();

                if (!lineWords.Any() || lineWords.Count < 2) continue;

                // Extract func 
                var funcWords = lineWords.Where(w => w.BoundingBox.Centroid.X >= funcColumnStart &&
                w.BoundingBox.Centroid.X < funcColumnEnd).ToList();
                string functionCode = string.Join(" ", funcWords.Select(w => w.Text));

                // Extract name
                var nameWords = lineWords.Where(w => w.BoundingBox.Centroid.X >= nameColumnStart &&
                w.BoundingBox.Centroid.X < nameColumnEnd).ToList();
                string name = string.Join(" ", nameWords.Select(w => w.Text));

                if (!string.IsNullOrWhiteSpace(functionCode) &&
                    !string.IsNullOrWhiteSpace(name)) {
                    data.CrewMembers.Add(new CrewMember
                    {
                        Name = name,
                        Function = functionCode
                    });
                }
            }
            if (data.CrewMembers.Count == 0) {
                data.AddError("No crew members could be extracted from the table.");
            }
        }
    }
}
