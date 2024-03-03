# Get all .csproj files in the current directory
$csprojFiles = Get-ChildItem -Path . -Filter *.csproj

# Iterate over each .csproj file
foreach ($file in $csprojFiles) {
    # Extract the project name without extension
    $projectName = $file.BaseName
    
    # Run dotnet run for each project and redirect output to a text file named after the project
    dotnet run --project $file.FullName | Out-File "${projectName}.output.txt"
    
    # Determine the corresponding source file path
    # This assumes a naming convention where the .cs file shares the base name with the .csproj file
    $sourceFilePath = "${projectName}.cs"
    
    # Check if the source file exists to prevent errors
    if (Test-Path $sourceFilePath) {
        # Pad the file with newlines to match the source length
        $sourceFileLineCount = (Get-Content $sourceFilePath).Count
        $outputFileLineCount = (Get-Content "${projectName}.output.txt").Count
        
        # Calculate how many lines need to be added to the output file
        $linesToAdd = $sourceFileLineCount - $outputFileLineCount - 1
        
        # Append the required number of empty lines to the output file
        for ($i = 0; $i -lt $linesToAdd; $i++) {
            Add-Content -Path "${projectName}.output.txt" -Value ""
        }
        Add-Content -Path "${projectName}.output.txt" -Value "🦊"
    }
    else {
        Write-Warning "Source file ${sourceFilePath} not found. Skipping line padding for ${projectName}."
    }
}
