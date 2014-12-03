-- Generates a SQL script that will drop all databases (except system and sample ones) on the current server.

DECLARE @name nvarchar(255)

DECLARE db CURSOR FOR 
SELECT Name FROM sysdatabases
WHERE Name NOT IN ('master', 'tempdb', 'model', 'msdb', 'AdventureWorks2012', 'Chinook', 'Northwind', 'pubs')

OPEN db;

FETCH NEXT FROM db 
INTO @name;

WHILE @@FETCH_STATUS = 0
BEGIN
    PRINT 'DROP DATABASE [' + REPLACE( @name, ']', ']]' ) + ']'
    PRINT 'GO'
    
    FETCH NEXT FROM db 
    INTO @name

END
CLOSE db;
DEALLOCATE db;