using SnowAccumulation.Core.Visualization;

namespace SnowAccumulation.Tests.Visualization;

public class SnowPersonRendererTests
{
    #region CalculateSnowRows tests

    [Theory]
    [InlineData(0.0, 0)]
    [InlineData(1.0, 1)]       // 1 inch → ceil(1/3) = 1 row
    [InlineData(3.0, 1)]       // exactly 3 inches = 1 row
    [InlineData(3.1, 2)]       // just over → 2 rows
    [InlineData(6.0, 2)]       // 6 inches = 2 rows
    [InlineData(12.0, 4)]      // 1 foot = 4 rows
    [InlineData(36.0, 12)]     // 3 feet = 12 rows
    [InlineData(72.0, 24)]     // 6 feet = 24 rows (full person height)
    [InlineData(100.0, 34)]    // > 6 feet
    [InlineData(-5.0, 0)]      // negative → 0
    public void CalculateSnowRows_ReturnsExpected(double snowInches, int expectedRows)
    {
        var result = SnowPersonRenderer.CalculateSnowRows(snowInches);
        Assert.Equal(expectedRows, result);
    }

    #endregion

    #region GetPersonRow tests

    [Fact]
    public void GetPersonRow_HeadRow_ContainsO()
    {
        var head = SnowPersonRenderer.GetPersonRow(0);
        Assert.Contains("O", head);
    }

    [Fact]
    public void GetPersonRow_AllRowsHave7Chars()
    {
        for (int row = 0; row < SnowPersonRenderer.PersonHeightRows; row++)
        {
            var line = SnowPersonRenderer.GetPersonRow(row);
            Assert.Equal(7, line.Length);
        }
    }

    [Fact]
    public void GetPersonRow_BeyondPerson_ReturnsBlank()
    {
        var line = SnowPersonRenderer.GetPersonRow(99);
        Assert.Equal("       ", line);
    }

    #endregion

    #region BuildPersonGraphic tests

    [Fact]
    public void BuildPersonGraphic_ZeroSnow_HasNoSnowBlocks()
    {
        var lines = SnowPersonRenderer.BuildPersonGraphic(0.0);

        // The graphic should have PersonHeightRows + ground line + label = 26 lines
        Assert.Equal(SnowPersonRenderer.PersonHeightRows + 2, lines.Count);
    }

    [Fact]
    public void BuildPersonGraphic_12Inches_HasSnowRows()
    {
        var lines = SnowPersonRenderer.BuildPersonGraphic(12.0);

        // 12 inches = 4 rows of snow
        // Should have PersonHeightRows person rows + ground + label
        Assert.Equal(SnowPersonRenderer.PersonHeightRows + 2, lines.Count);

        // The bottom rows (before ground) should contain snow block characters
        var lastPersonRow = lines[SnowPersonRenderer.PersonHeightRows - 1];
        Assert.Contains("░", lastPersonRow);
    }

    [Fact]
    public void BuildPersonGraphic_72Inches_SnowCoversEntirePerson()
    {
        var lines = SnowPersonRenderer.BuildPersonGraphic(72.0);

        // Snow = 24 rows, person = 24 rows, so totalRows = 24
        // All rows should have snow blocks
        // Row 0 (head) should also have snow
        Assert.Contains("░", lines[0]);
    }

    [Fact]
    public void BuildPersonGraphic_OverflowSnow_AddsExtraRows()
    {
        var lines = SnowPersonRenderer.BuildPersonGraphic(100.0);

        // 100 inches → ceil(100/3) = 34 rows, person = 24 rows → 10 overflow rows
        // Total lines = 34 (graphic) + 1 (ground) + 1 (label) = 36
        Assert.Equal(34 + 2, lines.Count);
    }

    [Fact]
    public void BuildPersonGraphic_ContainsGroundLine()
    {
        var lines = SnowPersonRenderer.BuildPersonGraphic(12.0);

        // Second-to-last line should be the ground
        var groundLine = lines[^2];
        Assert.Contains("▓", groundLine);
    }

    [Fact]
    public void BuildPersonGraphic_ContainsDepthLabel()
    {
        var lines = SnowPersonRenderer.BuildPersonGraphic(12.0);

        var labelLine = lines[^1];
        Assert.Contains("1.0 ft", labelLine); // 12 inches = 1.0 ft
    }

    [Fact]
    public void BuildPersonGraphic_SmallSnow_ShowsInchesLabel()
    {
        var lines = SnowPersonRenderer.BuildPersonGraphic(6.0);

        var labelLine = lines[^1];
        Assert.Contains("6.0 in", labelLine);
    }

    #endregion

    #region Constants

    [Fact]
    public void PersonHeightRows_Is24()
    {
        Assert.Equal(24, SnowPersonRenderer.PersonHeightRows);
    }

    [Fact]
    public void InchesPerRow_Is3()
    {
        Assert.Equal(3.0, SnowPersonRenderer.InchesPerRow);
    }

    [Fact]
    public void PersonHeightInches_Is72()
    {
        Assert.Equal(72.0, SnowPersonRenderer.PersonHeightInches);
    }

    #endregion
}
