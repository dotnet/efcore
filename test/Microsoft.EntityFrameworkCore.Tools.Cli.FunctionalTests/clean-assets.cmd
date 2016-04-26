@ECHO OFF
:again
if not "%1" == "" (
    echo "Deleting %1\TestProjects"
    rmdir /s /q %1\TestProjects
    echo "Deleting %1\artifacts"
    rmdir /s /q %1\artifacts
    shift
    goto again
)
