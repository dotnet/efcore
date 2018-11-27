// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Design
{
    /// <summary>
    ///     <para>
    ///         Base class to be used by database providers when implementing an <see cref="IAnnotationCodeGenerator" />
    ///     </para>
    ///     <para>
    ///         This implementation returns <c>false</c> for all 'IsHandledByConvention' methods and
    ///         <c>null</c> for all 'GenerateFluentApi' methods. Providers should override for the
    ///         annotations that they understand.
    ///     </para>
    /// </summary>
    public class AnnotationCodeGenerator : IAnnotationCodeGenerator
    {
        /// <summary>
        ///     Initializes a new instance of this class.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this service. </param>
        public AnnotationCodeGenerator([NotNull] AnnotationCodeGeneratorDependencies dependencies)
        {
            Check.NotNull(dependencies, nameof(dependencies));

            Dependencies = dependencies;
        }

        /// <summary>
        ///     Parameter object containing dependencies for this service.
        /// </summary>
        protected virtual AnnotationCodeGeneratorDependencies Dependencies { get; }

        /// <summary>
        ///     Returns <c>false</c> unless overridden to do otherwise.
        /// </summary>
        /// <param name="model"> The <see cref="IModel" />. </param>
        /// <param name="annotation"> The <see cref="IAnnotation" />. </param>
        /// <returns>
        ///     <c>False.</c>
        /// </returns>
        public virtual bool IsHandledByConvention(IModel model, IAnnotation annotation)
        {
            Check.NotNull(model, nameof(model));
            Check.NotNull(annotation, nameof(annotation));

            return false;
        }

        /// <summary>
        ///     Returns <c>false</c> unless overridden to do otherwise.
        /// </summary>
        /// <param name="entityType"> The <see cref="IEntityType" />. </param>
        /// <param name="annotation"> The <see cref="IAnnotation" />. </param>
        /// <returns>
        ///     <c>False.</c>
        /// </returns>
        public virtual bool IsHandledByConvention(IEntityType entityType, IAnnotation annotation)
        {
            Check.NotNull(entityType, nameof(entityType));
            Check.NotNull(annotation, nameof(annotation));

            return false;
        }

        /// <summary>
        ///     Returns <c>false</c> unless overridden to do otherwise.
        /// </summary>
        /// <param name="key"> The <see cref="IKey" />. </param>
        /// <param name="annotation"> The <see cref="IAnnotation" />. </param>
        /// <returns>
        ///     <c>False.</c>
        /// </returns>
        public virtual bool IsHandledByConvention(IKey key, IAnnotation annotation)
        {
            Check.NotNull(key, nameof(key));
            Check.NotNull(annotation, nameof(annotation));

            return false;
        }

        /// <summary>
        ///     Returns <c>false</c> unless overridden to do otherwise.
        /// </summary>
        /// <param name="property"> The <see cref="IProperty" />. </param>
        /// <param name="annotation"> The <see cref="IAnnotation" />. </param>
        /// <returns>
        ///     <c>False.</c>
        /// </returns>
        public virtual bool IsHandledByConvention(IProperty property, IAnnotation annotation)
        {
            Check.NotNull(property, nameof(property));
            Check.NotNull(annotation, nameof(annotation));

            return false;
        }

        /// <summary>
        ///     Returns <c>false</c> unless overridden to do otherwise.
        /// </summary>
        /// <param name="foreignKey"> The <see cref="IForeignKey" />. </param>
        /// <param name="annotation"> The <see cref="IAnnotation" />. </param>
        /// <returns>
        ///     <c>False.</c>
        /// </returns>
        public virtual bool IsHandledByConvention(IForeignKey foreignKey, IAnnotation annotation)
        {
            Check.NotNull(foreignKey, nameof(foreignKey));
            Check.NotNull(annotation, nameof(annotation));

            return false;
        }

        /// <summary>
        ///     Returns <c>false</c> unless overridden to do otherwise.
        /// </summary>
        /// <param name="index"> The <see cref="IIndex" />. </param>
        /// <param name="annotation"> The <see cref="IAnnotation" />. </param>
        /// <returns>
        ///     <c>False.</c>
        /// </returns>
        public virtual bool IsHandledByConvention(IIndex index, IAnnotation annotation)
        {
            Check.NotNull(index, nameof(index));
            Check.NotNull(annotation, nameof(annotation));

            return false;
        }

        /// <summary>
        ///     Returns <c>null</c> unless overridden to do otherwise.
        /// </summary>
        /// <param name="model"> The <see cref="IModel" />. </param>
        /// <param name="annotation"> The <see cref="IAnnotation" />. </param>
        /// <returns>
        ///     <c>Null.</c>
        /// </returns>
        public virtual MethodCallCodeFragment GenerateFluentApi(IModel model, IAnnotation annotation)
        {
            Check.NotNull(model, nameof(model));
            Check.NotNull(annotation, nameof(annotation));

            return null;
        }

        /// <summary>
        ///     Returns <c>null</c> unless overridden to do otherwise.
        /// </summary>
        /// <param name="entityType"> The <see cref="IEntityType" />. </param>
        /// <param name="annotation"> The <see cref="IAnnotation" />. </param>
        /// <returns>
        ///     <c>Null.</c>
        /// </returns>
        public virtual MethodCallCodeFragment GenerateFluentApi(IEntityType entityType, IAnnotation annotation)
        {
            Check.NotNull(entityType, nameof(entityType));
            Check.NotNull(annotation, nameof(annotation));

            return null;
        }

        /// <summary>
        ///     Returns <c>null</c> unless overridden to do otherwise.
        /// </summary>
        /// <param name="key"> The <see cref="IKey" />. </param>
        /// <param name="annotation"> The <see cref="IAnnotation" />. </param>
        /// <returns>
        ///     <c>Null.</c>
        /// </returns>
        public virtual MethodCallCodeFragment GenerateFluentApi(IKey key, IAnnotation annotation)
        {
            Check.NotNull(key, nameof(key));
            Check.NotNull(annotation, nameof(annotation));

            return null;
        }

        /// <summary>
        ///     Returns <c>null</c> unless overridden to do otherwise.
        /// </summary>
        /// <param name="property"> The <see cref="IProperty" />. </param>
        /// <param name="annotation"> The <see cref="IAnnotation" />. </param>
        /// <returns>
        ///     <c>Null.</c>
        /// </returns>
        public virtual MethodCallCodeFragment GenerateFluentApi(IProperty property, IAnnotation annotation)
        {
            Check.NotNull(property, nameof(property));
            Check.NotNull(annotation, nameof(annotation));

            return null;
        }

        /// <summary>
        ///     Returns <c>null</c> unless overridden to do otherwise.
        /// </summary>
        /// <param name="foreignKey"> The <see cref="IForeignKey" />. </param>
        /// <param name="annotation"> The <see cref="IAnnotation" />. </param>
        /// <returns>
        ///     <c>Null.</c>
        /// </returns>
        public virtual MethodCallCodeFragment GenerateFluentApi(IForeignKey foreignKey, IAnnotation annotation)
        {
            Check.NotNull(foreignKey, nameof(foreignKey));
            Check.NotNull(annotation, nameof(annotation));

            return null;
        }

        /// <summary>
        ///     Returns <c>null</c> unless overridden to do otherwise.
        /// </summary>
        /// <param name="index"> The <see cref="IIndex" />. </param>
        /// <param name="annotation"> The <see cref="IAnnotation" />. </param>
        /// <returns>
        ///     <c>Null.</c>
        /// </returns>
        public virtual MethodCallCodeFragment GenerateFluentApi(IIndex index, IAnnotation annotation)
        {
            Check.NotNull(index, nameof(index));
            Check.NotNull(annotation, nameof(annotation));

            return null;
        }
    }
}
