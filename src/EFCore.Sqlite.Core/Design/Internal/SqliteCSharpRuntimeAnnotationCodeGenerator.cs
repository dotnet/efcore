// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Sqlite.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Sqlite.Design.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class SqliteCSharpRuntimeAnnotationCodeGenerator : RelationalCSharpRuntimeAnnotationCodeGenerator
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public SqliteCSharpRuntimeAnnotationCodeGenerator(
            CSharpRuntimeAnnotationCodeGeneratorDependencies dependencies,
            RelationalCSharpRuntimeAnnotationCodeGeneratorDependencies relationalDependencies)
            : base(dependencies, relationalDependencies)
        {
        }

        /// <inheritdoc />
        public override void Generate(IProperty property, CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
        {
            var annotations = parameters.Annotations;
            if (!parameters.IsRuntime)
            {
                annotations.Remove(SqliteAnnotationNames.Srid);
            }

            base.Generate(property, parameters);
        }
    }
}
