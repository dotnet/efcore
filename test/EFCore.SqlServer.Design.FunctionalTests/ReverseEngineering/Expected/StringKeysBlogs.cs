using System;
using System.Collections.Generic;

namespace E2ETest.Namespace
{
    public partial class StringKeysBlogs
    {
        public StringKeysBlogs()
        {
            StringKeysPosts = new HashSet<StringKeysPosts>();
        }

        public string PrimaryKey { get; set; }
        public string AlternateKey { get; set; }
        public string IndexProperty { get; set; }
        public byte[] RowVersion { get; set; }

        public ICollection<StringKeysPosts> StringKeysPosts { get; set; }
    }
}
