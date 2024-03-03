$csprojFiles = Get-ChildItem -Path . -Filter *.csproj

foreach ($file in $csprojFiles) {
    $projectName = $file.BaseName
    
    # Run dotnet run for each project and redirect output to a text file named after the project
    dotnet run --project $file.FullName | Out-File "${projectName}.output.txt"
            
    
    # Pad the file with newlines to match the source length    
    # $sourceFilePath = "${projectName}.cs"
    # if (Test-Path $sourceFilePath) {        
    #     $sourceFileLineCount = (Get-Content $sourceFilePath).Count
    #     $outputFileLineCount = (Get-Content "${projectName}.output.txt").Count
    #     
    #     $linesToAdd = $sourceFileLineCount - $outputFileLineCount - 1
    #     
    #     for ($i = 0; $i -lt $linesToAdd; $i++) {
    #         Add-Content -Path "${projectName}.output.txt" -Value ""
    #     }
    #     Add-Content -Path "${projectName}.output.txt" -Value "🦊"
    # }
    # else {
    #     Write-Warning "Source file ${sourceFilePath} not found. Skipping line padding for ${projectName}."
    # }
}
