// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.IO;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Scaffolding.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class CSharpModelGenerator : ModelCodeGenerator
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual ICSharpDbContextGenerator CSharpDbContextGenerator { get; }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual ICSharpEntityTypeGenerator CSharpEntityTypeGenerator { get; }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public CSharpModelGenerator(
            ModelCodeGeneratorDependencies dependencies,
            ICSharpDbContextGenerator cSharpDbContextGenerator,
            ICSharpEntityTypeGenerator cSharpEntityTypeGenerator)
            : base(dependencies)
        {
            Check.NotNull(cSharpDbContextGenerator, nameof(cSharpDbContextGenerator));
            Check.NotNull(cSharpEntityTypeGenerator, nameof(cSharpEntityTypeGenerator));

            CSharpDbContextGenerator = cSharpDbContextGenerator;
            CSharpEntityTypeGenerator = cSharpEntityTypeGenerator;
        }

        private const string FileExtension = ".cs";

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override string Language
            => "C#";

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override ScaffoldedModel GenerateModel(
            IModel model,
            ModelCodeGenerationOptions options)
        {
            Check.NotNull(model, nameof(model));
            Check.NotNull(options, nameof(options));

            if (options.ContextName == null)
            {
                throw new ArgumentException(
                    CoreStrings.ArgumentPropertyNull(nameof(options.ContextName), nameof(options)), nameof(options));
            }

            if (options.ConnectionString == null)
            {
                throw new ArgumentException(
                    CoreStrings.ArgumentPropertyNull(nameof(options.ConnectionString), nameof(options)), nameof(options));
            }

            var generatedCode = CSharpDbContextGenerator.WriteCode(
                model,
                options.ContextName,
                options.ConnectionString,
                options.ContextNamespace,
                options.ModelNamespace,
                options.UseDataAnnotations,
                options.UseNullableReferenceTypes,
                options.SuppressConnectionStringWarning,
                options.SuppressOnConfiguring);

            // output DbContext .cs file
            var dbContextFileName = options.ContextName + FileExtension;
            var resultingFiles = new ScaffoldedModel
            {
                ContextFile = new ScaffoldedFile
                {
                    Path = options.ContextDir != null
                        ? Path.Combine(options.ContextDir, dbContextFileName)
                        : dbContextFileName,
                    Code = generatedCode
                }
            };

            foreach (var entityType in model.GetEntityTypes())
            {
                generatedCode = CSharpEntityTypeGenerator.WriteCode(
                    entityType,
                    options.ModelNamespace,
                    options.UseDataAnnotations,
                    options.UseNullableReferenceTypes);

                // output EntityType poco .cs file
                var entityTypeFileName = entityType.Name + FileExtension;
                resultingFiles.AdditionalFiles.Add(
                    new ScaffoldedFile { Path = entityTypeFileName, Code = generatedCode });
            }

            return resultingFiles;
        }
    }
}
