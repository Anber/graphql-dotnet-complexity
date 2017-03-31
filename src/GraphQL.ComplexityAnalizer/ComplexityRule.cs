using System;
using System.Collections.Generic;
using System.Linq;
using GraphQL.Language.AST;
using GraphQL.Validation;
using GraphQL.Validation.Complexity;

namespace GraphQL.ComplexityAnalizer
{
    public class ComplexityRule : IValidationRule
    {
        public delegate double Complexity(double children);

        public string TooComplexMessage(string fieldName)
        {
            return $"Field {fieldName} is too complex to execute.";
        }

        public ComplexityConfiguration Configuration { get; set; }

        public double CalculatedComplexity { get; protected set; }

        protected Stack<List<Func<double>>> Stack = new Stack<List<Func<double>>>();

        public INodeVisitor Validate(ValidationContext context)
        {
            return new EnterLeaveListener(listener =>
            {
                listener.Match<Document>(fragment =>
                {
                    this.Stack.Push(new List<Func<double>>());
                }, fragment =>
                {
                    var functions = this.Stack.Pop();
                    this.CalculatedComplexity = functions.Sum(f => f());
                });

                listener.Match<Field>(
                    field =>
                    {
                        this.Stack.Push(new List<Func<double>>());
                    },
                    field =>
                    {
                        var type = context.TypeInfo.GetParentType().GetNamedType();
                        if (type != null)
                        {
                            var childrenComplexity = this.Stack.Pop();
                            var fieldDef = context.TypeInfo.GetFieldDef();
                            this.Stack.Peek().Add(() =>
                            {
                                var sum = fieldDef.GetComplexity(childrenComplexity.Sum(f => f()));
                                if (sum > (Configuration?.MaxComplexity ?? double.MaxValue))
                                {
                                    // Retport an error, including helpful suggestions.
                                    context.ReportError(new ValidationError(
                                        context.OriginalQuery,
                                        "complexity", // TODO: error code
                                        TooComplexMessage(field.Name),
                                        field
                                    ));
                                }
                                return sum;
                            });
                        }
                    }
                );
            });
        }
    }
}
