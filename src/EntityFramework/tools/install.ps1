param ($installPath, $toolsPath, $package, $project)

$project.ProjectItems.Item('deleteme').Delete()
$DTE.ItemOperations.Navigate((Join-Path $installPath readme.html)) | Out-Null
