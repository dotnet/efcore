// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.EntityFrameworkCore.Migrations.Design;
using Microsoft.EntityFrameworkCore.Tools.Core.Utilities.Internal;
using Newtonsoft.Json;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Tests.Utilities
{
    public class JsonUtilityTest
    {
#pragma warning disable 649
        private readonly ITestOutputHelper _output;
#pragma warning restore 649
        public JsonUtilityTest(ITestOutputHelper output)
        {
            // uncomment to debug test
            //_output = output;
        }

        [Fact]
        public void SerializesMigrationFiles()
        {
            var actual = JsonUtility.Serialize(new MigrationFiles
            {
                MigrationFile = @"C:\MigrationFile.cs",
                MetadataFile = @"C:/MetadataFile.cs",
                SnapshotFile = @"""C:\Folder A\SnapshotFile.cs"""
            });
            _output?.WriteLine(actual);
            Assert.Equal(
@"{
    ""MigrationFile"": ""C:\\MigrationFile.cs"",
    ""MetadataFile"": ""C:/MetadataFile.cs"",
    ""SnapshotFile"": """ + "\\\"C:\\\\Folder A\\\\SnapshotFile.cs\\\"" + @"""
}"
            , actual);
            Assert.NotNull(JsonConvert.DeserializeObject(actual));
        }

        [Fact]
        public void SerializesArray()
        {
            var typeNames = new[] { typeof(JsonUtilityTest), typeof(string) }
               .Select(t => new { fullName = t.FullName })
               .ToArray();
            var actual = JsonUtility.Serialize(typeNames);
            _output?.WriteLine(actual);
            Assert.Equal(
@"[
    {
        ""fullName"": ""Microsoft.EntityFrameworkCore.Tests.Utilities.JsonUtilityTest""
    },
    {
        ""fullName"": ""System.String""
    }
]"
               , actual);
            Assert.NotNull(JsonConvert.DeserializeObject(actual));
        }

        [Fact]
        public void SerializesAnonymousType()
        {
            var t = new
            {
                id = "Microsoft.EntityFrameworkCore.EFCore10",
                escapedChars = "EFCore\\\"\t\f\b\r\n",
                floatNo = 1.89f,
                decimalNo = 48.1m,
                doubleNo = 48d,
                uintNo = 12u,
                intNo = -23,
                ulongNo = 123UL,
                longNo = -123L,
                trueType = true,
                falseType = false,
                nullProp = (object)null,
                emptyArray = new object[] { }
            };
            var actual = JsonUtility.Serialize(t);
            _output?.WriteLine(actual);
            Assert.Equal(
@"{
    ""id"": ""Microsoft.EntityFrameworkCore.EFCore10"",
    ""escapedChars"": ""EFCore\\" + "\\\"" + @"\t\f\b\r\n"",
    ""floatNo"": 1.89,
    ""decimalNo"": 48.1,
    ""doubleNo"": 48,
    ""uintNo"": 12,
    ""intNo"": -23,
    ""ulongNo"": 123,
    ""longNo"": -123,
    ""trueType"": true,
    ""falseType"": false,
    ""nullProp"": null,
    ""emptyArray"": [
    ]
}"
              , actual);
            Assert.NotNull(JsonConvert.DeserializeObject(actual));
        }
    }
}