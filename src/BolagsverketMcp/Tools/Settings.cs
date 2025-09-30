namespace AspNetCoreMcpPerSessionTools.Tools
{
    public class Settings
    {
        public static string BulkFilePath => System.Environment.GetEnvironmentVariable("BulkFilePath") ?? @"C:\Users\blasi\Downloads\bolagsverket_bulkfil\bolagsverket_bulkfil.txt";

        public static string LookupTableFilePath => System.Environment.GetEnvironmentVariable("LookupTableFilePath") ?? @"C:\projects\bolagsverket\data\database.txt";

        public static string CloudZipFilePath => System.Environment.GetEnvironmentVariable("CloudZipFilePath") ?? "C:\\projects\\bolagsverket\\cz.exe";

        public static string SqliteDatabasePath => System.Environment.GetEnvironmentVariable("SqliteDatabasePath") ?? @"C:\projects\bolagsverket\data\bolagsverket.db";
    }
}
