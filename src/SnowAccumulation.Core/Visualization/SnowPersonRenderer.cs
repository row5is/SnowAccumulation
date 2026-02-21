using Spectre.Console;
using Spectre.Console.Rendering;
using SnowAccumulation.Core.Models;

namespace SnowAccumulation.Core.Visualization;

/// <summary>
/// Renders an ASCII/Spectre.Console graphic comparing snow accumulation to a 6-foot person.
/// </summary>
public class SnowPersonRenderer
{
    /// <summary>
    /// Height of the person in rows in the visual. Each row = 3 inches = 0.25 feet.
    /// 6 feet = 24 rows.
    /// </summary>
    public const int PersonHeightRows = 24;

    /// <summary>
    /// Inches per row in the visualization.
    /// </summary>
    public const double InchesPerRow = 3.0;

    /// <summary>
    /// Total person height in inches (72).
    /// </summary>
    public const double PersonHeightInches = 72.0;

    /// <summary>
    /// Renders the snow-vs-person comparison to the given <see cref="IAnsiConsole"/>.
    /// </summary>
    public void Render(IAnsiConsole console, SnowForecast forecast, bool useMetric = false)
    {
        ArgumentNullException.ThrowIfNull(console);
        ArgumentNullException.ThrowIfNull(forecast);

        var totalInches = forecast.TotalSnowInches;
        var totalCm = forecast.TotalSnowCm;

        if (!forecast.HasSnow)
        {
            console.MarkupLine("[yellow]No snow expected for this forecast period![/]");
            console.WriteLine();
            return;
        }

        var depthLabel = useMetric
            ? $"{totalCm:F1} cm"
            : $"{totalInches:F1} in";

        // Header
        console.MarkupLine($"[bold blue]Expected Snow Accumulation: {Markup.Escape(depthLabel)}[/]");
        console.WriteLine();

        var lines = BuildPersonGraphic(totalInches);

        var panel = new Panel(string.Join(Environment.NewLine, lines))
        {
            Header = new PanelHeader(" Snow vs. 6-foot Person "),
            Border = BoxBorder.Rounded,
            Padding = new Padding(1, 0, 1, 0)
        };

        console.Write(panel);
        console.WriteLine();

        // Summary line
        if (totalInches >= PersonHeightInches)
        {
            console.MarkupLine($"[bold red]Snow depth ({Markup.Escape(depthLabel)}) would completely bury a 6-foot person![/]");
        }
        else if (totalInches >= 48)
        {
            console.MarkupLine($"[bold red]Snow depth ({Markup.Escape(depthLabel)}) would reach chest height on a 6-foot person![/]");
        }
        else if (totalInches >= 24)
        {
            console.MarkupLine($"[bold yellow]Snow depth ({Markup.Escape(depthLabel)}) would reach knee height on a 6-foot person![/]");
        }
        else
        {
            console.MarkupLine($"[blue]Snow depth ({Markup.Escape(depthLabel)}) — ankle to shin height on a 6-foot person.[/]");
        }

        console.WriteLine();
    }

    /// <summary>
    /// Builds the ASCII lines for the person-vs-snow graphic.
    /// Returns a list of markup-enabled strings from top to bottom.
    /// </summary>
    internal static List<string> BuildPersonGraphic(double snowInches)
    {
        var snowRows = CalculateSnowRows(snowInches);
        var lines = new List<string>();

        // Column layout (each line):
        //   [person col ~7 chars] [spacer 3] [snow col ~10 chars] [spacer 3] [ruler col ~6 chars]
        // We build from top (row 0 = top of person's head) to bottom (row PersonHeightRows - 1 = feet).
        // Extra rows above if snow exceeds person height.

        var totalRows = Math.Max(PersonHeightRows, snowRows);
        var overflowRows = totalRows - PersonHeightRows; // extra rows above person for deep snow

        // Feet ruler labels: mark every 4 rows (1 foot)
        // Row index from top of the person (excluding overflow): 0 = 6ft, 4 = 5ft, ... 20 = 1ft
        var rulerLabels = new Dictionary<int, string>();
        for (int ft = 6; ft >= 1; ft--)
        {
            var rowFromTopOfPerson = (6 - ft) * 4;
            rulerLabels[rowFromTopOfPerson] = $"{ft}ft";
        }

        for (int row = 0; row < totalRows; row++)
        {
            var personRow = row - overflowRows; // row index relative to person (0 = head)
            var rowFromBottom = totalRows - 1 - row;

            // Person column
            string personPart;
            if (personRow < 0)
                personPart = "       "; // above person
            else
                personPart = GetPersonRow(personRow);

            // Snow column: filled if this row is within snow depth (from bottom)
            string snowPart;
            if (rowFromBottom < snowRows)
                snowPart = "[white on blue]░░░░░░░░░░[/]";
            else
                snowPart = "          ";

            // Ruler column
            string rulerPart = "      ";
            if (personRow >= 0 && rulerLabels.TryGetValue(personRow, out var label))
                rulerPart = $"── {label}";
            else if (personRow >= 0)
                rulerPart = "│     ";

            lines.Add($"{personPart}   {snowPart}   {rulerPart}");
        }

        // Ground line
        lines.Add($"{"▓▓▓▓▓▓▓"}   {"▓▓▓▓▓▓▓▓▓▓"}   {"▓▓▓▓▓▓"}");

        // Snow depth label under the snow column
        var depthStr = snowInches >= 12
            ? $"{snowInches / 12.0:F1} ft ({snowInches:F1} in)"
            : $"{snowInches:F1} in";
        lines.Add($"{"Person"}    {"↑ " + depthStr,-10}");

        return lines;
    }

    /// <summary>
    /// Returns the ASCII art for a specific row of the 6-foot person (0 = top of head).
    /// The person is 24 rows tall (3 inches per row).
    /// </summary>
    internal static string GetPersonRow(int row)
    {
        return row switch
        {
            0 =>  "   O   ",  // head top
            1 =>  "  /|\\  ",  // shoulders + head
            2 =>  "  /|\\  ",  // upper torso
            3 =>  "   |   ",  // torso
            4 =>  "   |   ",  // torso
            5 =>  "   |   ",  // torso
            6 =>  "   |   ",  // torso
            7 =>  "   |   ",  // torso
            8 =>  "   |   ",  // waist
            9 =>  "   |   ",  // waist
            10 => "   |   ",  // hips
            11 => "   |   ",  // hips
            12 => "   |   ",  // upper legs
            13 => "  / \\  ",  // legs split
            14 => "  / \\  ",  // legs
            15 => " /   \\ ",  // legs
            16 => " /   \\ ",  // legs
            17 => " /   \\ ",  // legs
            18 => " |   | ",  // lower legs
            19 => " |   | ",  // lower legs
            20 => " |   | ",  // shins
            21 => " |   | ",  // shins
            22 => " |   | ",  // ankles
            23 => " |   | ",  // feet
            _ =>  "       ",  // beyond person
        };
    }

    /// <summary>
    /// Calculates the number of display rows the snow occupies.
    /// </summary>
    internal static int CalculateSnowRows(double snowInches)
    {
        if (snowInches <= 0) return 0;
        return (int)Math.Ceiling(snowInches / InchesPerRow);
    }
}
