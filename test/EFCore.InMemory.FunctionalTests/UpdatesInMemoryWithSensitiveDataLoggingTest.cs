using Microsoft.EntityFrameworkCore.Internal;

namespace Microsoft.EntityFrameworkCore
{
    public class UpdatesInMemoryWithSensitiveDataLoggingTest : UpdatesInMemoryTestBase<UpdatesInMemoryWithSensitiveDataLoggingFixture>
    {
        public UpdatesInMemoryWithSensitiveDataLoggingTest(UpdatesInMemoryWithSensitiveDataLoggingFixture fixture)
            : base(fixture)
        {
        }

        protected override string UpdateConcurrencyTokenMessage
            => InMemoryStrings.UpdateConcurrencyTokenExceptionSensitive("Product", "Id:984ade3c-2f7b-4651-a351-642e92ab7146", "Price:3.49", "Price:1.49");
    }
}