using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static crafinginterpreters.cslox.Interpreter;

namespace crafinginterpreters.cslox
{
    public class Environment
    {
        private Dictionary<string, object> values = new Dictionary<string, object>();
        public void define(string name, object value)
        {
            values.Add(name, value);
        }
        public object get(Token name)
        {
            if (values.ContainsKey(name._lexme))
            {
                return values[name._lexme];
            }
            throw new RuntimeError(name, "Undefined Variable: '" + name._lexme + "'.");
        }

        public void assign(Token name, Object value)
        {
            if (values.ContainsKey(name._lexme))
            {
                values[name._lexme] = value;
                return;
            }
            throw new RuntimeError(name, "Undefined Variable '" + name._lexme + "'.");
        }
    }
}
