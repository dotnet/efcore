---
name: Ask a question
about: Ask a question about Entity Framework Core or Microsoft.Data.Sqlite
labels: customer-reported
---

## Ask a question

Remember:

* Please make your question as clear and specific as possible.
* Please check that the [documentation](https://docs.microsoft.com/ef/) does not answer your question.
* Please search in both [open](https://github.com/dotnet/efcore/issues) and [closed](https://github.com/dotnet/efcore/issues?q=is%3Aissue+is%3Aclosed) issues to check that your question has not already been answered.

### Include your code

Usually the best way to ask a clear question and get a quick response is to show your code. Preferably, attach a small, runnable project or post a small, runnable code listing that reproduces what you are seeing.

Use triple-tick code fences for any posted code. For example:

```C#
Console.WriteLine("Hello, World!");
```

### Include stack traces

Include the full exception message and stack trace for any exception you encounter.

Use triple-tick fences for stack traces. For example:

```
Unhandled exception. System.NullReferenceException: Object reference not set to an instance of an object.
   at SixFour.Sub() in C:\Stuff\AllTogetherNow\SixFour\SixFour.cs:line 49
   at SixFour.Main() in C:\Stuff\AllTogetherNow\SixFour\SixFour.cs:line 54
```

### Include verbose output

Please include verbose output when asking questions about the `dotnet ef` or Package Manager Console tools.

Use triple-tick fences for tool output. For example:

```
C:\Stuff\AllTogetherNow\FiveOh>dotnet ef dbcontext list --verbose
Using project 'C:\Stuff\AllTogetherNow\FiveOh\FiveOh.csproj'.
...
Finding DbContext classes in the project...
Found DbContext 'BlogContext'.
BlogContext
```

### Include provider and version information

EF Core version:
Database provider: (e.g. Microsoft.EntityFrameworkCore.SqlServer)
Target framework: (e.g. .NET 6.0)
Operating system:
IDE: (e.g. Visual Studio 2022 17.4)
