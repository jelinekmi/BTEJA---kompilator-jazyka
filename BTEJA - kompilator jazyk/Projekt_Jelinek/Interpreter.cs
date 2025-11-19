using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq; // Přidáno pro metodu OfType()

namespace Projekt_Jelinek
{
    public class Interpreter
    {
        private readonly Dictionary<string, Variable> _variables = new();
        private readonly Dictionary<string, FunctionDeclarationNode> _functions = new();

        public void Interpret(ProgramNode program)
        {
            foreach (var statement in program.Statements)
            {
                Execute(statement);
            }

            // Výpis výsledků po interpretaci
            Console.WriteLine("\nFinal variable states:");
            foreach (var (name, variable) in _variables)
            {
                Console.WriteLine($"{name} = {FormatValue(variable.Value)}");

            }
        }


        private void Execute(AstNode node)
        {
            switch (node)
            {
                case VariableDeclarationNode varDecl:
                    ExecuteVariableDeclaration(varDecl);
                    break;

                case AssignmentNode assignment:
                    ExecuteAssignment(assignment);
                    break;

                case FunctionDeclarationNode funcDecl:
                    ExecuteFunctionDeclaration(funcDecl);
                    break;

                case FunctionCallNode funcCall:
                    ExecuteFunctionCall(funcCall);
                    break;

                case IfNode ifNode:
                    ExecuteIfStatement(ifNode);
                    break;

                default:
                    throw new Exception($"Unsupported node type: {node.GetType().Name}");
            }
        }

        private void ExecuteIfStatement(IfNode node)
        {
            var condition = Evaluate(node.Condition);

            if (condition is not bool boolCondition)
            {
                throw new Exception($"Condition must evaluate to a boolean, got {condition.GetType().Name}.");
            }

            if (boolCondition)
            {
                foreach (var statement in node.ThenStatements)
                {
                    Execute(statement);
                }
            }
            else if (node.ElseStatements.Count > 0)
            {
                if (node.ElseStatements.Count == 1 && node.ElseStatements[0] is IfNode elseIfNode)
                {
                    ExecuteIfStatement(elseIfNode);
                }
                else
                {
                    foreach (var statement in node.ElseStatements)
                    {
                        Execute(statement);
                    }
                }
            }
        }



        private void ExecuteVariableDeclaration(VariableDeclarationNode node)
        {
            // Kontrola, zda proměnná již existuje
            if (_variables.ContainsKey(node.Name))
            {
                throw new Exception($"Variable '{node.Name}' is already declared.");
            }

            // Vyhodnocení inicializační hodnoty
            object value = null;
            if (node.InitialValue != null)
            {
                value = Evaluate(node.InitialValue);

                // Kontrola kompatibility typů
                if (!IsTypeCompatible(node.Type, value))
                {
                    throw new Exception($"Type mismatch: Variable '{node.Name}' of type '{node.Type}' cannot be assigned a value of type '{value.GetType().Name}'.");
                }
            }

            // Uložení proměnné do paměti
            _variables[node.Name] = new Variable(node.Name, value, node.Type);
            Console.WriteLine($"Declared variable: {node.Type} {node.Name} = {FormatValue(value)}");

        }



        private void ExecuteAssignment(AssignmentNode node)
        {
            if (!_variables.ContainsKey(node.VariableName))
            {
                throw new Exception($"Variable '{node.VariableName}' is not declared.");
            }

            var variable = _variables[node.VariableName];
            var value = Evaluate(node.Value);

            if (!IsTypeCompatible(variable.Type, value))
            {
                throw new Exception($"Type mismatch: Cannot assign {value.GetType().Name} to {variable.Type}.");
            }
            _variables[node.VariableName] = new Variable(variable.Name, value, variable.Type);

            Console.WriteLine($"Assigned: {node.VariableName} = {FormatValue(value)}");

        }

        private void ExecuteFunctionDeclaration(FunctionDeclarationNode node)
        {
            if (_functions.ContainsKey(node.Name))
            {
                throw new Exception($"Function '{node.Name}' is already declared.");
            }

            _functions[node.Name] = node;
            Console.WriteLine($"Function declared: {node.Name}");
        }

        private object ExecuteFunctionCall(FunctionCallNode node)
        {
            if (!_functions.ContainsKey(node.FunctionName))
            {
                throw new Exception($"Function '{node.FunctionName}' is not defined.");
            }

            var function = _functions[node.FunctionName];
            var arguments = new List<object>();

            foreach (var argument in node.Arguments)
            {
                arguments.Add(Evaluate(argument));
            }

            return CallFunction(function, arguments);
        }


        private object CallFunction(FunctionDeclarationNode function, List<object> arguments)
        {
            if (function.Parameters.Count != arguments.Count)
            {
                throw new Exception($"Function '{function.Name}' expects {function.Parameters.Count} arguments, but {arguments.Count} were provided.");
            }

            // Záloha aktuálních proměnných
            var backupVariables = new Dictionary<string, Variable>(_variables);

            try
            {
                // Přiřazení argumentů k parametrům
                for (int i = 0; i < function.Parameters.Count; i++)
                {
                    var (type, name) = function.Parameters[i];
                    _variables[name] = new Variable(name, arguments[i], type);
                }

                // Spuštění těla funkce
                object returnValue = null;
                foreach (var statement in function.Body)
                {
                    if (statement is ReturnNode returnNode)
                    {
                        returnValue = Evaluate(returnNode.Expression);
                        break;
                    }
                    Execute(statement);
                }

                return returnValue;
            }
            finally
            {
                // Vyčištění aktuálního obsahu a obnovení původního kontextu proměnných
                _variables.Clear();
                foreach (var variable in backupVariables)
                {
                    _variables[variable.Key] = variable.Value;
                }
            }
        }


