using System;
using System.Numerics;

namespace ComplexGraph
{
    /// <summary>
    /// Represents complex-valued function and its name.
    /// </summary>
    public class Function
    {
        private static readonly Lazy<Function> identity = new(() => new Function("#", c => c));
        private readonly Func<Complex, Complex> action;

        public Function(
            string namePattern,
            Func<Complex, Complex> func)
            : this(new FunctionName(namePattern), func)
        {
        }

        public Function(
            FunctionName name,
            Func<Complex, Complex> func)
        {
            Name = name;
            this.action = func;
        }

        /// <summary>
        /// Creates and returns identity function over the given area.
        /// </summary>
        public static Function Identity => identity.Value;

        /// <summary>
        /// Gets the name of the function.
        /// </summary>
        public FunctionName Name { get; }

        /// <summary>
        /// Gets the image of the given point after func action.
        /// </summary>
        public Complex this[Complex value] => action(value);

        /// <summary>
        /// Gets a new function that is a right compositon of the given and
        /// the current one. E.g. if the current function is f() and
        /// the given one is g(), then it returns g(f).
        /// </summary>
        public Function RightCompose(
            string namePattern,
            Func<Complex, Complex> func)
            => this.RightCompose(new FunctionName(namePattern), func);

        /// <summary>
        /// Gets a new function that is a right compositon of the given and
        /// the current one. E.g. if the current function is f() and
        /// the given one is g(), then it returns g(f).
        /// </summary>
        public Function RightCompose(
            FunctionName name,
            Func<Complex, Complex> func)
            => new Function(
                name.Compose(this.Name),
                c => func(this.action(c)));

        /// <summary>
        /// Gets a new function that is a right compositon of the given and
        /// the current one. E.g. if the current function is f() and
        /// the given one is g(), then it returns g(f).
        /// </summary>
        public Function RightCompose(Function func)
            => new Function(
                func.Name.Compose(this.Name),
                c => func[this.action(c)]);

        /// <summary>
        /// Gets a new function that is a left compositon of the given and
        /// the current one. E.g. if the current function is f() and
        /// the given one is g(), then it returns f(g).
        /// </summary>
        public Function LeftCompose(
            string namePattern,
            Func<Complex, Complex> func)
            => this.LeftCompose(new FunctionName(namePattern), func);

        /// <summary>
        /// Gets a new function that is a left compositon of the given and
        /// the current one. E.g. if the current function is f() and
        /// the given one is g(), then it returns f(g).
        /// </summary>
        public Function LeftCompose(
            FunctionName name,
            Func<Complex, Complex> func)
            => new Function(
                this.Name.Compose(name),
                c => this.action(func(c)));

        /// <summary>
        /// Gets a new function that is a left compositon of the given and
        /// the current one. E.g. if the current function is f() and
        /// the given one is g(), then it returns f(g).
        /// </summary>
        public Function LeftCompose(Function func)
            => new Function(
                this.Name.Compose(func.Name),
                c => this.action(func[c]));

        /// <summary>
        /// Gets a new function that is a compositon of the given and
        /// the current one as operands of the infix binary operation. E.g.
        /// if the current function is f(),
        /// the given one is g(), and the operation is * then it returns
        /// f() * g().
        /// </summary>
        public Function Compose(
            string operationName,
            Func<Complex, Complex, Complex> operation,
            Function func)
            => new Function(
                this.Name.Compose(operationName, func.Name),
                c => operation(this[c], func[c]));
    }
}