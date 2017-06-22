// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Design.Internal
{
    public class SqlServerAnnotationCodeGenerator : AnnotationCodeGenerator
    {
        public SqlServerAnnotationCodeGenerator([NotNull] AnnotationCodeGeneratorDependencies dependencies)
            : base(dependencies)
        {
        }

        public override bool IsHandledByConvention(IModel model, IAnnotation annotation)
        {
            Check.NotNull(model, nameof(model));
            Check.NotNull(annotation, nameof(annotation));

            if (annotation.Name == RelationalAnnotationNames.DefaultSchema
                && string.Equals("dbo", (string)annotation.Value))
            {
                return true;
            }

            return false;
        }

        public override string GenerateFluentApi(IKey key, IAnnotation annotation, string language)
        {
            Check.NotNull(key, nameof(key));
            Check.NotNull(annotation, nameof(annotation));
            Check.NotNull(language, nameof(language));

            return annotation.Name == SqlServerAnnotationNames.Clustered && language == "CSharp"
                ? $".{nameof(SqlServerIndexBuilderExtensions.ForSqlServerIsClustered)}({((bool)annotation.Value == false ? "false" : "")})"
                : null;
        }

        public override string GenerateFluentApi(IIndex index, IAnnotation annotation, string language)
        {
            Check.NotNull(index, nameof(index));
            Check.NotNull(annotation, nameof(annotation));
            Check.NotNull(language, nameof(language));

            return annotation.Name == SqlServerAnnotationNames.Clustered && language == "CSharp"
                ? $".{nameof(SqlServerIndexBuilderExtensions.ForSqlServerIsClustered)}({((bool)annotation.Value == false ? "false" : "")})"
                : null;
        }
    }
}
