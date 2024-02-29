using HtmlAgilityPack;
using OpenQA.Selenium.Chrome;
using System.Text;

async Task ConvertTableToCSV(string url)
{
    // Setup WebDriver (make sure to have chromedriver.exe in your project or in your PATH)
    var options = new ChromeOptions();
    options.AddArguments(["headless", "--log-level=3"]); // Optional: Run Chrome in headless mode
    using var driver = new ChromeDriver(options);

    // Navigate to the page
    driver.Navigate().GoToUrl(url);

    // Wait for page to load
    while (driver?.PageSource is null) { await Task.Delay(500); }

    // Get the page source
    var html = driver.PageSource;

    // Now parse the HTML as before
    var doc = new HtmlDocument();
    doc.LoadHtml(html);

    // Find the table with the specified class
    var table = doc.DocumentNode.SelectSingleNode("//table[@class='w3-table sortable w3-responsive w3-striped w3-auto']");

    if (table == null)
        return; // Return an empty JSON array if table is not found

    var headers = table.SelectNodes(".//th").Select(th => th.InnerText.Trim()).ToList();
    var rows = table.SelectNodes(".//tr[position()>1]"); // Skip header row

    var csvBuilder = new StringBuilder();

    // Add headers
    csvBuilder.AppendLine(string.Join(",", headers));

    // Add rows
    foreach (var row in rows)
    {
        var cells = row.SelectNodes(".//td").Select(td => $"\"{td.InnerText.Trim().Replace("\"", "\"\"")}\""); // Escape quotes
        csvBuilder.AppendLine(string.Join(",", cells));
    }

    // Save the CSV content to a file
    await File.WriteAllTextAsync(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\Repeaters.csv", csvBuilder.ToString());
}

try
{
    await ConvertTableToCSV("https://www.repeaterbook.com/repeaters/Display_SS.php?state_id=26&loc=%&call=%&use=%");
    Console.WriteLine("\n=================================\nDocument written to desktop.\nPress any key to exit.");
}
catch (Exception e)
{
    Console.WriteLine($"\n=================================\nAn error occured: {e.Message}.\nPress any key to exit.");
}

Console.ReadKey(false);
