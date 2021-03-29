using System;
using System.Numerics;

namespace ComplexGraph
{
    /// <summary>
    /// Represents complex-valued function and its action.
    /// </summary>
    public class Function
    {
        private readonly Func<Complex, Complex> action;

        public Function(
            FunctionName name,
            Func<Complex, Complex> func,
            Area preimage)
        {
            Name = name;
            Preimage = preimage;
            this.action = func;
        }

        /// <summary>
        /// Gets the complex plane area on that the function acts.
        /// </summary>
        public Area Preimage { get; }

        /// <summary>
        /// Gets the name of the function.
        /// </summary>
        public FunctionName Name { get; }

        /// <summary>
        /// Gets the image of the given point after func action.
        /// </summary>
        public Complex this[Complex value] => action(value);

        /// <summary>
        /// Creates and returns identity function over the given area.
        /// </summary>
        public static Function Identity(Area preimage)
            => new Function(new FunctionName(), c => c, preimage);

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
                c => func(this.action(c)),
                this.Preimage);

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
                c => this.action(func(c)),
                this.Preimage);
    }
}