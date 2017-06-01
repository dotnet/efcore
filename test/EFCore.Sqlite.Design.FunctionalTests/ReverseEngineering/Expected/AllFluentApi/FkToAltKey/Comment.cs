using System;
using System.Collections.Generic;

namespace E2E.Sqlite
{
    public partial class Comment
    {
        public long Id { get; set; }
        public long UserAltId { get; set; }
        public string Contents { get; set; }

        public User UserAlt { get; set; }
    }
}
