using Microsoft.Data.Sqlite;
using System.Text;

if (args.Length == 0)
{
    Console.WriteLine("Usage: BolagsverketBulkToSqlite <input-file.txt> [output-database.db]");
    Console.WriteLine("Example: BolagsverketBulkToSqlite data/bolagsverket_bulkfil.txt bolagsverket.db");
    return 1;
}

string inputFile = args[0];
string outputDb = args.Length > 1 ? args[1] : "bolagsverket.db";

if (!File.Exists(inputFile))
{
    Console.Error.WriteLine($"Error: Input file '{inputFile}' not found.");
    return 1;
}

Console.WriteLine($"Converting {inputFile} to SQLite database {outputDb}...");

// Delete existing database
if (File.Exists(outputDb))
{
    File.Delete(outputDb);
    Console.WriteLine($"Deleted existing database: {outputDb}");
}

try
{
    using var connection = new SqliteConnection($"Data Source={outputDb}");
    connection.Open();

    // Create table
    using (var cmd = connection.CreateCommand())
    {
        cmd.CommandText = @"
            CREATE TABLE Organisationer (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Organisationsidentitet TEXT NOT NULL,
                Namnskyddslopnummer TEXT,
                Registreringsland TEXT,
                Organisationsnamn TEXT,
                Organisationsform TEXT,
                Avregistreringsdatum TEXT,
                Avregistreringsorsak TEXT,
                PagandeAvvecklingsEllerOmstruktureringsforfarande TEXT,
                Registreringsdatum TEXT,
                Verksamhetsbeskrivning TEXT,
                Postadress TEXT
            );

            CREATE INDEX idx_organisationsidentitet ON Organisationer(Organisationsidentitet);
            CREATE INDEX idx_registreringsdatum ON Organisationer(Registreringsdatum);
            CREATE INDEX idx_organisationsnamn ON Organisationer(Organisationsnamn);
        ";
        cmd.ExecuteNonQuery();
    }

    Console.WriteLine("Created database schema with indexes");

    // Read and parse file
    using var reader = new StreamReader(inputFile, Encoding.UTF8);

    // Skip header line
    string? headerLine = reader.ReadLine();
    if (headerLine == null)
    {
        Console.Error.WriteLine("Error: File is empty");
        return 1;
    }

    Console.WriteLine("Processing records...");

    using var transaction = connection.BeginTransaction();
    using var insertCmd = connection.CreateCommand();
    insertCmd.CommandText = @"
        INSERT INTO Organisationer (
            Organisationsidentitet, Namnskyddslopnummer, Registreringsland,
            Organisationsnamn, Organisationsform, Avregistreringsdatum,
            Avregistreringsorsak, PagandeAvvecklingsEllerOmstruktureringsforfarande,
            Registreringsdatum, Verksamhetsbeskrivning, Postadress
        ) VALUES (
            $p1, $p2, $p3, $p4, $p5, $p6, $p7, $p8, $p9, $p10, $p11
        )
    ";

    for (int i = 1; i <= 11; i++)
    {
        insertCmd.Parameters.Add($"$p{i}", SqliteType.Text);
    }

    long recordCount = 0;
    long errorCount = 0;
    string? line;

    while ((line = reader.ReadLine()) != null)
    {
        try
        {
            var fields = ParseSemicolonDelimitedLine(line);

            if (fields.Length != 11)
            {
                Console.WriteLine($"Warning: Line {recordCount + 2} has {fields.Length} fields, expected 11. Skipping.");
                errorCount++;
                continue;
            }

            for (int i = 0; i < 11; i++)
            {
                insertCmd.Parameters[i].Value = fields[i];
            }

            insertCmd.ExecuteNonQuery();
            recordCount++;

            if (recordCount % 10000 == 0)
            {
                Console.WriteLine($"Processed {recordCount:N0} records...");
            }
        }
        catch (Exception ex)
        {
            errorCount++;
            Console.WriteLine($"Error processing line {recordCount + 2}: {ex.Message}");
        }
    }

    transaction.Commit();

    Console.WriteLine($"\nConversion complete!");
    Console.WriteLine($"  Total records inserted: {recordCount:N0}");
    Console.WriteLine($"  Errors: {errorCount:N0}");
    Console.WriteLine($"  Database: {outputDb}");

    // Show file size
    var fileInfo = new FileInfo(outputDb);
    Console.WriteLine($"  Database size: {fileInfo.Length / (1024.0 * 1024.0):F2} MB");

    return 0;
}
catch (Exception ex)
{
    Console.Error.WriteLine($"Fatal error: {ex.Message}");
    Console.Error.WriteLine(ex.StackTrace);
    return 1;
}

static string[] ParseSemicolonDelimitedLine(string line)
{
    var fields = new List<string>();
    var currentField = new StringBuilder();
    bool inQuotes = false;

    for (int i = 0; i < line.Length; i++)
    {
        char c = line[i];

        if (c == '"')
        {
            inQuotes = !inQuotes;
        }
        else if (c == ';' && !inQuotes)
        {
            fields.Add(currentField.ToString());
            currentField.Clear();
        }
        else
        {
            currentField.Append(c);
        }
    }

    // Add last field
    fields.Add(currentField.ToString());

    return fields.ToArray();
}