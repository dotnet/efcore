// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Infrastructure
{
    public abstract class ModelValidatorTest
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
        }

        protected class D : A
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
            public bool? ImNot { get; set; }
        }

        protected ModelValidatorTest()
        {
            Log = new List<(LogLevel, EventId, string)>();
            Logger = CreateLogger();
            ModelLogger = CreateModelLogger();
        }

        protected List<(LogLevel Level, EventId Id, string Message)> Log { get; }

        protected IDiagnosticsLogger<DbLoggerCategory.Model.Validation> Logger { get; set; }

        protected IDiagnosticsLogger<DbLoggerCategory.Model> ModelLogger { get; set; }

        protected virtual void VerifyWarning(string expectedMessage, IModel model)
        {
            Validate(model);

            Assert.Equal(1, Log.Count);
            Assert.Equal(LogLevel.Warning, Log[0].Level);
            Assert.Equal(expectedMessage, Log[0].Message);
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

        protected DiagnosticsLogger<DbLoggerCategory.Model.Validation> CreateLogger(bool sensitiveDataLoggingEnabled = false)
        {
            var options = new LoggingOptions();
            options.Initialize(new DbContextOptionsBuilder().EnableSensitiveDataLogging(sensitiveDataLoggingEnabled).Options);
            return new DiagnosticsLogger<DbLoggerCategory.Model.Validation>(
                new ListLoggerFactory(Log, l => l == DbLoggerCategory.Model.Validation.Name),
                options,
                new DiagnosticListener("Fake"));
        }

        protected DiagnosticsLogger<DbLoggerCategory.Model> CreateModelLogger(bool sensitiveDataLoggingEnabled = false)
        {
            var options = new LoggingOptions();
            options.Initialize(new DbContextOptionsBuilder().EnableSensitiveDataLogging(sensitiveDataLoggingEnabled).Options);
            return new DiagnosticsLogger<DbLoggerCategory.Model>(
                new ListLoggerFactory(Log, l => l == DbLoggerCategory.Model.Name),
                options,
                new DiagnosticListener("Fake"));
        }

        protected abstract IModelValidator CreateModelValidator();
    }
}
