@echo off
echo Shrinking the model database several times
sqlcmd -S "(local)\SQL2016" -Q "DBCC SHRINKDATABASE (model) WITH NO_INFOMSGS"
sqlcmd -S "(local)\SQL2016" -Q "DBCC SHRINKDATABASE (model) WITH NO_INFOMSGS"
sqlcmd -S "(local)\SQL2016" -Q "DBCC SHRINKDATABASE (model) WITH NO_INFOMSGS"
sqlcmd -S "(local)\SQL2016" -Q "DBCC SHRINKDATABASE (model) WITH NO_INFOMSGS"
sqlcmd -S "(local)\SQL2016" -Q "DBCC SHRINKDATABASE (model) WITH NO_INFOMSGS"
sqlcmd -S "(local)\SQL2016" -Q "DBCC SHRINKDATABASE (model) WITH NO_INFOMSGS"
sqlcmd -S "(local)\SQL2016" -Q "DBCC SHRINKDATABASE (model) WITH NO_INFOMSGS"
sqlcmd -S "(local)\SQL2016" -Q "DBCC SHRINKDATABASE (model) WITH NO_INFOMSGS"
sqlcmd -S "(local)\SQL2016" -Q "DBCC SHRINKDATABASE (model) WITH NO_INFOMSGS"
echo Configuring model to grow with default settings
sqlcmd -S (local)\SQL2016 -Q "ALTER DATABASE model MODIFY FILE (NAME=modeldev,FILEGROWTH=10%%);"
sqlcmd -S (local)\SQL2016 -Q "ALTER DATABASE model MODIFY FILE (NAME=modellog,FILEGROWTH=10%%);"
