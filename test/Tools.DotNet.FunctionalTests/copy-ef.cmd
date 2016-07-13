@ECHO OFF
mkdir %2\tools
mkdir %2\tools\net451
mkdir %2\tools\netcoreapp1.0
dotnet build ..\..\src\Tools.Console -f net451 -c %1
dotnet build ..\..\src\Tools.Console -f net451 -c %1_x86
copy ..\..\src\Tools.Console\bin\%1\net451\*.exe %2\tools\net451\
copy ..\..\src\Tools.Console\bin\%1_x86\net451\*.exe %2\tools\net451\
copy ..\..\src\Tools.Console\bin\%1\netcoreapp1.0\ef.dll %2\tools\netcoreapp1.0\
copy ..\..\src\Tools.Console\bin\%1\netcoreapp1.0\ef.runtimeconfig.json %2\tools\netcoreapp1.0\
