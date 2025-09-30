using Microsoft.Extensions.AI;
using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Data.Sqlite;

namespace AspNetCoreMcpPerSessionTools.Tools;

/// <summary>
/// User information tools
/// </summary>
[McpServerToolType]
public sealed class BolagsverketTool
{
    [McpServerTool, Description("Looking information about bolag (companies) in sweden using SQLite database")]
    public static string GetCompanyInfo(
         [Description("Search string to look for in the company data, e.g. Company Name, Address or Industry")] string searchString)
    {
        if (string.IsNullOrWhiteSpace(searchString))
        {
            return "Error: Search string cannot be empty";
        }

        if (!File.Exists(Settings.SqliteDatabasePath))
        {
            return $"Error: Database file not found at {Settings.SqliteDatabasePath}. Please run the converter tool first.";
        }

        var sb = new StringBuilder();
        int resultCount = 0;
        const int maxResults = 100;

        try
        {
            using var connection = new SqliteConnection($"Data Source={Settings.SqliteDatabasePath}");
            connection.Open();

            using var command = connection.CreateCommand();

            // Search across multiple columns using LIKE with wildcards
            command.CommandText = @"
                SELECT
                    Organisationsidentitet,
                    Organisationsnamn,
                    Organisationsform,
                    Registreringsdatum,
                    Avregistreringsdatum,
                    Verksamhetsbeskrivning,
                    Postadress
                FROM Organisationer
                WHERE
                    Organisationsnamn LIKE '%' || $search || '%' 
                LIMIT $limit
            ";
            /*
             * 
             * OR
                    Organisationsidentitet LIKE '%' || $search || '%' OR
                    Verksamhetsbeskrivning LIKE '%' || $search || '%' OR
                    Postadress LIKE '%' || $search || '%'
            */
            command.Parameters.AddWithValue("$search", searchString);
            command.Parameters.AddWithValue("$limit", maxResults);

            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                resultCount++;

                sb.AppendLine($"--- Company {resultCount} ---");
                sb.AppendLine($"Organization ID: {reader.GetString(0)}");
                sb.AppendLine($"Name: {reader.GetString(1)}");
                sb.AppendLine($"Form: {reader.GetString(2)}");
                sb.AppendLine($"Registration Date: {reader.GetString(3)}");

                if (!reader.IsDBNull(4))
                    sb.AppendLine($"Deregistration Date: {reader.GetString(4)}");

                if (!reader.IsDBNull(5))
                {
                    string description = reader.GetString(5);
                    if (description.Length > 200)
                        description = description.Substring(0, 200) + "...";
                    sb.AppendLine($"Description: {description}");
                }

                if (!reader.IsDBNull(6))
                    sb.AppendLine($"Address: {reader.GetString(6)}");

                sb.AppendLine();
            }

            if (resultCount == 0)
            {
                return $"No companies found matching '{searchString}'";
            }

            if (resultCount >= maxResults)
            {
                sb.AppendLine($"Note: Showing first {maxResults} results. Please refine your search for more specific results.");
            }
            else
            {
                sb.AppendLine($"Total results: {resultCount}");
            }

            return sb.ToString();
        }
        catch (Exception ex)
        {
            return $"Error querying database: {ex.Message}";
        }
    }

    [McpServerTool(Name = "getInlineAnnualReport"), Description("Get a annual inline report")]
    public static AIContent GetAnnualReport() {
        var data = File.ReadAllBytes(@"C:\Users\blasi\Downloads\5591900047_2019-12-31\9eb71ed6-f93b-4813-bf51-04b7a444ec62.xhtml");

        return new DataContent("data:text/html;base64," + Convert.ToBase64String(data), "text/html");        
    }
   

    [McpServerTool, Description("Check which years have annual reports available for a specific organization without downloading them")]
    public static string CheckAvailableYears(
        [Description("Organization number (e.g. 559190-0047)")] string organizationId)
    {
        if (string.IsNullOrWhiteSpace(organizationId))
        {
            return "Error: Organization ID cannot be empty";
        }

        string filePath = Settings.LookupTableFilePath;

        if (!File.Exists(filePath))
        {
            return $"Error: Lookup table file not found at {filePath}";
        }

        // Normalize the organization ID by removing dashes
        string normalizedOrgId = organizationId.Replace("-", "");

        var availableYears = new SortedSet<int>();

        try
        {
            using (var reader = new StreamReader(filePath))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    // Skip "Processing:" lines
                    if (line.StartsWith("Processing: "))
                        continue;

                    // Look for lines containing the organization ID
                    // Format: filename like "5560864414_2019-06-30.zip"
                    if (line.Contains(normalizedOrgId + "_"))
                    {
                        // Extract the filename from the line (it's at the end after tabs)
                        int lastTabIndex = line.LastIndexOf('\t');
                        if (lastTabIndex > 0)
                        {
                            string filename = line.Substring(lastTabIndex + 1).Trim();

                            // Extract year from filename pattern: orgId_YYYY-MM-DD.zip
                            var match = Regex.Match(filename, normalizedOrgId + @"_(\d{4})-\d{2}-\d{2}\.zip");
                            if (match.Success && match.Groups.Count > 1)
                            {
                                if (int.TryParse(match.Groups[1].Value, out int year))
                                {
                                    availableYears.Add(year);
                                }
                            }
                        }
                    }
                }
            }

            if (availableYears.Count == 0)
            {
                return $"No annual reports found for organization {organizationId}";
            }

            return $"Organization {organizationId} has annual reports available for the following years:\n" +
                   string.Join(", ", availableYears);
        }
        catch (Exception ex)
        {
            return $"Error reading lookup table: {ex.Message}";
        }
    }

    [McpServerTool, Description("Get the annual report for a specific company and year")]
    public static string GetAnnualReport(
        [Description("Organization number (e.g. 559190-0047)")] string organizationId,
        [Description("Year for the annual report (e.g. 2019)")] int year)
    {
        // Since we don't have access to real data, return the example XHTML content from disk
        // In a real implementation, this would fetch the actual annual report from a database or API
        var exampleFilePath = @"C:\Users\blasi\Downloads\5591900047_2019-12-31\9eb71ed6-f93b-4813-bf51-04b7a444ec62.xhtml";

        var res = SearchWithContext(organizationId, year);

        if (res != null && res.Count > 0)
        {
            var s = res.FirstOrDefault().Split(" ");
            var outputPath = System.IO.Path.Combine(Directory.CreateTempSubdirectory("bolagsverket").FullName, s.ElementAt(1));
            if (ExtractFileFromS3ArchiveSync("https://vardefulla-datamangder.bolagsverket.se/arsredovisningar-bulkfiler/" + s.ElementAt(0), s.ElementAt(1), outputPath))
            {
                return XhtmlZipExtractor.GetAllXhtmlAsString(outputPath);
            }

            return string.Join("\n", res.ToArray());
        }

        return $"No result found for search term {organizationId} for year {year}";
    }

    /// <summary>
    /// Searches for lines containing the search string and returns them with context from preceding "Processing:" lines
    /// </summary>
    /// <param name="searchString">The string to search for</param>
    /// <param name="filePath">Path to the file to search</param>
    /// <returns>List of formatted results with processing context</returns>
    /// <exception cref="ArgumentException">Thrown when parameters are null or empty</exception>
    /// <exception cref="FileNotFoundException">Thrown when the file doesn't exist</exception>
    public static List<string> SearchWithContext(string searchString, int year)
    {
        string filePath = Settings.LookupTableFilePath; 
        // Validate parameters
        if (string.IsNullOrEmpty(searchString))
            throw new ArgumentException("Search string cannot be null or empty", nameof(searchString));

        if (string.IsNullOrEmpty(filePath))
            throw new ArgumentException("File path cannot be null or empty", nameof(filePath));

        if (!File.Exists(filePath))
            throw new FileNotFoundException($"File '{filePath}' not found");

        var results = new List<string>();
        string currentProcessing = null;
        bool foundProcessing = false;

        searchString = searchString.Replace("-", "") + "_" + year.ToString();

        try
        {
            using (var reader = new StreamReader(filePath))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    // Check if line starts with "Processing: "
                    if (line.StartsWith("Processing: "))
                    {
                        // Extract everything after "Processing: " (12 characters)
                        currentProcessing = line.Substring(12);
                        foundProcessing = true;
                    }
                    // Check if line contains search string and we have a processing context
                    else if (line.Contains(searchString) && foundProcessing && currentProcessing != null)
                    {
                        // Extract substring starting from position 60 (if line is long enough)
                        string lineSubstring = line.Length > 59 ? line.Substring(59) : line;
                        results.Add($"{currentProcessing} {lineSubstring}");

                        // Reset the processing flag after finding a match
                        foundProcessing = false;
                    }
                }
            }
        }
        catch (Exception ex) when (!(ex is ArgumentException || ex is FileNotFoundException))
        {
            throw new IOException($"Error reading file '{filePath}': {ex.Message}", ex);
        }

        return results;
    }

    /// <summary>
    /// Synchronous version of the file extraction method
    /// </summary>
    public static bool ExtractFileFromS3ArchiveSync(string s3BucketPath, string filePathInArchive, string outputFilePath)
    {
        try
        {
            var processInfo = new ProcessStartInfo
            {
                FileName = Settings.CloudZipFilePath,
                //WorkingDirectory = "C:\\projects\\bolagsverket",
                Arguments = $"cat {s3BucketPath} {filePathInArchive}",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using var process = Process.Start(processInfo);
            if (process == null)
            {
                Console.WriteLine("Failed to start the cz process");
                return false;
            }

            // Read the output and write it to the specified file
            using var outputStream = File.Create(outputFilePath);
            process.StandardOutput.BaseStream.CopyTo(outputStream);

            // Wait for the process to complete
            process.WaitForExit();

            // Check if there were any errors
            if (process.ExitCode != 0)
            {
                string errorOutput = process.StandardError.ReadToEnd();
                Console.WriteLine($"Error executing cz command: {errorOutput}");
                return false;
            }

            Console.WriteLine($"Successfully extracted {filePathInArchive} to {outputFilePath}");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception occurred: {ex.Message}");
            return false;
        }
    }
}