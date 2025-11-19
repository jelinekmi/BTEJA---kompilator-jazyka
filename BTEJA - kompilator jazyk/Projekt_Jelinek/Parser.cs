using Projekt_Jelinek.Common;
using System;
using System.Collections.Generic;

namespace Projekt_Jelinek
{
    public abstract class AstNode { }

    public class ProgramNode : AstNode
    {
        public List<AstNode> Statements { get; } = new List<AstNode>();
    }

    public class VariableDeclarationNode : AstNode
    {
        public string Name { get; }
        public string Type { get; }
        public AstNode InitialValue { get; }

        public VariableDeclarationNode(string name, string type, AstNode initialValue = null)
        {
            Name = name;
            Type = type;
            InitialValue = initialValue;
        }
    }

    public class AssignmentNode : AstNode
    {
        public string VariableName { get; }
        public AstNode Value { get; }

        public AssignmentNode(string variableName, AstNode value)
        {
            VariableName = variableName;
            Value = value;
        }
    }

    public class ReturnNode : AstNode
    {
        public AstNode Expression { get; }

        public ReturnNode(AstNode expression)
        {
            Expression = expression;
        }
    }

    public class IfNode : AstNode
    {
        public AstNode Condition { get; }
        public List<AstNode> ThenStatements { get; }
        public List<AstNode> ElseStatements { get; }

        public IfNode(AstNode condition, List<AstNode> thenStatements, List<AstNode> elseStatements = null)
        {
            Condition = condition;
            ThenStatements = thenStatements;
            ElseStatements = elseStatements ?? new List<AstNode>();
        }
    }

    public class ForNode : AstNode
    {
        public AstNode Initialization { get; }
        public AstNode Condition { get; }
        public AstNode Increment { get; }
        public List<AstNode> Body { get; }

        public ForNode(AstNode initialization, AstNode condition, AstNode increment, List<AstNode> body)
        {
            Initialization = initialization;
            Condition = condition;
            Increment = increment;
            Body = body;
        }
    }

    public class FunctionDeclarationNode : AstNode
    {
        public string Name { get; }
        public string ReturnType { get; }
        public List<(string Type, string Name)> Parameters { get; }
        public List<AstNode> Body { get; }

        public FunctionDeclarationNode(string name, string returnType, List<(string Type, string Name)> parameters, List<AstNode> body)
        {
            Name = name;
            ReturnType = returnType;
            Parameters = parameters;
            Body = body;
        }
    }


    public class FunctionCallNode : AstNode
    {
        public string FunctionName { get; }
        public List<AstNode> Arguments { get; }

        public FunctionCallNode(string functionName, List<AstNode> arguments)
        {
            FunctionName = functionName;
            Arguments = arguments;
        }
    }

    public class ExpressionNode : AstNode
    {
        public string Operator { get; }
        public AstNode Left { get; }
        public AstNode Right { get; }

        public ExpressionNode(AstNode left, string op, AstNode right)
        {
            Left = left;
            Operator = op;
            Right = right;
        }
    }

    public class LiteralNode : AstNode
    {
        public object Value { get; }

        public LiteralNode(object value)
        {
            Value = value;
        }
    }

    public class ArrayAccessNode : AstNode
    {
        public IdentifierNode Array { get; }
        public AstNode Index { get; }

        public ArrayAccessNode(IdentifierNode array, AstNode index)
        {
            Array = array;
            Index = index;
        }
    }
    public class ArrayLiteralNode : AstNode
    {
        public List<AstNode> Elements { get; }

        public ArrayLiteralNode(List<AstNode> elements)
        {
            Elements = elements;
        }
    }

    public class IdentifierNode : AstNode
    {
        public string Name { get; }

        public IdentifierNode(string name)
        {
            Name = name;
        }
    }

    public class Parser
    {
        private List<Token> _tokens;
        private int _currentIndex;

        public Parser(List<Token> tokens)
        {
            _tokens = tokens;
            _currentIndex = 0;
        }

        public ProgramNode Parse()
        {
            var program = new ProgramNode();
            while (!IsAtEnd())
            {
                program.Statements.Add(ParseStatement());
            }
            return program;
        }

        private AstNode ParseStatement()
        {
            if (Check(TokenType.Var))
                return ParseVariableDeclaration();

            if (Check(TokenType.Func))
                return ParseFunctionDeclaration();

            if (Check(TokenType.If))
                return ParseIfStatement();

            if (Check(TokenType.For))
                return ParseForStatement();

            if (Check(TokenType.Return))
                return ParseReturnStatement();

            if (Check(TokenType.Identifier) && PeekNext().Type == TokenType.Equals)
                return ParseAssignment();

            if (Check(TokenType.Identifier) && PeekNext().Type == TokenType.LeftParen)
                return ParseFunctionCall();

            throw new Exception($"Unexpected token: {Peek().Type}");
        }

