@ECHO OFF
:again
if not "%1" == "" (
    echo "Deleting %1\TestAssets"
    rmdir /s /q %1\TestAssets
    echo "Deleting %1\artifacts"
    rmdir /s /q %1\artifacts
    shift
    goto again
)
