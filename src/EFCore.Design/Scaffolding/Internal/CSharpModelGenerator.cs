// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using JetBrains.Annotations;
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
        public virtual ICSharpEntityTypeConfigurationGenerator CSharpEntityTypeConfigurationGenerator { get; }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public CSharpModelGenerator(
            [NotNull] ModelCodeGeneratorDependencies dependencies,
            [NotNull] ICSharpDbContextGenerator cSharpDbContextGenerator,
            [NotNull] ICSharpEntityTypeGenerator cSharpEntityTypeGenerator,
            [NotNull] ICSharpEntityTypeConfigurationGenerator cSharpEntityTypeConfigurationGenerator)
            : base(dependencies)
        {
            Check.NotNull(cSharpDbContextGenerator, nameof(cSharpDbContextGenerator));
            Check.NotNull(cSharpEntityTypeGenerator, nameof(cSharpEntityTypeGenerator));
            Check.NotNull(cSharpEntityTypeConfigurationGenerator, nameof(cSharpEntityTypeConfigurationGenerator));

            CSharpDbContextGenerator = cSharpDbContextGenerator;
            CSharpEntityTypeGenerator = cSharpEntityTypeGenerator;
            CSharpEntityTypeConfigurationGenerator = cSharpEntityTypeConfigurationGenerator;
        }

        private const string FileExtension = ".cs";

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override string Language => "C#";

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

            var resultingFiles = new ScaffoldedModel();

            var configLocation = EntityConfigurationLocation.OnModelCreating;
            if (options.UseDataAnnotations)
            {
                configLocation = EntityConfigurationLocation.DataAnnotations;
            }
            else if (options.GenerateEntityTypeConfigurationFiles)
            {
                configLocation = EntityConfigurationLocation.IEntityTypeConfiguration;
            }

            var generatedCode = CSharpDbContextGenerator.WriteCode(
                model,
                options.ContextNamespace ?? options.ModelNamespace,
                options.ContextName,
                options.ConnectionString,
                options.SuppressConnectionStringWarning,
                configLocation);

            // output DbContext .cs file
            var dbContextFileName = options.ContextName + FileExtension;
            resultingFiles.ContextFile = new ScaffoldedFile
            {
                Path = options.ContextDir != null
                    ? Path.Combine(options.ContextDir, dbContextFileName)
                    : dbContextFileName,
                Code = generatedCode
            };

            foreach (var entityType in model.GetEntityTypes())
            {
                generatedCode = CSharpEntityTypeGenerator.WriteCode(entityType, options.ModelNamespace, configLocation == EntityConfigurationLocation.DataAnnotations);

                // output EntityType poco .cs file
                var entityTypeFileName = entityType.DisplayName() + FileExtension;
                resultingFiles.AdditionalFiles.Add(
                    new ScaffoldedFile { Path = entityTypeFileName, Code = generatedCode });

                //if the config needs to go into a custom configuration class, then we do that here
                if(configLocation == EntityConfigurationLocation.IEntityTypeConfiguration)
                {
                    generatedCode = CSharpEntityTypeConfigurationGenerator.WriteCode(entityType, options.ModelNamespace, options.EntityTypeConfigurationClassSuffix);

                    // output EntityTypeConfiguration .cs file
                    var configFilename = entityType.DisplayName()+ options.EntityTypeConfigurationClassSuffix + FileExtension;
                    resultingFiles.AdditionalFiles.Add(
                        new ScaffoldedFile { Path = configFilename, Code = generatedCode });
                }
            }

            return resultingFiles;
        }
    }
}
