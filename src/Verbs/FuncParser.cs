using System;
using System.Globalization;
using System.Numerics;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using BinFunc = System.Func<
    System.Numerics.Complex,
    System.Numerics.Complex,
    System.Numerics.Complex>;

namespace ComplexGraph.Verbs
{
    /// <summary>
    /// Parses string representation of the function.
    /// </summary>
    static class FuncParser
    {
        private const string ArgTerm = "z";

        private const string AddTerm = "+";
        private const string SubTerm = "-";
        private const string MulTerm = "*";
        private const string DivTerm = "/";
        private const string PowTerm = "^";

        private const string ExpTerm = "exp";
        private const string LogTerm = "ln";
        private const string SinTerm = "sin";
        private const string CosTerm = "cos";
        private const string TanTerm = "tan";

        private static readonly Dictionary<string, Function> Funcs = new()
        {
            [SubTerm] = new Function("-#", Complex.Negate),
            [ExpTerm] = new Function("exp(#)", Complex.Exp),
            [LogTerm] = new Function("ln(#)", Complex.Log),
            [SinTerm] = new Function("sin(#)", Complex.Sin),
            [CosTerm] = new Function("cos(#)", Complex.Cos),
            [TanTerm] = new Function("tan(#)", Complex.Tan),
        };

        private static readonly Dictionary<string, BinFunc> BinaryOperations = new()
        {
            [AddTerm] = Complex.Add,
            [SubTerm] = Complex.Subtract,
            [MulTerm] = Complex.Multiply,
            [DivTerm] = Complex.Divide,
            [PowTerm] = Complex.Pow,
        };

        /// <summary>
        /// Parses string returning the fucntion that is described by it.
        /// </summary>
        public static Function Parse(this string description)
        {
            try
            {
                var parts = $"{description}"
                    .Split(
                        ' ',
                        StringSplitOptions.RemoveEmptyEntries |
                        StringSplitOptions.TrimEntries)
                    .SelectMany(p => Regex.Split(
                        p,
                        @"(\{[^\}]*\})|([*()\^\/]|(?<!E)[\+\-])",
                        RegexOptions.CultureInvariant |
                        RegexOptions.IgnoreCase))
                    .Where(p => string.Empty != p)
                    .Append(string.Empty);

                var ctxt = new Context(parts.GetEnumerator());
                ctxt.Terms.MoveNext();

                if (Expression(ctxt) && ctxt.Func is not null)
                {
                    return ctxt.Func;
                }
            }
            catch
            {
                throw new InvalidOperationException(
                    "Inner code error has been occured. Cannot perform action");
            }

            throw new ArgumentException("Wrong func description");
        }

        private static bool Expression(Context ctxt)
        {
            string? unaryTerm = null;
            if (Funcs.ContainsKey(ctxt.Terms.Current))
            {
                unaryTerm = ctxt.Terms.Current;
                ctxt.Terms.MoveNext();
            }

            if (!Operand(ctxt))
            {
                if (unaryTerm is not null)
                {
                    throw new ArgumentException(
                        "Expected operand of unary operation");
                }

                return false;
            }

            if (unaryTerm is not null)
            {
                if (ctxt.Number is not null)
                {
                    ctxt.Number = Funcs[unaryTerm][ctxt.Number.Value];
                }
                else if (ctxt.Func is not null)
                {
                    ctxt.Func = ctxt.Func.RightCompose(Funcs[unaryTerm]);
                }
                else
                {
                    throw new ArgumentException("Invalid expression");
                }
            }

            BinaryTail(ctxt);
            return true;
        }

        private static bool Operand(Context ctxt)
        {
            return Parantheses(ctxt) ||
                   Argument(ctxt) ||
                   Number(ctxt) ||
                   Expression(ctxt);
        }

        private static bool Parantheses(Context ctxt)
        {
            if (ctxt.Terms.Current == "(")
            {
                ctxt.Terms.MoveNext();
                if (!Expression(ctxt))
                {
                    return false;
                }

                if (ctxt.Terms.Current != ")")
                {
                    throw new ArgumentException("Expected closing paranthesis");
                }

                ctxt.Terms.MoveNext();
                return true;
            }

            return false;
        }

        private static bool Argument(Context ctxt)
        {
            if (ctxt.Terms.Current == ArgTerm)
            {
                ctxt.Func = Function.Identity;
                ctxt.Terms.MoveNext();
                return true;
            }
            else
            {
                return false;
            }
        }

        private static bool BinaryTail(Context ctxt)
        {
            if (!BinaryOperations.ContainsKey(ctxt.Terms.Current))
            {
                return false;
            }

            var op = ctxt.Terms.Current;
            var func = ctxt.Func;
            var num = ctxt.Number;
            ctxt.Func = null;
            ctxt.Number = null;
            ctxt.Terms.MoveNext();

            if (!Operand(ctxt))
            {
                throw new ArgumentException(
                    "Expected second operand of binary operation");
            }

            if (func is not null && ctxt.Func is not null)
            {
                ctxt.Func = func.Compose(op, BinaryOperations[op], ctxt.Func);
            }
            else if (func is not null && ctxt.Number is not null)
            {
                var num2 = ctxt.Number.Value;
                var nStr = ComplexNumberParser.ToString(num2);
                ctxt.Func = func.LeftCompose(
                    $"#{op}{nStr}",
                    c => BinaryOperations[op](c, num2));
            }
            else if (num is not null && ctxt.Func is not null)
            {
                var nStr = ComplexNumberParser.ToString(num.Value);
                ctxt.Func = ctxt.Func.RightCompose(
                    $"{nStr}{op}#",
                    c => BinaryOperations[op](num.Value, c));
            }
            else if (num is not null && ctxt.Number is not null)
            {
                ctxt.Number = BinaryOperations[op](
                    num.Value,
                    ctxt.Number.Value);
            }
            else
            {
                throw new ArgumentException(
                    "Invalid operands of binary operation");
            }

            return true;
        }

        private static bool Number(Context ctxt)
        {
            if (!ComplexNumberParser.TryParse(ctxt.Terms.Current, out var num))
            {
                return false;
            }

            ctxt.Number = num;
            ctxt.Terms.MoveNext();
            return true;
        }

        private class Context
        {
            public Context(IEnumerator<string> terms)
                => Terms = terms;

            public IEnumerator<string> Terms { get; }

            public Function? Func { get; set; } = null;

            public Complex? Number { get; set; } = null;
        }
    }
}