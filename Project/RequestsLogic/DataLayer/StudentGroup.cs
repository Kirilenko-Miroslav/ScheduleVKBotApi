using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic.DataLayer
{
    internal class StudentGroup
    {
        public long peer_id { get; set; }
        public string group { get; set; }
        public StudentGroup(long peer_id, string group)
        {
            this.peer_id = peer_id;
            this.group = group;
        }
    }
}
