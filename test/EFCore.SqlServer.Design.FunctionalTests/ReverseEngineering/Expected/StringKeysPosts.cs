using System;
using System.Collections.Generic;

namespace E2ETest.Namespace
{
    public partial class StringKeysPosts
    {
        public int Id { get; set; }
        public string BlogAlternateKey { get; set; }

        public StringKeysBlogs BlogAlternateKeyNavigation { get; set; }
    }
}
