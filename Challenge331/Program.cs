using System;
using System.Collections.Generic;
using System.Linq;

using System.Diagnostics;
using System.IO;
using System.Text;
using System.Runtime.Serialization;

namespace Challenge331
{
    enum TokeType { OpenParen, CloseParen, Number, Equals, Plus, Minus, Multiply, Divide,  Power,  Symbol, Fun, Comma}
    class Token
    {
        public string Value { get; set; } = "";
        public TokeType Type { get; set; }
        public override string ToString() =>  $"[{Type.ToString()} {Value}]";
    }

    class Expr
    {

    }

    class BinaryExpr : Expr
    {
        public Expr Left { get; set; }
        public Token Operator { get; set; }
        public Expr Right { get; set; }
        public override string ToString() => $"{Left}{Operator}{Right}";
    }

    class AssignExpr : Expr
    {
        public Token Variable { get; set; }
        public Expr Right { get; set; }
        public override string ToString() => $"{Variable}={Right}";
    }

    class FunDeclExpr : Expr
    {
        public Token Function { get; set; }
        public IEnumerable<Token> Arguments { get; set; }
        public Expr Body { get; set; }
        public override string ToString() => $"{Function.Value}({String.Join(',',Arguments.Select(a => a.Value))})={Body}";
    }


    class Program
    {
        static void Main(string[] args)
        {
            var tokens = Tokenize("sum(a,10,b) = a + b");
            foreach (var token in tokens)
            {
                Debug.WriteLine(token.ToString());
            }
            var expr = Parse(tokens);
            Debug.WriteLine(expr.ToString());
        }

        static Expr Parse(IEnumerable<Token> tokens)
        {
            var first = tokens.ElementAt(0);
            var second = tokens.ElementAt(1);
            if (first.Type == TokeType.Symbol && second.Type == TokeType.Equals)
            {
                return ParseAssignExpr(tokens);
            }
            if (first.Type == TokeType.Symbol && second.Type == TokeType.OpenParen)
            {
                foreach (var token in tokens)
                {
                    if (token.Type == TokeType.Equals) return ParseFunDeclExpr(tokens);
                    if (token.Type != TokeType.Comma && token.Type != TokeType.Symbol && token.Type != TokeType.CloseParen && token.Type != TokeType.OpenParen)
                        throw new InvalidFunctionDeclarationException($"Expected A list of Arguments , Found {token} Instead");
                    
                }
                
            }
            return new Expr();
        }

        static AssignExpr ParseAssignExpr(IEnumerable<Token> tokens)
        {
            var assignExpr = new AssignExpr();
            assignExpr.Variable = tokens.First();
            assignExpr.Right = Parse(tokens.Skip(1));
            return assignExpr;
        }

        static FunDeclExpr ParseFunDeclExpr(IEnumerable<Token> tokens)
        {
            var funDclExpr = new FunDeclExpr();
            funDclExpr.Function = tokens.First();

            List<Token> Arguments = new List<Token>();
            var ts = tokens.Skip(1);
            while (ts.First().Type  != TokeType.Equals)
            {
                if (ts.First().Type == TokeType.Symbol) Arguments.Add(ts.First());
                ts = ts.Skip(1);
            }
            funDclExpr.Arguments = Arguments;
            ts = ts.Skip(1);

            funDclExpr.Body = Parse(ts);
            
            return funDclExpr;
        }

        /// <summary>
        /// Tokenize The Input
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
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

                    if (Char.IsDigit(c))
                    {
                        var number = new StringBuilder();
                        while (Char.IsDigit(c) || c == '.' )
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
                            tokens.Add(new Token() { Type = TokeType.Minus });
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

        [Serializable]
        private class InvalidFunctionDeclarationException : Exception
        {
            public InvalidFunctionDeclarationException(string message) : base(message)
            {
            }
        }
    }
}
