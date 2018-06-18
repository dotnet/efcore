// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.SqlServer.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.SqlServer.Design.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class SqlServerAnnotationCodeGenerator : AnnotationCodeGenerator
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public SqlServerAnnotationCodeGenerator([NotNull] AnnotationCodeGeneratorDependencies dependencies)
            : base(dependencies)
        {
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override bool IsHandledByConvention(IModel model, IAnnotation annotation)
        {
            Check.NotNull(model, nameof(model));
            Check.NotNull(annotation, nameof(annotation));

            if (annotation.Name == RelationalAnnotationNames.DefaultSchema)
            {
                return string.Equals("dbo", (string)annotation.Value);
            }

            return annotation.Name == SqlServerAnnotationNames.ValueGenerationStrategy
                ? (SqlServerValueGenerationStrategy)annotation.Value == SqlServerValueGenerationStrategy.IdentityColumn
                : false;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override MethodCallCodeFragment GenerateFluentApi(IKey key, IAnnotation annotation)
        {
            return annotation.Name == SqlServerAnnotationNames.Clustered
                ? (bool)annotation.Value == false
                    ? new MethodCallCodeFragment(nameof(SqlServerIndexBuilderExtensions.ForSqlServerIsClustered), false)
                    : new MethodCallCodeFragment(nameof(SqlServerIndexBuilderExtensions.ForSqlServerIsClustered))
                : null;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override MethodCallCodeFragment GenerateFluentApi(IIndex index, IAnnotation annotation)
        {
            return annotation.Name == SqlServerAnnotationNames.Clustered
                ? (bool)annotation.Value == false
                    ? new MethodCallCodeFragment(nameof(SqlServerIndexBuilderExtensions.ForSqlServerIsClustered), false)
                    : new MethodCallCodeFragment(nameof(SqlServerIndexBuilderExtensions.ForSqlServerIsClustered))
                : null;
        }
    }
}
