// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Design.Internal
{
    /// <summary>
    ///     Implemented by database providers to generate the code for annotations.
    /// </summary>
    /// <remarks>
    ///     The service lifetime is <see cref="ServiceLifetime.Singleton" />. This means a single instance
    ///     is used by many <see cref="DbContext" /> instances. The implementation must be thread-safe.
    ///     This service cannot depend on services registered as <see cref="ServiceLifetime.Scoped" />.
    /// </remarks>
    public interface ICSharpRuntimeAnnotationCodeGenerator
    {
        /// <summary>
        ///     Generates code to create the given annotations.
        /// </summary>
        /// <param name="model">The model to which the annotations are applied.</param>
        /// <param name="parameters">Additional parameters used during code generation.</param>
        void Generate(IModel model, CSharpRuntimeAnnotationCodeGeneratorParameters parameters);

        /// <summary>
        ///     Generates code to create the given annotations.
        /// </summary>
        /// <param name="entityType">The entity type to which the annotations are applied.</param>
        /// <param name="parameters">Additional parameters used during code generation.</param>
        void Generate(IEntityType entityType, CSharpRuntimeAnnotationCodeGeneratorParameters parameters);

        /// <summary>
        ///     Generates code to create the given annotations.
        /// </summary>
        /// <param name="property">The property to which the annotations are applied.</param>
        /// <param name="parameters">Additional parameters used during code generation.</param>
        void Generate(IProperty property, CSharpRuntimeAnnotationCodeGeneratorParameters parameters);

        /// <summary>
        ///     Generates code to create the given annotations.
        /// </summary>
        /// <param name="property">The property to which the annotations are applied.</param>
        /// <param name="parameters">Additional parameters used during code generation.</param>
        void Generate(IServiceProperty property, CSharpRuntimeAnnotationCodeGeneratorParameters parameters);

        /// <summary>
        ///     Generates code to create the given annotations.
        /// </summary>
        /// <param name="key">The key to which the annotations are applied.</param>
        /// <param name="parameters">Additional parameters used during code generation.</param>
        void Generate(IKey key, CSharpRuntimeAnnotationCodeGeneratorParameters parameters);

        /// <summary>
        ///     Generates code to create the given annotations.
        /// </summary>
        /// <param name="foreignKey">The foreign key to which the annotations are applied.</param>
        /// <param name="parameters">Additional parameters used during code generation.</param>
        void Generate(IForeignKey foreignKey, CSharpRuntimeAnnotationCodeGeneratorParameters parameters);

        /// <summary>
        ///     Generates code to create the given annotations.
        /// </summary>
        /// <param name="navigation">The navigation to which the annotations are applied.</param>
        /// <param name="parameters">Additional parameters used during code generation.</param>
        void Generate(INavigation navigation, CSharpRuntimeAnnotationCodeGeneratorParameters parameters);

        /// <summary>
        ///     Generates code to create the given annotations.
        /// </summary>
        /// <param name="navigation">The skip navigation to which the annotations are applied.</param>
        /// <param name="parameters">Additional parameters used during code generation.</param>
        void Generate(ISkipNavigation navigation, CSharpRuntimeAnnotationCodeGeneratorParameters parameters);

        /// <summary>
        ///     Generates code to create the given annotations.
        /// </summary>
        /// <param name="index">The index to which the annotations are applied.</param>
        /// <param name="parameters">Additional parameters used during code generation.</param>
        void Generate(IIndex index, CSharpRuntimeAnnotationCodeGeneratorParameters parameters);

        /// <summary>
        ///     Generates code to create the given annotations.
        /// </summary>
        /// <param name="typeConfiguration">The scalar type configuration to which the annotations are applied.</param>
        /// <param name="parameters">Additional parameters used during code generation.</param>
        void Generate(ITypeMappingConfiguration typeConfiguration, CSharpRuntimeAnnotationCodeGeneratorParameters parameters);
    }
}
