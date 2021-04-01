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
        private const string Arg = "z";

        private const string Add = "+";
        private const string Sub = "-";
        private const string Mul = "*";
        private const string Div = "/";
        private const string Pow = "^";

        private const string Exp = "exp";
        private const string Log = "ln";
        private const string Sin = "sin";
        private const string Cos = "cos";
        private const string Tan = "tan";

        private static readonly Dictionary<string, Function> Funcs = new()
        {
            [Sub] = new Function("-#", Complex.Negate),
            [Exp] = new Function("exp #", Complex.Exp),
            [Log] = new Function("ln #", Complex.Log),
            [Sin] = new Function("sin #", Complex.Sin),
            [Cos] = new Function("cos #", Complex.Cos),
            [Tan] = new Function("tan #", Complex.Tan),
        };

        private static readonly Dictionary<string, BinFunc> BinaryOperations = new()
        {
            [Add] = Complex.Add,
            [Sub] = Complex.Subtract,
            [Mul] = Complex.Multiply,
            [Div] = Complex.Divide,
            [Pow] = Complex.Pow,
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
                    .Append(string.Empty);  // for proper handling terms end

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

        /// <summary>
        /// Parses one expression that is a term or sum or subtract of
        /// two terms.
        /// </summary>
        private static bool Expression(Context ctxt)
        {
            if (!Term(ctxt))
            {
                return false;
            }

            var operationName = ctxt.Terms.Current;
            if (operationName != Add &&
                operationName != Sub)
            {
                return true;    // it's one adding term expression
            }

            ctxt.Terms.MoveNext();
            var operation = BinaryOperations[operationName];
            var leftOperand = ctxt.Flush();

            if (!Term(ctxt))
            {
                throw new ArgumentException(
                    "Expected right operand of add/subtract");
            }

            PerformBinaryOperation(ctxt, leftOperand, operationName, operation);
            return true;
        }

        /// <summary>
        /// Parses one term that is a factor or multiplying or division of
        /// two factors.
        /// </summary>
        private static bool Term(Context ctxt)
        {
            if (!Factor(ctxt))
            {
                return false;
            }

            var operationName = ctxt.Terms.Current;
            if (operationName != Mul &&
                operationName != Div)
            {
                return true;    // it's simple factor
            }

            ctxt.Terms.MoveNext();
            var operation = BinaryOperations[operationName];
            var leftOperand = ctxt.Flush();

            if (!Factor(ctxt))
            {
                throw new ArgumentException(
                    "Expected right operand of multiply/divide");
            }

            PerformBinaryOperation(ctxt, leftOperand, operationName, operation);
            return true;
        }

        /// <summary>
        /// Parses one factor that is a single factor part or power expression.
        /// </summary>
        private static bool Factor(Context ctxt)
        {
            if (!FactorPart(ctxt))
            {
                return false;
            }

            var operationName = ctxt.Terms.Current;
            if (operationName != Pow)
            {
                return true;    // it's not a power
            }

            ctxt.Terms.MoveNext();
            var operation = BinaryOperations[operationName];
            var leftOperand = ctxt.Flush();

            if (!FactorPart(ctxt))
            {
                throw new ArgumentException(
                    "Expected right operand of power operation");
            }

            PerformBinaryOperation(ctxt, leftOperand, operationName, operation);
            return true;
        }

        /// <summary>
        /// Parses unary operation aplying to some atom.
        /// </summary>
        private static bool FactorPart(Context ctxt)
        {
            string? unaryOperation = null;
            if (Funcs.ContainsKey(ctxt.Terms.Current))
            {
                unaryOperation = ctxt.Terms.Current;
                ctxt.Terms.MoveNext();
            }

            if (!Atom(ctxt))
            {
                if (unaryOperation is not null)
                {
                    throw new ArgumentException(
                        "Expected operand of unary operation");
                }

                return false;
            }

            if (unaryOperation is not null)
            {
                PerformUnaryOperation(ctxt, unaryOperation);
            }

            return true;
        }

        /// <summary>
        /// Parses a single expresion operand that can be argument mark,
        /// number, parantheses, or factor part.
        /// </summary>
        private static bool Atom(Context ctxt)
        {
            if (ctxt.Terms.Current == Arg)
            {
                ctxt.Func = Function.Identity;
                ctxt.Terms.MoveNext();
                return true;
            }

            if (ComplexNumberParser.TryParse(ctxt.Terms.Current, out var num))
            {
                ctxt.Number = num;
                ctxt.Terms.MoveNext();
                return true;
            }

            return Parantheses(ctxt) || FactorPart(ctxt);
        }

        /// <summary>
        /// Parses expression in parantheses.
        /// </summary>
        private static bool Parantheses(Context ctxt)
        {
            if (ctxt.Terms.Current == "(")
            {
                ctxt.Terms.MoveNext();
                if (!Expression(ctxt))
                {
                    throw new ArgumentException(
                        "Expected expression in parantheses");
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

        /// <summary>
        /// Applies some binary operation, storing result in the context.
        /// </summary>
        private static void PerformBinaryOperation(
            Context ctxt,
            (Function?, Complex?) leftOperand,
            string operationName,
            BinFunc operation)
        {
            var (func, num) = leftOperand;
            if (func is not null && ctxt.Func is not null)
            {
                ctxt.Func = func.Compose(operationName, operation, ctxt.Func);
            }
            else if (func is not null && ctxt.Number is not null)
            {
                var num2 = ctxt.Number.Value;
                var nStr = ComplexNumberParser.ToString(num2);
                ctxt.Func = func.RightCompose(
                    $"(#{operationName}{nStr})",
                    c => operation(c, num2));
            }
            else if (num is not null && ctxt.Func is not null)
            {
                var nStr = ComplexNumberParser.ToString(num.Value);
                ctxt.Func = ctxt.Func.LeftCompose(
                    $"({nStr}{operationName}#)",
                    c => operation(num.Value, c));
            }
            else if (num is not null && ctxt.Number is not null)
            {
                ctxt.Number = operation(num.Value, ctxt.Number.Value);
            }
            else
            {
                throw new ArgumentException(
                    "Invalid operands of binary operation");
            }
        }

        /// <summary>
        /// Applies some unary operation, storing the result in the context.
        /// </summary>
        private static void PerformUnaryOperation(
            Context ctxt,
            string unaryOperation)
        {
            if (ctxt.Func is not null)
            {
                ctxt.Func = ctxt.Func.RightCompose(Funcs[unaryOperation]);
            }
            else if (ctxt.Number is not null)
            {
                ctxt.Number = Funcs[unaryOperation][ctxt.Number.Value];
            }
            else
            {
                throw new ArgumentException("Invalid unary operation");
            }
        }

        /// <summary>
        /// Parsing context.
        /// </summary>
        private class Context
        {
            public Context(IEnumerator<string> terms)
                => Terms = terms;

            /// <summary>
            /// Enumerator of all description atomic terms.
            /// </summary>
            public IEnumerator<string> Terms { get; }

            /// <summary>
            /// Holder for accumulating function.
            /// </summary>
            public Function? Func { get; set; } = null;

            /// <summary>
            /// Holder for accumulating number.
            /// </summary>
            public Complex? Number { get; set; } = null;

            /// <summary>
            /// Flushes holders, reseting them and returning their last values.
            /// </summary>
            public (Function?, Complex?) Flush()
            {
                var res = (Func, Number);
                this.Func = null;
                this.Number = null;
                return res;
            }
        }
    }
}