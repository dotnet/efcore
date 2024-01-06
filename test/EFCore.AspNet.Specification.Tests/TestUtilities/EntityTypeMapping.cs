// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestUtilities;

public class EntityTypeMapping
{
    public EntityTypeMapping()
    {
    }

    public EntityTypeMapping(IEntityType entityType)
    {
        Name = entityType.Name;
        TableName = entityType.GetTableName();
        PrimaryKey = entityType.FindPrimaryKey()!.ToDebugString(MetadataDebugStringOptions.SingleLineDefault);

        Properties.AddRange(
            entityType.GetProperties()
                .Select(p => p.ToDebugString(MetadataDebugStringOptions.SingleLineDefault)));

        Indexes.AddRange(
            entityType.GetIndexes().Select(i => $"{i.Properties.Format()} {(i.IsUnique ? "Unique" : "")}"));

        FKs.AddRange(
            entityType.GetForeignKeys().Select(f => f.ToDebugString(MetadataDebugStringOptions.SingleLineDefault)));

        Navigations.AddRange(
            entityType.GetNavigations().Select(n => n.ToDebugString(MetadataDebugStringOptions.SingleLineDefault)));

        SkipNavigations.AddRange(
            entityType.GetSkipNavigations().Select(n => n.ToDebugString(MetadataDebugStringOptions.SingleLineDefault)));
    }

    public string Name { get; set; }
    public string TableName { get; set; }
    public string PrimaryKey { get; set; }
    public List<string> Properties { get; } = [];
    public List<string> Indexes { get; } = [];
    public List<string> FKs { get; } = [];
    public List<string> Navigations { get; } = [];
    public List<string> SkipNavigations { get; } = [];

    public override string ToString()
    {
        var builder = new StringBuilder();

        builder.AppendLine("new()");
        builder.AppendLine("{");

        builder.AppendLine($"    Name = \"{Name}\",");
        builder.AppendLine($"    TableName = \"{TableName}\",");
        builder.AppendLine($"    PrimaryKey = \"{PrimaryKey}\",");

        builder.AppendLine("    Properties =");
        builder.AppendLine("    {");
        foreach (var property in Properties)
        {
            builder.AppendLine($"        \"{property}\",");
        }

        builder.AppendLine("    },");

        if (Indexes.Any())
        {
            builder.AppendLine("    Indexes =");
            builder.AppendLine("    {");
            foreach (var index in Indexes)
            {
                builder.AppendLine($"        \"{index}\",");
            }

            builder.AppendLine("    },");
        }

        if (FKs.Any())
        {
            builder.AppendLine("    FKs =");
            builder.AppendLine("    {");
            foreach (var fk in FKs)
            {
                builder.AppendLine($"        \"{fk}\",");
            }

            builder.AppendLine("    },");
        }

        if (Navigations.Any())
        {
            builder.AppendLine("    Navigations =");
            builder.AppendLine("    {");
            foreach (var navigation in Navigations)
            {
                builder.AppendLine($"        \"{navigation}\",");
            }

            builder.AppendLine("    },");
        }

        builder.AppendLine("},");

        return builder.ToString();
    }

    public static void AssertEqual(IReadOnlyList<EntityTypeMapping> expected, IReadOnlyList<EntityTypeMapping> actual)
    {
        Assert.Equal(expected.Count, actual.Count);
        for (var i = 0; i < expected.Count; i++)
        {
            var e = expected[i];
            var a = actual[i];

            Assert.Equal(e.Name, a.Name);
            Assert.Equal(e.TableName, a.TableName);
            Assert.Equal(e.PrimaryKey, a.PrimaryKey);
            Assert.Equal(e.Properties, a.Properties);
            Assert.Equal(e.Indexes, a.Indexes);
            Assert.Equal(e.FKs, a.FKs);
            Assert.Equal(e.Navigations, a.Navigations);
            Assert.Equal(e.SkipNavigations, a.SkipNavigations);
        }
    }

    public static string Serialize(IEnumerable<EntityTypeMapping> mappings)
    {
        var builder = new StringBuilder();
        foreach (var mapping in mappings)
        {
            builder.Append(mapping);
        }

        return builder.ToString();
    }
}
