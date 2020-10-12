using System;
using System.CodeDom;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace crafinginterpreters.cslox
{
    public class Parser 
    { 

        private readonly List<Token> _tokens;
        private int _current = 0;

        class ParseError : Exception { }

        public Parser(List<Token> tokens)
        {
            _tokens = tokens;
        }

        public List<Stmt> Parse()
        {
            List<Stmt> statements = new List<Stmt>();
            while (!isAtEnd())
            {
                statements.Add(declaration());
            }
            return statements;
        }

        private Stmt declaration()
        {
            try
            {
                if (match(TokenType.VAR)) return varDeclaration();
                return statement();
            }
            catch (ParseError e)
            {
                synchronize();
                return null;
            }
        }

        private Stmt varDeclaration()
        {
            Token name = consume(TokenType.IDENTIFIER, "Expect Variable Name.");
            Expr intitialize = null;
            if (match(TokenType.EQUAL))
            {
                intitialize = express();
            }
            consume(TokenType.SEMICOLON, "Expect ; after Variable Declaration");
            return new Stmt.Var(name, intitialize);

        }

        private Stmt statement()
        {
            if (match(TokenType.PRINT)) return printStatement();
            return expressionStatement();
        
        }

        private Stmt expressionStatement()
        {
            Expr expr = express();
            consume(TokenType.SEMICOLON, "Expect ';' after expression");
            return new Stmt.Expression(expr);
        }

        private Expr assignment()
        {
            Expr expr = equality();
            if (match(TokenType.EQUAL))
            {
                Token equals = previous();
                Expr value = assignment();

                if (expr is Expr.Variable)
                {
                    Token name = ((Expr.Variable)expr).Name;
                    return new Expr.Assign(name, value);
                }
                error(equals, "Invalid assignment target.");
            }
            return expr;
        }
        private Stmt printStatement()
        {
            Expr value = express();
            consume(TokenType.SEMICOLON, "Expect ';' after value");
            return new Stmt.Print(value);
        }

        private Expr express()
        {
            return assignment();
        }

        private Expr equality()
        {
            Expr expr = comparison();
            while (match(TokenType.BANG_EQUAL, TokenType.EQUAL_EQUAL))
            {
                Token op = previous();
                Expr right = comparison();
                expr = new Expr.Binary(expr, op, right);
            }
            return expr;
        }

        private Expr comparison()
        {
            Expr expr = addition();
            while (match(TokenType.GREATER, TokenType.GREATER_EQUAL,TokenType.LESS, TokenType.LESS_EQUAL))
            {
                Token op = previous();
                Expr right = addition();
                expr = new Expr.Binary(expr, op, right);
            }
            return expr;
        }

        private Expr addition()
        {
            Expr expr = multiplication();
            while(match(TokenType.MINUS, TokenType.PLUS))
            {
                Token op = previous();
                Expr right = multiplication();
                expr = new Expr.Binary(expr, op, right);
            }
            return expr;
        }

        private Expr multiplication()
        {
            Expr expr = unary();

            while (match(TokenType.SLASH, TokenType.STAR))
            {
                Token op = previous();
                Expr right = unary();
                expr = new Expr.Binary(expr, op, right);
            }
            return expr;
        }

        private Expr unary()
        {
            if (match(TokenType.BANG, TokenType.MINUS))
            {
                Token op = previous();
                Expr right = unary();
                return new Expr.Unary(op, right);
            }
            return primary();
        }

        private Expr primary()
        {
            if (match(TokenType.FALSE))     return new Expr.Literal(false);
            if (match(TokenType.TRUE))      return new Expr.Literal(true);
            if (match(TokenType.NIL))       return new Expr.Literal(null);
            if (match(TokenType.NUMBER, TokenType.STRING)) return new Expr.Literal(previous()._literal);
            if (match(TokenType.IDENTIFIER)) return new Expr.Variable(previous());
            if (match(TokenType.LEFT_PARENTHESIS)){
                Expr expr = express();
                consume(TokenType.RIGHT_PARENTHESIS, "Expect ')' after expression");
                return new Expr.Grouping(expr);
            }
            throw error(peek(), "Expect Expression");
        }

        private Token consume(TokenType type, string message)
        {
            if (check(type)) return advance();
            throw error(peek(), message);
        }

        private ParseError error(Token token, String message)
        {
            Program.error(token, message);
            return new ParseError();
        }

        private bool match(params TokenType[] types)
        {
            foreach(TokenType type in types)
            {
                if (check(type))
                {
                    advance();
                    return true;
                }
            }
            return false;

        }

        private Token advance()
        {
            if (!isAtEnd()) _current++;
            return previous();
        }

        private Token previous()
        {
            return _tokens[_current - 1];
        }

        private bool isAtEnd()
        {
            return peek()._type == TokenType.EOF;
        }

        private bool check(TokenType type)
        {
            if (isAtEnd()) return false;
            return peek()._type == type;
        }

        private Token peek()
        {
            return _tokens[_current];
        }

        private void synchronize()
        {
            advance();
            while (!isAtEnd())
            {
                if (previous()._type == TokenType.SEMICOLON) return;
                switch (peek()._type)
                {
                    case TokenType.CLASS:
                    case TokenType.FUN:
                    case TokenType.VAR:
                    case TokenType.FOR:
                    case TokenType.IF:
                    case TokenType.WHILE:
                    case TokenType.PRINT:
                    case TokenType.RETURN:
                        return;
                }
                advance();
            }
        }

    }
}
