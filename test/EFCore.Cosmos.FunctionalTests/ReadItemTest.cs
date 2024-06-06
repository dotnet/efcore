// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations.Schema;

namespace Microsoft.EntityFrameworkCore;

public class ReadItemTest : IClassFixture<ReadItemTest.ReadItemFixture>
{
    public ReadItemTest(ReadItemFixture fixture)
    {
        Fixture = fixture;
        fixture.TestSqlLoggerFactory.Clear();
    }

    protected ReadItemFixture Fixture { get; }

    [ConditionalTheory] // Issue #20693
    [InlineData(QueryTrackingBehavior.TrackAll)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    public virtual async Task FirstOrDefault_int_key_constant_is_translated_to_ReadItem(QueryTrackingBehavior trackingBehavior)
    {
        using var context = CreateContext();
        var entity = await ApplyTrackingBehavior(context.Set<IntKey>(), trackingBehavior).FirstOrDefaultAsync(e => e.Id == 77);

        AssertSql(
            """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "IntKey") AND (c["Id"] = 77))
OFFSET 0 LIMIT 1
""");

        ValidateIntKeyValues(entity!);
    }

    [ConditionalTheory] // Issue #20693
    [InlineData(QueryTrackingBehavior.TrackAll)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    public virtual async Task FirstOrDefault_int_key_variable_is_translated_to_ReadItem(QueryTrackingBehavior trackingBehavior)
    {
        using var context = CreateContext();

        var val = 77;
        var entity = await ApplyTrackingBehavior(context.Set<IntKey>(), trackingBehavior).FirstOrDefaultAsync(e => e.Id == val);

        AssertSql(
            """
@__val_0='77'

SELECT c
FROM root c
WHERE ((c["Discriminator"] = "IntKey") AND (c["Id"] = @__val_0))
OFFSET 0 LIMIT 1
""");

        ValidateIntKeyValues(entity!);
    }

    [ConditionalTheory] // Issue #20693
    [InlineData(QueryTrackingBehavior.TrackAll)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    public virtual async Task FirstOrDefault_int_key_constant_value_first_is_translated_to_ReadItem(QueryTrackingBehavior trackingBehavior)
    {
        using var context = CreateContext();
        var entity = await ApplyTrackingBehavior(context.Set<IntKey>(), trackingBehavior).FirstOrDefaultAsync(e => 77 == e.Id);

        AssertSql(
            """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "IntKey") AND (77 = c["Id"]))
OFFSET 0 LIMIT 1
""");

        ValidateIntKeyValues(entity!);
    }

    [ConditionalTheory] // Issue #20693
    [InlineData(QueryTrackingBehavior.TrackAll)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    public virtual async Task FirstOrDefault_int_key_variable_value_first_is_translated_to_ReadItem(QueryTrackingBehavior trackingBehavior)
    {
        using var context = CreateContext();

        var val = 77;
        var entity = await ApplyTrackingBehavior(context.Set<IntKey>(), trackingBehavior).FirstOrDefaultAsync(e => val == e.Id);

        AssertSql(
            """
@__val_0='77'

SELECT c
FROM root c
WHERE ((c["Discriminator"] = "IntKey") AND (@__val_0 = c["Id"]))
OFFSET 0 LIMIT 1
""");

        ValidateIntKeyValues(entity!);
    }

    [ConditionalTheory] // Issue #20693
    [InlineData(QueryTrackingBehavior.TrackAll)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    public virtual async Task First_int_key_constant_is_translated_to_ReadItem(QueryTrackingBehavior trackingBehavior)
    {
        using var context = CreateContext();
        var entity = await ApplyTrackingBehavior(context.Set<IntKey>(), trackingBehavior).FirstAsync(e => e.Id == 77);

        AssertSql(
            """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "IntKey") AND (c["Id"] = 77))
OFFSET 0 LIMIT 1
""");

        AssertSql(
        );
    }

    [ConditionalTheory] // Issue #20693
    [InlineData(QueryTrackingBehavior.TrackAll)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    public virtual async Task First_int_key_variable_is_translated_to_ReadItem(QueryTrackingBehavior trackingBehavior)
    {
        using var context = CreateContext();

        var val = 77;
        var entity = await ApplyTrackingBehavior(context.Set<IntKey>(), trackingBehavior).FirstAsync(e => e.Id == val);

        AssertSql(
            """
@__val_0='77'

SELECT c
FROM root c
WHERE ((c["Discriminator"] = "IntKey") AND (c["Id"] = @__val_0))
OFFSET 0 LIMIT 1
""");

        ValidateIntKeyValues(entity);
    }

    [ConditionalTheory] // Issue #20693
    [InlineData(QueryTrackingBehavior.TrackAll)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    public virtual async Task First_int_key_constant_value_first_is_translated_to_ReadItem(QueryTrackingBehavior trackingBehavior)
    {
        using var context = CreateContext();
        var entity = await ApplyTrackingBehavior(context.Set<IntKey>(), trackingBehavior).FirstAsync(e => 77 == e.Id);

        AssertSql(
            """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "IntKey") AND (77 = c["Id"]))
OFFSET 0 LIMIT 1
""");
    }

    [ConditionalTheory] // Issue #20693
    [InlineData(QueryTrackingBehavior.TrackAll)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    public virtual async Task First_int_key_variable_value_first_is_translated_to_ReadItem(QueryTrackingBehavior trackingBehavior)
    {
        using var context = CreateContext();

        var val = 77;
        var entity = await ApplyTrackingBehavior(context.Set<IntKey>(), trackingBehavior).FirstAsync(e => val == e.Id);

        AssertSql(
            """
@__val_0='77'

SELECT c
FROM root c
WHERE ((c["Discriminator"] = "IntKey") AND (@__val_0 = c["Id"]))
OFFSET 0 LIMIT 1
""");
    }

    [ConditionalTheory] // Issue #20693
    [InlineData(QueryTrackingBehavior.TrackAll)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    public virtual async Task SingleOrDefault_int_key_constant_is_translated_to_ReadItem(QueryTrackingBehavior trackingBehavior)
    {
        using var context = CreateContext();
        var entity = await ApplyTrackingBehavior(context.Set<IntKey>(), trackingBehavior).SingleOrDefaultAsync(e => e.Id == 77);

        AssertSql(
            """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "IntKey") AND (c["Id"] = 77))
OFFSET 0 LIMIT 2
""");

        AssertSql(
        );
    }

    [ConditionalTheory] // Issue #20693
    [InlineData(QueryTrackingBehavior.TrackAll)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    public virtual async Task SingleOrDefault_int_key_variable_is_translated_to_ReadItem(QueryTrackingBehavior trackingBehavior)
    {
        using var context = CreateContext();

        var val = 77;
        var entity = await ApplyTrackingBehavior(context.Set<IntKey>(), trackingBehavior).SingleOrDefaultAsync(e => e.Id == val);

        AssertSql(
            """
@__val_0='77'

SELECT c
FROM root c
WHERE ((c["Discriminator"] = "IntKey") AND (c["Id"] = @__val_0))
OFFSET 0 LIMIT 2
""");

        ValidateIntKeyValues(entity!);
    }

    [ConditionalTheory] // Issue #20693
    [InlineData(QueryTrackingBehavior.TrackAll)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    public virtual async Task SingleOrDefault_int_key_constant_value_first_is_translated_to_ReadItem(QueryTrackingBehavior trackingBehavior)
    {
        using var context = CreateContext();
        var entity = await ApplyTrackingBehavior(context.Set<IntKey>(), trackingBehavior).SingleOrDefaultAsync(e => 77 == e.Id);

        AssertSql(
            """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "IntKey") AND (77 = c["Id"]))
OFFSET 0 LIMIT 2
""");

        ValidateIntKeyValues(entity!);
    }

    [ConditionalTheory] // Issue #20693
    [InlineData(QueryTrackingBehavior.TrackAll)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    public virtual async Task SingleOrDefault_int_key_variable_value_first_is_translated_to_ReadItem(QueryTrackingBehavior trackingBehavior)
    {
        using var context = CreateContext();

        var val = 77;
        var entity = await ApplyTrackingBehavior(context.Set<IntKey>(), trackingBehavior).SingleOrDefaultAsync(e => val == e.Id);

        AssertSql(
            """
@__val_0='77'

SELECT c
FROM root c
WHERE ((c["Discriminator"] = "IntKey") AND (@__val_0 = c["Id"]))
OFFSET 0 LIMIT 2
""");

        ValidateIntKeyValues(entity!);
    }

    [ConditionalTheory] // Issue #20693
    [InlineData(QueryTrackingBehavior.TrackAll)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    public virtual async Task Single_int_key_constant_is_translated_to_ReadItem(QueryTrackingBehavior trackingBehavior)
    {
        using var context = CreateContext();
        var entity = await ApplyTrackingBehavior(context.Set<IntKey>(), trackingBehavior).SingleAsync(e => e.Id == 77);

        AssertSql(
            """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "IntKey") AND (c["Id"] = 77))
OFFSET 0 LIMIT 2
""");

        ValidateIntKeyValues(entity);
    }

    [ConditionalTheory] // Issue #20693
    [InlineData(QueryTrackingBehavior.TrackAll)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    public virtual async Task Single_int_key_variable_is_translated_to_ReadItem(QueryTrackingBehavior trackingBehavior)
    {
        using var context = CreateContext();

        var val = 77;
        var entity = await ApplyTrackingBehavior(context.Set<IntKey>(), trackingBehavior).SingleAsync(e => e.Id == val);

        AssertSql(
            """
@__val_0='77'

SELECT c
FROM root c
WHERE ((c["Discriminator"] = "IntKey") AND (c["Id"] = @__val_0))
OFFSET 0 LIMIT 2
""");
    }

    [ConditionalTheory] // Issue #20693
    [InlineData(QueryTrackingBehavior.TrackAll)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    public virtual async Task Single_int_key_constant_value_first_is_translated_to_ReadItem(QueryTrackingBehavior trackingBehavior)
    {
        using var context = CreateContext();
        var entity = await ApplyTrackingBehavior(context.Set<IntKey>(), trackingBehavior).SingleAsync(e => 77 == e.Id);

        AssertSql(
            """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "IntKey") AND (77 = c["Id"]))
OFFSET 0 LIMIT 2
""");

        ValidateIntKeyValues(entity);
    }

    [ConditionalTheory] // Issue #20693
    [InlineData(QueryTrackingBehavior.TrackAll)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    public virtual async Task Single_int_key_variable_value_first_is_translated_to_ReadItem(QueryTrackingBehavior trackingBehavior)
    {
        using var context = CreateContext();

        var val = 77;
        var entity = await ApplyTrackingBehavior(context.Set<IntKey>(), trackingBehavior).SingleAsync(e => val == e.Id);

        AssertSql(
            """
@__val_0='77'

SELECT c
FROM root c
WHERE ((c["Discriminator"] = "IntKey") AND (@__val_0 = c["Id"]))
OFFSET 0 LIMIT 2
""");

        ValidateIntKeyValues(entity);
    }

    [ConditionalTheory] // Issue #20693
    [InlineData(QueryTrackingBehavior.TrackAll)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    public virtual async Task FirstOrDefault_int_key_constant_with_EF_Property_is_translated_to_ReadItem(
        QueryTrackingBehavior trackingBehavior)
    {
        using var context = CreateContext();
        var entity = await ApplyTrackingBehavior(context.Set<IntKey>(), trackingBehavior)
            .FirstOrDefaultAsync(e => EF.Property<int>(e, nameof(IntKey.Id)) == 77);

        AssertSql(
            """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "IntKey") AND (c["Id"] = 77))
OFFSET 0 LIMIT 1
""");

        ValidateIntKeyValues(entity!);
    }

    [ConditionalTheory]
    [InlineData(QueryTrackingBehavior.TrackAll)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    public virtual async Task FirstOrDefault_int_key_variable_with_EF_Property_is_translated_to_ReadItem(
        QueryTrackingBehavior trackingBehavior)
    {
        using var context = CreateContext();

        var val = 77;
        var entity = await ApplyTrackingBehavior(context.Set<IntKey>(), trackingBehavior)
            .FirstOrDefaultAsync(e => EF.Property<int>(e, nameof(IntKey.Id)) == val);

        AssertSql(
            """
ReadItem(None, IntKey|77)
""");

        ValidateIntKeyValues(entity!);
    }

    [ConditionalTheory] // Issue #20693
    [InlineData(QueryTrackingBehavior.TrackAll)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    public virtual async Task FirstOrDefault_int_key_constant_value_first_with_EF_Property_is_translated_to_ReadItem(
        QueryTrackingBehavior trackingBehavior)
    {
        using var context = CreateContext();
        var entity = await ApplyTrackingBehavior(context.Set<IntKey>(), trackingBehavior)
            .FirstOrDefaultAsync(e => 77 == EF.Property<int>(e, nameof(IntKey.Id)));

        AssertSql(
            """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "IntKey") AND (77 = c["Id"]))
OFFSET 0 LIMIT 1
""");

        ValidateIntKeyValues(entity!);
    }

    [ConditionalTheory] // Issue #20693
    [InlineData(QueryTrackingBehavior.TrackAll)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    public virtual async Task FirstOrDefault_int_key_variable_value_first_with_EF_Property_is_translated_to_ReadItem(
        QueryTrackingBehavior trackingBehavior)
    {
        using var context = CreateContext();

        var val = 77;
        var entity = await ApplyTrackingBehavior(context.Set<IntKey>(), trackingBehavior)
            .FirstOrDefaultAsync(e => val == EF.Property<int>(e, nameof(IntKey.Id)));

        AssertSql(
            """
@__val_0='77'

SELECT c
FROM root c
WHERE ((c["Discriminator"] = "IntKey") AND (@__val_0 = c["Id"]))
OFFSET 0 LIMIT 1
""");

        ValidateIntKeyValues(entity!);
    }

    [ConditionalTheory] // Issue #20693
    [InlineData(QueryTrackingBehavior.TrackAll)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    public virtual async Task First_int_key_constant_with_EF_Property_is_translated_to_ReadItem(QueryTrackingBehavior trackingBehavior)
    {
        using var context = CreateContext();
        var entity = await ApplyTrackingBehavior(context.Set<IntKey>(), trackingBehavior)
            .FirstAsync(e => EF.Property<int>(e, nameof(IntKey.Id)) == 77);

        AssertSql(
            """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "IntKey") AND (c["Id"] = 77))
OFFSET 0 LIMIT 1
""");

        AssertSql(
        );
    }

    [ConditionalTheory] // Issue #20693
    [InlineData(QueryTrackingBehavior.TrackAll)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    public virtual async Task First_int_key_variable_with_EF_Property_is_translated_to_ReadItem(QueryTrackingBehavior trackingBehavior)
    {
        using var context = CreateContext();

        var val = 77;
        var entity = await ApplyTrackingBehavior(context.Set<IntKey>(), trackingBehavior)
            .FirstAsync(e => EF.Property<int>(e, nameof(IntKey.Id)) == val);

        AssertSql(
            """
@__val_0='77'

SELECT c
FROM root c
WHERE ((c["Discriminator"] = "IntKey") AND (c["Id"] = @__val_0))
OFFSET 0 LIMIT 1
""");

        ValidateIntKeyValues(entity);
    }

    [ConditionalTheory] // Issue #20693
    [InlineData(QueryTrackingBehavior.TrackAll)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    public virtual async Task First_int_key_constant_value_first_with_EF_Property_is_translated_to_ReadItem(
        QueryTrackingBehavior trackingBehavior)
    {
        using var context = CreateContext();
        var entity = await ApplyTrackingBehavior(context.Set<IntKey>(), trackingBehavior)
            .FirstAsync(e => 77 == EF.Property<int>(e, nameof(IntKey.Id)));

        AssertSql(
            """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "IntKey") AND (77 = c["Id"]))
OFFSET 0 LIMIT 1
""");
    }

    [ConditionalTheory] // Issue #20693
    [InlineData(QueryTrackingBehavior.TrackAll)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    public virtual async Task First_int_key_variable_value_first_with_EF_Property_is_translated_to_ReadItem(
        QueryTrackingBehavior trackingBehavior)
    {
        using var context = CreateContext();

        var val = 77;
        var entity = await ApplyTrackingBehavior(context.Set<IntKey>(), trackingBehavior)
            .FirstAsync(e => val == EF.Property<int>(e, nameof(IntKey.Id)));

        AssertSql(
            """
@__val_0='77'

SELECT c
FROM root c
WHERE ((c["Discriminator"] = "IntKey") AND (@__val_0 = c["Id"]))
OFFSET 0 LIMIT 1
""");
    }

    [ConditionalTheory] // Issue #20693
    [InlineData(QueryTrackingBehavior.TrackAll)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    public virtual async Task SingleOrDefault_int_key_constant_with_EF_Property_is_translated_to_ReadItem(
        QueryTrackingBehavior trackingBehavior)
    {
        using var context = CreateContext();
        var entity = await ApplyTrackingBehavior(context.Set<IntKey>(), trackingBehavior)
            .SingleOrDefaultAsync(e => EF.Property<int>(e, nameof(IntKey.Id)) == 77);

        AssertSql(
            """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "IntKey") AND (c["Id"] = 77))
OFFSET 0 LIMIT 2
""");

        AssertSql(
        );
    }

    [ConditionalTheory] // Issue #20693
    [InlineData(QueryTrackingBehavior.TrackAll)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    public virtual async Task SingleOrDefault_int_key_variable_with_EF_Property_is_translated_to_ReadItem(
        QueryTrackingBehavior trackingBehavior)
    {
        using var context = CreateContext();

        var val = 77;
        var entity = await ApplyTrackingBehavior(context.Set<IntKey>(), trackingBehavior)
            .SingleOrDefaultAsync(e => EF.Property<int>(e, nameof(IntKey.Id)) == val);

        AssertSql(
            """
@__val_0='77'

SELECT c
FROM root c
WHERE ((c["Discriminator"] = "IntKey") AND (c["Id"] = @__val_0))
OFFSET 0 LIMIT 2
""");

        ValidateIntKeyValues(entity!);
    }

    [ConditionalTheory] // Issue #20693
    [InlineData(QueryTrackingBehavior.TrackAll)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    public virtual async Task SingleOrDefault_int_key_constant_value_first_with_EF_Property_is_translated_to_ReadItem(
        QueryTrackingBehavior trackingBehavior)
    {
        using var context = CreateContext();
        var entity = await ApplyTrackingBehavior(context.Set<IntKey>(), trackingBehavior)
            .SingleOrDefaultAsync(e => 77 == EF.Property<int>(e, nameof(IntKey.Id)));

        AssertSql(
            """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "IntKey") AND (77 = c["Id"]))
OFFSET 0 LIMIT 2
""");

        ValidateIntKeyValues(entity!);
    }

    [ConditionalTheory] // Issue #20693
    [InlineData(QueryTrackingBehavior.TrackAll)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    public virtual async Task SingleOrDefault_int_key_variable_value_first_with_EF_Property_is_translated_to_ReadItem(
        QueryTrackingBehavior trackingBehavior)
    {
        using var context = CreateContext();

        var val = 77;
        var entity = await ApplyTrackingBehavior(context.Set<IntKey>(), trackingBehavior)
            .SingleOrDefaultAsync(e => val == EF.Property<int>(e, nameof(IntKey.Id)));

        AssertSql(
            """
@__val_0='77'

SELECT c
FROM root c
WHERE ((c["Discriminator"] = "IntKey") AND (@__val_0 = c["Id"]))
OFFSET 0 LIMIT 2
""");

        ValidateIntKeyValues(entity!);
    }

    [ConditionalTheory] // Issue #20693
    [InlineData(QueryTrackingBehavior.TrackAll)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    public virtual async Task Single_int_key_constant_with_EF_Property_is_translated_to_ReadItem(QueryTrackingBehavior trackingBehavior)
    {
        using var context = CreateContext();
        var entity = await ApplyTrackingBehavior(context.Set<IntKey>(), trackingBehavior)
            .SingleAsync(e => EF.Property<int>(e, nameof(IntKey.Id)) == 77);

        AssertSql(
            """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "IntKey") AND (c["Id"] = 77))
OFFSET 0 LIMIT 2
""");

        ValidateIntKeyValues(entity);
    }

    [ConditionalTheory] // Issue #20693
    [InlineData(QueryTrackingBehavior.TrackAll)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    public virtual async Task Single_int_key_variable_with_EF_Property_is_translated_to_ReadItem(QueryTrackingBehavior trackingBehavior)
    {
        using var context = CreateContext();

        var val = 77;
        var entity = await ApplyTrackingBehavior(context.Set<IntKey>(), trackingBehavior)
            .SingleAsync(e => EF.Property<int>(e, nameof(IntKey.Id)) == val);

        AssertSql(
            """
@__val_0='77'

SELECT c
FROM root c
WHERE ((c["Discriminator"] = "IntKey") AND (c["Id"] = @__val_0))
OFFSET 0 LIMIT 2
""");
    }

    [ConditionalTheory] // Issue #20693
    [InlineData(QueryTrackingBehavior.TrackAll)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    public virtual async Task Single_int_key_constant_value_first_with_EF_Property_is_translated_to_ReadItem(
        QueryTrackingBehavior trackingBehavior)
    {
        using var context = CreateContext();
        var entity = await ApplyTrackingBehavior(context.Set<IntKey>(), trackingBehavior)
            .SingleAsync(e => 77 == EF.Property<int>(e, nameof(IntKey.Id)));

        AssertSql(
            """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "IntKey") AND (77 = c["Id"]))
OFFSET 0 LIMIT 2
""");

        ValidateIntKeyValues(entity);
    }

    [ConditionalTheory] // Issue #20693
    [InlineData(QueryTrackingBehavior.TrackAll)]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    public virtual async Task Single_int_key_variable_value_first_with_EF_Property_is_translated_to_ReadItem(
        QueryTrackingBehavior trackingBehavior)
    {
        using var context = CreateContext();

        var val = 77;
        var entity = await ApplyTrackingBehavior(context.Set<IntKey>(), trackingBehavior)
            .SingleAsync(e => val == EF.Property<int>(e, nameof(IntKey.Id)));

        AssertSql(
            """
@__val_0='77'

SELECT c
FROM root c
WHERE ((c["Discriminator"] = "IntKey") AND (@__val_0 = c["Id"]))
OFFSET 0 LIMIT 2
""");

        ValidateIntKeyValues(entity);
    }

    private static void ValidateIntKeyValues(IntKey entity)
    {
        Assert.Equal("Smokey", entity.Foo);
        Assert.Equal(7, entity.OwnedReference.Prop);
        Assert.Equal(2, entity.OwnedCollection.Count);
        Assert.Contains(71, entity.OwnedCollection.Select(e => e.Prop));
        Assert.Contains(72, entity.OwnedCollection.Select(e => e.Prop));
        Assert.Equal("7", entity.OwnedReference.NestedOwned.Prop);
        Assert.Equal(2, entity.OwnedReference.NestedOwnedCollection.Count);
        Assert.Contains("71", entity.OwnedReference.NestedOwnedCollection.Select(e => e.Prop));
        Assert.Contains("72", entity.OwnedReference.NestedOwnedCollection.Select(e => e.Prop));
    }

    // [ConditionalTheory]
    // [InlineData((int)CancellationType.Right)]
    // [InlineData((int)CancellationType.Wrong)]
    // [InlineData((int)CancellationType.None)]
    // public virtual async Task Returns_null_for_int_key_not_in_store_async(CancellationType cancellationType)
    // {
    //     using var context = CreateContext();
    //     Assert.Null(await Finder.FindAsync<IntKey>(cancellationType, context, [99]));
    // }
    //
    // [ConditionalTheory]
    // [InlineData((int)CancellationType.Right)]
    // [InlineData((int)CancellationType.Wrong)]
    // [InlineData((int)CancellationType.None)]
    // public virtual async Task Find_nullable_int_key_tracked_async(CancellationType cancellationType)
    // {
    //     using var context = CreateContext();
    //     var entity = context.Attach(
    //         new NullableIntKey { Id = 88 }).Entity;
    //
    //     Assert.Same(entity, await Finder.FindAsync<NullableIntKey>(cancellationType, context, [88]));
    // }
    //
    // [ConditionalTheory]
    // [InlineData((int)CancellationType.Right)]
    // [InlineData((int)CancellationType.Wrong)]
    // [InlineData((int)CancellationType.None)]
    // public virtual async Task Find_nullable_int_key_from_store_async(CancellationType cancellationType)
    // {
    //     using var context = CreateContext();
    //     Assert.Equal("Smokey", (await Finder.FindAsync<NullableIntKey>(cancellationType, context, [77])).Foo);
    // }
    //
    // [ConditionalTheory]
    // [InlineData((int)CancellationType.Right)]
    // [InlineData((int)CancellationType.Wrong)]
    // [InlineData((int)CancellationType.None)]
    // public virtual async Task Returns_null_for_nullable_int_key_not_in_store_async(CancellationType cancellationType)
    // {
    //     using var context = CreateContext();
    //     Assert.Null(await Finder.FindAsync<NullableIntKey>(cancellationType, context, [99]));
    // }
    //
    // [ConditionalTheory]
    // [InlineData((int)CancellationType.Right)]
    // [InlineData((int)CancellationType.Wrong)]
    // [InlineData((int)CancellationType.None)]
    // public virtual async Task Find_string_key_tracked_async(CancellationType cancellationType)
    // {
    //     using var context = CreateContext();
    //     var entity = context.Attach(
    //         new StringKey { Id = "Rabbit" }).Entity;
    //
    //     Assert.Same(entity, await Finder.FindAsync<StringKey>(cancellationType, context, ["Rabbit"]));
    // }
    //
    // [ConditionalTheory]
    // [InlineData((int)CancellationType.Right)]
    // [InlineData((int)CancellationType.Wrong)]
    // [InlineData((int)CancellationType.None)]
    // public virtual async Task Find_string_key_from_store_async(CancellationType cancellationType)
    // {
    //     using var context = CreateContext();
    //     Assert.Equal("Alice", (await Finder.FindAsync<StringKey>(cancellationType, context, ["Cat"])).Foo);
    // }
    //
    // [ConditionalTheory]
    // [InlineData((int)CancellationType.Right)]
    // [InlineData((int)CancellationType.Wrong)]
    // [InlineData((int)CancellationType.None)]
    // public virtual async Task Returns_null_for_string_key_not_in_store_async(CancellationType cancellationType)
    // {
    //     using var context = CreateContext();
    //     Assert.Null(await Finder.FindAsync<StringKey>(cancellationType, context, ["Fox"]));
    // }
    //
    // [ConditionalTheory]
    // [InlineData((int)CancellationType.Right)]
    // [InlineData((int)CancellationType.Wrong)]
    // [InlineData((int)CancellationType.None)]
    // public virtual async Task Find_composite_key_tracked_async(CancellationType cancellationType)
    // {
    //     using var context = CreateContext();
    //     var entity = context.Attach(
    //         new CompositeKey { Id1 = 88, Id2 = "Rabbit" }).Entity;
    //
    //     Assert.Same(entity, await Finder.FindAsync<CompositeKey>(cancellationType, context, [88, "Rabbit"]));
    // }
    //
    // [ConditionalTheory]
    // [InlineData((int)CancellationType.Right)]
    // [InlineData((int)CancellationType.Wrong)]
    // [InlineData((int)CancellationType.None)]
    // public virtual async Task Find_composite_key_from_store_async(CancellationType cancellationType)
    // {
    //     using var context = CreateContext();
    //     Assert.Equal("Olive", (await Finder.FindAsync<CompositeKey>(cancellationType, context, [77, "Dog"])).Foo);
    // }
    //
    // [ConditionalTheory]
    // [InlineData((int)CancellationType.Right)]
    // [InlineData((int)CancellationType.Wrong)]
    // [InlineData((int)CancellationType.None)]
    // public virtual async Task Returns_null_for_composite_key_not_in_store_async(CancellationType cancellationType)
    // {
    //     using var context = CreateContext();
    //     Assert.Null(await Finder.FindAsync<CompositeKey>(cancellationType, context, [77, "Fox"]));
    // }
    //
    // [ConditionalTheory]
    // [InlineData((int)CancellationType.Right)]
    // [InlineData((int)CancellationType.Wrong)]
    // [InlineData((int)CancellationType.None)]
    // public virtual async Task Find_base_type_tracked_async(CancellationType cancellationType)
    // {
    //     using var context = CreateContext();
    //     var entity = context.Attach(
    //         new BaseType { Id = 88 }).Entity;
    //
    //     Assert.Same(entity, await Finder.FindAsync<BaseType>(cancellationType, context, [88]));
    // }
    //
    // [ConditionalTheory]
    // [InlineData((int)CancellationType.Right)]
    // [InlineData((int)CancellationType.Wrong)]
    // [InlineData((int)CancellationType.None)]
    // public virtual async Task Find_base_type_from_store_async(CancellationType cancellationType)
    // {
    //     using var context = CreateContext();
    //     Assert.Equal("Baxter", (await Finder.FindAsync<BaseType>(cancellationType, context, [77])).Foo);
    // }
    //
    // [ConditionalTheory]
    // [InlineData((int)CancellationType.Right)]
    // [InlineData((int)CancellationType.Wrong)]
    // [InlineData((int)CancellationType.None)]
    // public virtual async Task Returns_null_for_base_type_not_in_store_async(CancellationType cancellationType)
    // {
    //     using var context = CreateContext();
    //     Assert.Null(await Finder.FindAsync<BaseType>(cancellationType, context, [99]));
    // }
    //
    // [ConditionalTheory]
    // [InlineData((int)CancellationType.Right)]
    // [InlineData((int)CancellationType.Wrong)]
    // [InlineData((int)CancellationType.None)]
    // public virtual async Task Find_derived_type_tracked_async(CancellationType cancellationType)
    // {
    //     using var context = CreateContext();
    //     var entity = context.Attach(
    //         new DerivedType { Id = 88 }).Entity;
    //
    //     Assert.Same(entity, await Finder.FindAsync<DerivedType>(cancellationType, context, [88]));
    // }
    //
    // [ConditionalTheory]
    // [InlineData((int)CancellationType.Right)]
    // [InlineData((int)CancellationType.Wrong)]
    // [InlineData((int)CancellationType.None)]
    // public virtual async Task Find_derived_type_from_store_async(CancellationType cancellationType)
    // {
    //     using var context = CreateContext();
    //     var derivedType = await Finder.FindAsync<DerivedType>(cancellationType, context, [78]);
    //     Assert.Equal("Strawberry", derivedType.Foo);
    //     Assert.Equal("Cheesecake", derivedType.Boo);
    // }
    //
    // [ConditionalTheory]
    // [InlineData((int)CancellationType.Right)]
    // [InlineData((int)CancellationType.Wrong)]
    // [InlineData((int)CancellationType.None)]
    // public virtual async Task Returns_null_for_derived_type_not_in_store_async(CancellationType cancellationType)
    // {
    //     using var context = CreateContext();
    //     Assert.Null(await Finder.FindAsync<DerivedType>(cancellationType, context, [99]));
    // }
    //
    // [ConditionalTheory]
    // [InlineData((int)CancellationType.Right)]
    // [InlineData((int)CancellationType.Wrong)]
    // [InlineData((int)CancellationType.None)]
    // public virtual async Task Find_base_type_using_derived_set_tracked_async(CancellationType cancellationType)
    // {
    //     using var context = CreateContext();
    //     context.Attach(
    //         new BaseType { Id = 88 });
    //
    //     Assert.Null(await Finder.FindAsync<DerivedType>(cancellationType, context, [88]));
    // }
    //
    // [ConditionalTheory]
    // [InlineData((int)CancellationType.Right)]
    // [InlineData((int)CancellationType.Wrong)]
    // [InlineData((int)CancellationType.None)]
    // public virtual async Task Find_base_type_using_derived_set_from_store_async(CancellationType cancellationType)
    // {
    //     using var context = CreateContext();
    //     Assert.Null(await Finder.FindAsync<DerivedType>(cancellationType, context, [77]));
    // }
    //
    // [ConditionalTheory]
    // [InlineData((int)CancellationType.Right)]
    // [InlineData((int)CancellationType.Wrong)]
    // [InlineData((int)CancellationType.None)]
    // public virtual async Task Find_derived_type_using_base_set_tracked_async(CancellationType cancellationType)
    // {
    //     using var context = CreateContext();
    //     var entity = context.Attach(
    //         new DerivedType { Id = 88 }).Entity;
    //
    //     Assert.Same(entity, await Finder.FindAsync<BaseType>(cancellationType, context, [88]));
    // }
    //
    // [ConditionalTheory]
    // [InlineData((int)CancellationType.Right)]
    // [InlineData((int)CancellationType.Wrong)]
    // [InlineData((int)CancellationType.None)]
    // public virtual async Task Find_derived_using_base_set_type_from_store_async(CancellationType cancellationType)
    // {
    //     using var context = CreateContext();
    //     var derivedType = await Finder.FindAsync<BaseType>(cancellationType, context, [78]);
    //     Assert.Equal("Strawberry", derivedType.Foo);
    //     Assert.Equal("Cheesecake", ((DerivedType)derivedType).Boo);
    // }
    //
    // [ConditionalTheory]
    // [InlineData((int)CancellationType.Right)]
    // [InlineData((int)CancellationType.Wrong)]
    // [InlineData((int)CancellationType.None)]
    // public virtual async Task Find_shadow_key_tracked_async(CancellationType cancellationType)
    // {
    //     using var context = CreateContext();
    //     var entry = context.Entry(new ShadowKey());
    //     entry.Property("Id").CurrentValue = 88;
    //     entry.State = EntityState.Unchanged;
    //
    //     Assert.Same(entry.Entity, await Finder.FindAsync<ShadowKey>(cancellationType, context, [88]));
    // }
    //
    // [ConditionalTheory]
    // [InlineData((int)CancellationType.Right)]
    // [InlineData((int)CancellationType.Wrong)]
    // [InlineData((int)CancellationType.None)]
    // public virtual async Task Find_shadow_key_from_store_async(CancellationType cancellationType)
    // {
    //     using var context = CreateContext();
    //     Assert.Equal("Clippy", (await Finder.FindAsync<ShadowKey>(cancellationType, context, [77])).Foo);
    // }
    //
    // [ConditionalTheory]
    // [InlineData((int)CancellationType.Right)]
    // [InlineData((int)CancellationType.Wrong)]
    // [InlineData((int)CancellationType.None)]
    // public virtual async Task Returns_null_for_shadow_key_not_in_store_async(CancellationType cancellationType)
    // {
    //     using var context = CreateContext();
    //     Assert.Null(await Finder.FindAsync<ShadowKey>(cancellationType, context, [99]));
    // }
    //
    // [ConditionalTheory]
    // [InlineData((int)CancellationType.Right)]
    // [InlineData((int)CancellationType.Wrong)]
    // [InlineData((int)CancellationType.None)]
    // public virtual async Task Returns_null_for_null_key_values_array_async(CancellationType cancellationType)
    // {
    //     using var context = CreateContext();
    //     Assert.Null(await Finder.FindAsync<CompositeKey>(cancellationType, context, null));
    // }
    //
    // [ConditionalTheory]
    // [InlineData((int)CancellationType.Right)]
    // [InlineData((int)CancellationType.Wrong)]
    // [InlineData((int)CancellationType.None)]
    // public virtual async Task Returns_null_for_null_key_async(CancellationType cancellationType)
    // {
    //     using var context = CreateContext();
    //     Assert.Null(await Finder.FindAsync<IntKey>(cancellationType, context, [null]));
    // }
    //
    // [ConditionalTheory]
    // [InlineData((int)CancellationType.Right)]
    // [InlineData((int)CancellationType.Wrong)]
    // [InlineData((int)CancellationType.None)]
    // public virtual async Task Returns_null_for_null_in_composite_key_async(CancellationType cancellationType)
    // {
    //     using var context = CreateContext();
    //     Assert.Null(await Finder.FindAsync<CompositeKey>(cancellationType, context, [77, null]));
    // }
    //
    // [ConditionalTheory]
    // [InlineData((int)CancellationType.Right)]
    // [InlineData((int)CancellationType.Wrong)]
    // [InlineData((int)CancellationType.None)]
    // public virtual async Task Throws_for_multiple_values_passed_for_simple_key_async(CancellationType cancellationType)
    // {
    //     using var context = CreateContext();
    //     Assert.Equal(
    //         CoreStrings.FindNotCompositeKey("IntKey", cancellationType == CancellationType.Wrong ? 3 : 2),
    //         (await Assert.ThrowsAsync<ArgumentException>(
    //             () => Finder.FindAsync<IntKey>(cancellationType, context, [77, 88]).AsTask())).Message);
    // }
    //
    // [ConditionalTheory]
    // [InlineData((int)CancellationType.Right)]
    // [InlineData((int)CancellationType.Wrong)]
    // [InlineData((int)CancellationType.None)]
    // public virtual async Task Throws_for_wrong_number_of_values_for_composite_key_async(CancellationType cancellationType)
    // {
    //     using var context = CreateContext();
    //     Assert.Equal(
    //         cancellationType == CancellationType.Wrong
    //             ? CoreStrings.FindValueTypeMismatch(1, "CompositeKey", "CancellationToken", "string")
    //             : CoreStrings.FindValueCountMismatch("CompositeKey", 2, 1),
    //         (await Assert.ThrowsAsync<ArgumentException>(
    //             () => Finder.FindAsync<CompositeKey>(cancellationType, context, [77]).AsTask())).Message);
    // }
    //
    // [ConditionalTheory]
    // [InlineData((int)CancellationType.Right)]
    // [InlineData((int)CancellationType.Wrong)]
    // [InlineData((int)CancellationType.None)]
    // public virtual async Task Throws_for_bad_type_for_simple_key_async(CancellationType cancellationType)
    // {
    //     using var context = CreateContext();
    //     Assert.Equal(
    //         CoreStrings.FindValueTypeMismatch(0, "IntKey", "string", "int"),
    //         (await Assert.ThrowsAsync<ArgumentException>(
    //             () => Finder.FindAsync<IntKey>(cancellationType, context, ["77"]).AsTask())).Message);
    // }
    //
    // [ConditionalTheory]
    // [InlineData((int)CancellationType.Right)]
    // [InlineData((int)CancellationType.Wrong)]
    // [InlineData((int)CancellationType.None)]
    // public virtual async Task Throws_for_bad_type_for_composite_key_async(CancellationType cancellationType)
    // {
    //     using var context = CreateContext();
    //     Assert.Equal(
    //         CoreStrings.FindValueTypeMismatch(1, "CompositeKey", "int", "string"),
    //         (await Assert.ThrowsAsync<ArgumentException>(
    //             () => Finder.FindAsync<CompositeKey>(cancellationType, context, [77, 78]).AsTask())).Message);
    // }
    //
    // [ConditionalTheory]
    // [InlineData((int)CancellationType.Right)]
    // [InlineData((int)CancellationType.Wrong)]
    // [InlineData((int)CancellationType.None)]
    // public virtual async Task Throws_for_bad_entity_type_async(CancellationType cancellationType)
    // {
    //     using var context = CreateContext();
    //     Assert.Equal(
    //         CoreStrings.InvalidSetType(nameof(Random)),
    //         (await Assert.ThrowsAsync<InvalidOperationException>(
    //             () => Finder.FindAsync<Random>(cancellationType, context, [77]).AsTask())).Message);
    // }
    //
    // [ConditionalTheory]
    // [InlineData((int)CancellationType.Right)]
    // [InlineData((int)CancellationType.Wrong)]
    // [InlineData((int)CancellationType.None)]
    // public virtual async Task Throws_for_bad_entity_type_with_different_namespace_async(CancellationType cancellationType)
    // {
    //     using var context = CreateContext();
    //
    //     Assert.Equal(
    //         CoreStrings.InvalidSetSameTypeWithDifferentNamespace(
    //             typeof(DifferentNamespace.ShadowKey).DisplayName(), typeof(ShadowKey).DisplayName()),
    //         (await Assert.ThrowsAsync<InvalidOperationException>(
    //             () => Finder.FindAsync<DifferentNamespace.ShadowKey>(cancellationType, context, [77]).AsTask()))
    //         .Message);
    // }

    public enum CancellationType
    {
        Right,
        Wrong,
        None
    }

    protected class BaseType
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        public string? Foo { get; set; }
    }

    protected class DerivedType : BaseType
    {
        public string? Boo { get; set; }
    }

    protected class IntKey
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        public string? Foo { get; set; }

        public Owned1 OwnedReference { get; set; } = null!;
        public List<Owned1> OwnedCollection { get; set; } = null!;
    }

    protected class NullableIntKey
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int? Id { get; set; }

        public string? Foo { get; set; }
    }

    protected class StringKey
    {
        public string Id { get; set; } = null!;

        public string? Foo { get; set; }
    }

    protected class CompositeKey
    {
        public int Id1 { get; set; }
        public string Id2 { get; set; } = null!;
        public string? Foo { get; set; }
    }

    protected class ShadowKey
    {
        public string? Foo { get; set; }
    }

    [Owned]
    protected class Owned1
    {
        public int Prop { get; set; }
        public Owned2 NestedOwned { get; set; } = null!;
        public List<Owned2> NestedOwnedCollection { get; set; } = null!;
    }

    [Owned]
    protected class Owned2
    {
        public string Prop { get; set; } = null!;
    }

    protected DbContext CreateContext()
        => Fixture.CreateContext();

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    private static IQueryable<IntKey> ApplyTrackingBehavior(IQueryable<IntKey> query, QueryTrackingBehavior trackingBehavior)
    {
        query = trackingBehavior switch
        {
            QueryTrackingBehavior.TrackAll => query,
            QueryTrackingBehavior.NoTracking => query.AsNoTracking(),
            QueryTrackingBehavior.NoTrackingWithIdentityResolution => query.AsNoTrackingWithIdentityResolution(),
            _ => throw new ArgumentOutOfRangeException(nameof(trackingBehavior), trackingBehavior, null)
        };
        return query;
    }

    public class ReadItemFixture : SharedStoreFixtureBase<PoolableDbContext>
    {
        protected override string StoreName
            => "ReadItemTest";

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            modelBuilder.Entity<IntKey>();
            modelBuilder.Entity<NullableIntKey>();
            modelBuilder.Entity<StringKey>();
            modelBuilder.Entity<CompositeKey>().HasKey(
                e => new { e.Id1, e.Id2 });
            modelBuilder.Entity<BaseType>();
            modelBuilder.Entity<DerivedType>();
            modelBuilder.Entity<ShadowKey>().Property(typeof(int), "Id").ValueGeneratedNever();
        }

        protected override Task SeedAsync(PoolableDbContext context)
        {
            context.AddRange(
                new IntKey
                {
                    Id = 77,
                    Foo = "Smokey",
                    OwnedReference = new()
                    {
                        Prop = 7,
                        NestedOwned = new() { Prop = "7" },
                        NestedOwnedCollection = new() { new() { Prop = "71" }, new() { Prop = "72" } }
                    },
                    OwnedCollection = new() { new() { Prop = 71 }, new() { Prop = 72 } }
                },
                new NullableIntKey { Id = 77, Foo = "Smokey" },
                new StringKey { Id = "Cat", Foo = "Alice" },
                new CompositeKey
                {
                    Id1 = 77,
                    Id2 = "Dog",
                    Foo = "Olive"
                },
                new BaseType { Id = 77, Foo = "Baxter" },
                new DerivedType
                {
                    Id = 78,
                    Foo = "Strawberry",
                    Boo = "Cheesecake"
                });

            var entry = context.Entry(
                new ShadowKey { Foo = "Clippy" });
            entry.Property("Id").CurrentValue = 77;
            entry.State = EntityState.Added;

            return context.SaveChangesAsync();
        }

        public TestSqlLoggerFactory TestSqlLoggerFactory
            => (TestSqlLoggerFactory)ListLoggerFactory;

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            => base.AddOptions(builder).ConfigureWarnings(w => w.Ignore(CosmosEventId.NoPartitionKeyDefined));

        protected override ITestStoreFactory TestStoreFactory
            => CosmosTestStoreFactory.Instance;
    }
}
