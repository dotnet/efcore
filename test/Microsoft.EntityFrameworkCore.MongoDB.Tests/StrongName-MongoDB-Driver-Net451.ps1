<#
 The MongoDB team has stopped strong-naming their assemblies:
    http://mongodb.github.io/mongo-csharp-driver/2.0/upgrading/#packaging

 We can raise the issue with their team later. In the meantime, this script serves
 the sole purpose of strong-naming their assemblies so that certain unit tests can
 pass  without typeload errors. Consumers of the MongoDB EntityFramework package
 should ensure that the MongoDB driver they're using is strong-named according
 to their own needs.
#>

Param(
    [Parameter(Mandatory=$true, Position=1)][string]$Platform
)

if ($Platform -ne "net451") {
    exit
}

#TODO: verify that these exist, which is probably moot given that this is a build-chain script
$ildasm = "${env:ProgramFiles(x86)}\Microsoft SDKs\Windows\v10.0A\bin\NETFX 4.6.1 Tools\ildasm.exe"
$ilasm = "${env:windir}\Microsoft.NET\Framework\v4.0.30319\ilasm.exe"
$sn = "${env:ProgramFiles(x86)}\Microsoft SDKs\Windows\v10.0A\bin\NETFX 4.6.1 Tools\sn.exe"

#formats pulled from random disassembled dlls
$refPattern = "(\.assembly extern /\*\d{8}\*/ MongoDB\.[\w\.]+\r?\n?)\{([^\}]*)\}"
$refReplace = "`$1{`r`n  .publickeytoken = (AD B9 79 38 29 DD AE 60 )`r`n`$2}"
#TODO: validate/extract this public key token from our SNK in case it gets replaced

#if the DLL references other MongoDB driver dlls, rewrite the references to include the public key token we're about to add
Function Rewrite-References([string]$ilContent) {
    $ilContent = $ilContent -replace $refPattern, $refReplace

    #remove all InternalsVisibleTo attributes (currently only used for test assemblies)

    $lineList = New-Object System.Collections.ArrayList
    $lineList.AddRange($ilContent -split "\r\n")

    for ($i = 0; $i -le $lineList.Count; $i++) {
        if ($lineList[$i] -match "System\.Runtime\.CompilerServices\.InternalsVisibleToAttribute") {
            while ($lineList[$i] -notmatch "\)\s+// .*$") {
                $lineList.RemoveAt($i)
            }
            $lineList.RemoveAt($i--)
        }
    }

    $lineList -join "`r`n"
}

Function Rewrite-Assembly([string]$assembly) {
    Write-Host -NoNewline "Strong-naming ${assembly}..."

    #disassemble the dlls to IL
    &"$ildasm" /nobar /all /out="${assembly}.il" "$assembly"

    Set-Content "${assembly}.il" (Rewrite-References(Get-Content "${assembly}.il" -Raw))

    #re-assemble the modified IL, overwriting the original dll and signing it using our SNK
    &"$ilasm" /dll /debug /quiet /key="..\..\tools\Key.snk" /out="${assembly}.strongname" "${assembly}.il"
    Copy-Item -Force $assembly "$assembly.original"

    #good citizenship: cleanup
    del "$assembly.il"
    Write-Host "done!"
}

Function Check-Dll([string]$package, [string]$assembly) {
    Write-Host "Checking library ${package}..."

    if (Test-Path("$assembly.strongname")) {
        Write-Host "Skipping ${package}: already strong-named..."
    } else {
        Write-Host "Library ${package} not strong-named..."
        Rewrite-Assembly $assembly
    }

    Write-Host "Replacing ${assembly} with strong-named version..."
    Copy-Item -Force "$assembly.strongname" $assembly
}

#check all MongoDB driver dlls
"MongoDB.Bson", "MongoDB.Driver", "MongoDB.Driver.Core" | % { Check-Dll $_ "${env:UserProfile}\.nuget\packages\$_\2.4.0\lib\net45\$_.dll"}

#rewrite MongoDB EF provider library so that it references the newly strong-named driver dlls
Check-Dll "Microsoft.EntityFrameworkCore.MongoDB" "..\..\src\Microsoft.EntityFrameworkCore.MongoDB\bin\Debug\net451\Microsoft.EntityFrameworkCore.MongoDB.dll"