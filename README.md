# Task: Extract Data from a PDF File

## Overview

This module provides a flexible solution for extracting text fields from a PDF file. It's designed to handle different PDF structures while maintaining a clean, extensible architecture. The primary goal is to reliably extract specific information, regardless of formatting variations.

## Approach and Process

I started by reading through the task itself and researching the PDF format. I’ve used PDF files countless times in my life, but I had never actually looked into how they function or how they're structured. Initially, I considered an approach where, for example, you could use a ```BufferedReader``` (a Java class) to read the content of a text file line by line. However, it quickly became clear that this would be a significant undertaking in itself. So, I opted to use PdfPig (https://uglytoad.github.io/PdfPig/) instead.

## Why PdfPig?

- Apache 2.0 license (safe to use legally)
- C# Native library

## Class Structure

The data extraction process focuses on two types of information: data from the Operational Flight Plan (OFP) and the corresponding Crew Briefing, which is assigned per flight. I also had to account for the possibility that assignments could change. For example, a flight from ZRH to FRA might be planned, but the crew could change if a pilot or flight attendant becomes unavailable. Nevertheless, every flight leg has a corresponding crew assignment.

To neatly organize the data, I decided to create two primary classes:

- ```FlightLegData```: Container for all flight-specific operational information
- ```CrewBriefingData```: Container for all crew assignment information
- ```CrewMember```: Container for individual crew member information

These classes serve as data containers, with each specific flight leg (for example, ZRH–FRA) represented by an instance of ```FlightLegData```, and its corresponding crew information stored in a ```CrewBriefingData``` instance. To establish the relationship between these entities, I use a ```FlightPlan``` class that links a flight leg with its appropriate crew briefing.

Now that I’ve defined the basic structure, the next step is to extract actual data from the PDF. For this, I initially considered using separate extractors:

- ```PdfExtractor```: A base class with common PDF handling
- ```FlightLegExtractor```, ```CrewBriefingExtractor```: For extracting specific information

While this approach offers higher cohesion, it introduces the challenge of coordinating multiple extractors and potentially processing pages twice, which isn’t efficient. Additionally, it would require extra logic to assemble the extracted data into ```FlightPlan``` objects.

Therefore, I opted for a single combined ```FlightPlanExtractor``` that:

- Processes each page only once
- Identifies whether the page contains flight information, crew information or both
- Extracts the relevant data into the appropriate objects
- Links flight legs with their crew briefings

This approach simplifies the extraction process while maintaining a clean separation in the data model. The extractor uses pattern recognition to determine the content type of each page and extract the corresponding data, ensuring each flight leg is properly associated with its crew information.

For the presentation of data on the console, I will use a class ```FlightPlanConsolePresenter```,which focuses exclusively on console output. Finally, the ```Main``` class will serve as the entry point for the program.

## External Communication

One of the requirements is that the application should be prepared for integration into a larger system. However, this is somewhat vague, as it doesn’t specify whether other classes will call into this module or if there’s a need to interact with a database. To keep things simple, I’ve limited external communication to the following:

- Export to JSON: Save the extracted information into a JSON file
- Interface: Provide an interface that other classes can use to access the data

## Error Handling Consideration

When designing the ```FlightLegData``` and ```CrewBriefingData``` classes, I need to address how to handle missing fields in the PDF documents. For example, if the Zero Fuel Mass (ZFM) is missing, should I make the ZFM field nullable (using int?) and allow it to be null, or treat the entire flight plan as invalid?

The Zero Fuel Mass is a critical aspect of the flight that's vital to performance calculations. However, it's ultimately the flight crew's responsibility to ensure all data is present. If something is missing, they would need to take corrective action, such as re-downloading the flight plan or consulting with the load manager.

For the sake of program integrity and usability, I've decided that when an invalid or non-existent data point is encountered, the corresponding data field of the object will be set to ```null```. This approach allows the program to continue processing while making it clear to users that certain data points may require verification.

## Testing

For testing, I've decided to use MSTest to keep things simple for my first C# project and to get acquainted with testing in C#.

--