// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.EntityFrameworkCore.XuGu.Infrastructure.Internal;

namespace Microsoft.EntityFrameworkCore.XuGu.Storage.Internal
{
    public class XGSqlGenerationHelper : RelationalSqlGenerationHelper
    {
        private readonly IXGOptions _options;

        public XGSqlGenerationHelper(
            [NotNull] RelationalSqlGenerationHelperDependencies dependencies,
            IXGOptions options)
            : base(dependencies)
        {
            _options = options;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override string EscapeIdentifier(string identifier)
            => Check.NotEmpty(identifier, nameof(identifier)).Replace("`", "``");

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override void EscapeIdentifier(StringBuilder builder, string identifier)
        {
            Check.NotEmpty(identifier, nameof(identifier));

            var initialLength = builder.Length;
            builder.Append(identifier);
            builder.Replace("`", "``", initialLength, identifier.Length);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override string DelimitIdentifier(string identifier)
            => $"`{EscapeIdentifier(Check.NotEmpty(identifier, nameof(identifier)))}`"; // Interpolation okay; strings

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override void DelimitIdentifier(StringBuilder builder, string identifier)
        {
            Check.NotEmpty(identifier, nameof(identifier));
            builder.Append('`');
            EscapeIdentifier(builder, identifier);
            builder.Append('`');
        }

        /// <summary>
        ///     Generates the delimited SQL representation of an identifier (column name, table name, etc.).
        /// </summary>
        /// <param name="name">The identifier to delimit.</param>
        /// <param name="schema">The schema of the identifier.</param>
        /// <returns>
        ///     The generated string.
        /// </returns>
        public override string DelimitIdentifier(string name, string schema)
            => base.DelimitIdentifier(GetObjectName(name, schema), GetSchemaName(name, schema));

        /// <summary>
        ///     Writes the delimited SQL representation of an identifier (column name, table name, etc.).
        /// </summary>
        /// <param name="builder">The <see cref="StringBuilder" /> to write generated string to.</param>
        /// <param name="name">The identifier to delimit.</param>
        /// <param name="schema">The schema of the identifier.</param>
        public override void DelimitIdentifier(StringBuilder builder, string name, string schema)
            => base.DelimitIdentifier(builder, GetObjectName(name, schema), GetSchemaName(name, schema));

        public virtual string GetObjectName(string name, string schema)
            => !string.IsNullOrEmpty(schema) && _options.SchemaNameTranslator != null
                ? _options.SchemaNameTranslator(schema, name)
                : name;

        public virtual string GetSchemaName(string name, string schema) => null;
    }
}