        private AstNode ParseVariableDeclaration()
        {
            Consume(TokenType.Var, "Expected 'var'.");
            string name = Consume(TokenType.Identifier, "Expected variable name.").Value;
            Consume(TokenType.Colon, "Expected ':'.");
            string type = ParseType();

            AstNode initialValue = null;
            if (Match(TokenType.Equals))
                initialValue = ParseExpression();

            Consume(TokenType.Semicolon, "Expected ';' after declaration.");
            return new VariableDeclarationNode(name, type, initialValue);
        }

        private string ParseType()
        {
            Console.WriteLine($"Parsing type, current token: {Peek().Type}");

            if (Match(TokenType.LeftBracket))
            {
                var elementTypes = new List<string>();

                do
                {
                    elementTypes.Add(ParseType());
                } while (Match(TokenType.Comma));

                Consume(TokenType.RightBracket, "Expected ']' after array type.");

                return $"[{string.Join(", ", elementTypes)}]";
            }

            if (Match(TokenType.IntType)) return "int";
            if (Match(TokenType.Float64Type)) return "real";
            if (Match(TokenType.StringType)) return "string";
            if (Match(TokenType.BoolType)) return "bool";

            throw new Exception("Expected valid type.");
        }


        private AssignmentNode ParseAssignment()
        {
            string name = Consume(TokenType.Identifier, "Expected variable name.").Value;
            Consume(TokenType.Equals, "Expected '='.");
            AstNode value;
            if (Check(TokenType.Identifier) && PeekNext().Type == TokenType.LeftParen)
            {
                value = ParseFunctionCall();
            }
            else
            {
                value = ParseExpression();
            }
            Consume(TokenType.Semicolon, "Expected ';'.");
            return new AssignmentNode(name, value);
        }

        private FunctionDeclarationNode ParseFunctionDeclaration()
        {
            Consume(TokenType.Func, "Expected 'func'.");
            string name = Consume(TokenType.Identifier, "Expected function name.").Value;

            Consume(TokenType.LeftParen, "Expected '('.");

            var parameters = new List<(string Type, string Name)>();
            while (!Check(TokenType.RightParen))
            {
                string paramName = Consume(TokenType.Identifier, "Expected parameter name.").Value;
                Consume(TokenType.Colon, "Expected ':' after parameter name.");
                string paramType = ParseType();
                parameters.Add((paramType, paramName));

                if (!Match(TokenType.Comma))
                    break;
            }

            Consume(TokenType.RightParen, "Expected ')'.");

            string returnType = "void";
            if (Match(TokenType.Colon))
            {
                returnType = ParseType();
            }

            Consume(TokenType.LeftBrace, "Expected '{'.");

            var body = ParseFunctionBlock(returnType);

            return new FunctionDeclarationNode(name, returnType, parameters, body);
        }


        private FunctionCallNode ParseFunctionCall()
        {
            string functionName = Consume(TokenType.Identifier, "Expected function name.").Value;
            Consume(TokenType.LeftParen, "Expected '('.");

            var arguments = new List<AstNode>();
            while (!Match(TokenType.RightParen))
            {
                arguments.Add(ParseExpression());
                if (!Check(TokenType.RightParen))
                    Consume(TokenType.Comma, "Expected ',' between arguments.");
            }

            return new FunctionCallNode(functionName, arguments);
        }

        private IfNode ParseIfStatement()
        {
            Consume(TokenType.If, "Expected 'if'.");
            var condition = ParseExpression();
            Consume(TokenType.LeftBrace, "Expected '{'.");

            var thenStatements = ParseBlock();
            List<AstNode> elseStatements = null;

            if (Match(TokenType.Else))
            {
                if (Check(TokenType.If))
                {
                    elseStatements = new List<AstNode> { ParseIfStatement() };
                }
                else
                {
                    Consume(TokenType.LeftBrace, "Expected '{'.");
                    elseStatements = ParseBlock();
                }
            }

            return new IfNode(condition, thenStatements, elseStatements);
        }


        private ForNode ParseForStatement()
        {
            Consume(TokenType.For, "Expected 'for'.");
            Consume(TokenType.LeftParen, "Expected '('.");

            var initialization = ParseStatement();
            var condition = ParseExpression();
            Consume(TokenType.Semicolon, "Expected ';'.");
            var increment = ParseStatement();

            Consume(TokenType.RightParen, "Expected ')'.");
            Consume(TokenType.LeftBrace, "Expected '{'.");

            var body = ParseBlock();
            return new ForNode(initialization, condition, increment, body);
        }

