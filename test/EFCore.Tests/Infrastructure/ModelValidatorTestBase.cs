// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Linq;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

// ReSharper disable MemberHidesStaticFromOuterClass
namespace Microsoft.EntityFrameworkCore.Infrastructure
{
    public abstract class ModelValidatorTestBase
    {
        protected virtual void SetBaseType(IMutableEntityType entityType, IMutableEntityType baseEntityType)
            => entityType.BaseType = baseEntityType;

        protected IMutableKey CreateKey(IMutableEntityType entityType, int startingPropertyIndex = -1, int propertyCount = 1)
        {
            if (startingPropertyIndex == -1)
            {
                startingPropertyIndex = entityType.GetProperties().Count() - 1;
            }

            var keyProperties = new IMutableProperty[propertyCount];
            for (var i = 0; i < propertyCount; i++)
            {
                var propertyName = "P" + (startingPropertyIndex + i);
                keyProperties[i] = entityType.FindProperty(propertyName)
                                   ?? entityType.AddProperty(propertyName, typeof(int?));
                keyProperties[i].IsNullable = false;
            }

            return entityType.AddKey(keyProperties);
        }

        public void AddProperties(IMutableEntityType entityTypeA)
        {
            entityTypeA.AddProperty(nameof(A.P0), typeof(int?));
            entityTypeA.AddProperty(nameof(A.P1), typeof(int?));
            entityTypeA.AddProperty(nameof(A.P2), typeof(int?));
            entityTypeA.AddProperty(nameof(A.P3), typeof(int?));
        }

        public void SetPrimaryKey(IMutableEntityType entityType)
            => entityType.SetPrimaryKey(entityType.AddProperty("Id", typeof(int)));

        protected IMutableForeignKey CreateForeignKey(IMutableKey dependentKey, IMutableKey principalKey)
            => CreateForeignKey(dependentKey.DeclaringEntityType, dependentKey.Properties, principalKey);

        protected IMutableForeignKey CreateForeignKey(
            IMutableEntityType dependEntityType, IReadOnlyList<IMutableProperty> dependentProperties, IMutableKey principalKey)
        {
            var foreignKey = dependEntityType.AddForeignKey(dependentProperties, principalKey, principalKey.DeclaringEntityType);
            foreignKey.IsUnique = true;

            return foreignKey;
        }

        protected class A
        {
            public int Id { get; set; }

            public int? P0 { get; set; }
            public int? P1 { get; set; }
            public int? P2 { get; set; }
            public int? P3 { get; set; }
        }

        protected class B
        {
            public int Id { get; set; }

            public int? P0 { get; set; }
            public int? P1 { get; set; }
            public int? P2 { get; set; }
            public int? P3 { get; set; }

            public A A { get; set; }

            [NotMapped]
            public A AnotherA { get; set; }

            [NotMapped]
            public ICollection<A> ManyAs { get; set; }
        }

        protected class C : A
        {
        }

        protected class D : A
        {
        }

        protected class F : D
        {
        }

        protected abstract class Abstract : A
        {
        }

        // ReSharper disable once UnusedTypeParameter
        protected class Generic<T> : Abstract
        {
        }

        public class SampleEntity
        {
            public int Id { get; set; }
            public int Number { get; set; }
            public string Name { get; set; }
            public ReferencedEntity ReferencedEntity { get; set; }
            public ICollection<SampleEntity> OtherSamples { get; set; }
        }

        public class AnotherSampleEntity
        {
            public int Id { get; set; }
            public ReferencedEntity ReferencedEntity { get; set; }
        }

        public class ReferencedEntity
        {
            public int Id { get; set; }
            public int SampleEntityId { get; set; }
        }

        public class SampleEntityMinimal
        {
            public int Id { get; set; }
            public ReferencedEntityMinimal ReferencedEntity { get; set; }
        }

        public class ReferencedEntityMinimal
        {
        }

        public class AnotherSampleEntityMinimal
        {
            public int Id { get; set; }
            public ReferencedEntityMinimal ReferencedEntity { get; set; }
        }

