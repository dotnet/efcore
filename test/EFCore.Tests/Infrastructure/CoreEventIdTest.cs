// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Infrastructure
{
    public class CoreEventIdTest : EventIdTestBase
    {
        [ConditionalFact]
        public void Every_eventId_has_a_logger_method_and_logs_when_level_enabled()
        {
            var propertyInfo = typeof(DateTime).GetTypeInfo().GetDeclaredProperty(nameof(DateTime.Now));
            var entityType = new Model(new ConventionSet()).AddEntityType(typeof(object), ConfigurationSource.Convention);
            var property = entityType.AddProperty("A", typeof(int), ConfigurationSource.Convention, ConfigurationSource.Convention);
            var otherEntityType = new EntityType(typeof(object), entityType.Model, ConfigurationSource.Convention);
            var otherProperty = otherEntityType.AddProperty(
                "A", typeof(int), ConfigurationSource.Convention, ConfigurationSource.Convention);
            var otherKey = otherEntityType.AddKey(otherProperty, ConfigurationSource.Convention);
            var foreignKey = new ForeignKey(new[] { property }, otherKey, entityType, otherEntityType, ConfigurationSource.Convention);
            var navigation = new Navigation("N", propertyInfo, null, foreignKey);
            entityType.Model.FinalizeModel();
            var options = new DbContextOptionsBuilder()
                .UseInternalServiceProvider(InMemoryFixture.DefaultServiceProvider)
                .UseInMemoryDatabase("D").Options;

            var fakeFactories = new Dictionary<Type, Func<object>>
            {
                { typeof(Type), () => typeof(object) },
                { typeof(DbContext), () => new DbContext(options) },
                { typeof(DbContextOptions), () => options },
                { typeof(string), () => "Fake" },
                { typeof(ExpressionPrinter), () => new ExpressionPrinter() },
                { typeof(Expression), () => Expression.Constant("A") },
                { typeof(IEntityType), () => entityType },
                { typeof(IKey), () => new Key(new[] { property }, ConfigurationSource.Convention) },
                { typeof(IPropertyBase), () => property },
                { typeof(IServiceProvider), () => new FakeServiceProvider() },
                { typeof(ICollection<IServiceProvider>), () => new List<IServiceProvider>() },
                { typeof(IReadOnlyList<IPropertyBase>), () => new[] { property } },
                {
                    typeof(IEnumerable<Tuple<MemberInfo, Type>>),
                    () => new[] { new Tuple<MemberInfo, Type>(propertyInfo, typeof(object)) }
                },
                { typeof(MemberInfo), () => propertyInfo },
                { typeof(IReadOnlyList<Exception>), () => new[] { new Exception() } },
                { typeof(IProperty), () => property },
                { typeof(INavigation), () => navigation },
                { typeof(IForeignKey), () => foreignKey },
                { typeof(InternalEntityEntry), () => new FakeInternalEntityEntry(entityType) },
                { typeof(ISet<object>), () => new HashSet<object>() },
                {
                    typeof(IList<IDictionary<string, string>>),
                    () => new List<IDictionary<string, string>> { new Dictionary<string, string> { { "A", "B" } } }
                },
                { typeof(IDictionary<string, string>), () => new Dictionary<string, string>() }
            };

            TestEventLogging(
                typeof(CoreEventId),
                typeof(CoreLoggerExtensions),
                typeof(TestLoggingDefinitions),
                fakeFactories);
        }

        private class FakeServiceProvider : IServiceProvider
        {
            public object GetService(Type serviceType) => null;
        }

        private class FakeInternalEntityEntry : InternalEntityEntry
        {
            public FakeInternalEntityEntry(IEntityType entityType)
                : base(new FakeStateManager(), entityType)
            {
            }

            public override object Entity { get; }
        }
    }
}
