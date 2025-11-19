using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projekt_Jelinek.Common
{
    public enum TokenType
    {
        // Keywords
        Program,
        Block,
        Var,
        Func,
        If,
        Else,
        For,
        True,
        False,
        Return,

        // Operators
        Equals,
        Plus,
        Minus,
        Multiply,
        Divide,
        LessThan,
        LessThanOrEqual,
        GreaterThan,
        GreaterThanOrEqual,
        NotEqual,
        EqualEqual,
        Not,

        // Delimiters
        LeftBrace,
        RightBrace,
        LeftParen,
        RightParen,
        LeftBracket,
        RightBracket,
        Comma,
        Semicolon,
        Colon,


        // Literals
        Identifier,
        IntegerConstant,
        RealConstant,
        CharConstant,
        BooleanConstant,
        StringConstant,

        // Types

        IntType,
        Float64Type,
        BoolType,
        StringType,

        // EOF
        EOF
    }

}