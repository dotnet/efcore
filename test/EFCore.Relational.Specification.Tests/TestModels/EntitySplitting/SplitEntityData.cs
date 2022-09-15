// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.EntitySplitting;

public class SplitEntityData : ISetSource
{
    public static readonly SplitEntityData Instance = new();

    private readonly SplitEntityOne[] _splitEntityOnes;

    private SplitEntityData()
    {
        _splitEntityOnes = CreateSplitEntityOnes();
    }

    public IQueryable<TEntity> Set<TEntity>()
        where TEntity : class
    {
        if (typeof(TEntity) == typeof(SplitEntityOne))
        {
            return (IQueryable<TEntity>)_splitEntityOnes.AsQueryable();
        }

        throw new InvalidOperationException("Invalid entity type: " + typeof(TEntity));
    }

    private static SplitEntityOne[] CreateSplitEntityOnes()
        => new SplitEntityOne[] { };

    public void Seed(EntitySplittingContext context)
    {
        context.AddRange(_splitEntityOnes);

        context.SaveChanges();
    }
}
