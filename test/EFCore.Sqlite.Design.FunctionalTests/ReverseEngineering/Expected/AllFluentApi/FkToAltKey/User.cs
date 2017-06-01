using System;
using System.Collections.Generic;

namespace E2E.Sqlite
{
    public partial class User
    {
        public User()
        {
            Comment = new HashSet<Comment>();
        }

        public long Id { get; set; }
        public long AltId { get; set; }

        public ICollection<Comment> Comment { get; set; }
    }
}
