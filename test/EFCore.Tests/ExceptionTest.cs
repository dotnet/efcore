// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

// ReSharper disable InconsistentNaming

namespace Microsoft.EntityFrameworkCore;

public class ExceptionTest
{
    [ConditionalFact]
    public void RetryLimitExceededException_exposes_public_empty_constructor()
        => new RetryLimitExceededException();

    [ConditionalFact]
    public void RetryLimitExceededException_exposes_public_string_constructor()
        => Assert.Equal("Foo", new RetryLimitExceededException("Foo").Message);

    [ConditionalFact]
    public void RetryLimitExceededException_exposes_public_string_and_inner_exception_constructor()
    {
        var inner = new Exception();

        var ex = new RetryLimitExceededException("Foo", inner);

        Assert.Equal("Foo", ex.Message);
        Assert.Same(inner, ex.InnerException);
    }

    [ConditionalFact]
    public void DbUpdateException_exposes_public_empty_constructor()
        => new DbUpdateException();

    [ConditionalFact]
    public void DbUpdateException_exposes_public_string_constructor()
        => Assert.Equal("Foo", new DbUpdateException("Foo").Message);

    [ConditionalFact]
    public void DbUpdateException_exposes_public_string_and_inner_exception_constructor()
    {
        var inner = new Exception();

        var ex = new DbUpdateException("Foo", inner);

        Assert.Equal("Foo", ex.Message);
        Assert.Same(inner, ex.InnerException);
    }

    [ConditionalFact]
    public void DbUpdateConcurrencyException_exposes_public_empty_constructor()
        => new DbUpdateConcurrencyException();

    [ConditionalFact]
    public void DbUpdateConcurrencyException_exposes_public_string_constructor()
        => Assert.Equal("Foo", new DbUpdateConcurrencyException("Foo").Message);

    [ConditionalFact]
    public void DbUpdateConcurrencyException_exposes_public_string_and_inner_exception_constructor()
    {
        var inner = new Exception();

        var ex = new DbUpdateConcurrencyException("Foo", inner);

        Assert.Equal("Foo", ex.Message);
        Assert.Same(inner, ex.InnerException);
    }

    [ConditionalFact]
    public void DbUpdateException_exposes_public_string_and_entries_constructor()
    {
        var entries = new List<EntityEntry>
        {
            new(new InternalEntityEntry(new FakeStateManager(), CreateEntityType(), null!)),
            new(new InternalEntityEntry(new FakeStateManager(), CreateEntityType(), null!))
        };
        var exception = new DbUpdateException("Foo", entries);

        Assert.Equal("Foo", exception.Message);
        Assert.Same(entries, exception.Entries);
    }

    [ConditionalFact]
    public void DbUpdateException_exposes_public_string_and_inner_exception_and_entries_constructor()
    {
        var inner = new Exception();
        var entries = new List<EntityEntry>
        {
            new(new InternalEntityEntry(new FakeStateManager(), CreateEntityType(), null!)),
            new(new InternalEntityEntry(new FakeStateManager(), CreateEntityType(), null!))
        };
        var exception = new DbUpdateException("Foo", inner, entries);

        Assert.Equal("Foo", exception.Message);
        Assert.Same(inner, exception.InnerException);
        Assert.Same(entries, exception.Entries);
    }

    private class FakeUpdateEntry : IUpdateEntry
    {
        public DbContext Context
            => throw new NotImplementedException();

        public void SetOriginalValue(IProperty property, object value)
            => throw new NotImplementedException();

        public void SetPropertyModified(IProperty property)
            => throw new NotImplementedException();

        public IEntityType EntityType { get; }
        public EntityState EntityState { get; set; }
        public IUpdateEntry SharedIdentityEntry { get; }

        public bool IsModified(IProperty property)
            => throw new NotImplementedException();

        public bool HasTemporaryValue(IProperty property)
            => throw new NotImplementedException();

        public bool HasStoreGeneratedValue(IProperty property)
            => throw new NotImplementedException();

        public bool IsStoreGenerated(IProperty property)
            => throw new NotImplementedException();

        public object GetCurrentValue(IPropertyBase propertyBase)
            => throw new NotImplementedException();

        public object GetOriginalValue(IPropertyBase propertyBase)
            => throw new NotImplementedException();

        public object GetOriginalOrCurrentValue(IPropertyBase propertyBase)
            => throw new NotImplementedException();

        public TProperty GetCurrentValue<TProperty>(IPropertyBase propertyBase)
            => throw new NotImplementedException();

        public TProperty GetOriginalValue<TProperty>(IProperty property)
            => throw new NotImplementedException();

        public void SetStoreGeneratedValue(IProperty property, object value, bool setModified = true)
            => throw new NotImplementedException();

        public EntityEntry ToEntityEntry()
            => new(new InternalEntityEntry(new FakeStateManager(), CreateEntityType(), null!));

        public object GetRelationshipSnapshotValue(IPropertyBase propertyBase)
            => throw new NotImplementedException();

        public object GetPreStoreGeneratedCurrentValue(IPropertyBase propertyBase)
            => throw new NotImplementedException();

        public bool IsConceptualNull(IProperty property)
            => throw new NotImplementedException();
    }

    private static IEntityType CreateEntityType()
    {
        var model = new Model();
        model.AddEntityType(typeof(object), owned: false, ConfigurationSource.Convention);
        return model.FinalizeModel().FindEntityType(typeof(object));
    }
}
