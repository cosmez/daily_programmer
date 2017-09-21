using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.IO;
using System.Text;


namespace Challenge331
{
    enum TokeType { OpenParen, CloseParen, Number, Equals, Plus, Minus, Multiply, Divide,  Power,  Symbol, Fun, Comma}
    class Token
    {
        public string Value { get; set; } = "";
        public TokeType Type { get; set; }
        public override string ToString() =>  $"[{Type.ToString()} {Value}]";
    }

    class Expr { }

    class VariableExpr : Expr
    {
        public Token Variable { get; set; }
        public override string ToString() => $"{Variable.Value}";
    }

    class NumberExpr : Expr
    {
        public Token Number { get; set; }
        public override string ToString() => $"{Number.Value}";
    }

    class OpExpr : Expr
    {
        public Stack<Expr> Exprs { get; set; }
        public Stack<Token> Operators { get; set; }
        public override string ToString() => $"({String.Join(',', Exprs.Select(a => a.ToString()))} : {String.Join(',', Operators.Select(a => a))})";
    }

    class AssignExpr : Expr
    {
        public Token Variable { get; set; }
        public Expr Right { get; set; }
        public override string ToString() => $"{Variable}={Right}";
    }

    class FunApplExpr : Expr
    {
        public Token Function { get; set; }
        public IEnumerable<Expr> Arguments { get; set; }
        public override string ToString() => $"{Function.Value}({String.Join(',', Arguments.Select(a => a))})";
    }

    class FunDeclExpr : Expr
    {
        public Token Function { get; set; }
        public IEnumerable<Token> Arguments { get; set; }
        public Expr Body { get; set; }
        public override string ToString() => $"{Function.Value}({String.Join(',',Arguments.Select(a => a.Value))})={Body}";
    }


    class Value {  }
   
    class NumberValue : Value
    {
        public double Value { get; set; }
        public override string ToString() => $"{Value.ToString()}";
    }

    class LambdaValue : Value
    {
        public IEnumerable<Token> Arguments { get; set; }
        public Expr Body { get; set; }
        public Dictionary<string, NumberValue> Store { get; set; }
    }


    class Program
    {
        static void Main(string[] args)
        {
            Dictionary<string, NumberValue> store = new Dictionary<string, NumberValue>();
            Dictionary<string, LambdaValue> functions = new Dictionary<string, LambdaValue>();
            Run("5 + 5", store, functions);
            Run("(2 * 5 + 1) / 10", store, functions);
            Run("x =  1 / 2", store, functions);
            Run("y = x * 2", store, functions);
            Run("9 + 10", store, functions);
            Run("(2 * 5 + 1) / 10", store, functions);
            Run("x =  1 / 2", store, functions);
            Run("y = x * 2", store, functions);
            Run("(x + 2) * (y * (5 - 100))", store, functions);
            Run("z = 5*-3.14", store, functions);
            Run("2.6^(2 + 3/2) * (2-z)", store, functions);
            Run("a = 10", store, functions);
            Run("a() = 2", store, functions);
            Run("a() + a", store, functions);
            Run("avg(a, b) = (a + b) / 2", store, functions);
            Run("x = avg(69, 96)", store, functions);
            Run("avg(x, avg(a(), a)) + a", store, functions);


        }

        static void Run(string command, Dictionary<string, NumberValue> store, Dictionary<string, LambdaValue> functions)
        {
            Debug.WriteLine(Interp(Parse(Tokenize(command)), store, functions));
        }

