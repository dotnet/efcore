// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore.Update;
using Xunit;

namespace Microsoft.EntityFrameworkCore
{
    public class ExceptionTest
    {
        [Fact]
        public void RetryLimitExceededException_exposes_public_empty_constructor()
        {
            new RetryLimitExceededException();
        }

        [Fact]
        public void RetryLimitExceededException_exposes_public_string_constructor()
        {
            Assert.Equal("Foo", new RetryLimitExceededException("Foo").Message);
        }

        [Fact]
        public void RetryLimitExceededException_exposes_public_string_and_inner_exception_constructor()
        {
            var inner = new Exception();

            var ex = new RetryLimitExceededException("Foo", inner);

            Assert.Equal("Foo", ex.Message);
            Assert.Same(inner, ex.InnerException);
        }

        [Fact]
        public void Deserialized_RetryLimitExceededException_can_be_serialized_and_deserialized_again()
        {
            var transportedException = SerializeAndDeserialize(
                SerializeAndDeserialize(
                    new RetryLimitExceededException(
                        "But somehow the vital connection is made",
                        new Exception("Bang!"))));

            Assert.Equal("But somehow the vital connection is made", transportedException.Message);
            Assert.Equal("Bang!", transportedException.InnerException.Message);
        }

        [Fact]
        public void DbUpdateException_exposes_public_empty_constructor()
        {
            new DbUpdateException();
        }

        [Fact]
        public void DbUpdateException_exposes_public_string_constructor()
        {
            Assert.Equal("Foo", new DbUpdateException("Foo").Message);
        }

        [Fact]
        public void DbUpdateException_exposes_public_string_and_inner_exception_constructor()
        {
            var inner = new Exception();

            var ex = new DbUpdateException("Foo", inner);

            Assert.Equal("Foo", ex.Message);
            Assert.Same(inner, ex.InnerException);
        }

        [Fact]
        public void Deserialized_DbUpdateException_can_be_serialized_and_deserialized_again()
        {
            var transportedException = SerializeAndDeserialize(
                SerializeAndDeserialize(
                    new DbUpdateException("But somehow the vital connection is made")));

            Assert.Equal("But somehow the vital connection is made", transportedException.Message);
        }

        [Fact]
        public void Deserialized_DbUpdateException_can_be_serialized_and_deserialized_again_with_entries()
        {
            var transportedException = SerializeAndDeserialize(
                SerializeAndDeserialize(
                    new DbUpdateException(
                        "But somehow the vital connection is made",
                        new Exception("Bang!"),
                        new IUpdateEntry[]
                        {
                            new FakeUpdateEntry()
                        })));

            Assert.Equal("But somehow the vital connection is made", transportedException.Message);
            Assert.Equal("Bang!", transportedException.InnerException.Message);
            Assert.Empty(transportedException.Entries); // Because the entries cannot be serialized
        }

        [Fact]
        public void DbUpdateConcurrencyException_exposes_public_empty_constructor()
        {
            new DbUpdateConcurrencyException();
        }

        [Fact]
        public void DbUpdateConcurrencyException_exposes_public_string_constructor()
        {
            Assert.Equal("Foo", new DbUpdateConcurrencyException("Foo").Message);
        }

        [Fact]
        public void DbUpdateConcurrencyException_exposes_public_string_and_inner_exception_constructor()
        {
            var inner = new Exception();

            var ex = new DbUpdateConcurrencyException("Foo", inner);

            Assert.Equal("Foo", ex.Message);
            Assert.Same(inner, ex.InnerException);
        }

        [Fact]
        public void Deserialized_DbUpdateConcurrencyException_can_be_serialized_and_deserialized_again()
        {
            var transportedException = SerializeAndDeserialize(
                SerializeAndDeserialize(
                    new DbUpdateConcurrencyException("But somehow the vital connection is made")));

            Assert.Equal(
                "But somehow the vital connection is made",
                transportedException.Message);
        }

        [Fact]
        public void Deserialized_DbUpdateConcurrencyException_can_be_serialized_and_deserialized_again_with_entries()
        {
            var transportedException = SerializeAndDeserialize(
                SerializeAndDeserialize(
                    new DbUpdateConcurrencyException(
                        "But somehow the vital connection is made",
                        new Exception("Bang!"),
                        new IUpdateEntry[]
                        {
                            new FakeUpdateEntry()
                        })));

            Assert.Equal("But somehow the vital connection is made", transportedException.Message);
            Assert.Equal("Bang!", transportedException.InnerException.Message);
            Assert.Empty(transportedException.Entries); // Because the entries cannot be serialized
        }

        private class FakeUpdateEntry : IUpdateEntry
        {
            public void SetOriginalValue(IProperty property, object value) => throw new NotImplementedException();
            public void SetPropertyModified(IProperty property) => throw new NotImplementedException();
            public IEntityType EntityType { get; }
            public EntityState EntityState { get; set; }
            public IUpdateEntry SharedIdentityEntry { get; }
            public bool IsModified(IProperty property) => throw new NotImplementedException();
            public bool HasTemporaryValue(IProperty property) => throw new NotImplementedException();
            public bool IsStoreGenerated(IProperty property) => throw new NotImplementedException();
            public object GetCurrentValue(IPropertyBase propertyBase) => throw new NotImplementedException();
            public object GetOriginalValue(IPropertyBase propertyBase) => throw new NotImplementedException();
            public TProperty GetCurrentValue<TProperty>(IPropertyBase propertyBase) => throw new NotImplementedException();
            public TProperty GetOriginalValue<TProperty>(IProperty property) => throw new NotImplementedException();
            public void SetStoreGeneratedValue(IProperty property, object value) => throw new NotImplementedException();
            public EntityEntry ToEntityEntry() => new EntityEntry(new FakeInternalEntityEntry());
        }

        private class FakeInternalEntityEntry : InternalEntityEntry
        {
            public FakeInternalEntityEntry()
                : base(
                    new FakeStateManager(),
                    new Model(new ConventionSet()).AddEntityType(typeof(object), ConfigurationSource.Convention))
            {
            }

            public override object Entity { get; }
        }

        private TException SerializeAndDeserialize<TException>(TException exception) where TException : Exception
        {
            var stream = new MemoryStream();
            var formatter = new BinaryFormatter();

            formatter.Serialize(stream, exception);
            stream.Seek(0, SeekOrigin.Begin);

            return (TException)formatter.Deserialize(stream);
        }
    }
}
