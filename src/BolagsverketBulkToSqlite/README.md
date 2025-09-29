# Bolagsverket Bulk to SQLite Converter

Converts Bolagsverket's bulk text file format to a queryable SQLite database.

## Usage

```bash
dotnet run <input-file.txt> [output-database.db]
```

### Example

```bash
# From the project directory
dotnet run ../../data/bolagsverket_bulkfil.txt bolagsverket.db

# Or build and use the executable
dotnet build -c Release
./bin/Release/net9.0/BolagsverketBulkToSqlite ../../data/bolagsverket_bulkfil.txt bolagsverket.db
```

## Database Schema

The tool creates a table `Organisationer` with the following columns:

- `Id` - Auto-incrementing primary key
- `Organisationsidentitet` - Organization ID (indexed)
- `Namnskyddslopnummer` - Name protection number
- `Registreringsland` - Registration country
- `Organisationsnamn` - Organization name (indexed)
- `Organisationsform` - Organization form
- `Avregistreringsdatum` - Deregistration date
- `Avregistreringsorsak` - Deregistration reason
- `PagandeAvvecklingsEllerOmstruktureringsforfarande` - Ongoing liquidation/restructuring
- `Registreringsdatum` - Registration date (indexed)
- `Verksamhetsbeskrivning` - Business description
- `Postadress` - Postal address

## Features

- Handles quoted fields with semicolons
- Progress reporting every 10,000 records
- Transaction-based bulk insert for performance
- Automatic index creation on key fields
- UTF-8 encoding support for Swedish characters
- Error handling with detailed reporting

## Query Examples

```sql
-- Find companies by name
SELECT * FROM Organisationer
WHERE Organisationsnamn LIKE '%Nordea%';

-- Count by organization form
SELECT Organisationsform, COUNT(*) as Count
FROM Organisationer
GROUP BY Organisationsform
ORDER BY Count DESC;

-- Recently registered companies
SELECT Organisationsnamn, Registreringsdatum
FROM Organisationer
ORDER BY Registreringsdatum DESC
LIMIT 100;
```