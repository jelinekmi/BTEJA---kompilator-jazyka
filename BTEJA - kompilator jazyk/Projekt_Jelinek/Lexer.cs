using Projekt_Jelinek.Common;
using System;
using System.Collections.Generic;

namespace Projekt_Jelinek
{
    public class Lexer
    {
        private readonly string _input;
        private int _currentPosition;

        // Seznam klíčových slov pro rozpoznání
        private static readonly Dictionary<string, TokenType> Keywords = new()
        {
            { "program", TokenType.Program },
            { "var", TokenType.Var },
            { "func", TokenType.Func },
            { "if", TokenType.If },
            { "else", TokenType.Else },
            { "for", TokenType.For },
            { "true", TokenType.BooleanConstant },
            { "false", TokenType.BooleanConstant },
            { "int", TokenType.IntType },
            { "float64", TokenType.Float64Type },
            { "string", TokenType.StringType },
            { "return", TokenType.Return }
        };

        public Lexer(string input)
        {
            _input = input;
            _currentPosition = 0;
        }

        // Hlavní funkce pro tokenizaci vstupního řetězce
        public List<Token> Tokenize()
        {
            List<Token> tokens = new List<Token>();

            while (_currentPosition < _input.Length)
            {
                char currentChar = _input[_currentPosition];

                if (char.IsWhiteSpace(currentChar))
                {
                    _currentPosition++;
                    continue;
                }

                if (currentChar == '"')
                {
                    string value = ReadString();
                    tokens.Add(new Token(TokenType.StringConstant, value));
                    continue;
                }

                // Víceznakové operátory
                if (_currentPosition + 1 < _input.Length)
                {
                    string lookahead = _input.Substring(_currentPosition, 2);
                    switch (lookahead)
                    {
                        case "==":
                            tokens.Add(new Token(TokenType.EqualEqual, "=="));
                            _currentPosition += 2;
                            continue;
                        case "!=":
                            tokens.Add(new Token(TokenType.NotEqual, "!="));
                            _currentPosition += 2;
                            continue;
                        case "<=":
                            tokens.Add(new Token(TokenType.LessThanOrEqual, "<="));
                            _currentPosition += 2;
                            continue;
                        case ">=":
                            tokens.Add(new Token(TokenType.GreaterThanOrEqual, ">="));
                            _currentPosition += 2;
                            continue;
                    }
                }

                // Jednoznakové symboly
                switch (currentChar)
                {
                    case '{':
                        tokens.Add(new Token(TokenType.LeftBrace, "{"));
                        break;
                    case '}':
                        tokens.Add(new Token(TokenType.RightBrace, "}"));
                        break;
                    case '(':
                        tokens.Add(new Token(TokenType.LeftParen, "("));
                        break;
                    case ')':
                        tokens.Add(new Token(TokenType.RightParen, ")"));
                        break;
                    case '[':
                        tokens.Add(new Token(TokenType.LeftBracket, "["));
                        break;
                    case ']':
                        tokens.Add(new Token(TokenType.RightBracket, "]"));
                        break;
                    case ';':
                        tokens.Add(new Token(TokenType.Semicolon, ";"));
                        break;
                    case ':':
                        tokens.Add(new Token(TokenType.Colon, ":"));
                        break;
                    case ',':
                        tokens.Add(new Token(TokenType.Comma, ","));
                        break;
                    case '=':
                        tokens.Add(new Token(TokenType.Equals, "="));
                        break;
                    case '+':
                        tokens.Add(new Token(TokenType.Plus, "+"));
                        break;
                    case '-':
                        tokens.Add(new Token(TokenType.Minus, "-"));
                        break;
                    case '*':
                        tokens.Add(new Token(TokenType.Multiply, "*"));
                        break;
                    case '/':
                        if (_currentPosition + 1 < _input.Length && _input[_currentPosition + 1] == '/')
                        {
                            while (_currentPosition < _input.Length && _input[_currentPosition] != '\n')
                            {
                                _currentPosition++;
                            }
                            continue;
                        }
                        else if (_currentPosition + 1 < _input.Length && _input[_currentPosition + 1] == '*')
                        {
                            _currentPosition += 2;
                            while (_currentPosition + 1 < _input.Length && (_currentPosition != '*' || _currentPosition + 1 != '/'))
                            {
                                _currentPosition++;
                            }
                            _currentPosition += 2;
                            continue;
                        }
                        else
                        {
                            tokens.Add(new Token(TokenType.Divide, "/"));
                        }
                        break;
                    case '<':
                        tokens.Add(new Token(TokenType.LessThan, "<"));
                        break;
                    case '>':
                        tokens.Add(new Token(TokenType.GreaterThan, ">"));
                        break;
                    default:
                        if (char.IsLetter(currentChar)) // Identifikátory a klíčová slova
                        {
                            string value = ReadWord();
                            TokenType type = DetermineKeyword(value);
                            tokens.Add(new Token(type, value));
                        }
                        else if (char.IsDigit(currentChar)) // Čísla
                        {
                            string value = ReadNumber();
                            tokens.Add(new Token(TokenType.IntegerConstant, value));
                        }
                        else
                        {
                            throw new Exception($"Unrecognized character '{currentChar}' at position {_currentPosition}");
                        }
                        continue; // Přeskoč ruční posun, jelikož už jsme jej provedli v ReadWord nebo ReadNumber
                }

                _currentPosition++; // Posuň pozici pouze, pokud jsme ji neposouvali dříve
            }

            tokens.Add(new Token(TokenType.EOF, ""));
            return tokens;
        }

