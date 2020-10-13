using System;
using System.Collections.Generic;

namespace crafinginterpreters.cslox.Interpreting
{
    public abstract class StandartLibFun: ILoxCallable
    {
        public abstract int Arity();
        public abstract object Call(Interpreter interpreter, List<object> arguments);




        public class Clock : StandartLibFun
        {
            public override int Arity() => 0;
            //Milliseconds since 00:00:00 01.01.0001. 1Tik == 100ns
            public override object Call(Interpreter interpreter, List<object> arguments) => (double)DateTime.Now.Ticks / 10000;
            public override string ToString() => "<native fun>";
        }
    }
}
