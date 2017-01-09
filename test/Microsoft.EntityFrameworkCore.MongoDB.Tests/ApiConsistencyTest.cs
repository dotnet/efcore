using System.Reflection;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Tests;

namespace Microsoft.EntityFrameworkCore.MongoDB.Tests
{
    public class ApiConsistencyTest : ApiConsistencyTestBase
    {
        protected override Assembly TargetAssembly => typeof(MongoDbDatabase).GetTypeInfo().Assembly;
    }
}