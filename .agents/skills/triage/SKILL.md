---
name: triage
description: Use this skill to triage an incoming bug report on the EF repo, attempting to arrive at a minimal repro reproducing the bug, checking whether it represents a regression, finding possible duplicates, etc.
---

# EF Bug Report Triage

This skill covers triaging and reproducing incoming issues on the Entity Framework Core repository. To do so, read the issue in question (provided as input in the prompt), as well as any linked issues/code/resources, and to try to arrive at a minimal repro. User-submitted issues frequently provide only fragmentary information and code snippets, forcing you to try to fill in the missing information in the effort to create a minimal repro; valuable information is frequently provided in free-form text, which you need to integrate into the repro as code.

## Reproducing the error

The minimal repro should be created as a completely separate console program, outside of the EF repo. Use the following as your starting point:

```csharp
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

await using var context = new TestContext();
await context.Database.EnsureDeletedAsync();
await context.Database.EnsureCreatedAsync();

public class TestContext : DbContext
{
    public DbSet<Blog> Blogs { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder
            // Modify the following line to switch EF providers
            .UseSqlServer(Environment.GetEnvironmentVariable("Test__SqlServer__DefaultConnection"))
            .LogTo(Console.WriteLine, LogLevel.Information)
            .EnableSensitiveDataLogging();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
    }
}

public class Blog
{
    public int Id { get; set; }
    public string Name { get; set; }
}
```

* Try to integrate the user's code into the minimal template, incorporating any textual instructions from the issue, or clues that you can glean.
* At the end of the process, the minimal repro should compile and execute, and reproduce the user's reported error.
* The program should use code that's as close as possible to the user-reported code, including type/property naming and things that seem irrelevant.

### Database providers used in the bug report and repro

* We're ideally looking for a repro using SQL Server (or, as a fallback, using SQLite), which are the built-in EF providers; these are easiest to investigate and reproduce. So reproducing on SQL Server should be your starting point.
* However, if the bug isn't immediately reproducible on SQL Server/SQLite, it may require a different database; check the issue report for the reported database, or try to infer from the textual description what database the user is using. Then, attempt to reproduce the bug on that database. If you've managed to repro a problem on a provider other than SQL Server, please attempt to port the repro back to SQL Server, as that's the easiest built-in provider to diagnose/debug on. When doing this, you need to see the same error/exception on SQL Server as on the non-SQL Server provider, otherwise that may be showing a different issue. This would also confirm whether the bug is specific to e.g. the PostgreSQL provider, or a general EF Core bug.
* Some databases (PostgreSQL, MySQL) should already be installed on the github runner image which you're running - but you may still need to bring them up. Others may require bringing in a testcontainer to run the repro against.
* Pay attention to the EF provider version being used, as the bug may be specific to the version reported by the user. Once you have a working repro, try other, newer versions to confirm where the bug still occurs, and whether it has already been fixed.
* Once you've pinned down a provider to repro on (ideally SQL Server), do not keep code for multiple providers - the repro should only have code to repro on a single provider.

## Make the repro as minimal as possible

Once you've managed to reproduce the bug, work to make the repro as minimal as possible, removing any code that isn't absolutely necessary to triggering the bug:

* If the repro includes a LINQ query, try to remove any irrelevant LINQ operators from that query, as long as the error continues to reproduce.
* If the repro makes use of AutoMapper, attempt to remove it, reproducing the raw LINQ query which Automapper produces.
* If the repro is a query translation issue and does not actually require seed data to reproduce, remove any seeding as well, keeping only the query.
* Do not include any non-necessary Console.WriteLine, banners, comments, summaries or other long-form text inside the code to explain what's going on. Add minimal one-line comments at most, and only where they're really necessary to follow a complicated flow or document results of calls; otherwise no comments are necessary.
* Do not encapsulate code in functions unless really necessary - prefer a simple, minimal function-less program with only top-level statements.
* Do not catch exceptions in order to convert them to a friendlier message; just allow them to bubble up and terminate the program.
* However, leave the LogTo code that ensures that SQL gets logged to the console for diagnostics.
* Do DbContext configuration within the OnConfiguring method of the DbContext type, rather than building the options externally and passing them to the constructor. Avoid any sort of DI unless it's necessary to reproducing the bug.
* In general, the less lines of code, the better.

## Post-repro steps

* If you've managed to confirm a bug in your repro and the user claims they are reporting a regression, please test your repro on both the failing version and the previous working version, to confirm that it's indeed a regression. Provide clear feedback confirming or refuting the fact that the reported issue is a regression.
* If you've managed to confirm a bug, please try to find possible duplicate issues - opened or closed - in the EF Core repo (https://github.com/dotnet/efcore), and post some candidates.

## Posting your findings

* Post your findings on the triaged issue as a comment.
* The comment should begin with a first-level heading with the text "AI Triage", followed by the sentence "The below is an AI-generated analysis and may contain inaccuracies."
* The minimal repro console program should be contained within the posted comment, wrapped inside a collapsible HTML `<details>` block, to not take up too much space (the summary should be "minimal repro").
* In your response, make sure that all links to issues, pull requests or source files are to the repo on github.com, and not local `vscode://` links, as your answer will be posted online.
