using Microsoft.EntityFrameworkCore.MongoDB.Adapter;
using Microsoft.EntityFrameworkCore.MongoDB.Tests.TestDomain;
using MongoDB.Bson.Serialization;
using Xunit;

namespace Microsoft.EntityFrameworkCore.MongoDB.Tests.MongoDB.Adapter
{
    public class IgnoreNullOrEmptyStringsConventionTests
    {
        [Theory]
        [InlineData(data: null)]
        [InlineData("")]
        [InlineData(" \t\v\r\n")]
        [InlineData("TestData")]
        public void Should_not_serialize_null_or_empty_strings(string value)
        {
            var bsonClassMap = new BsonClassMap<SimpleRecord>();
            BsonMemberMap bsonMemberMap = bsonClassMap.MapMember(cr => cr.StringProperty);
            var ignoreNullOrEmptyStringsConvention = new IgnoreNullOrEmptyStringsConvention();
            ignoreNullOrEmptyStringsConvention.Apply(bsonMemberMap);
            var simpleRecord = new SimpleRecord
            {
                StringProperty = value
            };
            Assert.Equal(!string.IsNullOrEmpty(value), bsonMemberMap.ShouldSerialize(simpleRecord, simpleRecord.StringProperty));
        }
    }
}