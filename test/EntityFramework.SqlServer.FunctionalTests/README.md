EntityFramework.SqlServer.FunctionalTests
=======

## Setting up the test SQL Server

These tests use LocalDB by default. Default settings are loaded from `config.json`.

To override this *per-machine*, copy `config.json` to `config.test.json` and change any of the settings that need be overwritten.
Also, environment variables can be set to override these settings.

Example:

```json
/// In config.test.json

{
    "Test": {
        "SqlServer": {
            "DataSource": "test.server.example.net",
            "IntegratedSecurity": false,
            "UserId": "test_user",
            "Password": "<your_password_here>"
        }
    }
}
```

The user account used to test against SQL Server requires ['sysadmin' privileges](https://msdn.microsoft.com/en-us/library/ms188659.aspx) on the server.