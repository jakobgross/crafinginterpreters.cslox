using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Configuration;

namespace crafinginterpreters.cslox
{
    public class Scanner
    {
        private readonly string _data;
        private List<Token> _tokens = new List<Token>();
        private int _start = 0;
        private int _current = 0;
        private int _line = 1;
        private readonly IDictionary<string,TokenType> _keywords = new Dictionary<string, TokenType>()
        {
            {"and",         TokenType.AND },
            {"class",       TokenType.CLASS},
            {"else",        TokenType.ELSE },
            {"false",       TokenType.FALSE },
            {"for",         TokenType.FOR },
            {"fun",         TokenType.FUN },
            {"if",          TokenType.IF },
            {"nil",         TokenType.NIL },
            {"or",          TokenType.OR },
            {"print",       TokenType.PRINT },
            {"return",      TokenType.RETURN },
            {"super",       TokenType.SUPER },
            {"this",        TokenType.THIS },
            {"true",        TokenType.TRUE },
            {"var",         TokenType.VAR },
            {"while",       TokenType.WHILE }

        };


        public Scanner(string data)
        {
            _data = data;
        }

        internal List<Token> scanTokens()
        {
            while (!finished())
            {
                _start = _current;
                scanToken();
            }
            _tokens.Add(new Token(TokenType.EOF, "", null, _line));
            return _tokens;
        }

        private void scanToken()
        {
            char c = advance();
            switch (c)
            {
                case '(':   addToken(TokenType.LEFT_PARENTHESIS);   break;
                case ')':   addToken(TokenType.RIGHT_PARENTHESIS);  break;
                case '{':   addToken(TokenType.LEFT_BRACE);         break;
                case '}':   addToken(TokenType.RIGHT_BRACE);        break;
                case '-':   addToken(TokenType.MINUS);              break;
                case '+':   addToken(TokenType.PLUS);               break;
                case '*':   addToken(TokenType.STAR);               break;
                //case '/':   addToken(TokenType.SLASH);              break;
                case '.':   addToken(TokenType.DOT);                break;
                case ',':   addToken(TokenType.COMMA);              break;
                case ';':   addToken(TokenType.SEMICOLON);          break;

                case '!':   addToken(match('=') ? TokenType.BANG_EQUAL : TokenType.BANG);       break;
                case '=':   addToken(match('=') ? TokenType.EQUAL_EQUAL : TokenType.EQUAL);     break;
                case '<':   addToken(match('=') ? TokenType.LESS_EQUAL : TokenType.LESS);       break;
                case '>':   addToken(match('=') ? TokenType.GREATER_EQUAL : TokenType.GREATER); break;

                // Comments
                case '/':
                    if (match('/'))
                    {
                        while (peek() != '\n' && !isAtEnd())
                            advance();
                    }
                    else if (match('*'))
                    {
                        while (peek() != '*' && peekNext() != '/' && !isAtEnd())
                        {
                            if (peek() == '\n') _line++;
                            advance();
                        }
                        advance();
                        advance();
                    }
                    else
                        addToken(TokenType.SLASH);
                    break;

                // Whitespace
                case ' ':
                case '\r':
                case '\t':
                    break;
                case '\n':
                    _line++;
                    break;

                // Strings
                case '"': lexString(); break;


                default:
                    if (Char.IsDigit(c))
                    {
                        lexNumber();
                    }
                    else if (Char.IsLetter(c))
                    {
                        lexIdentifier();
                    }
                    else
                    {
                        Program.Error(_line, $"unrecognised Token: {c}");
                    }
                    break;
            }
        }

        private void lexIdentifier()
        {
            while (Char.IsLetterOrDigit(peek())) advance();
            string val = _data.Substring(_start, _current - _start);
            TokenType type;
            if (_keywords.TryGetValue(val,out type))
            {
                addToken(type);
            }
            else
            {
                addToken(TokenType.IDENTIFIER, val);
            }


        }

        private void lexNumber()
        {
            while (Char.IsDigit(peek())) advance();
            if (peek() == '.' && Char.IsDigit(peekNext()))
            {
                advance();

                while (Char.IsDigit(peek())) advance();
            }
            addToken(TokenType.NUMBER, Double.Parse(_data.Substring(_start, _current - _start),System.Globalization.NumberStyles.Any,System.Globalization.CultureInfo.InvariantCulture));
        }

        private char peekNext()
        {
            if (_current + 1 >= _data.Length) return '\0';
            return _data[_current + 1];
        }

        private void lexString()
        {
            while(peek() != '"' && !isAtEnd())
            {
                if (peek() == '\n') _line++;
                advance();
            }

            if(isAtEnd())
            {
                Program.Error(_line, "Unterminated String.");
                return;
            }
            advance();
            string val = _data.Substring(_start + 1, _current - _start - 2);
            addToken(TokenType.STRING, val);
        }

        private char peek()
        {
            if (isAtEnd())
                return '\0';
            return _data[_current];
        }

        private void addToken(TokenType type)
        {
            addToken(type, null);
        }

        private void addToken(TokenType type, object literal)
        {
            string text = _data.Substring(_start, _current - _start);
            _tokens.Add(new Token(type, text, literal, _line));
        }

        private bool isAtEnd()
        {
            return _current >= _data.Length;
        }

        private bool match(char expected)
        {
            if (isAtEnd())
                return false;
            if (_data[_current] != expected)
                return false;
            _current++;
            return true;
        }

        private char advance()
        {
            _current++;
            return _data[_current-1];
        }

        private bool finished()
        {
            return _current >= _data.Length;
        }

    }
}