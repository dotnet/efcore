@ECHO OFF
set TOOLS_BASE=%2\..\..\tools
mkdir %TOOLS_BASE%
mkdir %TOOLS_BASE%\net451
mkdir %TOOLS_BASE%\netcoreapp1.0
dotnet build ..\..\src\Tools.Console -f net451 -c %1
dotnet build ..\..\src\Tools.Console -f net451 -c %1_x86
copy ..\..\src\Tools.Console\bin\%1\net451\*.exe %TOOLS_BASE%\net451\
copy ..\..\src\Tools.Console\bin\%1_x86\net451\*.exe %TOOLS_BASE%\net451\
copy ..\..\src\Tools.Console\bin\%1\netcoreapp1.0\ef.dll %TOOLS_BASE%\netcoreapp1.0\
