$recipeDirs = Get-ChildItem -Path $PSScriptRoot -Directory | Where-Object { Test-Path (Join-Path $_.FullName "$($_.Name).csproj") }

foreach ($dir in $recipeDirs) {
    $projectName = $dir.Name
    $projectFile = Join-Path $dir.FullName "$projectName.csproj"

    # Run each recipe and capture its output next to the source file
    dotnet run --project $projectFile | Out-File (Join-Path $dir.FullName "$projectName.output.txt") -Encoding UTF8
}
