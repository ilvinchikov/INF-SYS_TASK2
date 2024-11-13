using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace task2_taskmngr
{
    class ClassConnectionDisks
    {
        public string LogicalDisk { get; set; }
        public string PhysycalDisk { get; set; }
        public ClassConnectionDisks (string LogicalDisk, string PhysycalDisk)
        {
            this.LogicalDisk = LogicalDisk;
            this.PhysycalDisk = PhysycalDisk;
        }
    }
}
