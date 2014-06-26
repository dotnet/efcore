// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
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

        public virtual void Main(string[] args)
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

        public virtual void Run(params string[] args)
        {
            string command = null;
            string[] commandArgs = null;

            if (args != null && args.Any())
            {
                command = args.First();
                commandArgs = args.Skip(1).ToArray();
            }

            Run(command, commandArgs);
        }

        public virtual void Run(string command, params string[] commandArgs)
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

        public virtual void CommitConfiguration(string[] commandArgs)
        {
            var tool = CreateMigrationTool(CommandCode.CommitConfiguration, commandArgs);

            tool.CommitConfiguration();
        }

        public virtual void CreateMigration(string[] commandArgs)
        {
            var tool = CreateMigrationTool(CommandCode.CreateMigration, commandArgs);

            tool.CreateMigration();
        }

        public virtual void ListMigrations(string[] commandArgs)
        {
            var tool = CreateMigrationTool(CommandCode.ListMigrations, commandArgs);

            OutputMigrations(tool.GetMigrations());
        }

        protected virtual void OutputMigrations(IReadOnlyList<IMigrationMetadata> migrations)
        {
            foreach (var migration in migrations)
            {
                Console.WriteLine(migration.Timestamp + " " + migration.Name);
            }            
        }

        public virtual void GenerateScript(string[] commandArgs)
        {
            var tool = CreateMigrationTool(CommandCode.GenerateScript, commandArgs);

            OutputScript(tool.GenerateScript());
        }

        protected virtual void OutputScript(IReadOnlyList<SqlStatement> statements)
        {
            foreach (var statement in statements)
            {
                Console.WriteLine(statement.Sql);
            }
        }

        public virtual void UpdateDatabase(string[] commandArgs)
        {
            var tool = CreateMigrationTool(CommandCode.UpdateDatabase, commandArgs);

            tool.UpdateDatabase();
        }

        protected virtual MigrationTool CreateMigrationTool(CommandCode commandCode, string[] commandArgs)
        {
            CommandLineConfigurationSource commandLineConfigSource;
            string configFile;

            if (commandArgs != null && commandArgs.Any())
            {
                commandLineConfigSource = new CommandLineConfigurationSource(commandArgs);
                commandLineConfigSource.Load();
                commandLineConfigSource.TryGet(MigrationTool.Constants.ConfigFile, out configFile);
            }
            else
            {
                commandLineConfigSource = null;
                configFile = null;
            }

            var tool = CreateMigrationTool();

            if (commandCode != CommandCode.CommitConfiguration
                && !string.IsNullOrEmpty(configFile))
            {
                tool.Configuration.AddIniFile(tool.ResolvePath(configFile));
            }

            if (commandLineConfigSource != null)
            {
                tool.Configuration.Add(commandLineConfigSource);
            }

            return tool;
        }

        protected virtual MigrationTool CreateMigrationTool()
        {
            return new MigrationTool();
        }
    }
}
