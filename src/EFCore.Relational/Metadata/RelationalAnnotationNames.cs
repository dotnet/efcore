// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     Names for well-known relational model annotations. Applications should not use these names
    ///     directly, but should instead use the extension methods on metadata objects.
    /// </summary>
    public static class RelationalAnnotationNames
    {
        /// <summary>
        ///     The prefix used for all relational annotations.
        /// </summary>
        public const string Prefix = "Relational:";

        /// <summary>
        ///     The name for column name annotations.
        /// </summary>
        public const string ColumnName = Prefix + "ColumnName";

        /// <summary>
        ///     The name for column name annotations.
        /// </summary>
        public const string ViewColumnName = Prefix + "ViewColumnName";

        /// <summary>
        ///     The name for column type annotations.
        /// </summary>
        public const string ColumnType = Prefix + "ColumnType";

        /// <summary>
        ///     The name for default value SQL expression annotations.
        /// </summary>
        public const string DefaultValueSql = Prefix + "DefaultValueSql";

        /// <summary>
        ///     The name for computed value SQL expression annotations.
        /// </summary>
        public const string ComputedColumnSql = Prefix + "ComputedColumnSql";

        /// <summary>
        ///     The name for default value annotations.
        /// </summary>
        public const string DefaultValue = Prefix + "DefaultValue";

        /// <summary>
        ///     The name for table name annotations.
        /// </summary>
        public const string TableName = Prefix + "TableName";

        /// <summary>
        ///     The name for schema name annotations.
        /// </summary>
        public const string Schema = Prefix + "Schema";

        /// <summary>
        ///     The name for view name annotations.
        /// </summary>
        public const string ViewName = Prefix + "ViewName";

        /// <summary>
        ///     The name for view schema name annotations.
        /// </summary>
        public const string ViewSchema = Prefix + "ViewSchema";

        /// <summary>
        ///     The name for comment annotations.
        /// </summary>
        public const string Comment = Prefix + "Comment";

        /// <summary>
        ///     The name for default schema annotations.
        /// </summary>
        public const string DefaultSchema = Prefix + "DefaultSchema";

        /// <summary>
        ///     The name for constraint name annotations.
        /// </summary>
        public const string Name = Prefix + "Name";

        /// <summary>
        ///     The prefix for serialized sequence annotations.
        /// </summary>
        [Obsolete("All sequences are stored in a single annotation now")]
        public const string SequencePrefix = Prefix + "Sequence:";

        /// <summary>
        ///     The name for sequence annotation.
        /// </summary>
        public const string Sequences = Prefix + "Sequences";

        /// <summary>
        ///     The name for check constraint annotations.
        /// </summary>
        public const string CheckConstraints = Prefix + "CheckConstraints";

        /// <summary>
        ///     The name for filter annotations.
        /// </summary>
        public const string Filter = Prefix + "Filter";

        /// <summary>
        ///     The name for DbFunction annotation.
        /// </summary>
        [Obsolete("Use DbFunctions")]
        public const string DbFunction = Prefix + "DbFunction";

        /// <summary>
        ///     The name for functions annotation.
        /// </summary>
        public const string DbFunctions = Prefix + "DbFunctions";

        /// <summary>
        ///     The name for the annotation containing the maximum length for database identifiers.
        /// </summary>
        public const string MaxIdentifierLength = Prefix + "MaxIdentifierLength";

        /// <summary>
        ///     The name for the annotation containing a flag indicating whether the property is constrained to fixed length values.
        /// </summary>
        public const string IsFixedLength = Prefix + "IsFixedLength";

        /// <summary>
        ///     The name for the annotation containing the definition of a database view.
        /// </summary>
        public const string ViewDefinition = Prefix + "ViewDefinition";

        /// <summary>
        ///     The name for database model annotation.
        /// </summary>
        public const string RelationalModel = Prefix + "RelationalModel";

        /// <summary>
        ///     The name for table mappings annotations.
        /// </summary>
        public const string TableMappings = Prefix + "TableMappings";

        /// <summary>
        ///     The name for column mappings annotations.
        /// </summary>
        public const string TableColumnMappings = Prefix + "TableColumnMappings";

        /// <summary>
        ///     The name for table mappings annotations.
        /// </summary>
        public const string ViewMappings = Prefix + "ViewMappings";

        /// <summary>
        ///     The name for column mappings annotations.
        /// </summary>
        public const string ViewColumnMappings = Prefix + "ViewColumnMappings";

        /// <summary>
        ///     The name for foreign key mappings annotations.
        /// </summary>
        public const string ForeignKeyMappings = Prefix + "ForeignKeyMappings";

        /// <summary>
        ///     The name for table index mappings annotations.
        /// </summary>
        public const string TableIndexMappings = Prefix + "TableIndexMappings";

        /// <summary>
        ///     The name for unique constraint mappings annotations.
        /// </summary>
        public const string UniqueConstraintMappings = Prefix + "UniqueConstraintMappings";
    }
}
