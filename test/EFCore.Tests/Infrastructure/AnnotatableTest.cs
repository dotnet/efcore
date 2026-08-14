// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Infrastructure;

public class AnnotatableTest
{
    [ConditionalFact]
    public void Can_add_and_remove_annotation()
    {
        var annotatable = new Annotatable();
        Assert.Null(annotatable.FindAnnotation("Foo"));
        Assert.Null(annotatable.RemoveAnnotation("Foo"));

        var annotation = annotatable.AddAnnotation("Foo", "Bar");

        Assert.NotNull(annotation);
        Assert.Equal("Bar", annotation.Value);
        Assert.Equal("Bar", annotatable["Foo"]);
        Assert.Same(annotation, annotatable.FindAnnotation("Foo"));

        Assert.Equal([annotation], annotatable.GetAnnotations().ToArray());

        Assert.Same(annotation, annotatable.RemoveAnnotation(annotation.Name));

        Assert.Empty(annotatable.GetAnnotations());
        Assert.Null(annotatable.RemoveAnnotation(annotation.Name));
        Assert.Null(annotatable["Foo"]);
        Assert.Null(annotatable.FindAnnotation("Foo"));
    }

    [ConditionalFact]
    public void Added_annotation_type_is_consistent()
    {
        var annotatable = new Annotatable();

        annotatable.AddAnnotations(new[] { new ConventionAnnotation("Foo", "Bar", ConfigurationSource.Convention) });

        Assert.Equal(typeof(Annotation), annotatable.FindAnnotation("Foo").GetType());

        var conventionAnnotatable = new Model();

        conventionAnnotatable.AddAnnotations(new[] { new Annotation("Foo", "Bar") });

        Assert.Equal(typeof(ConventionAnnotation), conventionAnnotatable.FindAnnotation("Foo").GetType());
    }

    [ConditionalFact]
    public void Adding_duplicate_annotation_throws()
    {
        var annotatable = new Annotatable();

        annotatable.AddAnnotation("Foo", "Bar");

        Assert.Equal(
            CoreStrings.DuplicateAnnotation("Foo", annotatable.ToString()),
            Assert.Throws<InvalidOperationException>(() => annotatable.AddAnnotation("Foo", "Bar")).Message);
    }

    [ConditionalFact]
    public void Can_get_and_set_model_annotations()
    {
        IMutableAnnotatable annotatable = new Annotatable();
        Assert.Empty(annotatable.GetAnnotations());
        var annotation = annotatable.AddAnnotation("Foo", "Bar");

        Assert.NotNull(annotation);
        Assert.Same(annotation, annotatable.FindAnnotation("Foo"));
        Assert.Same(annotation, annotatable.GetAnnotation("Foo"));
        Assert.Null(annotatable["foo"]);
        Assert.Null(annotatable.FindAnnotation("foo"));

        annotatable["Foo"] = "horse";

        Assert.Equal("horse", annotatable["Foo"]);

        annotatable["Foo"] = null;

        Assert.Null(annotatable["Foo"]);
        Assert.Empty(annotatable.GetAnnotations());

        Assert.Equal(
            CoreStrings.AnnotationNotFound("Foo", "Microsoft.EntityFrameworkCore.Infrastructure.Annotatable"),
            Assert.Throws<InvalidOperationException>(() => annotatable.GetAnnotation("Foo")).Message);
    }

    [ConditionalFact]
    public void Annotations_are_ordered_by_name()
    {
        var annotatable = new Annotatable();

        var annotation1 = annotatable.AddAnnotation("Z", "Foo");
        var annotation2 = annotatable.AddAnnotation("A", "Bar");

        Assert.True(new[] { annotation2, annotation1 }.SequenceEqual(annotatable.GetAnnotations()));
    }

    [ConditionalFact]
    public void Can_add_and_remove_runtime_annotation()
    {
        var annotatable = new Model().FinalizeModel();
        Assert.Empty(annotatable.GetRuntimeAnnotations());
        Assert.Null(annotatable.FindRuntimeAnnotation("Foo"));
        Assert.Null(annotatable.RemoveRuntimeAnnotation("Foo"));

        var annotation = annotatable.AddRuntimeAnnotation("Foo", "Bar");

        Assert.NotNull(annotation);
        Assert.Equal("Bar", annotation.Value);
        Assert.Null(annotatable["Foo"]);
        Assert.Same(annotation, annotatable.FindRuntimeAnnotation("Foo"));

        var annotation2 = annotatable.SetRuntimeAnnotation("A", "Foo");
        Assert.Equal(new[] { annotation2, annotation }, annotatable.GetRuntimeAnnotations());
        Assert.Empty(annotatable.GetAnnotations());

        Assert.Same(annotation, annotatable.RemoveRuntimeAnnotation(annotation.Name));
        Assert.Same(annotation2, annotatable.RemoveRuntimeAnnotation(annotation2.Name));

        Assert.Empty(annotatable.GetRuntimeAnnotations());
        Assert.Null(annotatable.RemoveRuntimeAnnotation(annotation.Name));
        Assert.Null(annotatable["Foo"]);
        Assert.Null(annotatable.FindRuntimeAnnotation("Foo"));
    }

    [ConditionalFact]
    public void Adding_duplicate_runtime_annotation_throws()
    {
        var annotatable = new Model().FinalizeModel();

        annotatable.AddRuntimeAnnotation("Foo", "Bar");

        Assert.Equal(
            CoreStrings.DuplicateAnnotation("Foo", annotatable.ToString()),
            Assert.Throws<InvalidOperationException>(() => annotatable.AddRuntimeAnnotation("Foo", "Bar")).Message);
    }
}