        private object Evaluate(AstNode node)
        {
            switch (node)
            {
                case LiteralNode literal:
                    return ConvertLiteralToType(literal.Value);

                case ArrayLiteralNode arrayLiteral:
                    return EvaluateArrayLiteral(arrayLiteral);

                case IdentifierNode identifier:
                    if (!_variables.ContainsKey(identifier.Name))
                    {
                        throw new Exception($"Variable '{identifier.Name}' is not defined.");
                    }
                    return _variables[identifier.Name].Value;

                case ArrayAccessNode arrayAccess:
                    return EvaluateArrayAccess(arrayAccess);

                case ExpressionNode expression:
                    return EvaluateExpression(expression);

                case FunctionCallNode funcCall:
                    return ExecuteFunctionCall(funcCall);

                default:
                    throw new Exception($"Unsupported node type: {node.GetType().Name}");
            }
        }

        private object EvaluateArrayLiteral(ArrayLiteralNode node)
        {
            var elements = new List<object>();
            foreach (var element in node.Elements)
            {
                elements.Add(Evaluate(element));
            }
            return elements;
        }


        private object EvaluateArrayAccess(ArrayAccessNode node)
        {
            var array = Evaluate(node.Array);

            if (array is not IList<object> list)
            {
                throw new Exception($"Variable '{node.Array.Name}' is not an array.");
            }

            var index = Evaluate(node.Index);

            if (index is not int intIndex)
            {
                throw new Exception($"Array index must be an integer, got {index.GetType().Name}.");
            }

            if (intIndex < 0 || intIndex >= list.Count)
            {
                throw new Exception($"Array index {intIndex} is out of bounds.");
            }

            return list[intIndex];
        }


        private object ConvertLiteralToType(object value)
        {
            if (value is string stringValue)
            {
                if (int.TryParse(stringValue, out int intValue))
                {
                    return intValue;
                }

                if (float.TryParse(stringValue, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out float floatValue))
                {
                    return floatValue;
                }

                if (bool.TryParse(stringValue, out bool boolValue))
                {
                    return boolValue;
                }

                // Pokud není možné převést na jiný typ, vracíme původní string
                return stringValue;
            }

            // Pokud hodnota není string, vracíme ji beze změny
            return value;
        }

        private object EvaluateExpression(ExpressionNode node)
        {
            var left = Evaluate(node.Left);
            var right = Evaluate(node.Right);

            if (left is int leftInt && right is int rightInt)
            {
                return node.Operator switch
                {
                    "+" => leftInt + rightInt,
                    "-" => leftInt - rightInt,
                    "*" => leftInt * rightInt,
                    "/" => leftInt / rightInt,
                    "<" => leftInt < rightInt,
                    "<=" => leftInt <= rightInt,
                    ">" => leftInt > rightInt,
                    ">=" => leftInt >= rightInt,
                    _ => throw new Exception($"Unsupported operator: {node.Operator}")
                };
            }
            else if (left is float leftFloat || right is float rightFloat)
            {
                float leftValue = Convert.ToSingle(left);
                float rightValue = Convert.ToSingle(right);

                return node.Operator switch
                {
                    "+" => leftValue + rightValue,
                    "-" => leftValue - rightValue,
                    "*" => leftValue * rightValue,
                    "/" => leftValue / rightValue,
                    "<" => leftValue < rightValue,
                    "<=" => leftValue <= rightValue,
                    ">" => leftValue > rightValue,
                    ">=" => leftValue >= rightValue,
                    _ => throw new Exception($"Unsupported operator: {node.Operator}")
                };
            }
            else if (left is string leftString && right is string rightString)
            {
                return node.Operator switch
                {
                    "==" => leftString == rightString,
                    "!=" => leftString != rightString,
                    _ => throw new Exception($"Unsupported operator: {node.Operator}")
                };
            }

            throw new Exception($"Unsupported operand types for operator '{node.Operator}': {left.GetType()} and {right.GetType()}");
        }


        private bool IsTypeCompatible(string type, object value)
        {
            if (type.StartsWith("[") && type.EndsWith("]") && value is IList<object> listValue)
            {
                var elementTypes = type.Trim('[', ']').Split(", ");

                if (elementTypes.Length == 1)
                {
                    var elementType = elementTypes[0];
                    foreach (var element in listValue)
                    {
                        if (!IsTypeCompatible(elementType, element))
                        {
                            return false;
                        }
                    }

                    return true;
                }
                else
                {
                    if (elementTypes.Length != listValue.Count)
                    {
                        return false;
                    }

                    for (int i = 0; i < elementTypes.Length; i++)
                    {
                        if (!IsTypeCompatible(elementTypes[i], listValue[i]))
                        {
                            return false;
                        }
                    }

                    return true;
                }
            }

            return type switch
            {
                "int" => value is int,
                "real" => value is float,
                "bool" => value is bool,
                "string" => value is string,
                _ => false
            };
        }

        private string FormatValue(object value)
        {
            if (value is IList<object> list)
            {
                return "[" + string.Join(", ", list.Select(FormatValue)) + "]";
            }
            return value?.ToString() ?? "null";
        }
    }



    // Třída pro ReturnStatementNode
    public class ReturnStatementNode : AstNode
    {
        public AstNode Value { get; }

        public ReturnStatementNode(AstNode value)
        {
            Value = value;
        }
    }
}