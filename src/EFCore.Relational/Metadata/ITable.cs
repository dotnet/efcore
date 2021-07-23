// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     Represents a table in the database.
    /// </summary>
    public interface ITable : ITableBase
    {
        /// <summary>
        ///     Gets the entity type mappings.
        /// </summary>
        new IEnumerable<ITableMapping> EntityTypeMappings { get; }

        /// <summary>
        ///     Gets the columns defined for this table.
        /// </summary>
        new IEnumerable<IColumn> Columns { get; }

        /// <summary>
        ///     Gets the value indicating whether the table should be managed by migrations
        /// </summary>
        bool IsExcludedFromMigrations { get; }

        /// <summary>
        ///     Gets the foreing key constraints for this table.
        /// </summary>
        IEnumerable<IForeignKeyConstraint> ForeignKeyConstraints { get; }

        /// <summary>
        ///     Gets the unique constraints including the primary key for this table.
        /// </summary>
        IEnumerable<IUniqueConstraint> UniqueConstraints { get; }

        /// <summary>
        ///     Gets the primary key for this table.
        /// </summary>
        IPrimaryKeyConstraint? PrimaryKey { get; }

        /// <summary>
        ///     Gets the indexes for this table.
        /// </summary>
        IEnumerable<ITableIndex> Indexes { get; }

        /// <summary>
        ///     Gets the check constraints for this table.
        /// </summary>
        IEnumerable<ICheckConstraint> CheckConstraints
            => EntityTypeMappings.SelectMany(m => m.EntityType.GetDeclaredCheckConstraints())
                .Distinct((x, y) => x!.Name == y!.Name);

        /// <summary>
        ///     Gets the comment for this table.
        /// </summary>
        public virtual string? Comment
            => EntityTypeMappings.Select(e => e.EntityType.GetComment()).FirstOrDefault(c => c != null);

        /// <summary>
        ///     Gets the column with a given name. Returns <see langword="null" /> if no column with the given name is defined.
        /// </summary>
        new IColumn? FindColumn(string name);

        /// <summary>
        ///     Gets the column mapped to the given property. Returns <see langword="null" /> if no column is mapped to the given property.
        /// </summary>
        new IColumn? FindColumn(IProperty property);

        /// <summary>
        ///     <para>
        ///         Creates a human-readable representation of the given metadata.
        ///     </para>
        ///     <para>
        ///         Warning: Do not rely on the format of the returned string.
        ///         It is designed for debugging only and may change arbitrarily between releases.
        ///     </para>
        /// </summary>
        /// <param name="options"> Options for generating the string. </param>
        /// <param name="indent"> The number of indent spaces to use before each new line. </param>
        /// <returns> A human-readable representation. </returns>
        string ToDebugString(MetadataDebugStringOptions options = MetadataDebugStringOptions.ShortDefault, int indent = 0)
        {
            var builder = new StringBuilder();
            var indentString = new string(' ', indent);

            builder
                .Append(indentString)
                .Append("Table: ");

            if (Schema != null)
            {
                builder
                    .Append(Schema)
                    .Append('.');
            }

            builder.Append(Name);

            if (IsExcludedFromMigrations)
            {
                builder.Append(" ExcludedFromMigrations");
            }

            if (PrimaryKey == null)
            {
                builder.Append(" Keyless");
            }
            else
            {
                if ((options & MetadataDebugStringOptions.SingleLine) == 0)
                {
                    builder.AppendLine();
                }

                builder.Append(PrimaryKey.ToDebugString(options, indent + 2));
            }

            if ((options & MetadataDebugStringOptions.SingleLine) == 0 && Comment != null)
            {
                builder
                    .AppendLine()
                    .Append(indentString)
                    .AppendLine(" Comment:")
                    .Append(indentString)
                    .Append(Comment);
            }

            if ((options & MetadataDebugStringOptions.SingleLine) == 0)
            {
                var mappings = EntityTypeMappings.ToList();
                if (mappings.Count != 0)
                {
                    builder.AppendLine().Append(indentString).Append("  EntityTypeMappings: ");
                    foreach (var mapping in mappings)
                    {
                        builder.AppendLine().Append(mapping.ToDebugString(options, indent + 4));
                    }
                }

                var columns = Columns.ToList();
                if (columns.Count != 0)
                {
                    builder.AppendLine().Append(indentString).Append("  Columns: ");
                    foreach (var column in columns)
                    {
                        builder.AppendLine().Append(column.ToDebugString(options, indent + 4));
                    }
                }

                var foreignKeyConstraints = ForeignKeyConstraints.ToList();
                if (foreignKeyConstraints.Count != 0)
                {
                    builder.AppendLine().Append(indentString).Append("  ForeignKeyConstraints: ");
                    foreach (var foreignKeyConstraint in foreignKeyConstraints)
                    {
                        builder.AppendLine().Append(foreignKeyConstraint.ToDebugString(options, indent + 4));
                    }
                }

                var indexes = Indexes.ToList();
                if (indexes.Count != 0)
                {
                    builder.AppendLine().Append(indentString).Append("  Indexes: ");
                    foreach (var index in indexes)
                    {
                        builder.AppendLine().Append(index.ToDebugString(options, indent + 4));
                    }
                }

                var uniqueConstraints = UniqueConstraints.Where(uc => !uc.GetIsPrimaryKey()).ToList();
                if (uniqueConstraints.Count != 0)
                {
                    builder.AppendLine().Append(indentString).Append("  UniqueConstraints: ");
                    foreach (var uniqueConstraint in uniqueConstraints)
                    {
                        builder.AppendLine().Append(uniqueConstraint.ToDebugString(options, indent + 4));
                    }
                }

                var checkConstraints = CheckConstraints.ToList();
                if (checkConstraints.Count != 0)
                {
                    builder.AppendLine().Append(indentString).Append("  Check constraints: ");
                    foreach (var checkConstraint in checkConstraints)
                    {
                        builder.AppendLine().Append(checkConstraint.ToDebugString(options, indent + 4));
                    }
                }

                if ((options & MetadataDebugStringOptions.IncludeAnnotations) != 0)
                {
                    builder.Append(AnnotationsToDebugString(indent + 2));
                }
            }

            return builder.ToString();
        }
    }
}