        protected class E
        {
            public int Id { get; set; }
            public bool ImBool { get; set; }
            public bool ImNotUsed { get; set; }
            public bool? ImNot { get; set; }
        }

        protected class EntityWithInvalidProperties
        {
            public int Id { get; set; }

            public bool NotImplemented
            {
                get => throw new NotImplementedException();
                set => throw new NotImplementedException();
            }

            public static int Static { get; set; }

            public int WriteOnly
            {
                // ReSharper disable once ValueParameterNotUsed
                set { }
            }

            // ReSharper disable once UnassignedGetOnlyAutoProperty
            public int ReadOnly { get; }

            // ReSharper disable once UnusedAutoPropertyAccessor.Local
            public int PrivateGetter { private get; set; }

            public int this[int index]
            {
                get => 0;
                // ReSharper disable once ValueParameterNotUsed
                set { }
            }
        }

        protected ModelValidatorTestBase()
            => LoggerFactory = new ListLoggerFactory(l => l == DbLoggerCategory.Model.Validation.Name || l == DbLoggerCategory.Model.Name);

        protected ListLoggerFactory LoggerFactory { get; }

        protected virtual void VerifyWarning(string expectedMessage, IMutableModel model, LogLevel level = LogLevel.Warning)
        {
            Validate(model);

            var logEntry = LoggerFactory.Log.Single(l => l.Level == level);
            Assert.Equal(expectedMessage, logEntry.Message);
        }

        protected virtual void VerifyError(string expectedMessage, IMutableModel model)
        {
            var message = Assert.Throws<InvalidOperationException>(() => Validate(model)).Message;
            Assert.Equal(expectedMessage, message);
        }

        protected virtual void Validate(IMutableModel model) => model.FinalizeModel();

        protected DiagnosticsLogger<DbLoggerCategory.Model.Validation> CreateValidationLogger(bool sensitiveDataLoggingEnabled = false)
        {
            var options = new LoggingOptions();
            options.Initialize(new DbContextOptionsBuilder().EnableSensitiveDataLogging(sensitiveDataLoggingEnabled).Options);
            return new DiagnosticsLogger<DbLoggerCategory.Model.Validation>(
                LoggerFactory,
                options,
                new DiagnosticListener("Fake"),
                TestHelpers.LoggingDefinitions);
        }

        protected DiagnosticsLogger<DbLoggerCategory.Model> CreateModelLogger(bool sensitiveDataLoggingEnabled = false)
        {
            var options = new LoggingOptions();
            options.Initialize(new DbContextOptionsBuilder().EnableSensitiveDataLogging(sensitiveDataLoggingEnabled).Options);
            return new DiagnosticsLogger<DbLoggerCategory.Model>(
                LoggerFactory,
                options,
                new DiagnosticListener("Fake"),
                TestHelpers.LoggingDefinitions);
        }

        protected virtual ModelBuilder CreateConventionalModelBuilder(bool sensitiveDataLoggingEnabled = false)
            => TestHelpers.CreateConventionBuilder(
                CreateModelLogger(sensitiveDataLoggingEnabled), CreateValidationLogger(sensitiveDataLoggingEnabled));

        protected virtual ModelBuilder CreateConventionlessModelBuilder(bool sensitiveDataLoggingEnabled = false)
        {
            var conventionSet = new ConventionSet();

            var dependencies = CreateDependencies(sensitiveDataLoggingEnabled);
            conventionSet.ModelFinalizedConventions.Add(new TypeMappingConvention(dependencies));
            conventionSet.ModelFinalizedConventions.Add(new ValidatingConvention(dependencies));

            return new ModelBuilder(conventionSet);
        }

        protected ProviderConventionSetBuilderDependencies CreateDependencies(bool sensitiveDataLoggingEnabled = false)
            => TestHelpers.CreateContextServices().GetRequiredService<ProviderConventionSetBuilderDependencies>()
                .With(CreateValidationLogger(sensitiveDataLoggingEnabled));

        protected virtual TestHelpers TestHelpers => InMemoryTestHelpers.Instance;
    }
}
