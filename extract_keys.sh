#!/bin/bash

# Check if XML file is provided as argument
if [ $# -eq 0 ]; then
    echo "Usage: $0 <xml_file>"
    echo "Example: $0 bucket_list.xml"
    exit 1
fi

XML_FILE="$1"

# Check if file exists
if [ ! -f "$XML_FILE" ]; then
    echo "Error: File '$XML_FILE' not found"
    exit 1
fi

# Extract Key tag values and save to links.txt
grep -oP '<Key>\K[^<]*' "$XML_FILE" > links2.txt

echo "Key values extracted to links.txt"
echo "Total keys found: $(wc -l < links2.txt)"