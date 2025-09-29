using System.IO.Compression;
using System.Text;

namespace AspNetCoreMcpPerSessionTools
{
    public class XhtmlZipExtractor
    {
        /// <summary>
        /// Extracts all XHTML files from a zip archive and returns them as a dictionary
        /// where the key is the file path and the value is the file content as string
        /// </summary>
        /// <param name="zipFilePath">Path to the zip file</param>
        /// <param name="encoding">Text encoding to use (defaults to UTF-8)</param>
        /// <returns>Dictionary containing file paths and their content as strings</returns>
        public static Dictionary<string, string> GetXhtmlFilesFromZip(string zipFilePath, Encoding encoding = null)
        {
            encoding ??= Encoding.UTF8;
            var xhtmlFiles = new Dictionary<string, string>();

            try
            {
                using var archive = ZipFile.OpenRead(zipFilePath);

                var xhtmlEntries = archive.Entries.Where(entry =>
                    IsXhtmlFile(entry.FullName) && !string.IsNullOrEmpty(entry.Name));

                foreach (var entry in xhtmlEntries)
                {
                    using var stream = entry.Open();
                    using var reader = new StreamReader(stream, encoding);
                    string content = reader.ReadToEnd();
                    xhtmlFiles[entry.FullName] = content;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading zip file: {ex.Message}");
                throw;
            }

            return xhtmlFiles;
        }

        /// <summary>
        /// Async version of GetXhtmlFilesFromZip
        /// </summary>
        public static async Task<Dictionary<string, string>> GetXhtmlFilesFromZipAsync(string zipFilePath, Encoding encoding = null)
        {
            encoding ??= Encoding.UTF8;
            var xhtmlFiles = new Dictionary<string, string>();

            try
            {
                using var archive = ZipFile.OpenRead(zipFilePath);

                var xhtmlEntries = archive.Entries.Where(entry =>
                    IsXhtmlFile(entry.FullName) && !string.IsNullOrEmpty(entry.Name));

                foreach (var entry in xhtmlEntries)
                {
                    using var stream = entry.Open();
                    using var reader = new StreamReader(stream, encoding);
                    string content = await reader.ReadToEndAsync();
                    xhtmlFiles[entry.FullName] = content;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading zip file: {ex.Message}");
                throw;
            }

            return xhtmlFiles;
        }

        /// <summary>
        /// Extracts XHTML files from a zip archive in memory (from byte array)
        /// </summary>
        /// <param name="zipBytes">Zip file as byte array</param>
        /// <param name="encoding">Text encoding to use (defaults to UTF-8)</param>
        /// <returns>Dictionary containing file paths and their content as strings</returns>
        public static Dictionary<string, string> GetXhtmlFilesFromZipBytes(byte[] zipBytes, Encoding encoding = null)
        {
            encoding ??= Encoding.UTF8;
            var xhtmlFiles = new Dictionary<string, string>();

            try
            {
                using var memoryStream = new MemoryStream(zipBytes);
                using var archive = new ZipArchive(memoryStream, ZipArchiveMode.Read);

                var xhtmlEntries = archive.Entries.Where(entry =>
                    IsXhtmlFile(entry.FullName) && !string.IsNullOrEmpty(entry.Name));

                foreach (var entry in xhtmlEntries)
                {
                    using var stream = entry.Open();
                    using var reader = new StreamReader(stream, encoding);
                    string content = reader.ReadToEnd();
                    xhtmlFiles[entry.FullName] = content;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading zip bytes: {ex.Message}");
                throw;
            }

            return xhtmlFiles;
        }

        /// <summary>
        /// Gets a specific XHTML file from a zip archive
        /// </summary>
        /// <param name="zipFilePath">Path to the zip file</param>
        /// <param name="xhtmlFileName">Name or path of the XHTML file to extract</param>
        /// <param name="encoding">Text encoding to use (defaults to UTF-8)</param>
        /// <returns>Content of the XHTML file as string, or null if not found</returns>
        public static string GetSpecificXhtmlFile(string zipFilePath, string xhtmlFileName, Encoding encoding = null)
        {
            encoding ??= Encoding.UTF8;

            try
            {
                using var archive = ZipFile.OpenRead(zipFilePath);

                var entry = archive.Entries.FirstOrDefault(e =>
                    e.FullName.Equals(xhtmlFileName, StringComparison.OrdinalIgnoreCase) ||
                    e.Name.Equals(xhtmlFileName, StringComparison.OrdinalIgnoreCase));

                if (entry != null && IsXhtmlFile(entry.FullName))
                {
                    using var stream = entry.Open();
                    using var reader = new StreamReader(stream, encoding);
                    return reader.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading specific XHTML file: {ex.Message}");
                throw;
            }

            return null;
        }

        /// <summary>
        /// Determines if a file is an XHTML file based on its extension
        /// </summary>
        /// <param name="fileName">The file name or path</param>
        /// <returns>True if the file is an XHTML file</returns>
        private static bool IsXhtmlFile(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return false;

            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            return extension == ".xhtml" || extension == ".xht";
        }

        /// <summary>
        /// Gets all XHTML files and returns them as a single concatenated string
        /// with file separators
        /// </summary>
        /// <param name="zipFilePath">Path to the zip file</param>
        /// <param name="separator">Separator between files (defaults to newlines with filename)</param>
        /// <param name="encoding">Text encoding to use (defaults to UTF-8)</param>
        /// <returns>All XHTML content as a single string</returns>
        public static string GetAllXhtmlAsString(string zipFilePath, string separator = null, Encoding encoding = null)
        {
            separator ??= Environment.NewLine + "<!-- ==================== {0} ==================== -->" + Environment.NewLine;

            var xhtmlFiles = GetXhtmlFilesFromZip(zipFilePath, encoding);
            var result = new StringBuilder();

            foreach (var kvp in xhtmlFiles.OrderBy(x => x.Key))
            {
                if (result.Length > 0)
                {
                    result.AppendLine(string.Format(separator, kvp.Key));
                }
                result.Append(kvp.Value);
            }

            return result.ToString();
        }
    }
}
