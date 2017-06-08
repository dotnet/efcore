// Copyright ([NotNull] c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, [NotNull] Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.Design
{
    public interface IAnnotationCodeGenerator
    {
        bool IsHandledByConvention([NotNull] IModel model, [NotNull] IAnnotation annotation);
        bool IsHandledByConvention([NotNull] IEntityType entityType, [NotNull] IAnnotation annotation);
        bool IsHandledByConvention([NotNull] IKey key, [NotNull] IAnnotation annotation);
        bool IsHandledByConvention([NotNull] IProperty property, [NotNull] IAnnotation annotation);
        bool IsHandledByConvention([NotNull] IForeignKey foreignKey, [NotNull] IAnnotation annotation);
        bool IsHandledByConvention([NotNull] IIndex index, [NotNull] IAnnotation annotation);

        string GenerateFluentApi([NotNull] IModel model, [NotNull] IAnnotation annotation, [NotNull] string language);
        string GenerateFluentApi([NotNull] IEntityType entityType, [NotNull] IAnnotation annotation, [NotNull] string language);
        string GenerateFluentApi([NotNull] IKey key, [NotNull] IAnnotation annotation, [NotNull] string language);
        string GenerateFluentApi([NotNull] IProperty property, [NotNull] IAnnotation annotation, [NotNull] string language);
        string GenerateFluentApi([NotNull] IForeignKey foreignKey, [NotNull] IAnnotation annotation, [NotNull] string language);
        string GenerateFluentApi([NotNull] IIndex index, [NotNull] IAnnotation annotation, [NotNull] string language);
    }
}
