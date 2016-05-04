@ECHO OFF
:again
if not "%1" == "" (
    if not exist %1\TestProjects goto artifactsDir
    echo "Deleting %1\TestProjects"
    rmdir /s /q %1\TestProjects

    :artifactsDir
    if not exist %1\artifacts goto iterate
    echo "Deleting %1\artifacts"
    rmdir /s /q %1\artifacts

    :iterate
    shift
    goto again
)
