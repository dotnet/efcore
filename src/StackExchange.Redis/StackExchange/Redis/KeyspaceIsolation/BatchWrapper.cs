using System;

namespace StackExchange.Redis.KeyspaceIsolation
{
    internal sealed class BatchWrapper : WrapperBase<IBatch>, IBatch
    {
        public BatchWrapper(IBatch inner, byte[] prefix)
            : base(inner, prefix)
        {
        }

        public void Execute()
        {
            this.Inner.Execute();
        }
    }
}
