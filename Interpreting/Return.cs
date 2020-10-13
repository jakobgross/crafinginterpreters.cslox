using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace crafinginterpreters.cslox.Interpreting
{
    class Return : Exception
    {
        public readonly object value;

        public Return(object value) : base()
        {
            this.value = value;
        }
    }
}