        static Value Interp(Expr expression, Dictionary<string, NumberValue> store, Dictionary<string, LambdaValue> functions)
        {
            switch (expression)
            {
                case NumberExpr exp:
                    return new NumberValue() { Value = double.Parse(exp.Number.Value) };
                case VariableExpr exp:
                    if (store.ContainsKey(exp.Variable.Value))
                        return store[exp.Variable.Value];
                    else throw new Exception($"Undefined Variable {exp.Variable.Value}");
                case AssignExpr exp:
                    store[exp.Variable.Value] = Interp(exp.Right, store, functions) as NumberValue;
                    return store[exp.Variable.Value];
                case FunDeclExpr exp:
                    LambdaValue lmbd = new LambdaValue();
                    lmbd.Arguments = exp.Arguments;
                    lmbd.Body = exp.Body;
                    lmbd.Store = new Dictionary<string, NumberValue>(store);
                    functions[exp.Function.Value] =  lmbd;
                    return null;
                    break;
                case FunApplExpr exp:
                    if (functions.ContainsKey(exp.Function.Value))
                    {
                        LambdaValue lambda = functions[exp.Function.Value];
                        var innerStore = new Dictionary<string, NumberValue>(store);
                        for (int iarg = 0; iarg < exp.Arguments.Count(); iarg++)
                        {
                            //evaluate each argument
                            var argExp = exp.Arguments.ElementAt(iarg);
                            NumberValue argVal = Interp(argExp, store, functions) as NumberValue;
                            var argName = lambda.Arguments.ElementAt(iarg).Value;
                            innerStore[argName] = argVal;
                        }
                        return Interp(lambda.Body, innerStore, functions);

                    }
                    else throw new Exception($"Undefined Function {exp.Function.Value}");
                case OpExpr exp:
                    var exprs = new Stack<Expr>(exp.Exprs);
                    var ops = new Stack<Token>(exp.Operators);
                    var left = Interp(exprs.Pop(), store, functions) as NumberValue;
                    while (exprs.Any())
                    {
                        if (ops.Any())
                        {
                            var operation = ops.Pop();
                            var right = Interp(exprs.Pop(), store, functions) as NumberValue;

                            switch (operation.Type)
                            {
                                case TokeType.Plus:
                                    left = new NumberValue() { Value = left.Value + right.Value };
                                    break;
                                case TokeType.Minus:
                                    left = new NumberValue() { Value = left.Value - right.Value };
                                    break;
                                case TokeType.Multiply:
                                    left = new NumberValue() { Value = left.Value * right.Value };
                                    break;
                                case TokeType.Divide:
                                    if (right.Value == 0) throw new DivideByZeroException();
                                    left = new NumberValue() { Value = left.Value / right.Value };
                                    break;
                                case TokeType.Power:
                                    left = new NumberValue() { Value = Math.Pow(left.Value, right.Value) };
                                    break;
                            }
                        }
                        else throw new Exception("WTF Just happened");
                    }

                    return left;

            }

            return null;
        }

        static Expr Parse(IEnumerable<Token> tokens)
        {
            var tokenStack = new Stack<Token>(tokens.Reverse());
            var expr = Parse(tokenStack);
            return expr;
        }

        static Expr Parse(Stack<Token> tokens)
        {
            var first = tokens.ElementAt(0);

            //Terminals
            if (tokens.Count() == 1) 
            {
                if (first.Type == TokeType.Number) return new NumberExpr() { Number = first };
                if (first.Type == TokeType.Symbol) return new VariableExpr() { Variable = first };
            }

            //Non Terminals
            var second = tokens.ElementAt(1);
            //parse a simple declaration
            if (first.Type == TokeType.Symbol && second.Type == TokeType.Equals)
            {
                return ParseAssignExpr(tokens);
            }
            //parse a function definition
            if (first.Type == TokeType.Symbol && second.Type == TokeType.OpenParen &&  tokens.Any(t => t.Type == TokeType.Equals))
            {
                foreach (var token in tokens)
                {
                    if (token.Type == TokeType.Equals) return ParseFunDeclExpr(tokens);
                    if (token.Type != TokeType.Comma && token.Type != TokeType.Symbol && token.Type != TokeType.CloseParen && token.Type != TokeType.OpenParen)
                        throw new Exception($"Expected A list of Arguments , Found {token} Instead");
                    
                }
            }

            

            return ParseOperationExpr(tokens);

        }

        static OpExpr ParseOperationExpr(Stack<Token> tokens)
        {
            
            OpExpr oprExpr = new OpExpr();

            oprExpr.Exprs = new Stack<Expr>();
            oprExpr.Operators = new Stack<Token>();

            Token outTok;
            while (tokens.TryPeek(out outTok))
            {
                var el = tokens.Peek();
                
                if (el.Type == TokeType.Symbol && tokens.Count() > 1 && tokens.ElementAt(1).Type == TokeType.OpenParen) //function application
                {
                    oprExpr.Exprs.Push(ParseFunApplExpr(tokens));
                }
                else if (el.Type == TokeType.Comma) 
                {
                    return oprExpr;
                }
                else if(el.Type == TokeType.OpenParen)
                {
                    tokens.Pop();
                    oprExpr.Exprs.Push(ParseOperationExpr(tokens));
                    tokens.Pop(); //close paren
                }
                else if (el.Type == TokeType.CloseParen)
                {
                    //tokens.Pop();
                    return oprExpr;
                }
                else if (el.Type == TokeType.Divide || el.Type == TokeType.Multiply || el.Type == TokeType.Minus || el.Type == TokeType.Plus || el.Type == TokeType.Power)
                {
                    oprExpr.Operators.Push(tokens.Pop());
                }
                else
                {
                    var singleItemStack = new Stack<Token>();
                    singleItemStack.Push(tokens.Pop());
                    oprExpr.Exprs.Push(Parse(singleItemStack));
                }

                
            }

            return oprExpr;

        }

