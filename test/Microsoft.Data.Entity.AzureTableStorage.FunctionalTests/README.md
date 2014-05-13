AzureTableStorage.FunctionalTests
=====
These tests require a Azure Storage account (for now). You can run them using [Azure Storage Emulator](http://www.microsoft.com/en-us/download/details.aspx?id=42317), but the results may not be the same as testing against a real account.

## Configuration
To run the tests, you must create a file called app.config and add a valid connection string for an Azure Storage account. ConfigurationManager may require the file be included in CSPROJ.

Example:
```xml
<?xml version="1.0" encoding="utf-8" ?>
<configuration>
  <appSettings>
    <add key="TestConnectionString" value="DefaultEndpointsProtocol=https;AccountName=testaccount1;AccountKey=(key);"/>
  </appSettings>
</configuration>
```