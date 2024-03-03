# Get all .csproj files in the current directory
$csprojFiles = Get-ChildItem -Path . -Filter *.csproj

# Iterate over each .csproj file
foreach ($file in $csprojFiles) {
    # Extract the project name without extension
    $projectName = $file.BaseName
    # Run dotnet run for each project and redirect output to a text file named after the project
    dotnet run --project $file.FullName > "${projectName}.output.txt"
}
