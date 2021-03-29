using System;

namespace ComplexGraph
{
    /// <summary>
    /// Represents a display name of a complex-valued function.
    /// </summary>
    public class FunctionName
    {
        /// <summary>
        /// Gets the standard name of an independed variable.
        /// </summary>
        public const string VariableName = "z";

        /// <summary>
        /// The special part in a name pattern that is a placeholder of
        /// the function argument.
        /// </summary>
        public const string ArgumentPlaceholder = "#";

        private readonly string namePattern;

        public FunctionName(string namePattern = ArgumentPlaceholder)
        {
            if (!namePattern.Contains(ArgumentPlaceholder))
            {
                throw new ArgumentException(
                    $"Name pattern should contain \"{ArgumentPlaceholder}\" " +
                    "that is a placeholder of argument");
            }

            Value = namePattern.Replace(ArgumentPlaceholder, VariableName);
            this.namePattern = namePattern;
        }

        public string Value { get; }

        /// <summary>
        /// Returns the name of the function f(g), where f is the current
        /// function name, and g is the given one.
        /// </summary>
        public FunctionName Compose(FunctionName innerFunction)
            => new FunctionName(
                this.namePattern.Replace(
                    ArgumentPlaceholder,
                    innerFunction.namePattern));
    }
}