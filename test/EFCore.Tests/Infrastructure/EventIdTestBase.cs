// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.EntityFrameworkCore.Infrastructure;

public abstract class EventIdTestBase
{
    public void TestEventLogging(
        Type eventIdType,
        Type loggerExtensionType,
        LoggingDefinitions loggerDefinitions,
        IDictionary<Type, Func<object>> fakeFactories,
        Dictionary<string, IList<string>> eventMappings = null)
        => TestEventLogging(
            eventIdType,
            loggerExtensionType,
            loggerMethodTypes: [],
            loggerDefinitions,
            fakeFactories,
            serviceCollectionBuilder: services => new EntityFrameworkServicesBuilder(services).TryAddCoreServices(),
            eventMappings);

    public void TestEventLogging(
        Type eventIdType,
        Type loggerExtensionType,
        Type[] loggerMethodTypes,
        LoggingDefinitions loggerDefinitions,
        IDictionary<Type, Func<object>> fakeFactories,
        Action<ServiceCollection> serviceCollectionBuilder,
        Dictionary<string, IList<string>> eventMappings = null)
    {
        var testLoggerFactory = new TestLoggerFactory(loggerDefinitions);
        var testLogger = testLoggerFactory.Logger;
        var testDiagnostics = new TestDiagnosticSource();
        var contextLogger = new TestDbContextLogger();

        var serviceCollection = new ServiceCollection();
        serviceCollection.TryAdd(new ServiceDescriptor(typeof(LoggingDefinitions), loggerDefinitions));
        serviceCollection.TryAdd(new ServiceDescriptor(typeof(ILoggerFactory), testLoggerFactory));
        serviceCollection.TryAdd(new ServiceDescriptor(typeof(DiagnosticSource), testDiagnostics));
        serviceCollection.TryAdd(new ServiceDescriptor(typeof(IDbContextLogger), contextLogger));
        serviceCollection.TryAdd(new ServiceDescriptor(typeof(IDbContextOptions), new DbContextOptionsBuilder().Options));
        serviceCollectionBuilder(serviceCollection);
        using var serviceProvider = serviceCollection.BuildServiceProvider(validateScopes: true);
        using var serviceScope = serviceProvider.CreateScope();
        var scopeServiceProvider = serviceScope.ServiceProvider;

        var eventIdFields = eventIdType.GetTypeInfo()
            .DeclaredFields
            .Where(p => p.FieldType == typeof(EventId) && p.GetCustomAttribute<ObsoleteAttribute>() == null)
            .ToList();

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
                var loggerMethod = loggerMethodTypes
                    .Append(loggerExtensionType)
                    .Select(t => t.GetMethod(mappedName))
                    .FirstOrDefault(m => m is not null);

                Assert.True(loggerMethod is not null, $"Couldn't find logger method {mappedName}");

                var isExtensionMethod = loggerMethod.IsStatic;
                var loggerParameters = loggerMethod.GetParameters();

                var category = isExtensionMethod
                    ? loggerParameters[0].ParameterType.GenericTypeArguments[0]
                    : loggerMethod.DeclaringType!.GetInterfaces()
                        .Single(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDiagnosticsLogger<>))
                        .GetGenericArguments()[0];

                if (category.ContainsGenericParameters)
                {
                    category = typeof(DbLoggerCategory.Infrastructure);
                    loggerMethod = loggerMethod.MakeGenericMethod(category);
                }

                var eventId = (EventId)eventIdField.GetValue(null);

                Assert.InRange(eventId.Id, CoreEventId.CoreBaseId, ushort.MaxValue);

                var categoryName = Activator.CreateInstance(category).ToString();
                Assert.Equal(categoryName + "." + eventName, eventId.Name);

                var diagnosticLogger = scopeServiceProvider.GetRequiredService(
                    isExtensionMethod
                        ? typeof(IDiagnosticsLogger<>).MakeGenericType(category)
                        : loggerMethod.DeclaringType);

                var args = new object[loggerParameters.Length];
                var i = 0;
                if (isExtensionMethod)
                {
                    args[i++] = diagnosticLogger;
                }

                for (; i < args.Length; i++)
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
                            Assert.Fail(
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

                        loggerMethod.Invoke(isExtensionMethod ? null : diagnosticLogger, args);

                        if (testLoggerFactory.LoggedAt != null)
                        {
                            Assert.Equal(logLevel, testLoggerFactory.LoggedAt);
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
