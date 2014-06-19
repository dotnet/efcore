AzureTableStorage.FunctionalTests
=====
These tests require a Azure Storage account. They can also run on the [Azure Storage Emulator](http://www.microsoft.com/en-us/download/details.aspx?id=42317), but the results and performance may vary.

## Configuration
To configure these tests, provide a connection string as an environment variable or in a config file. If there is no connection string, **all tests will be skipped** rather than failing.

#### Environment Variable
Set `CUSTOMCONNSTR_TestAccount` to a valid Azure Storage connection string.

#### Config File
Create a file called `app.config` in the source root for this project. Add a valid connection string for an Azure Storage account. See app.config.example. 

Example:
```xml
<config>
	<TestAccount>
		<ConnectionString>DefaultEndpointsProtocol=https;AccountName=name;AccountKey=key;</ConnectionString>
	</TestAccount>
</config>
```