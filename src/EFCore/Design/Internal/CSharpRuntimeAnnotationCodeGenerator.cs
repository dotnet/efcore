// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Design.Internal
{
    /// <summary>
    ///     Base class to be used by database providers when implementing an <see cref="ICSharpRuntimeAnnotationCodeGenerator" />
    /// </summary>
    /// <remarks>
    ///     The service lifetime is <see cref="ServiceLifetime.Singleton" />. This means a single instance
    ///     is used by many <see cref="DbContext" /> instances. The implementation must be thread-safe.
    ///     This service cannot depend on services registered as <see cref="ServiceLifetime.Scoped" />.
    /// </remarks>
    public class CSharpRuntimeAnnotationCodeGenerator : ICSharpRuntimeAnnotationCodeGenerator
    {
        /// <summary>
        ///     Initializes a new instance of this class.
        /// </summary>
        /// <param name="dependencies">Parameter object containing dependencies for this service.</param>
        public CSharpRuntimeAnnotationCodeGenerator(CSharpRuntimeAnnotationCodeGeneratorDependencies dependencies)
        {
            Check.NotNull(dependencies, nameof(dependencies));

            Dependencies = dependencies;
        }

        /// <summary>
        ///     Dependencies for this service.
        /// </summary>
        protected virtual CSharpRuntimeAnnotationCodeGeneratorDependencies Dependencies { get; }

        /// <inheritdoc />
        public virtual void Generate(IModel model, CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
        {
            var annotations = parameters.Annotations;
            if (!parameters.IsRuntime)
            {
                foreach (var annotation in annotations)
                {
                    if (CoreAnnotationNames.AllNames.Contains(annotation.Key)
                        && annotation.Key != CoreAnnotationNames.ProductVersion
                        && annotation.Key != CoreAnnotationNames.FullChangeTrackingNotificationsRequired)
                    {
                        annotations.Remove(annotation.Key);
                    }
                }
            }
            else
            {
                annotations.Remove(CoreAnnotationNames.ModelDependencies);
                annotations.Remove(CoreAnnotationNames.ReadOnlyModel);
            }

            GenerateSimpleAnnotations(parameters);
        }

        /// <inheritdoc />
        public virtual void Generate(IEntityType entityType, CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
        {
            var annotations = parameters.Annotations;
            if (!parameters.IsRuntime)
            {
                foreach (var annotation in annotations)
                {
                    if (CoreAnnotationNames.AllNames.Contains(annotation.Key)
                        && annotation.Key != CoreAnnotationNames.DiscriminatorMappingComplete)
                    {
                        annotations.Remove(annotation.Key);
                    }
                }
            }

            GenerateSimpleAnnotations(parameters);
        }

        /// <inheritdoc />
        public virtual void Generate(IProperty property, CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
        {
            if (!parameters.IsRuntime)
            {
                var annotations = parameters.Annotations;
                foreach (var annotation in annotations)
                {
                    if (CoreAnnotationNames.AllNames.Contains(annotation.Key))
                    {
                        annotations.Remove(annotation.Key);
                    }
                }
            }

            GenerateSimpleAnnotations(parameters);
        }

        /// <inheritdoc />
        public virtual void Generate(IServiceProperty property, CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
        {
            if (!parameters.IsRuntime)
            {
                var annotations = parameters.Annotations;
                foreach (var annotation in annotations)
                {
                    if (CoreAnnotationNames.AllNames.Contains(annotation.Key))
                    {
                        annotations.Remove(annotation.Key);
                    }
                }
            }

            GenerateSimpleAnnotations(parameters);
        }

        /// <inheritdoc />
        public virtual void Generate(IKey key, CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
        {
            if (!parameters.IsRuntime)
            {
                var annotations = parameters.Annotations;
                foreach (var annotation in annotations)
                {
                    if (CoreAnnotationNames.AllNames.Contains(annotation.Key))
                    {
                        annotations.Remove(annotation.Key);
                    }
                }
            }

            GenerateSimpleAnnotations(parameters);
        }

        /// <inheritdoc />
        public virtual void Generate(IForeignKey foreignKey, CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
        {
            if (!parameters.IsRuntime)
            {
                var annotations = parameters.Annotations;
                foreach (var annotation in annotations)
                {
                    if (CoreAnnotationNames.AllNames.Contains(annotation.Key))
                    {
                        annotations.Remove(annotation.Key);
                    }
                }
            }

            GenerateSimpleAnnotations(parameters);
        }

        /// <inheritdoc />
        public virtual void Generate(INavigation navigation, CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
        {
            if (!parameters.IsRuntime)
            {
                var annotations = parameters.Annotations;
                foreach (var annotation in annotations)
                {
                    if (CoreAnnotationNames.AllNames.Contains(annotation.Key))
                    {
                        annotations.Remove(annotation.Key);
                    }
                }
            }

            GenerateSimpleAnnotations(parameters);
        }

        /// <inheritdoc />
        public virtual void Generate(ISkipNavigation navigation, CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
        {
            if (!parameters.IsRuntime)
            {
                var annotations = parameters.Annotations;
                foreach (var annotation in annotations)
                {
                    if (CoreAnnotationNames.AllNames.Contains(annotation.Key))
                    {
                        annotations.Remove(annotation.Key);
                    }
                }
            }

            GenerateSimpleAnnotations(parameters);
        }

        /// <inheritdoc />
        public virtual void Generate(IIndex index, CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
        {
            if (!parameters.IsRuntime)
            {
                var annotations = parameters.Annotations;
                foreach (var annotation in annotations)
                {
                    if (CoreAnnotationNames.AllNames.Contains(annotation.Key))
                    {
                        annotations.Remove(annotation.Key);
                    }
                }
            }

            GenerateSimpleAnnotations(parameters);
        }

        /// <inheritdoc />
        public virtual void Generate(ITypeMappingConfiguration typeConfiguration, CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
        {
            if (!parameters.IsRuntime)
            {
                var annotations = parameters.Annotations;
                foreach (var annotation in annotations)
                {
                    if (CoreAnnotationNames.AllNames.Contains(annotation.Key))
                    {
                        annotations.Remove(annotation.Key);
                    }
                }
            }

            GenerateSimpleAnnotations(parameters);
        }

        /// <summary>
        ///     Generates code to create the given annotations using literals.
        /// </summary>
        /// <param name="parameters">Parameters used during code generation.</param>
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
        /// <param name="annotationName">The annotation name.</param>
        /// <param name="valueString">The annotation value as a literal.</param>
        /// <param name="parameters">Additional parameters used during code generation.</param>
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
        /// <param name="type">A type.</param>
        /// <param name="namespaces">The set of namespaces to add to.</param>
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
