using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PDFReader;

namespace PDFReader.Tests {
    [TestClass]
    public sealed class FlightLegDataTest {
        [TestMethod]
        public void TestFlightLegDataToStringCorrect() {
            // Arrange
            var flightLeg = new FlightLegData
            {
                Date = "26OCT2023",
                Registration = "G-ABCD",
                DepartureIcao = "EGLL",
                ArrivalIcao = "KJFK",
                FirstAlternate = "KBOS",
                SecondaryAlternate = "KIAD",
                FlightNumber = "BA123",
                AtcCallsign = "BAW123",
                DepartureTime = 1030,
                ArrivalTime = 1845,
                ZeroFuelMass = 55000,
                TimeToDestination = 480,
                FuelToDestination = 45000,
                TimeToAlternate = 60,
                FuelToAlternate = 5000,
                MinFuelRequired = 52000,
                RouteFirstNavPoint = "DET",
                RouteLastNavPoint = "CANAL",
                GainLoss = -5,
                Identifier = "LX10/SWR10A"
            };

            var expectedSb = new StringBuilder();
            expectedSb.AppendLine("Date: 26OCT2023");
            expectedSb.AppendLine("Flight: BA123/BAW123 (G-ABCD)");
            expectedSb.AppendLine("From: EGLL To: KJFK");
            expectedSb.AppendLine("Dep: 10:30Z Arr: 18:45Z");
            expectedSb.AppendLine("Alternates: 1st: KBOS, 2nd: KIAD");
            expectedSb.AppendLine("Time to Dest: 08:00, Fuel to Dest: 45000");
            expectedSb.AppendLine("Time to Alt: 01:00, Fuel to Alt: 5000");
            expectedSb.AppendLine("Min Fuel Req: 52000");
            expectedSb.AppendLine("Route Waypoints: 1st: DET, Last: CANAL");
            expectedSb.AppendLine("ZFM: 55000");
            expectedSb.AppendLine("Gain/Loss: -5");
            expectedSb.Append("Identifier: LX10/SWR10A");
            string expected = expectedSb.ToString();

            // Act
            string actual = flightLeg.ToString();

            // Assert
            Assert.AreEqual(expected, actual);

            // For Visualisation
            Console.WriteLine("=== ToString() Output ====");
            Console.WriteLine(actual);
            Console.WriteLine("=== End of Output ===");
        }
    }
}
