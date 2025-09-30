# PowerShell script to process top 10 lines from links.txt
# For each line, call cz ls on the URL and append results to database.txt

$linksFile = "links2.txt"
$outputFile = "database.txt"
$baseUrl = "https://vardefulla-datamangder.bolagsverket.se/arsredovisningar-bulkfiler/"

# Read the top 10 lines from links.txt
$top10Lines = Get-Content $linksFile #-TotalCount 10

# Clear or create the database.txt file
"" | Out-File -FilePath $outputFile -Encoding UTF8

foreach ($line in $top10Lines) {
    # Output the line from links.txt first
    Write-Output "Processing: $line"
    Add-Content -Path $outputFile -Value "Processing: $line" -Encoding UTF8

    # Construct the full URL
    $fullUrl = $baseUrl + $line

    # Call cz ls with the URL and capture output
    try {
        $czOutput = & ".\cz.exe" ls $fullUrl 2>&1

        # Append the output to database.txt
        Add-Content -Path $outputFile -Value $czOutput -Encoding UTF8
        Add-Content -Path $outputFile -Value "---" -Encoding UTF8

        Write-Output "Completed processing for: $line"
    }
    catch {
        $errorMsg = "Error processing $line : $_"
        Write-Output $errorMsg
        Add-Content -Path $outputFile -Value $errorMsg -Encoding UTF8
        Add-Content -Path $outputFile -Value "---" -Encoding UTF8
    }
}

Write-Output "Script completed. Results saved to $outputFile"