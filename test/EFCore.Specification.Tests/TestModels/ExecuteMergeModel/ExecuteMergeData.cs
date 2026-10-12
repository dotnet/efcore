// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.ExecuteMergeModel;

public class ExecuteMergeData : ISetSource
{
    public IReadOnlyList<Blog> Blogs { get; } =
    [
        new() { Id = 1, Name = "Blog1" },
        new() { Id = 2, Name = "Blog2", Note = "Note2" }
    ];

    public IReadOnlyList<DailyVisit> Visits { get; } =
    [
        new() { UserId = 1, Day = "mon", Visits = 5 }
    ];

    public IReadOnlyList<Account> Accounts { get; } =
    [
        new() { Id = 1, ExternalId = 10, Balance = 100 }
    ];

    public IQueryable<TEntity> Set<TEntity>()
        where TEntity : class
    {
        if (typeof(TEntity) == typeof(Blog))
        {
            return (IQueryable<TEntity>)Blogs.AsQueryable();
        }

        if (typeof(TEntity) == typeof(DailyVisit))
        {
            return (IQueryable<TEntity>)Visits.AsQueryable();
        }

        if (typeof(TEntity) == typeof(Account))
        {
            return (IQueryable<TEntity>)Accounts.AsQueryable();
        }

        throw new InvalidOperationException("Invalid entity type: " + typeof(TEntity));
    }
}
