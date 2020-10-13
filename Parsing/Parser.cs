using System;
using System.Collections.Generic;
using System.Linq;

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
            while (!IsLastToken())
            {
                statements.Add(ParseDeclaration());
            }
            return statements;
        }

        private Stmt ParseDeclaration()
        {
            try
            {
                if (MatchTokens(TokenType.FUN)) return ParseFunDeclaration("function");
                if (MatchTokens(TokenType.VAR)) return ParseVarDeclaration();
                return ParseStatement();
            }
            catch (ParseError e)
            {
                SynchronizeParser();
                return null;
            }
        }

        private Stmt ParseFunDeclaration(string kind)
        {
            Token name = ConsumeToken(TokenType.IDENTIFIER, $"Expect {kind} name");
            ConsumeToken(TokenType.LEFT_PARENTHESIS, $"Expecting '(' after {kind} name.");
            List<Token> args = new List<Token>();
            if (!CheckToken(TokenType.RIGHT_PARENTHESIS))
            {
                do
                {
                    if (args.Count() >= 255) DisplayParserError(PeekToken(), "Can't have more than 255 arguments");
                    args.Add(ConsumeToken(TokenType.IDENTIFIER, "Expect argument name"));
                } while (MatchTokens(TokenType.COMMA));
            }
            ConsumeToken(TokenType.RIGHT_PARENTHESIS, "Expect ')' after arguments");
            ConsumeToken(TokenType.LEFT_BRACE, $"Expect '{{' after {kind} declaration");
            List<Stmt> body = ParseBlockStatement();
            return new Stmt.Function(name, args, body);
        }

        private Stmt ParseVarDeclaration()
        {
            Token name = ConsumeToken(TokenType.IDENTIFIER, "Expect Variable Name.");
            Expr intitialize = null;
            if (MatchTokens(TokenType.EQUAL))
            {
                intitialize = ParseExpression();
            }
            ConsumeToken(TokenType.SEMICOLON, "Expect ; after Variable Declaration");
            return new Stmt.Var(name, intitialize);

        }

        private Stmt ParseStatement()
        {
            if (MatchTokens(TokenType.FOR)) return ParseForStatement();
            if (MatchTokens(TokenType.IF)) return ParseIfStatement();
            if (MatchTokens(TokenType.PRINT)) return ParsePrintStatement();
            if (MatchTokens(TokenType.RETURN)) return ParseReturnStatement();
            if (MatchTokens(TokenType.WHILE)) return ParseWhileStatement();
            if (MatchTokens(TokenType.LEFT_BRACE)) return new Stmt.Block(ParseBlockStatement());
            return ParseExpressionStatement();

        }

        private Stmt ParseReturnStatement()
        {
            Token keyword = PreviousToken();
            Expr value = null;
            if (!CheckToken(TokenType.SEMICOLON))
            {
                value = ParseExpression();
            }
            ConsumeToken(TokenType.SEMICOLON, "Expect ';' after return keyword or return value");
            return new Stmt.Return(keyword, value);
        }

        private Stmt ParseForStatement()
        {
            ConsumeToken(TokenType.LEFT_PARENTHESIS, "Expect '(' after for keyword");
            Stmt initializer;

            if (MatchTokens(TokenType.SEMICOLON)) initializer = null;
            else if (MatchTokens(TokenType.VAR)) initializer = ParseVarDeclaration();
            else initializer = ParseExpressionStatement();

            Expr condition = null;
            if (!CheckToken(TokenType.SEMICOLON)) condition = ParseExpression();
            ConsumeToken(TokenType.SEMICOLON, "Expect ';' after for loop condition");
            Expr increment = null;
            if (!CheckToken(TokenType.RIGHT_PARENTHESIS)) increment = ParseExpression();
            ConsumeToken(TokenType.RIGHT_PARENTHESIS, "Expect ')' after for clause");
            Stmt body = ParseStatement();
            if(increment != null)
            {
                body = new Stmt.Block(new List<Stmt>() { body, new Stmt.Expression(increment) });
            }
            if (condition is null) condition = new Expr.Literal(true);
            body = new Stmt.While(condition, body);
            if(initializer != null)
            {
                body = new Stmt.Block(new List<Stmt>() { initializer, body });
            }
            return body;

        }

        private Stmt ParseWhileStatement()
        {
            ConsumeToken(TokenType.LEFT_PARENTHESIS, "Expect '(' after while keyword");
            Expr condition = ParseExpression();
            ConsumeToken(TokenType.RIGHT_PARENTHESIS, "Expect ')' after while condition");
            Stmt body = ParseStatement();
            return new Stmt.While(condition, body);
        }

        private Stmt ParseIfStatement()
        {
            ConsumeToken(TokenType.LEFT_PARENTHESIS, "Expect '(' after if keyword");
            Expr condition = ParseExpression();
            ConsumeToken(TokenType.RIGHT_PARENTHESIS, "Expect ')' after if condition");
            Stmt thenBranch = ParseStatement();
            Stmt elseBranch = null;
            if (MatchTokens(TokenType.ELSE))
            {
                elseBranch = ParseStatement();
            }
            return new Stmt.If(condition, thenBranch, elseBranch);
        }

        private List<Stmt> ParseBlockStatement()
        {
            List<Stmt> statements = new List<Stmt>();
            while(!CheckToken(TokenType.RIGHT_BRACE) && !IsLastToken())
            {
                statements.Add(ParseDeclaration());
            }
            ConsumeToken(TokenType.RIGHT_BRACE, "Expect '}' after Block");
            return statements;
        }

        private Stmt ParseExpressionStatement()
        {
            Expr expr = ParseExpression();
            ConsumeToken(TokenType.SEMICOLON, "Expect ';' after expression");
            return new Stmt.Expression(expr);
        }

        private Expr ParseAssignmentExpression()
        {
            Expr expr = ParseOrExpression();
            if (MatchTokens(TokenType.EQUAL))
            {
                Token equals = PreviousToken();
                Expr value = ParseAssignmentExpression();

                if (expr is Expr.Variable)
                {
                    Token name = ((Expr.Variable)expr).Name;
                    return new Expr.Assign(name, value);
                }
                DisplayParserError(equals, "Invalid assignment target.");
            }
            return expr;
        }

        private Expr ParseOrExpression()
        {
            Expr expr = and();
            while (MatchTokens(TokenType.OR))
            {
                Token op = PreviousToken();
                Expr right = and();
                expr = new Expr.Logical(expr, op, right);
            }
            return expr;
        }

        private Expr and()
        {
            Expr expr = ParseEqualityExpression();
            while (MatchTokens(TokenType.AND))
            {
                Token op = PreviousToken();
                Expr right = ParseEqualityExpression();
                expr = new Expr.Logical(expr, op, right);
            }
            return expr;
        }

        private Stmt ParsePrintStatement()
        {
            Expr value = ParseExpression();
            ConsumeToken(TokenType.SEMICOLON, "Expect ';' after value");
            return new Stmt.Print(value);
        }

        private Expr ParseExpression()
        {
            return ParseAssignmentExpression();
        }

        private Expr ParseEqualityExpression()
        {
            Expr expr = ParseComparisionExpression();
            while (MatchTokens(TokenType.BANG_EQUAL, TokenType.EQUAL_EQUAL))
            {
                Token op = PreviousToken();
                Expr right = ParseComparisionExpression();
                expr = new Expr.Binary(expr, op, right);
            }
            return expr;
        }

        private Expr ParseComparisionExpression()
        {
            Expr expr = ParseAdditionExpression();
            while (MatchTokens(TokenType.GREATER, TokenType.GREATER_EQUAL, TokenType.LESS, TokenType.LESS_EQUAL))
            {
                Token op = PreviousToken();
                Expr right = ParseAdditionExpression();
                expr = new Expr.Binary(expr, op, right);
            }
            return expr;
        }

        private Expr ParseAdditionExpression()
        {
            Expr expr = ParseMultiplicationExpression();
            while (MatchTokens(TokenType.MINUS, TokenType.PLUS))
            {
                Token op = PreviousToken();
                Expr right = ParseMultiplicationExpression();
                expr = new Expr.Binary(expr, op, right);
            }
            return expr;
        }

        private Expr ParseMultiplicationExpression()
        {
            Expr expr = ParseUnaryExpression();

            while (MatchTokens(TokenType.SLASH, TokenType.STAR))
            {
                Token op = PreviousToken();
                Expr right = ParseUnaryExpression();
                expr = new Expr.Binary(expr, op, right);
            }
            return expr;
        }

        private Expr ParseUnaryExpression()
        {
            if (MatchTokens(TokenType.BANG, TokenType.MINUS))
            {
                Token op = PreviousToken();
                Expr right = ParseUnaryExpression();
                return new Expr.Unary(op, right);
            }
            return ParseCallExpression();
        }

        private Expr ParseCallExpression()
        {
            Expr expr = ParsePrimaryExpression();
            while (true)
            {
                if (MatchTokens(TokenType.LEFT_PARENTHESIS)) expr = FinishCallExpression(expr);
                else break;
            }
            return expr;
        }

        private Expr FinishCallExpression(Expr callee)
        {
            List<Expr> arguments = new List<Expr>();
            if (!CheckToken(TokenType.RIGHT_PARENTHESIS))
            {
                do
                {
                    if(arguments.Count >= 255) DisplayParserError(PeekToken(), "Function call mustn't have more than 255 arguments");  
                    arguments.Add(ParseExpression());
                } while (MatchTokens(TokenType.COMMA));
            }
            Token paren = ConsumeToken(TokenType.RIGHT_PARENTHESIS, " Expect ')' after function arguments");
            return new Expr.Call(callee, paren, arguments);
        }

        private Expr ParsePrimaryExpression()
        {
            if (MatchTokens(TokenType.FALSE)) return new Expr.Literal(false);
            if (MatchTokens(TokenType.TRUE)) return new Expr.Literal(true);
            if (MatchTokens(TokenType.NIL)) return new Expr.Literal(null);
            if (MatchTokens(TokenType.NUMBER, TokenType.STRING)) return new Expr.Literal(PreviousToken()._literal);
            if (MatchTokens(TokenType.IDENTIFIER)) return new Expr.Variable(PreviousToken());
            if (MatchTokens(TokenType.LEFT_PARENTHESIS))
            {
                Expr expr = ParseExpression();
                ConsumeToken(TokenType.RIGHT_PARENTHESIS, "Expect ')' after expression");
                return new Expr.Grouping(expr);
            }
            throw DisplayParserError(PeekToken(), "Expect Expression");
        }

        private Token ConsumeToken(TokenType type, string message)
        {
            if (CheckToken(type)) return AdvanceToken();
            throw DisplayParserError(PeekToken(), message);
        }

        private ParseError DisplayParserError(Token token, String message)
        {
            Program.DisplayError(token, message);
            return new ParseError();
        }

        private bool MatchTokens(params TokenType[] types)
        {
            foreach (TokenType type in types)
            {
                if (CheckToken(type))
                {
                    AdvanceToken();
                    return true;
                }
            }
            return false;

        }

        private Token AdvanceToken()
        {
            if (!IsLastToken()) _current++;
            return PreviousToken();
        }

        private Token PreviousToken()
        {
            return _tokens[_current - 1];
        }

        private bool IsLastToken()
        {
            return PeekToken()._type == TokenType.EOF;
        }

        private bool CheckToken(TokenType type)
        {
            if (IsLastToken()) return false;
            return PeekToken()._type == type;
        }

        private Token PeekToken()
        {
            return _tokens[_current];
        }

        private void SynchronizeParser()
        {
            AdvanceToken();
            while (!IsLastToken())
            {
                if (PreviousToken()._type == TokenType.SEMICOLON) return;
                switch (PeekToken()._type)
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
                AdvanceToken();
            }
        }

    }
}
