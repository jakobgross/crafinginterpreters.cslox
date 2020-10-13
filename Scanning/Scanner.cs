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
            while (!IsFinished())
            {
                _start = _current;
                ScanToken();
            }
            _tokens.Add(new Token(TokenType.EOF, "", null, _line));
            return _tokens;
        }

        private void ScanToken()
        {
            char c = Advance();
            switch (c)
            {
                case '(':   AddToken(TokenType.LEFT_PARENTHESIS);   break;
                case ')':   AddToken(TokenType.RIGHT_PARENTHESIS);  break;
                case '{':   AddToken(TokenType.LEFT_BRACE);         break;
                case '}':   AddToken(TokenType.RIGHT_BRACE);        break;
                case '-':   AddToken(TokenType.MINUS);              break;
                case '+':   AddToken(TokenType.PLUS);               break;
                case '*':   AddToken(TokenType.STAR);               break;
                //case '/':   addToken(TokenType.SLASH);              break;
                case '.':   AddToken(TokenType.DOT);                break;
                case ',':   AddToken(TokenType.COMMA);              break;
                case ';':   AddToken(TokenType.SEMICOLON);          break;

                case '!':   AddToken(Match('=') ? TokenType.BANG_EQUAL : TokenType.BANG);       break;
                case '=':   AddToken(Match('=') ? TokenType.EQUAL_EQUAL : TokenType.EQUAL);     break;
                case '<':   AddToken(Match('=') ? TokenType.LESS_EQUAL : TokenType.LESS);       break;
                case '>':   AddToken(Match('=') ? TokenType.GREATER_EQUAL : TokenType.GREATER); break;

                // Comments
                case '/':
                    if (Match('/'))
                    {
                        while (Peek() != '\n' && !IsAtEnd())
                            Advance();
                    }
                    else if (Match('*'))
                    {
                        while (Peek() != '*' && PeekNext() != '/' && !IsAtEnd())
                        {
                            if (Peek() == '\n') _line++;
                            Advance();
                        }
                        Advance();
                        Advance();
                    }
                    else
                        AddToken(TokenType.SLASH);
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
                case '"': LexString(); break;


                default:
                    if (Char.IsDigit(c))
                    {
                        LexNumber();
                    }
                    else if (Char.IsLetter(c))
                    {
                        LexIdentifier();
                    }
                    else
                    {
                        Program.DisplayError(_line, $"unrecognised Token: {c}");
                    }
                    break;
            }
        }

        private void LexIdentifier()
        {
            while (Char.IsLetterOrDigit(Peek())) Advance();
            string val = _data.Substring(_start, _current - _start);
            TokenType type;
            if (_keywords.TryGetValue(val,out type))
            {
                AddToken(type);
            }
            else
            {
                AddToken(TokenType.IDENTIFIER, val);
            }


        }

        private void LexNumber()
        {
            while (Char.IsDigit(Peek())) Advance();
            if (Peek() == '.' && Char.IsDigit(PeekNext()))
            {
                Advance();

                while (Char.IsDigit(Peek())) Advance();
            }
            AddToken(TokenType.NUMBER, Double.Parse(_data.Substring(_start, _current - _start),System.Globalization.NumberStyles.Any,System.Globalization.CultureInfo.InvariantCulture));
        }

        private char PeekNext()
        {
            if (_current + 1 >= _data.Length) return '\0';
            return _data[_current + 1];
        }

        private void LexString()
        {
            while(Peek() != '"' && !IsAtEnd())
            {
                if (Peek() == '\n') _line++;
                Advance();
            }

            if(IsAtEnd())
            {
                Program.DisplayError(_line, "Unterminated String.");
                return;
            }
            Advance();
            string val = _data.Substring(_start + 1, _current - _start - 2);
            AddToken(TokenType.STRING, val);
        }

        private char Peek()
        {
            if (IsAtEnd())
                return '\0';
            return _data[_current];
        }

        private void AddToken(TokenType type)
        {
            AddToken(type, null);
        }

        private void AddToken(TokenType type, object literal)
        {
            string text = _data.Substring(_start, _current - _start);
            _tokens.Add(new Token(type, text, literal, _line));
        }

        private bool IsAtEnd()
        {
            return _current >= _data.Length;
        }

        private bool Match(char expected)
        {
            if (IsAtEnd())
                return false;
            if (_data[_current] != expected)
                return false;
            _current++;
            return true;
        }

        private char Advance()
        {
            _current++;
            return _data[_current-1];
        }

        private bool IsFinished()
        {
            return _current >= _data.Length;
        }

    }
}