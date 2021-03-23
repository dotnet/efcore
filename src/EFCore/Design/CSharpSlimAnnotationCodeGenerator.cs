// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Design
{
    /// <summary>
    ///     <para>
    ///         Base class to be used by database providers when implementing an <see cref="ICSharpSlimAnnotationCodeGenerator" />
    ///     </para>
    /// </summary>
    public class CSharpSlimAnnotationCodeGenerator : ICSharpSlimAnnotationCodeGenerator
    {
        /// <summary>
        ///     Initializes a new instance of this class.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this service. </param>
        public CSharpSlimAnnotationCodeGenerator(CSharpSlimAnnotationCodeGeneratorDependencies dependencies)
        {
            Check.NotNull(dependencies, nameof(dependencies));

            Dependencies = dependencies;
        }

        /// <summary>
        ///     Parameter object containing dependencies for this service.
        /// </summary>
        protected virtual CSharpSlimAnnotationCodeGeneratorDependencies Dependencies { get; }

        /// <inheritdoc />
        public virtual void Generate(IModel model, CSharpSlimAnnotationCodeGeneratorParameters parameters)
        {
            if (parameters.IsRuntime)
            {
                parameters.Annotations.Remove(CoreAnnotationNames.ModelDependencies);
                parameters.Annotations.Remove(CoreAnnotationNames.ReadOnlyModel);
            }
            else
            {
                parameters.Annotations.Remove(CoreAnnotationNames.OwnedTypes);
                parameters.Annotations.Remove(CoreAnnotationNames.PropertyAccessMode);
            }

            GenerateSimpleAnnotations(parameters);
        }

        /// <inheritdoc />
        public virtual void Generate(IEntityType entityType, CSharpSlimAnnotationCodeGeneratorParameters parameters)
        {
            var annotations = parameters.Annotations;
            if (!parameters.IsRuntime)
            {
                annotations.Remove(CoreAnnotationNames.PropertyAccessMode);
                annotations.Remove(CoreAnnotationNames.NavigationAccessMode);
                annotations.Remove(CoreAnnotationNames.DiscriminatorProperty);
            }

            GenerateSimpleAnnotations(parameters);
        }

        /// <inheritdoc />
        public virtual void Generate(IProperty property, CSharpSlimAnnotationCodeGeneratorParameters parameters)
        {
            if (!parameters.IsRuntime)
            {
                var annotations = parameters.Annotations;
                annotations.Remove(CoreAnnotationNames.PropertyAccessMode);
                annotations.Remove(CoreAnnotationNames.BeforeSaveBehavior);
                annotations.Remove(CoreAnnotationNames.AfterSaveBehavior);
                annotations.Remove(CoreAnnotationNames.MaxLength);
                annotations.Remove(CoreAnnotationNames.Unicode);
                annotations.Remove(CoreAnnotationNames.Precision);
                annotations.Remove(CoreAnnotationNames.Scale);
                annotations.Remove(CoreAnnotationNames.ProviderClrType);
                annotations.Remove(CoreAnnotationNames.ValueConverter);
                annotations.Remove(CoreAnnotationNames.ValueComparer);
            }

            GenerateSimpleAnnotations(parameters);
        }

        /// <inheritdoc />
        public virtual void Generate(IServiceProperty property, CSharpSlimAnnotationCodeGeneratorParameters parameters)
        {
            if (!parameters.IsRuntime)
            {
                var annotations = parameters.Annotations;
                annotations.Remove(CoreAnnotationNames.PropertyAccessMode);
            }

            GenerateSimpleAnnotations(parameters);
        }

        /// <inheritdoc />
        public virtual void Generate(IKey key, CSharpSlimAnnotationCodeGeneratorParameters parameters)
        {
            GenerateSimpleAnnotations(parameters);
        }

        /// <inheritdoc />
        public virtual void Generate(IForeignKey foreignKey, CSharpSlimAnnotationCodeGeneratorParameters parameters)
        {
            GenerateSimpleAnnotations(parameters);
        }

        /// <inheritdoc />
        public virtual void Generate(INavigation navigation, CSharpSlimAnnotationCodeGeneratorParameters parameters)
        {
            if (!parameters.IsRuntime)
            {
                var annotations = parameters.Annotations;
                annotations.Remove(CoreAnnotationNames.PropertyAccessMode);
                annotations.Remove(CoreAnnotationNames.EagerLoaded);
            }

            GenerateSimpleAnnotations(parameters);
        }

        /// <inheritdoc />
        public virtual void Generate(ISkipNavigation navigation, CSharpSlimAnnotationCodeGeneratorParameters parameters)
        {
            if (!parameters.IsRuntime)
            {
                var annotations = parameters.Annotations;
                annotations.Remove(CoreAnnotationNames.PropertyAccessMode);
                annotations.Remove(CoreAnnotationNames.EagerLoaded);
            }

            GenerateSimpleAnnotations(parameters);
        }

        /// <inheritdoc />
        public virtual void Generate(IIndex index, CSharpSlimAnnotationCodeGeneratorParameters parameters)
        {
            GenerateSimpleAnnotations(parameters);
        }

        private void GenerateSimpleAnnotations(CSharpSlimAnnotationCodeGeneratorParameters parameters)
        {
            foreach (var (name, value) in parameters.Annotations)
            {
                if (value != null)
                {
                    AddNamespace(value.GetType(), parameters.Namespaces);
                }

                GenerateSimpleAnnotation(name, Dependencies.CSharpHelper.UnknownLiteral(value), parameters);
            }
        }

        /// <summary>
        ///     Generates code to create the given annotation.
        /// </summary>
        /// <param name="annotationName"> The annotation name. </param>
        /// <param name="valueString"> The annotation value. </param>
        /// <param name="parameters"> Additional parameters used during code generation. </param>
        protected virtual void GenerateSimpleAnnotation(
            string annotationName,
            string valueString,
            CSharpSlimAnnotationCodeGeneratorParameters parameters)
        {
            parameters.MainBuilder
                .Append(parameters.TargetName)
                .Append(parameters.IsRuntime ? ".AddRuntimeAnnotation(" : ".AddAnnotation(")
                .Append(Dependencies.CSharpHelper.Literal(annotationName))
                .Append(", ")
                .Append(valueString)
                .AppendLine(");");
        }

        /// <summary>
        ///     Adds the namespaces for the given type.
        /// </summary>
        /// <param name="type"> A type. </param>
        /// <param name="namespaces"> The set of namespaces to add to. </param>
        protected virtual void AddNamespace(Type type, ISet<string> namespaces)
        {
            if (type.IsNested)
            {
                AddNamespace(type.DeclaringType!, namespaces);
            }

            if (type.Namespace != null)
            {
                namespaces.Add(type.Namespace);
            }

            if (type.IsGenericType)
            {
                foreach (var argument in type.GenericTypeArguments)
                {
                    AddNamespace(argument, namespaces);
                }
            }

            var sequenceType = type.TryGetSequenceType();
            if (sequenceType != null)
            {
                AddNamespace(sequenceType, namespaces);
            }
        }
    }
}
