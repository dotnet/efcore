// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.Data.Entity.Migrations.Infrastructure;
using Microsoft.Data.Entity.Relational;
using Microsoft.Framework.ConfigurationModel;
using Moq;
using Moq.Protected;
using Xunit;

namespace Microsoft.Data.Entity.Design.Tests
{
    public class ProgramTest
    {
        [Fact]
        public void Command_line_is_added_after_ini_file_to_configuration()
        {
            var mock = new Mock<Program> { CallBase = true };

            mock.Protected()
                .Setup<IConfigurationSourceContainer>("CreateConfiguration")
                .Returns(
                    () =>
                        {
                            var configurationMock = new Mock<IConfigurationSourceContainer>();
                            var callCount = 0;

                            configurationMock
                                .Setup(m => m.Add(It.IsAny<IConfigurationSource>()))
                                .Callback<IConfigurationSource>(
                                    s =>
                                        {
                                            if (s is IniFileConfigurationSource)
                                            {
                                                Assert.Equal(0, callCount);
                                            }
                                            else
                                            {
                                                Assert.True(s is CommandLineConfigurationSource);
                                                Assert.Equal(1, callCount);
                                            }

                                            callCount++;
                                        });

                            return configurationMock.Object;
                        });

            mock.Protected()
                .Setup<MigrationTool>("CreateMigrationTool")
                .Returns(
                    () =>
                        {
                            var toolMock = new Mock<MigrationTool> { CallBase = true };

                            toolMock.Setup(m => m.CreateMigration(It.IsAny<IConfigurationSourceContainer>()));

                            return toolMock.Object;
                        });

            var args
                = new[]
                    {
                        "--ConfigFile=MyConfigFile.ini",
                        "--MigrationName=MyMigration"
                    };

            mock.Object.CreateMigration(args);

            mock.Protected().Verify<MigrationTool>("CreateMigrationTool", Times.Once());
            mock.Protected().Verify<IConfigurationSourceContainer>("CreateConfiguration", Times.Once());
        }

        [Fact]
        public static void Command_is_dispatched_to_tool()
        {
            Command_is_dispatched_to_tool("config", t => t.CommitConfiguration(It.IsAny<IConfigurationSourceContainer>()));
            Command_is_dispatched_to_tool("create", t => t.CreateMigration(It.IsAny<IConfigurationSourceContainer>()));
            Command_is_dispatched_to_tool("list", t => t.GetMigrations(It.IsAny<IConfigurationSourceContainer>()));
            Command_is_dispatched_to_tool("script", t => t.GenerateScript(It.IsAny<IConfigurationSourceContainer>()));
            Command_is_dispatched_to_tool("apply", t => t.UpdateDatabase(It.IsAny<IConfigurationSourceContainer>()));
        }

        public static void Command_is_dispatched_to_tool(string command, Expression<Action<MigrationTool>> expression)
        {
            var mock = new Mock<Program> { CallBase = true };
            var toolMock = new Mock<MigrationTool>();

            mock.Protected().Setup<MigrationTool>("CreateMigrationTool").Returns(toolMock.Object);
            mock.Protected().Setup("OutputMigrations", ItExpr.IsNull<IReadOnlyList<IMigrationMetadata>>());
            mock.Protected().Setup("OutputScript", ItExpr.IsNull<IReadOnlyList<SqlStatement>>());

            mock.Object.Run(command);

            mock.Protected().Verify<MigrationTool>("CreateMigrationTool", Times.Once());
            toolMock.Verify(expression, Times.Once());
        }
    }
}
