// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata;

public class SequenceTest
{
    [ConditionalFact]
    public void Can_be_created_with_default_values()
    {
        IMutableModel model = new Model();

        var sequence = model.AddSequence("Foo");

        Assert.Equal("Foo", sequence.Name);
        Assert.Null(sequence.Schema);
        Assert.Equal(1, sequence.IncrementBy);
        Assert.Equal(1, sequence.StartValue);
        Assert.Null(sequence.MinValue);
        Assert.Null(sequence.MaxValue);
        Assert.Same(typeof(long), sequence.Type);
        Assert.False(sequence.IsCyclic);
        Assert.True(sequence.IsCached);
        Assert.Null(sequence.CacheSize);
        Assert.Same(model, sequence.Model);

        model.SetDefaultSchema("db0");

        Assert.Equal("db0", sequence.Schema);

        var conventionSequence = (IConventionSequence)sequence;
        Assert.Equal(ConfigurationSource.Explicit, conventionSequence.GetConfigurationSource());
        Assert.Null(conventionSequence.GetIncrementByConfigurationSource());
        Assert.Null(conventionSequence.GetStartValueConfigurationSource());
        Assert.Null(conventionSequence.GetMinValueConfigurationSource());
        Assert.Null(conventionSequence.GetMaxValueConfigurationSource());
        Assert.Null(conventionSequence.GetTypeConfigurationSource());
        Assert.Null(conventionSequence.GetIsCyclicConfigurationSource());
        Assert.Null(conventionSequence.GetIsCachedConfigurationSource());
        Assert.Null(conventionSequence.GetCacheSizeConfigurationSource());
    }

    [ConditionalFact]
    public void Can_be_created_with_specified_values()
    {
        IMutableModel model = new Model();

        model.SetDefaultSchema("db0");

        var sequence = model.AddSequence("Foo", "Smoo");
        sequence.StartValue = 1729;
        sequence.IncrementBy = 11;
        sequence.MinValue = 2001;
        sequence.MaxValue = 2010;
        sequence.Type = typeof(int);
        sequence.IsCyclic = true;
        sequence.IsCached = true;
        sequence.CacheSize = 20;

        Assert.Equal("Foo", sequence.Name);
        Assert.Equal("Smoo", sequence.Schema);
        Assert.Equal(11, sequence.IncrementBy);
        Assert.Equal(1729, sequence.StartValue);
        Assert.Equal(2001, sequence.MinValue);
        Assert.Equal(2010, sequence.MaxValue);
        Assert.Same(typeof(int), sequence.Type);
        Assert.True(sequence.IsCyclic);
        Assert.True(sequence.IsCached);
        Assert.Equal(20, sequence.CacheSize);

        var conventionSequence = (IConventionSequence)sequence;
        Assert.Equal(ConfigurationSource.Explicit, conventionSequence.GetConfigurationSource());
        Assert.Equal(ConfigurationSource.Explicit, conventionSequence.GetIncrementByConfigurationSource());
        Assert.Equal(ConfigurationSource.Explicit, conventionSequence.GetStartValueConfigurationSource());
        Assert.Equal(ConfigurationSource.Explicit, conventionSequence.GetMinValueConfigurationSource());
        Assert.Equal(ConfigurationSource.Explicit, conventionSequence.GetMaxValueConfigurationSource());
        Assert.Equal(ConfigurationSource.Explicit, conventionSequence.GetTypeConfigurationSource());
        Assert.Equal(ConfigurationSource.Explicit, conventionSequence.GetIsCyclicConfigurationSource());
        Assert.Equal(ConfigurationSource.Explicit, conventionSequence.GetIsCachedConfigurationSource());
        Assert.Equal(ConfigurationSource.Explicit, conventionSequence.GetCacheSizeConfigurationSource());
    }

    [ConditionalFact]
    public void Can_only_be_created_for_byte_short_int_and_long_decimal()
    {
        var sequence = ((IMutableModel)new Model()).AddSequence("Foo");
        sequence.Type = typeof(byte);
        Assert.Same(typeof(byte), sequence.Type);
        sequence.Type = typeof(short);
        Assert.Same(typeof(short), sequence.Type);
        sequence.Type = typeof(int);
        Assert.Same(typeof(int), sequence.Type);
        sequence.Type = typeof(long);
        Assert.Same(typeof(long), sequence.Type);
        sequence.Type = typeof(decimal);
        Assert.Same(typeof(decimal), sequence.Type);

        Assert.Equal(
            RelationalStrings.BadSequenceType,
            Assert.Throws<ArgumentException>(
                () => sequence.Type = typeof(bool)).Message);
    }
}
