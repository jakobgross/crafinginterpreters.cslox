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
                RunFile(args[1]);
            }
            else
            {
                RunPrompt();
            }
        }

        private static void RunPrompt()
        {
            for(; ; )
            {
                Console.Write("> ");
                string line = Console.ReadLine();
                if (line == null)
                    break;
                Run(line);
            }
        }

        private static void RunFile(string inputfile)
        {
            if (!File.Exists(inputfile))
                throw new IOException($"File {inputfile} does not exists");
            string data = File.ReadAllText(inputfile);
            Run(data);
        }

        private static void Run(string data)
        {
            _error = false;
            _hadRuntimeError = false;
            Scanner scanner = new Scanner(data);
            List<Token> tokens = scanner.scanTokens();
            Parser parser = new Parser(tokens);
            List<Stmt> statements = parser.Parse();
            if (_error) return;
            interpreter.Interpret(statements);
        }

        public static void DisplayError(int line, String message)
        {
            ReportToCommandLine(line, "", message);
        }

        private static void ReportToCommandLine(int line, string where, string message)
        {
            Console.Error.WriteLine($"[line {line}] Error: {where}: {message}");
            _error = true;
        }

        internal static void DisplayError(Token token, string message)
        {
            if (token._type == TokenType.EOF)
            {
                ReportToCommandLine(token._line, "at end", message);
            }
            else
            {
                ReportToCommandLine(token._line, "at '" + token._lexme + "'", message);
            }
        }
        public static void DisplayRuntimeError(RuntimeError error)
        {
            Console.Error.WriteLine("[line " + error._token._line + "] Runtime Error: " + error.Message);
            _hadRuntimeError = true;
        }
    }
}
