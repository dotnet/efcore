using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.EntityFrameworkCore.TestUtilities
{
    public class DocumentDbNorthwindTestStoreFactory : DocumentDbTestStoreFactory
    {
        public const string Name = "Northwind";
        public new static DocumentDbNorthwindTestStoreFactory Instance { get; } = new DocumentDbNorthwindTestStoreFactory();

        protected DocumentDbNorthwindTestStoreFactory()
        {
        }

        public override TestStore GetOrCreate(string storeName)
            => DocumentDbTestStore.GetOrCreate(Name, "Northwind.json");
    }
}