        private AstNode ParseReturnStatement()
        {
            Consume(TokenType.Return, "Expected 'return'.");

            AstNode returnValue = null;
            if (!Check(TokenType.Semicolon))
            {
                returnValue = ParseExpression();
            }

            Consume(TokenType.Semicolon, "Expected ';' after return statement.");
            return new ReturnNode(returnValue);
        }


        private List<AstNode> ParseBlock()
        {
            var statements = new List<AstNode>();
            while (!Check(TokenType.RightBrace) && !IsAtEnd())
                statements.Add(ParseStatement());

            Consume(TokenType.RightBrace, "Expected '}'.");
            return statements;
        }
        private List<AstNode> ParseFunctionBlock(string returnType)
        {
            var statements = new List<AstNode>();
            bool hasReturn = false;

            while (!Check(TokenType.RightBrace) && !IsAtEnd())
            {
                var statement = ParseStatement();
                statements.Add(statement);

                if (statement is ReturnNode)
                {
                    hasReturn = true;
                }
            }

            Consume(TokenType.RightBrace, "Expected '}'.");

            if (returnType != "void" && !hasReturn)
            {
                throw new Exception($"Function with return type '{returnType}' must have a return statement.");
            }

            return statements;
        }


        private AstNode ParseExpression() => ParseEquality();

        private AstNode ParseEquality()
        {
            var node = ParseComparison();
            while (Match(TokenType.EqualEqual, TokenType.NotEqual))
                node = new ExpressionNode(node, Previous().Value, ParseComparison());
            return node;
        }

        private AstNode ParseComparison()
        {
            var node = ParseTerm();
            while (Match(TokenType.LessThan, TokenType.LessThanOrEqual, TokenType.GreaterThan, TokenType.GreaterThanOrEqual))
                node = new ExpressionNode(node, Previous().Value, ParseTerm());
            return node;
        }

        private AstNode ParseTerm()
        {
            var node = ParseFactor();
            while (Match(TokenType.Plus, TokenType.Minus))
                node = new ExpressionNode(node, Previous().Value, ParseFactor());
            return node;
        }

        private AstNode ParseFactor()
        {
            var node = ParseUnary();
            while (Match(TokenType.Multiply, TokenType.Divide))
                node = new ExpressionNode(node, Previous().Value, ParseUnary());
            return node;
        }

        private AstNode ParseUnary()
        {
            if (Match(TokenType.Not))
                return new ExpressionNode(null, Previous().Value, ParseUnary());
            return ParsePrimary();
        }

        private AstNode ParsePrimary()
        {
            if (Match(TokenType.IntegerConstant, TokenType.RealConstant, TokenType.StringConstant))
                return new LiteralNode(Previous().Value);

            if (Match(TokenType.LeftBracket))
            {
                var elements = new List<AstNode>();

                if (!Check(TokenType.RightBracket))
                {
                    do
                    {
                        elements.Add(ParseExpression());
                    } while (Match(TokenType.Comma));
                }

                Consume(TokenType.RightBracket, "Expected ']' after array literal.");

                return new ArrayLiteralNode(elements);
            }

            if (Match(TokenType.Identifier))
            {
                AstNode identifier = new IdentifierNode(Previous().Value);

                while (Match(TokenType.LeftBracket))
                {
                    var index = ParseExpression();
                    Consume(TokenType.RightBracket, "Expected ']' after array index.");
                    identifier = new ArrayAccessNode((IdentifierNode)identifier, index);
                }

                return identifier;
            }

            if (Match(TokenType.LeftParen))
            {
                var expr = ParseExpression();
                Consume(TokenType.RightParen, "Expected ')' after expression.");
                return expr;
            }

            throw new Exception("Unexpected token in expression.");
        }

        private bool Match(params TokenType[] types)
        {
            foreach (var type in types)
            {
                if (Check(type))
                {
                    Advance();
                    return true;
                }
            }
            return false;
        }

        private bool Check(TokenType type) => !IsAtEnd() && Peek().Type == type;

        private Token Peek() => _currentIndex < _tokens.Count ? _tokens[_currentIndex] : new Token(TokenType.EOF, "");

        private Token Advance() => !IsAtEnd() ? _tokens[_currentIndex++] : new Token(TokenType.EOF, "");

        private bool IsAtEnd() => Peek().Type == TokenType.EOF;

        private Token Consume(TokenType type, string errorMessage)
        {
            if (Check(type))
                return Advance();

            throw new Exception(errorMessage);
        }

        private Token PeekNext() => _currentIndex + 1 < _tokens.Count ? _tokens[_currentIndex + 1] : new Token(TokenType.EOF, "");

        private Token Previous() => _tokens[_currentIndex - 1];
    }
}