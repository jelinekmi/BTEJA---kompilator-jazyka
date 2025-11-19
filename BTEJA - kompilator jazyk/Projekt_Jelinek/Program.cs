using System;
using System.IO;
using System.Collections.Generic;
using Projekt_Jelinek.Common;

namespace Projekt_Jelinek
{
    class Program
    {
        static void Main(string[] args)
        {
            // Cesta k souboru, kde bude vstupní kód
            string filePath = "../../../program1.txt";


            try
            {
                // Načtení obsahu souboru
                var input = File.ReadAllText(filePath);

                // 1. Inicializace lexeru
                var lexer = new Lexer(input);
                var tokens = lexer.Tokenize();

                Console.WriteLine("=== Tokens ===");
                foreach (var token in tokens)
                {
                    Console.WriteLine($"Token: {token.Type}, Value: {token.Value}");
                }

                // 2. Inicializace parseru
                var parser = new Parser(tokens);
                var programNode = parser.Parse();

                Console.WriteLine("\n=== AST (Abstract Syntax Tree) ===");
                Console.WriteLine(programNode);

                // 3. Spuštění interpretu
                var interpreter = new Interpreter();
                interpreter.Interpret(programNode);

                // Výpis výsledků po interpretaci
                Console.WriteLine("\n=== Final Output ===");
                Console.WriteLine("Execution completed successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nError: {ex.Message}");

            }
        }
    }
}
