using System.Data.SqlTypes;
using System.IO;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Microsoft.EntityFrameworkCore.SqlServer.Storage
{
    internal class SqlServerHierarchyIdValueConverter : ValueConverter<HierarchyId, SqlBytes>
    {
        public SqlServerHierarchyIdValueConverter()
            : base(h => toProvider(h), b => fromProvider(b))
        {
        }

        private static SqlBytes toProvider(HierarchyId hid)
        {
            using (var memory = new MemoryStream())
            using (var writer = new BinaryWriter(memory))
            {
                hid.Write(writer);
                return new SqlBytes(memory.ToArray());
            }
        }

        private static HierarchyId fromProvider(SqlBytes bytes)
        {
            using (var memory = new MemoryStream(bytes.Value))
            using (var reader = new BinaryReader(memory))
            {
                return HierarchyId.Read(reader);
            }
        }
    }
}
