@echo off
sqlcmd -S "(localdb)\mssqllocaldb" -i "DropAllDatabases.sql" -o "DropAll.sql"
sqlcmd -S "(localdb)\mssqllocaldb" -i "DropAll.sql"
del "DropAll.sql"
sqllocaldb stop mssqllocaldb
sqllocaldb delete mssqllocaldb

%~dp0ShrinkLocalDBModel.cmd