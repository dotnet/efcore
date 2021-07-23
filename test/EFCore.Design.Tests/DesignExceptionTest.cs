// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Microsoft.EntityFrameworkCore.Design;
using Xunit;

namespace Microsoft.EntityFrameworkCore
{
    public class DesignExceptionTest
    {
        [ConditionalFact]
        public void OperationException_exposes_public_empty_constructor()
        {
            new OperationException();
        }

        [ConditionalFact]
        public void OperationException_exposes_public_string_constructor()
        {
            Assert.Equal("Foo", new OperationException("Foo").Message);
        }

        [ConditionalFact]
        public void OperationException_exposes_public_string_and_inner_exception_constructor()
        {
            var inner = new Exception();

            var ex = new OperationException("Foo", inner);

            Assert.Equal("Foo", ex.Message);
            Assert.Same(inner, ex.InnerException);
        }

        [ConditionalFact]
        public void Deserialized_OperationException_can_be_serialized_and_deserialized_again()
        {
            var transportedException = SerializeAndDeserialize(
                SerializeAndDeserialize(
                    new OperationException(
                        "But somehow the vital connection is made",
                        new Exception("Bang!"))));

            Assert.Equal("But somehow the vital connection is made", transportedException.Message);
            Assert.Equal("Bang!", transportedException.InnerException.Message);
        }

        private TException SerializeAndDeserialize<TException>(TException exception)
            where TException : Exception
        {
            var stream = new MemoryStream();
            var formatter = new BinaryFormatter();

#pragma warning disable SYSLIB0011 // Issue https://github.com/dotnet/runtime/issues/39289 tracks finding an alternative to BinaryFormatter
            formatter.Serialize(stream, exception);
            stream.Seek(0, SeekOrigin.Begin);

            return (TException)formatter.Deserialize(stream);
#pragma warning restore SYSLIB0011
        }
    }
}
