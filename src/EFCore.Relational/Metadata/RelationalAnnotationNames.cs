// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     Names for well-known relational model annotations. Applications should not use these names
    ///     directly, but should instead use the extension methods on metadata objects.
    /// </summary>
    public static class RelationalAnnotationNames
    {
        /// <summary>
        ///     The prefix used for any relational annotation.
        /// </summary>
        public const string Prefix = "Relational:";

        /// <summary>
        ///     The name for column name annotations.
        /// </summary>
        public const string ColumnName = Prefix + "ColumnName";

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
        public const string SequencePrefix = Prefix + "Sequence:";

        /// <summary>
        ///     The name for check constraint annotations.
        /// </summary>
        public const string CheckConstraints = Prefix + "CheckConstraints";

        /// <summary>
        ///     The name for filter annotations.
        /// </summary>
        public const string Filter = Prefix + "Filter";

        /// <summary>
        ///     The name for DbFunction annotations.
        /// </summary>
        public const string DbFunction = Prefix + "DbFunction";

        /// <summary>
        ///     The maximum length for database identifiers.
        /// </summary>
        public const string MaxIdentifierLength = Prefix + "MaxIdentifierLength";

        /// <summary>
        ///     A flag indicating whether the property is constrained to fixed length values.
        /// </summary>
        public const string IsFixedLength = Prefix + "IsFixedLength";

        /// <summary>
        ///     The definition of a database view.
        /// </summary>
        public const string ViewDefinition = Prefix + "ViewDefinition";
    }
}
