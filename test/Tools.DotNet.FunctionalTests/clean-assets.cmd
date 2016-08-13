@ECHO OFF
:again
if not "%1" == "" (
    if not exist %1\TestProjects goto toolsDir
    echo "Deleting %1\TestProjects"
    rmdir /s /q %1\TestProjects

    :toolsDir
    if not exist %1\tools goto iterate
    echo "Deleting %1\tools"
    rmdir /s /q %1\tools

    :iterate
    shift
    goto again
)
