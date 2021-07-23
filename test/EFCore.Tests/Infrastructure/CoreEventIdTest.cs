﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;
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
            var model = new Model();
            var entityType = model.AddEntityType(typeof(object), owned: false, ConfigurationSource.Convention);
            var property = entityType.AddProperty("A", typeof(int), ConfigurationSource.Convention, ConfigurationSource.Convention);
            var otherEntityType = new EntityType(typeof(object), entityType.Model, owned: false, ConfigurationSource.Convention);
            var otherProperty = otherEntityType.AddProperty(
                "A", typeof(int), ConfigurationSource.Convention, ConfigurationSource.Convention);
            var otherKey = otherEntityType.AddKey(otherProperty, ConfigurationSource.Convention);
            var foreignKey = new ForeignKey(new[] { property }, otherKey, entityType, otherEntityType, ConfigurationSource.Convention);
            var navigation = new Navigation("N", propertyInfo, null, foreignKey);
            var skipNavigation = new SkipNavigation(
                "SN", propertyInfo, null, entityType, otherEntityType, true, false, ConfigurationSource.Convention);
            var navigationBase = new FakeNavigationBase("FNB", ConfigurationSource.Convention, entityType);

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
                { typeof(IReadOnlyEntityType), () => entityType },
                { typeof(IConventionEntityType), () => entityType },
                { typeof(IKey), () => new Key(new[] { property }, ConfigurationSource.Convention) },
                { typeof(IPropertyBase), () => property },
                { typeof(IReadOnlyProperty), () => property },
                { typeof(IServiceProvider), () => new FakeServiceProvider() },
                { typeof(ICollection<IServiceProvider>), () => new List<IServiceProvider>() },
                { typeof(IReadOnlyList<IPropertyBase>), () => new[] { property } },
                { typeof(IReadOnlyList<IReadOnlyPropertyBase>), () => new[] { property } },
                {
                    typeof(IEnumerable<Tuple<MemberInfo, Type>>),
                    () => new[] { new Tuple<MemberInfo, Type>(propertyInfo, typeof(object)) }
                },
                { typeof(MemberInfo), () => propertyInfo },
                { typeof(IReadOnlyList<Exception>), () => new[] { new Exception() } },
                { typeof(IProperty), () => property },
                { typeof(INavigation), () => navigation },
                { typeof(IReadOnlyNavigation), () => navigation },
                { typeof(ISkipNavigation), () => skipNavigation },
                { typeof(IReadOnlySkipNavigation), () => skipNavigation },
                { typeof(INavigationBase), () => navigationBase },
                { typeof(IForeignKey), () => foreignKey },
                { typeof(IReadOnlyForeignKey), () => foreignKey },
                { typeof(InternalEntityEntry), () => new InternalEntityEntry(new FakeStateManager(), entityType, null!) },
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
                new TestLoggingDefinitions(),
                fakeFactories);
        }

        private class FakeServiceProvider : IServiceProvider
        {
            public object GetService(Type serviceType)
                => null;
        }

        private class FakeNavigationBase : PropertyBase, INavigationBase
        {
            public FakeNavigationBase(string name, ConfigurationSource configurationSource, EntityType entityType)
                : base(name, null, null, configurationSource)
            {
                DeclaringType = entityType;
            }

            public IEntityType DeclaringEntityType
                => (IEntityType)DeclaringType;

            public IEntityType TargetEntityType
                => throw new NotImplementedException();

            public INavigationBase Inverse
                => throw new NotImplementedException();

            public bool IsCollection
                => throw new NotImplementedException();

            public override TypeBase DeclaringType { get; }

            public override Type ClrType
                => throw new NotImplementedException();

            IReadOnlyEntityType IReadOnlyNavigationBase.DeclaringEntityType
                => (IReadOnlyEntityType)DeclaringType;

            IReadOnlyEntityType IReadOnlyNavigationBase.TargetEntityType
                => throw new NotImplementedException();

            IReadOnlyNavigationBase IReadOnlyNavigationBase.Inverse
                => throw new NotImplementedException();

            public IClrCollectionAccessor GetCollectionAccessor()
                => throw new NotImplementedException();
        }
    }
}
