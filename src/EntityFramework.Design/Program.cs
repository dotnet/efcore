// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Design.Utilities;
using Microsoft.Data.Entity.Migrations.Infrastructure;
using Microsoft.Data.Entity.Relational;
using Microsoft.Framework.ConfigurationModel;

namespace Microsoft.Data.Entity.Design
{
    // TODO: Add logging.
    public class Program
    {
        public enum CommandCode
        {
            CommitConfiguration,
            CreateMigration,
            ListMigrations,
            GenerateScript,
            UpdateDatabase
        }

        public virtual void Main([CanBeNull] string[] args)
        {
            try
            {
                Run(args);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
            }
        }

        public virtual void Run([CanBeNull] params string[] args)
        {
            string command = null;
            string[] commandArgs = null;

            if (args != null
                && args.Any())
            {
                command = args.First();
                commandArgs = args.Skip(1).ToArray();
            }

            Run(command, commandArgs);
        }

        public virtual void Run([CanBeNull] string command, [CanBeNull] params string[] commandArgs)
        {
            if (string.Equals(command, "config", StringComparison.OrdinalIgnoreCase))
            {
                CommitConfiguration(commandArgs);
            }
            else if (string.Equals(command, "create", StringComparison.OrdinalIgnoreCase))
            {
                CreateMigration(commandArgs);
            }
            else if (string.Equals(command, "list", StringComparison.OrdinalIgnoreCase))
            {
                ListMigrations(commandArgs);
            }
            else if (string.Equals(command, "script", StringComparison.OrdinalIgnoreCase))
            {
                GenerateScript(commandArgs);
            }
            else if (string.Equals(command, "apply", StringComparison.OrdinalIgnoreCase))
            {
                UpdateDatabase(commandArgs);
            }
            else
            {
                throw new InvalidOperationException(Strings.ToolUsage);
            }
        }

        public virtual void CommitConfiguration([NotNull] string[] commandArgs)
        {
            Check.NotNull(commandArgs, "commandArgs");

            var tool = CreateMigrationTool();
            var configuration = CreateConfiguration(tool, CommandCode.CommitConfiguration, commandArgs);

            tool.CommitConfiguration(configuration);
        }

        public virtual void CreateMigration([NotNull] string[] commandArgs)
        {
            Check.NotNull(commandArgs, "commandArgs");

            var tool = CreateMigrationTool();
            var configuration = CreateConfiguration(tool, CommandCode.CreateMigration, commandArgs);

            tool.CreateMigration(configuration);
        }

        public virtual void ListMigrations([NotNull] string[] commandArgs)
        {
            Check.NotNull(commandArgs, "commandArgs");

            var tool = CreateMigrationTool();
            var configuration = CreateConfiguration(tool, CommandCode.ListMigrations, commandArgs);

            OutputMigrations(tool.GetMigrations(configuration));
        }

        protected virtual void OutputMigrations(IReadOnlyList<IMigrationMetadata> migrations)
        {
            foreach (var migration in migrations)
            {
                Console.WriteLine(migration.MigrationId);
            }
        }

        public virtual void GenerateScript([NotNull] string[] commandArgs)
        {
            Check.NotNull(commandArgs, "commandArgs");

            var tool = CreateMigrationTool();
            var configuration = CreateConfiguration(tool, CommandCode.GenerateScript, commandArgs);

            OutputScript(tool.GenerateScript(configuration));
        }

        protected virtual void OutputScript(IReadOnlyList<SqlStatement> statements)
        {
            foreach (var statement in statements)
            {
                Console.WriteLine(statement.Sql);
            }
        }

        public virtual void UpdateDatabase([NotNull] string[] commandArgs)
        {
            Check.NotNull(commandArgs, "commandArgs");

            var tool = CreateMigrationTool();
            var configuration = CreateConfiguration(tool, CommandCode.UpdateDatabase, commandArgs);

            tool.UpdateDatabase(configuration);
        }

        protected virtual IConfigurationSourceContainer CreateConfiguration(
            MigrationTool tool, CommandCode commandCode, string[] commandArgs)
        {
            var configuration = CreateConfiguration();

            CommandLineConfigurationSource commandLineConfigSource;
            string configFile;

            if (commandArgs != null
                && commandArgs.Any())
            {
                commandLineConfigSource = new CommandLineConfigurationSource(commandArgs);
                commandLineConfigSource.Load();
                commandLineConfigSource.TryGet(MigrationTool.Constants.ConfigFileOption, out configFile);
            }
            else
            {
                commandLineConfigSource = null;
                configFile = null;
            }

            if (commandCode != CommandCode.CommitConfiguration)
            {
                if (!string.IsNullOrEmpty(configFile))
                {
                    configuration.AddIniFile(tool.ResolvePath(configFile));
                }
                else
                {
                    configFile = tool.ResolvePath(MigrationTool.Constants.DefaultConfigFile);
                    if (File.Exists(configFile))
                    {
                        configuration.AddIniFile(configFile);
                    }
                }
            }

            if (commandLineConfigSource != null)
            {
                configuration.Add(commandLineConfigSource);
            }

            return configuration;
        }

        protected virtual MigrationTool CreateMigrationTool()
        {
            return new MigrationTool();
        }

        protected virtual IConfigurationSourceContainer CreateConfiguration()
        {
            return new Configuration();
        }
    }
}
