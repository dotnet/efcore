// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Infrastructure
{
    public abstract class EventIdTestBase
    {
        public void TestEventLogging(
            Type eventIdType,
            Type loggerExtensionsType,
            Type loggerDefinitionsType,
            IDictionary<Type, Func<object>> fakeFactories,
            Dictionary<string, IList<string>> eventMappings = null)
        {
            var eventIdFields = eventIdType.GetTypeInfo()
                .DeclaredFields
                .Where(p => p.FieldType == typeof(EventId) && p.GetCustomAttribute<ObsoleteAttribute>() == null)
                .ToList();

            var declaredMethods = loggerExtensionsType.GetTypeInfo()
                .DeclaredMethods
                .Where(m => m.IsPublic)
                .OrderBy(e => e.Name)
                .ToList();

            var loggerMethods = declaredMethods
                .GroupBy(e => e.Name)
                .ToDictionary(m => m.Key, e => e.First());

            foreach (var eventIdField in eventIdFields)
            {
                var eventName = eventIdField.Name;
                if (eventMappings == null
                    || !eventMappings.TryGetValue(eventName, out var mappedNames))
                {
                    mappedNames = new List<string> { eventName };
                }

                foreach (var mappedName in mappedNames)
                {
                    Assert.Contains(mappedName, loggerMethods.Keys);

                    var loggerMethod = loggerMethods[mappedName];

                    var loggerParameters = loggerMethod.GetParameters();
                    var category = loggerParameters[0].ParameterType.GenericTypeArguments[0];

                    if (category.ContainsGenericParameters)
                    {
                        category = typeof(DbLoggerCategory.Infrastructure);
                        loggerMethod = loggerMethod.MakeGenericMethod(category);
                    }

                    var eventId = (EventId)eventIdField.GetValue(null);

                    Assert.InRange(eventId.Id, CoreEventId.CoreBaseId, ushort.MaxValue);

                    var categoryName = Activator.CreateInstance(category).ToString();
                    Assert.Equal(categoryName + "." + eventName, eventId.Name);

                    var testLogger =
                        (TestLoggerBase)Activator.CreateInstance(typeof(TestLogger<,>).MakeGenericType(category, loggerDefinitionsType));
                    var testDiagnostics = (TestDiagnosticSource)testLogger.DiagnosticSource;
                    var contextLogger = (TestDbContextLogger)testLogger.DbContextLogger;

                    var args = new object[loggerParameters.Length];
                    args[0] = testLogger;

                    for (var i = 1; i < args.Length; i++)
                    {
                        var type = loggerParameters[i].ParameterType;

                        if (fakeFactories.TryGetValue(type, out var factory))
                        {
                            args[i] = factory();
                        }
                        else
                        {
                            try
                            {
                                args[i] = Activator.CreateInstance(type);
                            }
                            catch (Exception)
                            {
                                Assert.True(
                                    false,
                                    "Need to add fake test factory for type "
                                    + type.DisplayName()
                                    + " in class "
                                    + eventIdType.Name
                                    + "Test");
                            }
                        }
                    }

                    foreach (var enableFor in new[] { "Foo", eventId.Name })
                    {
                        testDiagnostics.EnableFor = enableFor;

                        var logged = false;
                        foreach (LogLevel logLevel in Enum.GetValues(typeof(LogLevel)))
                        {
                            testLogger.EnabledFor = logLevel;
                            testLogger.LoggedAt = null;
                            testDiagnostics.LoggedEventName = null;

                            loggerMethod.Invoke(null, args);

                            if (testLogger.LoggedAt != null)
                            {
                                Assert.Equal(logLevel, testLogger.LoggedAt);
                                logged = true;

                                if (categoryName != DbLoggerCategory.Scaffolding.Name)
                                {
                                    Assert.Equal(logLevel, contextLogger.LoggedAt);
                                    Assert.Equal(eventId, contextLogger.LoggedEvent);
                                }
                            }

                            if (enableFor == eventId.Name
                                && categoryName != DbLoggerCategory.Scaffolding.Name)
                            {
                                Assert.Equal(eventId.Name, testDiagnostics.LoggedEventName);
                                if (testDiagnostics.LoggedMessage != null)
                                {
                                    Assert.Equal(testLogger.Message, testDiagnostics.LoggedMessage);
                                }
                            }
                            else
                            {
                                Assert.Null(testDiagnostics.LoggedEventName);
                            }
                        }

                        Assert.True(logged, "Failed for " + eventId.Name);
                    }
                }
            }
        }
    }
}
