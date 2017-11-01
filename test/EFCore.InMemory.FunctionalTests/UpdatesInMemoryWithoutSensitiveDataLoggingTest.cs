using Microsoft.EntityFrameworkCore.Internal;

namespace Microsoft.EntityFrameworkCore
{
    public class UpdatesInMemoryWithoutSensitiveDataLoggingTest : UpdatesInMemoryTestBase<UpdatesInMemoryWithoutSensitiveDataLoggingFixture>
    {
        public UpdatesInMemoryWithoutSensitiveDataLoggingTest(UpdatesInMemoryWithoutSensitiveDataLoggingFixture fixture)
            : base(fixture)
        {
        }

        protected override string UpdateConcurrencyTokenMessage
            => InMemoryStrings.UpdateConcurrencyTokenException("Product", "{'Price'}");
    }
}