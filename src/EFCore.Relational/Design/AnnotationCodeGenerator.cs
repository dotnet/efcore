// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Design
{
    public class AnnotationCodeGenerator : IAnnotationCodeGenerator
    {
        public AnnotationCodeGenerator([NotNull] AnnotationCodeGeneratorDependencies dependencies)
        {
            Check.NotNull(dependencies, nameof(dependencies));

            Dependencies = dependencies;
        }

        protected virtual AnnotationCodeGeneratorDependencies Dependencies { get; }

        public virtual bool IsHandledByConvention(IModel model, IAnnotation annotation)
        {
            Check.NotNull(model, nameof(model));
            Check.NotNull(annotation, nameof(annotation));

            return false;
        }

        public virtual bool IsHandledByConvention(IEntityType entityType, IAnnotation annotation)
        {
            Check.NotNull(entityType, nameof(entityType));
            Check.NotNull(annotation, nameof(annotation));

            return false;
        }

        public virtual bool IsHandledByConvention(IKey key, IAnnotation annotation)
        {
            Check.NotNull(key, nameof(key));
            Check.NotNull(annotation, nameof(annotation));

            return false;
        }

        public virtual bool IsHandledByConvention(IProperty property, IAnnotation annotation)
        {
            Check.NotNull(property, nameof(property));
            Check.NotNull(annotation, nameof(annotation));

            return false;
        }

        public virtual bool IsHandledByConvention(IForeignKey foreignKey, IAnnotation annotation)
        {
            Check.NotNull(foreignKey, nameof(foreignKey));
            Check.NotNull(annotation, nameof(annotation));

            return false;
        }

        public virtual bool IsHandledByConvention(IIndex index, IAnnotation annotation)
        {
            Check.NotNull(index, nameof(index));
            Check.NotNull(annotation, nameof(annotation));

            return false;
        }

        public virtual string GenerateFluentApi(IModel model, IAnnotation annotation, string language)
        {
            Check.NotNull(model, nameof(model));
            Check.NotNull(annotation, nameof(annotation));
            Check.NotNull(language, nameof(language));

            return null;
        }

        public virtual string GenerateFluentApi(IEntityType entityType, IAnnotation annotation, string language)
        {
            Check.NotNull(entityType, nameof(entityType));
            Check.NotNull(annotation, nameof(annotation));
            Check.NotNull(language, nameof(language));

            return null;
        }

        public virtual string GenerateFluentApi(IKey key, IAnnotation annotation, string language)
        {
            Check.NotNull(key, nameof(key));
            Check.NotNull(annotation, nameof(annotation));
            Check.NotNull(language, nameof(language));

            return null;
        }

        public virtual string GenerateFluentApi(IProperty property, IAnnotation annotation, string language)
        {
            Check.NotNull(property, nameof(property));
            Check.NotNull(annotation, nameof(annotation));
            Check.NotNull(language, nameof(language));

            return null;
        }

        public virtual string GenerateFluentApi(IForeignKey foreignKey, IAnnotation annotation, string language)
        {
            Check.NotNull(foreignKey, nameof(foreignKey));
            Check.NotNull(annotation, nameof(annotation));
            Check.NotNull(language, nameof(language));

            return null;
        }

        public virtual string GenerateFluentApi(IIndex index, IAnnotation annotation, string language)
        {
            Check.NotNull(index, nameof(index));
            Check.NotNull(annotation, nameof(annotation));
            Check.NotNull(language, nameof(language));

            return null;
        }
    }
}
