// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using System.Reflection;
using Microsoft.DotNet.Cli.CommandLine;
using Microsoft.EntityFrameworkCore.Tools.Properties;

namespace Microsoft.EntityFrameworkCore.Tools.Commands
{
    internal abstract class ProjectCommandBase : EFCommandBase
    {
        private CommandOption _assembly;
        private CommandOption _startupAssembly;
        private CommandOption _dataDir;
        private CommandOption _projectDir;
        private CommandOption _rootNamespace;
        private CommandOption _language;

        protected CommandOption WorkingDir { get; private set; }

        public override void Configure(CommandLineApplication command)
        {
            _assembly = command.Option("-a|--assembly <PATH>", Resources.AssemblyDescription);
            _startupAssembly = command.Option("-s|--startup-assembly <PATH>", Resources.StartupAssemblyDescription);
            _dataDir = command.Option("--data-dir <PATH>", Resources.DataDirDescription);
            _projectDir = command.Option("--project-dir <PATH>", Resources.ProjectDirDescription);
            _rootNamespace = command.Option("--root-namespace <NAMESPACE>", Resources.RootNamespaceDescription);
            _language = command.Option("--language <LANGUAGE>", Resources.LanguageDescription);
            WorkingDir = command.Option("--working-dir <PATH>", Resources.WorkingDirDescription);

            base.Configure(command);
        }

        protected override void Validate()
        {
            base.Validate();

            if (!_assembly.HasValue())
            {
                throw new CommandException(Resources.MissingOption(_assembly.LongName));
            }
        }

        protected IOperationExecutor CreateExecutor()
        {
            try
            {
#if NET461
                return new AppDomainOperationExecutor(
                    _assembly.Value(),
                    _startupAssembly.Value(),
                    _projectDir.Value(),
                    _dataDir.Value(),
                    _rootNamespace.Value(),
                    _language.Value());
#elif NETCOREAPP2_0
                return new ReflectionOperationExecutor(
                    _assembly.Value(),
                    _startupAssembly.Value(),
                    _projectDir.Value(),
                    _dataDir.Value(),
                    _rootNamespace.Value(),
                    _language.Value());
#else
#error target frameworks need to be updated.
#endif
            }
            catch (FileNotFoundException ex)
                when (new AssemblyName(ex.FileName).Name == OperationExecutorBase.DesignAssemblyName)
            {
                throw new CommandException(
                    Resources.DesignNotFound(
                        Path.GetFileNameWithoutExtension(
                            _startupAssembly.HasValue() ? _startupAssembly.Value() : _assembly.Value())),
                    ex);
            }
        }
    }
}
