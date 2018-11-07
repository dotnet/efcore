// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Linq;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Infrastructure
{
    public abstract class ModelValidatorTestBase
    {
        protected virtual void SetBaseType(EntityType entityType, EntityType baseEntityType) => entityType.HasBaseType(baseEntityType);

        protected Key CreateKey(EntityType entityType, int startingPropertyIndex = -1, int propertyCount = 1)
        {
            if (startingPropertyIndex == -1)
            {
                startingPropertyIndex = entityType.PropertyCount() - 1;
            }

            var keyProperties = new Property[propertyCount];
            for (var i = 0; i < propertyCount; i++)
            {
                var property = entityType.GetOrAddProperty("P" + (startingPropertyIndex + i), typeof(int?));
                keyProperties[i] = property;
                keyProperties[i].IsNullable = false;
            }

            return entityType.AddKey(keyProperties);
        }

        public void SetPrimaryKey(EntityType entityType)
        {
            var property = entityType.AddProperty("Id", typeof(int));
            entityType.SetPrimaryKey(property);
        }

        protected ForeignKey CreateForeignKey(Key dependentKey, Key principalKey)
            => CreateForeignKey(dependentKey.DeclaringEntityType, dependentKey.Properties, principalKey);

        protected ForeignKey CreateForeignKey(EntityType dependEntityType, IReadOnlyList<Property> dependentProperties, Key principalKey)
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
        {
            LoggerFactory = new ListLoggerFactory(l => l == DbLoggerCategory.Model.Validation.Name || l == DbLoggerCategory.Model.Name);
            ValidationLogger = CreateValidationLogger();
            ModelLogger = CreateModelLogger();
        }

        protected ListLoggerFactory LoggerFactory { get; }

        protected IDiagnosticsLogger<DbLoggerCategory.Model.Validation> ValidationLogger { get; set; }

        protected IDiagnosticsLogger<DbLoggerCategory.Model> ModelLogger { get; set; }

        protected virtual void VerifyWarning(string expectedMessage, IModel model, LogLevel level = LogLevel.Warning)
        {
            Validate(model);

            var logEntry = LoggerFactory.Log.Single(l => l.Level == level);
            Assert.Equal(expectedMessage, logEntry.Message);
        }

        protected virtual void VerifyError(string expectedMessage, IModel model)
        {
            ((Model)model).Validate();
            Assert.Equal(expectedMessage, Assert.Throws<InvalidOperationException>(() => Validate(model)).Message);
        }

        protected virtual void Validate(IModel model)
        {
            ((Model)model).Validate();
            CreateModelValidator().Validate(model);
        }

        protected DiagnosticsLogger<DbLoggerCategory.Model.Validation> CreateValidationLogger(bool sensitiveDataLoggingEnabled = false)
        {
            var options = new LoggingOptions();
            options.Initialize(new DbContextOptionsBuilder().EnableSensitiveDataLogging(sensitiveDataLoggingEnabled).Options);
            return new DiagnosticsLogger<DbLoggerCategory.Model.Validation>(
                LoggerFactory,
                options,
                new DiagnosticListener("Fake"));
        }

        protected DiagnosticsLogger<DbLoggerCategory.Model> CreateModelLogger(bool sensitiveDataLoggingEnabled = false)
        {
            var options = new LoggingOptions();
            options.Initialize(new DbContextOptionsBuilder().EnableSensitiveDataLogging(sensitiveDataLoggingEnabled).Options);
            return new DiagnosticsLogger<DbLoggerCategory.Model>(
                LoggerFactory,
                options,
                new DiagnosticListener("Fake"));
        }

        protected abstract IModelValidator CreateModelValidator();

        protected virtual ModelBuilder CreateConventionalModelBuilder()
            => InMemoryTestHelpers.Instance.CreateConventionBuilder();
    }
}
