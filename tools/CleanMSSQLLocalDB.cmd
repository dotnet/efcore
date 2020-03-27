@echo off
sqlcmd -S "(localdb)\mssqllocaldb" -i "%~dp0DropAllDatabases.sql" -o "DropAll.sql"
sqlcmd -S "(localdb)\mssqllocaldb" -i "DropAll.sql"
del "DropAll.sql"

%~dp0ShrinkLocalDBModel.cmd