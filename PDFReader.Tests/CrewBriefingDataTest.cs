using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PDFReader;

namespace PDFReader.Tests;

[TestClass]
public class CrewBriefingDataTest
{
    [TestMethod]
    public void TestCrewBriefingDataToStringCorrect() {
        // Arrange
        var crewBriefing = new CrewBriefingData
        {
            Identifier = "LX10/SWR10A",
            PaxBusiness = 12,
            PaxEconomy = 150,
            Dow = 42500,
            Doi = 23.5f,
            CrewMembers = new List<CrewMember>
            {
                new CrewMember { Name = "Werner Trütsch", Function = "CMD" },
                new CrewMember { Name = "Luca Andrea Marchetti", Function = "COP" },
                new CrewMember { Name = "Helen Meier", Function = "CAB" }
            }
        };

        var expectedSb = new StringBuilder();
        expectedSb.AppendLine("Identifier: LX10/SWR10A");
        expectedSb.AppendLine("Number of passengers in business (C) class: 12");
        expectedSb.AppendLine("Number of passengers in economy (Y) class: 150");
        expectedSb.AppendLine("Dry operating weight: 42500");
        expectedSb.AppendLine("Dry operating index: 23.5");
        expectedSb.AppendLine("Crew Members:");
        expectedSb.AppendLine("Werner Trütsch, CMD");
        expectedSb.AppendLine("Luca Andrea Marchetti, COP");
        expectedSb.AppendLine("Helen Meier, CAB");
        string expected = expectedSb.ToString();

        // Act
        string actual = crewBriefing.ToString();

        // Assert
        Assert.AreEqual(expected, actual);

        Console.WriteLine("=== ToString() Output ====");
        Console.WriteLine(actual);
        Console.WriteLine("=== End of Output ===");
    }
}
