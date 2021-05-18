// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Design
{
    /// <summary>
    ///     <para>
    ///         Base class to be used by database providers when implementing an <see cref="ICSharpRuntimeAnnotationCodeGenerator" />
    ///     </para>
    /// </summary>
    public class CSharpRuntimeAnnotationCodeGenerator : ICSharpRuntimeAnnotationCodeGenerator
    {
        /// <summary>
        ///     Initializes a new instance of this class.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this service. </param>
        public CSharpRuntimeAnnotationCodeGenerator(CSharpRuntimeAnnotationCodeGeneratorDependencies dependencies)
        {
            Check.NotNull(dependencies, nameof(dependencies));

            Dependencies = dependencies;
        }

        /// <summary>
        ///     Parameter object containing dependencies for this service.
        /// </summary>
        protected virtual CSharpRuntimeAnnotationCodeGeneratorDependencies Dependencies { get; }

        /// <inheritdoc />
        public virtual void Generate(IModel model, CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
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
        public virtual void Generate(IEntityType entityType, CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
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
        public virtual void Generate(IProperty property, CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
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
                annotations.Remove(CoreAnnotationNames.ValueGeneratorFactory);
                annotations.Remove(CoreAnnotationNames.ValueGeneratorFactoryType);
                annotations.Remove(CoreAnnotationNames.ValueConverter);
                annotations.Remove(CoreAnnotationNames.ValueConverterType);
                annotations.Remove(CoreAnnotationNames.ValueComparer);
                annotations.Remove(CoreAnnotationNames.ValueComparerType);
            }

            GenerateSimpleAnnotations(parameters);
        }

        /// <inheritdoc />
        public virtual void Generate(IServiceProperty property, CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
        {
            if (!parameters.IsRuntime)
            {
                var annotations = parameters.Annotations;
                annotations.Remove(CoreAnnotationNames.PropertyAccessMode);
            }

            GenerateSimpleAnnotations(parameters);
        }

        /// <inheritdoc />
        public virtual void Generate(IKey key, CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
        {
            GenerateSimpleAnnotations(parameters);
        }

        /// <inheritdoc />
        public virtual void Generate(IForeignKey foreignKey, CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
        {
            GenerateSimpleAnnotations(parameters);
        }

        /// <inheritdoc />
        public virtual void Generate(INavigation navigation, CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
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
        public virtual void Generate(ISkipNavigation navigation, CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
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
        public virtual void Generate(IIndex index, CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
        {
            GenerateSimpleAnnotations(parameters);
        }

        /// <summary>
        ///     Generates code to create the given annotations using literals.
        /// </summary>
        /// <param name="parameters"> Parameters used during code generation. </param>
        protected virtual void GenerateSimpleAnnotations(CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
        {
            foreach (var (name, value) in parameters.Annotations.OrderBy(a => a.Key))
            {
                if (value != null)
                {
                    AddNamespace(value as Type ?? value.GetType(), parameters.Namespaces);
                }

                GenerateSimpleAnnotation(name, Dependencies.CSharpHelper.UnknownLiteral(value), parameters);
            }
        }

        /// <summary>
        ///     Generates code to create the given annotation.
        /// </summary>
        /// <param name="annotationName"> The annotation name. </param>
        /// <param name="valueString"> The annotation value as a literal. </param>
        /// <param name="parameters"> Additional parameters used during code generation. </param>
        protected virtual void GenerateSimpleAnnotation(
            string annotationName,
            string valueString,
            CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
        {
            if (parameters.TargetName != "this")
            {
                parameters.MainBuilder
                    .Append(parameters.TargetName)
                    .Append('.');
            }

            parameters.MainBuilder
                .Append(parameters.IsRuntime ? "AddRuntimeAnnotation(" : "AddAnnotation(")
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
