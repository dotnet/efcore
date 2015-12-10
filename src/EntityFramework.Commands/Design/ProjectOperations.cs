// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IO;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Design.Internal;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Data.Entity.Scaffolding;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.Data.Entity.Design
{
    public class ProjectOperations
    {
        private readonly ILoggerProvider _loggerProvider;
        private readonly LazyRef<ILogger> _logger;
        private readonly string _projectDir;
        private readonly DesignTimeServicesBuilder _servicesBuilder;
        private readonly DbContextOperations _contextOperations;

        public ProjectOperations(
            [NotNull] ILoggerProvider loggerProvider,
            [NotNull] string assemblyName,
            [NotNull] string startupAssemblyName,
            [CanBeNull] string environment,
            [NotNull] string projectDir)
        {
            Check.NotNull(loggerProvider, nameof(loggerProvider));
            Check.NotEmpty(assemblyName, nameof(assemblyName));
            Check.NotEmpty(startupAssemblyName, nameof(startupAssemblyName));
            Check.NotNull(projectDir, nameof(projectDir));

            var loggerFactory = new LoggerFactory();
            loggerFactory.AddProvider(loggerProvider);

            _loggerProvider = loggerProvider;
            _logger = new LazyRef<ILogger>(() => loggerFactory.CreateCommandsLogger());
            _projectDir = projectDir;
            _contextOperations = new DbContextOperations(
                loggerProvider,
                assemblyName,
                startupAssemblyName,
                environment);

            var startup = new StartupInvoker(startupAssemblyName, environment);
            _servicesBuilder = new DesignTimeServicesBuilder(startup);
        }

        public virtual DirectiveFiles GenerateRuntimeDirectives()
        {
            var generator = new DirectiveGenerator();
            var members = new List<MemberInfo>();

            foreach (var contextType in _contextOperations.GetContextTypes())
            {
                var contextTypeName = contextType.GetTypeInfo().FullName;

                _logger.Value.LogInformation(CommandsStrings.LogUseContext(contextTypeName));

                using (var context = _contextOperations.CreateContext(contextType))
                {
                    var services = _servicesBuilder.Build(context);
                    var discoverer = services.GetRequiredService<RuntimeTypeDiscoverer>();

                    _logger.Value.LogDebug(CommandsStrings.BeginRuntimeTypeDiscovery(contextTypeName));
                    var start = members.Count;

                    members.AddRange(discoverer.Discover(typeof(EntityType).GetTypeInfo().Assembly, 
                        typeof(RelationalDatabase).GetTypeInfo().Assembly,
                        context.GetInfrastructure().GetRequiredService<IDbContextServices>().DatabaseProviderServices.GetType().GetTypeInfo().Assembly));

                    _logger.Value.LogDebug(CommandsStrings.EndRuntimeTypeDiscovery(members.Count - start, contextTypeName));
                }
            }
            var xml = generator.GenerateXml(members);

            var filename = Path.Combine(_projectDir, "Properties", "EntityFramework.g.rd.xml");

            if (!Directory.Exists(Path.GetDirectoryName(filename)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(filename));
            }

            _logger.Value.LogInformation(CommandsStrings.WritingDirectives(filename));

            File.WriteAllText(filename, xml);

            return new DirectiveFiles { GeneratedFile = filename };
        }
    }
}
