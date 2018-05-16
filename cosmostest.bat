dotnet build EFCore.Cosmos.sln
dotnet test .\test\EFCore.Cosmos.Sql.FunctionalTests\ --no-build
dotnet test .\test\EFCore.Cosmos.Sql.Tests\ --no-build