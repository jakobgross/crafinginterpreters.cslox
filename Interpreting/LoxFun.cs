using System.Collections.Generic;

namespace crafinginterpreters.cslox.Interpreting
{
    class LoxFun : ILoxCallable
    {
        private readonly Stmt.Function _declaration;
        
        public LoxFun(Stmt.Function declaration)
        {
            _declaration = declaration;
        }

        public int Arity() => _declaration.parameters.Count;

        public object Call(Interpreter interpreter, List<object> arguments)
        {
            Environment environment = new Environment(interpreter._globals);
            for(int i = 0; i < _declaration.parameters.Count; i++)
            {
                environment.Define(_declaration.parameters[i]._lexme, arguments[i]);
            }
            try
            {
                interpreter.ExecuteBlock(_declaration.body, environment);
            }
            catch (Return returnValue)
            {
                return returnValue.value;
            }
            return null;
        }

        public override string ToString() => $"<fun {_declaration.name._lexme}> ";
    }
}
