// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.TestUtilities;

public class RelationalModelAsserterTest
{
    [Fact]
    public void Comparing_member_annotations_checks_function_parameters()
    {
        var expected = CreateModel("expected");
        var actual = CreateModel("actual");

        RelationalModelAsserter.Instance.AssertEqual(expected, actual, [], [], compareMemberAnnotations: false);
        Assert.ThrowsAny<Exception>(
            () => RelationalModelAsserter.Instance.AssertEqual(expected, actual, [], [], compareMemberAnnotations: true));
        RelationalModelAsserter.Instance.AssertEqual(expected, CreateModel("expected"), [], [], compareMemberAnnotations: true);
    }

    private static IModel CreateModel(string annotation)
    {
        var builder = new ModelBuilder(new ConventionSet());
        ((Model)builder.Model).ModelId = Guid.Empty;
        var function = builder.HasDbFunction(typeof(RelationalModelAsserterTest).GetMethod(nameof(Function))!);
        function.HasParameter("value").Metadata["Custom"] = annotation;
        return (IModel)builder.Model;
    }

    public static int Function(int value)
        => value;
}
