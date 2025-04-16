# Task: Extract Data from a PDF File

## Overview

This module provides a flexible solution for extracting text fields from a PDF file. It's designed to handle different PDF structures while maintaining a clean, extensible architecture. The primary goal is to reliably extract specific information, regardless of formatting variations.

## Approach and Process

I started by reading through the task and researching the PDF format. I’ve used PDF files countless times in my life, but I had never actually looked into how they function or how they're structured. Initially, I considered an approach where, for example, you could use a `BufferedReader` (a Java class) to read the content of a text file line by line. However, it quickly became clear that this would be a significant undertaking in itself. So, I opted to use PdfPig ([https://uglytoad.github.io/PdfPig/](https://uglytoad.github.io/PdfPig/)) instead.

## Why PdfPig?

- Apache 2.0 license (safe to use legally)  
- C# native library

## Class Structure

The data extraction process focuses on two types of information: data from the Operational Flight Plan (OFP) and the corresponding Crew Briefing, which is assigned per flight. I also had to account for the possibility that assignments could change. For example, a flight from ZRH to FRA might be planned, but the crew could change if a pilot or flight attendant becomes unavailable. Nevertheless, every flight leg has a corresponding crew assignment.

To organize data cleanly and efficiently, I decided to use the following structure:

- `FlightLegData`: Container for all flight-specific operational information  
- `CrewBriefingData`: Container for all crew-specific information  
- `CrewMember`: Container for individual crew member information  
- `FlightPlan`: Combines/links a `FlightLegData` and `CrewBriefingData` for a complete flight  
- `FlightPlanExtractor`: Core class that handles detection and extraction logic  
- `FlightPlanLinker`: Responsible for matching flight leg data with corresponding crew briefing data  
- `FlightPlanConsolePresenter`: Handles console output formatting and display  
- `FlightPlanStatus`: Enum that tracks the completeness and validity of flight plans  
- `Program`: Application entry point  
- `IFlightPlanExtractor`, `IErrorReporter`: Interfaces for extensibility and abstraction

## Extraction Strategy

I initially considered using separate extractors:

- `PdfExtractor`: A base class with common PDF handling  
- `FlightLegExtractor`, `CrewBriefingExtractor`: For extracting specific information

While this approach offers higher cohesion, it introduces the challenge of coordinating multiple extractors and potentially processing pages twice, which isn’t efficient. Additionally, it would require extra logic to assemble the extracted data into `FlightPlan` objects.

Therefore, I opted for a single combined `FlightPlanExtractor` that:

- Processes each page only once  
- Identifies whether the page contains flight information, crew information, or both  
- Extracts the relevant data into the appropriate objects  
- Links flight legs with their crew briefings

This approach simplifies the extraction process while maintaining a clean separation in the data model. The extractor uses pattern recognition to determine the content type of each page and extract the corresponding data, ensuring each flight leg is properly associated with its crew information.

For presenting data on the console, I use a `FlightPlanConsolePresenter` class, which focuses exclusively on console output.

## External Communication

One of the requirements is that the application should be prepared for integration into a larger system. However, this is somewhat vague, as it doesn’t specify whether other classes will call into this module or if there’s a need to interact with a database. To keep things simple, I’ve limited external communication to the following:

- Export to JSON: Save the extracted information into a JSON file  
- Interface: Provide an interface that other classes can use to access the data

## Error Handling Consideration

When designing the `FlightLegData` and `CrewBriefingData` classes, I had to address how to handle missing fields in the PDF documents. For example, if the Zero Fuel Mass (ZFM) is missing, should I make the ZFM field nullable (using `int?`) and allow it to be null, or treat the entire flight plan as invalid?

The Zero Fuel Mass is a critical aspect of the flight that's vital to performance calculations. However, it's ultimately the flight crew's responsibility to ensure all data is present. If something is missing, they would need to take corrective action, such as re-downloading the flight plan or consulting with the load manager.

For the sake of program integrity and usability, I've decided that when an invalid or non-existent data point is encountered, the corresponding data field of the object will be set to `null`. This approach allows the program to continue processing while making it clear to users that certain data points may require verification.

## Console Presentation

To provide clear and structured output to users, I implemented a dedicated `FlightPlanConsolePresenter` class that handles all console formatting and display logic. This separation of concerns keeps the presentation logic isolated from the data processing and extraction logic.

## Testing

For testing, I decided to use MSTest to keep things simple for my first C# project and to get acquainted with testing in C#.

## Points to Improve

- Right now, all logging goes to the console. Switching to a proper logging framework (possibly with dependency injection) would be better.  
- There’s room to make the field parsing more generic and reusable.  
- More lambda functions could be used in the `FlightPlanExtractor`. For example, the methods that extract information from the Crew Briefing pages use lambdas more than those for the Flight Plan pages. This would make the code more readable and concise.  
- Testing is currently very basic and limited—it just processes the PDF and prints the output. It acts more like a sanity check than a proper verification of specific parsing behavior or error handling.  
- For a more complex system, instead of using a simple `FlightPlan` class to link `FlightLegData` and `CrewBriefingData`, the repository pattern could be used.

## Problems/Bugs

- Running the program "normally" leads to an error. I've created very simple tests to verify the program flow and get a better idea of all the function calls.  
- Extracting the list of crew members struggles with the last crew member’s name, leading to slightly incorrect output.

## Assistance Tools Used

- AI to aid me in writing the code and explaining how certain C# functions work, including syntax and semantic differences from Java  
- Official Microsoft C# documentation: https://learn.microsoft.com/en-us/dotnet/api/?view=net-9.0  
- For naming conventions: https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/identifier-names

## Time Used

This project took me approximately 18 hours, from start to the current state with all the documentation.

## Closing Words

I enjoyed taking on this task—working with C# made me appreciate the language a lot. Features like optionally nullable data fields are fantastic and something that Java lacks.  

I would have liked to implement more comprehensive tests, especially using mocks, as the current tests are very minimal. Additionally, the program has issues running normally, which isn't ideal. That said, I'm pleased with the code quality. It's not perfect by any means, but I tried my best to keep the code modular, performant, and easy to read.
