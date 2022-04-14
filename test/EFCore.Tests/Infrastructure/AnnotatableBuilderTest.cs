// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Infrastructure;

public class AnnotatableBuilderTest
{
    [ConditionalFact]
    public void Can_only_override_lower_source_annotation()
    {
        var builder = CreateAnnotatableBuilder();
        var metadata = builder.Metadata;

        Assert.NotNull(builder.HasAnnotation("Foo", "1", ConfigurationSource.Convention));
        Assert.NotNull(builder.HasAnnotation("Foo", "2", ConfigurationSource.DataAnnotation));

        Assert.Equal("2", metadata.GetAnnotations().Single().Value);

        Assert.Null(builder.HasAnnotation("Foo", "1", ConfigurationSource.Convention));
        Assert.Equal("2", metadata.GetAnnotations().Single().Value);
    }

    [ConditionalFact]
    public void Can_only_override_existing_annotation_explicitly()
    {
        var builder = CreateAnnotatableBuilder();
        var metadata = builder.Metadata;
        metadata["Foo"] = "1";

        Assert.NotNull(builder.HasAnnotation("Foo", "1", ConfigurationSource.DataAnnotation));
        Assert.Null(builder.HasAnnotation("Foo", "2", ConfigurationSource.DataAnnotation));

        Assert.Equal("1", metadata.GetAnnotations().Single().Value);

        Assert.NotNull(builder.HasAnnotation("Foo", "2", ConfigurationSource.Explicit));
        Assert.Equal("2", metadata.GetAnnotations().Single().Value);
    }

    [ConditionalFact]
    public void Annotation_set_explicitly_can_not_be_removed_by_convention()
    {
        var builder = CreateAnnotatableBuilder();
        var metadata = builder.Metadata;
        metadata["Foo"] = "1";

        Assert.Null(builder.HasAnnotation("Foo", null, ConfigurationSource.Convention));

        Assert.Equal("1", metadata.GetAnnotations().Single().Value);

        Assert.NotNull(builder.HasAnnotation("Foo", null, ConfigurationSource.Explicit));
        Assert.Null(metadata.GetAnnotations().Single().Value);
    }

    private AnnotatableBuilder<Model, InternalModelBuilder> CreateAnnotatableBuilder()
        => new InternalModelBuilder(new Model());
}
