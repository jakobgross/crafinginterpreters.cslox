using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static crafinginterpreters.cslox.Interpreter;

namespace crafinginterpreters.cslox
{
    class Program
    {
        private static bool _error = false;
        private static bool _hadRuntimeError;
        private static Interpreter interpreter = new Interpreter();

        static void Main(string[] args)
        {
            if (args.Length > 2)
            {
                Console.WriteLine("Usage cslox.exe [script]");
                System.Environment.Exit(-1);
            }
            else if(args.Length == 2)
            {
                runFile(args[1]);
            }
            else
            {
                runPrompt();
            }
        }

        private static void runPrompt()
        {
            for(; ; )
            {
                Console.Write("> ");
                string line = Console.ReadLine();
                if (line == null)
                    break;
                run(line);
            }
        }

        private static void runFile(string inputfile)
        {
            if (!File.Exists(inputfile))
                throw new IOException($"File {inputfile} does not exists");
            string data = File.ReadAllText(inputfile);
            run(data);
        }

        private static void run(string data)
        {
            _error = false;
            _hadRuntimeError = false;
            Scanner scanner = new Scanner(data);
            List<Token> tokens = scanner.scanTokens();
            Parser parser = new Parser(tokens);
            List<Stmt> statements = parser.Parse();
            if (_error) return;
            interpreter.interpret(statements);
        }

        public static void error(int line, String message)
        {
            report(line, "", message);
        }

        private static void report(int line, string where, string message)
        {
            Console.Error.WriteLine($"[line {line}] Error: {where}: {message}");
            _error = true;
        }

        internal static void error(Token token, string message)
        {
            if (token._type == TokenType.EOF)
            {
                report(token._line, "at end", message);
            }
            else
            {
                report(token._line, "at '" + token._lexme + "'", message);
            }
        }
        public static void runtimeError(RuntimeError error)
        {
            Console.Error.WriteLine("[line " + error._token._line + "] Runtime Error: " + error.Message);
            _hadRuntimeError = true;
        }
    }
}
