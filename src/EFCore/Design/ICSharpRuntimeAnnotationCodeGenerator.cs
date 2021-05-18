// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.Design
{
    /// <summary>
    ///     Implemented by database providers to generate the code for annotations.
    /// </summary>
    public interface ICSharpRuntimeAnnotationCodeGenerator
    {
        /// <summary>
        ///     Generates code to create the given annotations.
        /// </summary>
        /// <param name="model"> The model to which the annotations are applied. </param>
        /// <param name="parameters"> Additional parameters used during code generation. </param>
        void Generate(IModel model, CSharpRuntimeAnnotationCodeGeneratorParameters parameters);

        /// <summary>
        ///     Generates code to create the given annotations.
        /// </summary>
        /// <param name="entityType"> The entity type to which the annotations are applied. </param>
        /// <param name="parameters"> Additional parameters used during code generation. </param>
        void Generate(IEntityType entityType, CSharpRuntimeAnnotationCodeGeneratorParameters parameters);

        /// <summary>
        ///     Generates code to create the given annotations.
        /// </summary>
        /// <param name="property"> The property to which the annotations are applied. </param>
        /// <param name="parameters"> Additional parameters used during code generation. </param>
        void Generate(IProperty property, CSharpRuntimeAnnotationCodeGeneratorParameters parameters);

        /// <summary>
        ///     Generates code to create the given annotations.
        /// </summary>
        /// <param name="property"> The property to which the annotations are applied. </param>
        /// <param name="parameters"> Additional parameters used during code generation. </param>
        void Generate(IServiceProperty property, CSharpRuntimeAnnotationCodeGeneratorParameters parameters);

        /// <summary>
        ///     Generates code to create the given annotations.
        /// </summary>
        /// <param name="key"> The key to which the annotations are applied. </param>
        /// <param name="parameters"> Additional parameters used during code generation. </param>
        void Generate(IKey key, CSharpRuntimeAnnotationCodeGeneratorParameters parameters);

        /// <summary>
        ///     Generates code to create the given annotations.
        /// </summary>
        /// <param name="foreignKey"> The foreign key to which the annotations are applied. </param>
        /// <param name="parameters"> Additional parameters used during code generation. </param>
        void Generate(IForeignKey foreignKey, CSharpRuntimeAnnotationCodeGeneratorParameters parameters);

        /// <summary>
        ///     Generates code to create the given annotations.
        /// </summary>
        /// <param name="navigation"> The navigation to which the annotations are applied. </param>
        /// <param name="parameters"> Additional parameters used during code generation. </param>
        void Generate(INavigation navigation, CSharpRuntimeAnnotationCodeGeneratorParameters parameters);

        /// <summary>
        ///     Generates code to create the given annotations.
        /// </summary>
        /// <param name="navigation"> The skip navigation to which the annotations are applied. </param>
        /// <param name="parameters"> Additional parameters used during code generation. </param>
        void Generate(ISkipNavigation navigation, CSharpRuntimeAnnotationCodeGeneratorParameters parameters);

        /// <summary>
        ///     Generates code to create the given annotations.
        /// </summary>
        /// <param name="index"> The index to which the annotations are applied. </param>
        /// <param name="parameters"> Additional parameters used during code generation. </param>
        void Generate(IIndex index, CSharpRuntimeAnnotationCodeGeneratorParameters parameters);
    }
}
