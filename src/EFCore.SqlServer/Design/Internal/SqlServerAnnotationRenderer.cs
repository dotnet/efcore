// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Design.Internal
{
    public class SqlServerAnnotationRenderer : AnnotationRendererBase
    {
        public override bool IsHandledByConvention(IModel model, IAnnotation annotation)
        {
            Check.NotNull(model, nameof(model));
            Check.NotNull(annotation, nameof(annotation));

            if (annotation.Name == RelationalAnnotationNames.DefaultSchema
                && string.Equals("dbo", (string)annotation.Value))
            {
                return true;
            }

            if (annotation.Name == SqlServerDatabaseModelAnnotationNames.TypeAliases)
            {
                return true;
            }

            return false;
        }

        public override bool IsHandledByConvention(IProperty property, IAnnotation annotation)
        {
            Check.NotNull(property, nameof(property));
            Check.NotNull(annotation, nameof(annotation));

            if (annotation.Name == SqlServerDatabaseModelAnnotationNames.DataTypeSchemaName)
            {
                return true;
            }

            if (annotation.Name == SqlServerDatabaseModelAnnotationNames.IsIdentity)
            {
                return true;
            }

            return false;
        }

        public override string GenerateFluentApi(IIndex index, IAnnotation annotation)
        {
            Check.NotNull(index, nameof(index));
            Check.NotNull(annotation, nameof(annotation));

            return annotation.Name == SqlServerAnnotationNames.Clustered
                ? $".{nameof(SqlServerIndexBuilderExtensions.ForSqlServerIsClustered)}({((bool)annotation.Value == false ? "false" : "")})"
                : null;
        }
    }
}
