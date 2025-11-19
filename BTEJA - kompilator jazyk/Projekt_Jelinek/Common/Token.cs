using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projekt_Jelinek.Common
{
    public class Token
    {
        public TokenType Type { get; set; }
        public string Value { get; set; }
        public int LineNumber { get; set; } // Nová vlastnost pro sledování řádku



        public Token(TokenType type, string value, int lineNumber = 0)
        {
            Type = type;
            Value = value;
            LineNumber = lineNumber;
        }

        public override string ToString()
        {
            return $"Type: {Type}, Value: '{Value}'";
        }
    }
}
