Microsoft.Data.Sqlite
=====================

AppVeyor: [![AppVeyor build status](https://ci.appveyor.com/api/projects/status/p48patmrpydigrj0?svg=true)](https://ci.appveyor.com/project/aspnetci/microsoft-data-sqlite)

Travis: [![Travis build Status](https://travis-ci.org/aspnet/Microsoft.Data.Sqlite.svg?branch=dev)](https://travis-ci.org/aspnet/Microsoft.Data.Sqlite)

Contains SQLite implementations of the System.Data.Common interfaces.

This project is part of ASP.NET 5. You can find samples, documentation and getting started instructions for ASP.NET 5 at the [Home](https://github.com/aspnet/home) repo.

## Requirements
Requires SQLite >= 3.7.9

This library binds to the native SQLite library. On some systems, you must also install separately the SQLite library.

### Ubuntu
Requires "libsqlite3-dev", which is not installed by default.
```
sudo apt-get install libsqlite3-dev
```
