namespace Microsoft.Data.Entity
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public class EntityContext : IDisposable
    {
        private readonly Database _database = new Database();

        public EntityContext(string nameOrConnectionString)
        {
            // TODO
        }

        public virtual int SaveChanges()
        {
            // TODO
            return 0;
        }

        public virtual Task<int> SaveChangesAsync()
        {
            return SaveChangesAsync(CancellationToken.None);
        }

        public virtual Task<int> SaveChangesAsync(CancellationToken cancellationToken)
        {
            // TODO
            return Task.FromResult(0);
        }

        public void Dispose()
        {
            // TODO
        }

        public virtual Database Database
        {
            get { return _database; }
        }
    }
}