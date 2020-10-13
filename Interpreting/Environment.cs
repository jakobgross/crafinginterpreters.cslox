using System;
using System.Collections.Generic;
using static crafinginterpreters.cslox.Interpreter;

namespace crafinginterpreters.cslox
{
    public class Environment
    {
        public Environment enclosing;
        private Dictionary<string, object> values = new Dictionary<string, object>();

        public Environment()
        {
            enclosing = null;
        }

        public Environment(Environment enclosing)
        {
            this.enclosing = enclosing;
        }

        public void Define(string name, object value)
        {
            values.Add(name, value);
        }
        public object Get(Token name)
        {
            if (values.ContainsKey(name._lexme))
            {
                return values[name._lexme];
            }
            if (enclosing != null) return enclosing.Get(name);
            throw new RuntimeError(name, "Undefined Variable: '" + name._lexme + "'.");
        }

        public void Assign(Token name, Object value)
        {
            if (values.ContainsKey(name._lexme))
            {
                values[name._lexme] = value;
                return;
            }
            if (enclosing != null)
            {
                enclosing.Assign(name, value);
                return;
            }
            throw new RuntimeError(name, "Undefined Variable '" + name._lexme + "'.");
        }
    }
}
