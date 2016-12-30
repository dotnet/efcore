using Microsoft.EntityFrameworkCore.MongoDB.Adapter;
using Microsoft.EntityFrameworkCore.MongoDB.Tests.TestDomain;
using MongoDB.Bson.Serialization;
using Xunit;

namespace Microsoft.EntityFrameworkCore.MongoDB.Tests.MongoDB.Adapter
{
    public class IgnoreEmptyEnumerablesConventionTests
    {
        [Fact]
        public void Should_not_serialize_empty_enumerables()
        {
            var bsonClassMap = new BsonClassMap<ComplexSubDocument>();
            BsonMemberMap bsonMemberMap = bsonClassMap.MapMember(cr => cr.ComplexValueList);
            var ignoreEmptyEnumerableConvention = new IgnoreEmptyEnumerablesConvention();
            ignoreEmptyEnumerableConvention.Apply(bsonMemberMap);
            var complexSubDocument = new ComplexSubDocument();
            complexSubDocument.ComplexValueList.Clear();
            Assert.False(bsonMemberMap.ShouldSerialize(complexSubDocument, complexSubDocument.ComplexValueList));
        }

        [Fact]
        public void Should_serialize_non_empty_enumerables()
        {
            var bsonClassMap = new BsonClassMap<ComplexSubDocument>();
            BsonMemberMap bsonMemberMap = bsonClassMap.MapMember(cr => cr.ComplexValueList);
            var ignoreEmptyEnumerableConvention = new IgnoreEmptyEnumerablesConvention();
            ignoreEmptyEnumerableConvention.Apply(bsonMemberMap);
            var complexSubDocument = new ComplexSubDocument
            {
                ComplexValueList =
                {
                    new ComplexType(),
                    new ComplexType(),
                    new ComplexType()
                }
            };
            Assert.True(bsonMemberMap.ShouldSerialize(complexSubDocument, complexSubDocument.ComplexValueList));
        }
    }
}