using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace crafinginterpreters.cslox
{
    public  enum TokenType
    {
        // SINGLE CHAR TOKENS
        LEFT_PARENTHESIS, RIGHT_PARENTHESIS, LEFT_BRACE, RIGHT_BRACE,
        COMMA, DOT, MINUS, PLUS, SEMICOLON, SLASH, STAR,

        // ONE-TWO CHAR TOKENS
        BANG, BANG_EQUAL, EQUAL, EQUAL_EQUAL,
        GREATER, GREATER_EQUAL, LESS, LESS_EQUAL,

        // LITERALS
        IDENTIFIER, STRING, NUMBER,

        // KEYWORDS
        AND, OR, IF, ELSE, TRUE, FALSE, VAR, FUN, CLASS, FOR, WHILE,
        NIL, THIS, SUPER, RETURN, PRINT,

        EOF
    }
    public class Token
    {
        public readonly TokenType  _type;
        public readonly string     _lexme;
        public readonly Object     _literal;
        public readonly int        _line;

        public Token(TokenType type, string lexme, Object literal, int line)
        {
            _type = type;
            _lexme = lexme;
            _literal = literal;
            _line = line;
        }

        public override string ToString()
        {
            return $"[{_type}] {_lexme} {_literal}";
            
        }

    }
}