        // Pomocná funkce pro čtení slova (pro identifikátory a klíčová slova)
        private string ReadWord()
        {
            int start = _currentPosition;
            while (_currentPosition < _input.Length && (char.IsLetterOrDigit(_input[_currentPosition]) || _input[_currentPosition] == '_'))
            {
                _currentPosition++;
            }
            return _input.Substring(start, _currentPosition - start);
        }

        // Pomocná funkce pro čtení čísla
        private string ReadNumber()
        {
            int start = _currentPosition;
            bool hasDecimalPoint = false;

            while (_currentPosition < _input.Length && (char.IsDigit(_input[_currentPosition]) || (!hasDecimalPoint && _input[_currentPosition] == '.')))
            {
                if (_input[_currentPosition] == '.')
                {
                    hasDecimalPoint = true;
                }
                _currentPosition++;
            }

            return _input.Substring(start, _currentPosition - start);
        }

        // Pomocná funkce pro čtení řetězce
        private string ReadString()
        {
            int start = ++_currentPosition; // Přeskoč úvodní uvozovku
            while (_currentPosition < _input.Length && _input[_currentPosition] != '"')
            {
                _currentPosition++;
            }

            if (_currentPosition >= _input.Length)
            {
                throw new Exception("Unterminated string literal");
            }

            _currentPosition++; // Přeskoč závěrečnou uvozovku
            return _input.Substring(start, _currentPosition - start - 1);
        }

        // Pomocná funkce pro čtení znaku
        private string ReadChar()
        {
            int start = ++_currentPosition; // Přeskoč úvodní apostrof
            if (_currentPosition + 1 >= _input.Length || _input[_currentPosition + 1] != '\'')
            {
                throw new Exception("Invalid character constant");
            }

            _currentPosition += 2; // Přeskoč uzavírací apostrof
            return _input.Substring(start, 1);
        }

        // Rozpoznání klíčového slova
        private TokenType DetermineKeyword(string value)
        {
            var lowerCaseValue = value.ToLower(); // Převod na malá písmena
            return Keywords.TryGetValue(lowerCaseValue, out var tokenType) ? tokenType : TokenType.Identifier;
        }

    }
}