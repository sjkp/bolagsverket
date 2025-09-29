#!/bin/bash

# Usage: ./search_with_context.sh <search_string> <file_path>

if [ $# -ne 2 ]; then
    echo "Usage: $0 <search_string> <file_path>"
    exit 1
fi

search_string="$1"
file_path="$2"

if [ ! -f "$file_path" ]; then
    echo "Error: File '$file_path' not found"
    exit 1
fi

awk -v search="$search_string" '
/^Processing: / {
    # Extract everything after "Processing: " (12 chars + 1)
    processing = substr($0, 13)
    found_processing = 1
}
$0 ~ search && found_processing {    
    printf "%s %s\n", processing, substr($0, 60)
    
    found_processing = 0
}' "$file_path"