        static FunApplExpr ParseFunApplExpr(Stack<Token> tokens)
        {
            FunApplExpr funAppExpr = new FunApplExpr();
            funAppExpr.Function = tokens.Pop();
            var argumentExprs = new List<Expr>();
            //now parentheses
            if (tokens.Pop().Type != TokeType.OpenParen) throw new Exception("Bad Function Application");
            //now the argument expressions
            Token outTok;
            while (tokens.Any() && tokens.Peek().Type != TokeType.CloseParen)
            {
                argumentExprs.Add(ParseOperationExpr(tokens));
                if (tokens.Any())
                { 
                    var next = tokens.Peek();
                    //every expression must be followed by a comma
                    if (next.Type != TokeType.Comma && next.Type != TokeType.CloseParen)
                    {
                        throw new Exception("Bad Function Application, Expected an argument List or End of Application");
                    
                    }
                    if (next.Type == TokeType.CloseParen) continue;
                    tokens.Pop();
                }
            }
            if (tokens.Any() && tokens.Peek().Type == TokeType.CloseParen) tokens.Pop(); //pop close paren
            funAppExpr.Arguments = argumentExprs;
            return funAppExpr;
        }

        static AssignExpr ParseAssignExpr(Stack<Token> tokens)
        {
            var assignExpr = new AssignExpr();
            assignExpr.Variable = tokens.Pop();
            tokens.Pop(); //remove assignment operation
            assignExpr.Right = Parse(tokens);
            return assignExpr;
        }

        static FunDeclExpr ParseFunDeclExpr(Stack<Token> tokens)
        {
            var funDclExpr = new FunDeclExpr();
            funDclExpr.Function = tokens.Pop();

            List<Token> Arguments = new List<Token>();
            var first = tokens.Pop();
            while (first.Type  != TokeType.Equals)
            {
                if (first.Type == TokeType.Symbol) Arguments.Add(first);
                first  = tokens.Pop();
            }
            funDclExpr.Arguments = Arguments;


            funDclExpr.Body = Parse(tokens);
            
            return funDclExpr;
        }

        static IEnumerable<Token> Tokenize(string input)
        {
            var tokens = new List<Token>();
            using (var reader = new StringReader(input))
            {
                while (reader.Peek() != -1)
                {
                    char c = (char)reader.Peek();

                    if (Char.IsDigit(c))
                    {
                        var number = new StringBuilder();
                        while (Char.IsDigit(c) || c == '.')
                        {
                            number.Append((char)reader.Read());
                            c = (char)reader.Peek();
                        }
                        tokens.Add(new Token() { Type = TokeType.Number, Value = number.ToString() });
                    }


                    if (Char.IsLetter(c))
                    {
                        var symbol = new StringBuilder();
                        while (Char.IsLetterOrDigit(c))
                        {
                            symbol.Append((char)reader.Read());
                            c = (char)reader.Peek();
                        }
                        tokens.Add(new Token() { Type = TokeType.Symbol, Value = symbol.ToString() });
                    }

                    switch (c)
                    {
                        case '(':
                            reader.Read();
                            tokens.Add(new Token() { Type = TokeType.OpenParen });
                            break;
                        case ')':
                            reader.Read();
                            tokens.Add(new Token() { Type = TokeType.CloseParen });
                            break;
                        case '+':
                            reader.Read();
                            tokens.Add(new Token() { Type = TokeType.Plus });
                            break;
                        case '-':
                            reader.Read();
                            c = (char)reader.Peek();
                            if (Char.IsDigit(c))
                            {
                                var number = new StringBuilder();
                                number.Append('-');
                                while (Char.IsDigit(c) || c == '.')
                                {
                                    number.Append((char)reader.Read());
                                    c = (char)reader.Peek();
                                }
                                tokens.Add(new Token() { Type = TokeType.Number, Value = number.ToString() });
                            }
                            else tokens.Add(new Token() { Type = TokeType.Minus });
                            break;
                        case '*':
                            reader.Read();
                            tokens.Add(new Token() { Type = TokeType.Multiply });
                            break;
                        case '/':
                            reader.Read();
                            tokens.Add(new Token() { Type = TokeType.Divide });
                            break;
                        case '^':
                            reader.Read();
                            tokens.Add(new Token() { Type = TokeType.Power });
                            break;
                        case '=':
                            reader.Read();
                            tokens.Add(new Token() { Type = TokeType.Equals });
                            break;
                        case ',':
                            reader.Read();
                            tokens.Add(new Token() { Type = TokeType.Comma });
                            break;
                        default:
                            reader.Read();
                            break;
                    }
                }

            }

            return tokens;
        }

    }
}
