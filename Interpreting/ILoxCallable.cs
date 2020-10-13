using System.Collections.Generic;

namespace crafinginterpreters.cslox.Interpreting
{
    public interface ILoxCallable
    {
        public object Call(Interpreter interpreter, List<object> arguments);
        public int Arity();
    }
}
