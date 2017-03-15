@echo off
echo Shrinking the model database several times
sqlcmd -S "(localdb)\mssqllocaldb" -Q "DBCC SHRINKDATABASE (model) WITH NO_INFOMSGS"
sqlcmd -S "(localdb)\mssqllocaldb" -Q "DBCC SHRINKDATABASE (model) WITH NO_INFOMSGS"
sqlcmd -S "(localdb)\mssqllocaldb" -Q "DBCC SHRINKDATABASE (model) WITH NO_INFOMSGS"
sqlcmd -S "(localdb)\mssqllocaldb" -Q "DBCC SHRINKDATABASE (model) WITH NO_INFOMSGS"
sqlcmd -S "(localdb)\mssqllocaldb" -Q "DBCC SHRINKDATABASE (model) WITH NO_INFOMSGS"
sqlcmd -S "(localdb)\mssqllocaldb" -Q "DBCC SHRINKDATABASE (model) WITH NO_INFOMSGS"
sqlcmd -S "(localdb)\mssqllocaldb" -Q "DBCC SHRINKDATABASE (model) WITH NO_INFOMSGS"
sqlcmd -S "(localdb)\mssqllocaldb" -Q "DBCC SHRINKDATABASE (model) WITH NO_INFOMSGS"
sqlcmd -S "(localdb)\mssqllocaldb" -Q "DBCC SHRINKDATABASE (model) WITH NO_INFOMSGS"
echo Configuring model database to grow with default settings
sqlcmd -S "(localdb)\mssqllocaldb" -Q "ALTER DATABASE model MODIFY FILE (NAME=modeldev,FILEGROWTH=10%%);" 
sqlcmd -S "(localdb)\mssqllocaldb" -Q "ALTER DATABASE model MODIFY FILE (NAME=modellog,FILEGROWTH=10%%);"
