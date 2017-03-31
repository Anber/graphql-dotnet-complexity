using GraphQL.Types;

namespace GraphQL.ComplexityAnalizer
{
    public static class ComplexityFieldExtensions
    {
        private const string ComplexityMetaName = "complexity-func";

        private static readonly ComplexityRule.Complexity DefaultComplexity = d => 1 + d;

        public static void SetComplexity(this FieldType field, ComplexityRule.Complexity func)
        {
            field.Metadata.Add(ComplexityMetaName, func);
        }

        public static double GetComplexity(this FieldType field, double childrenComplexity)
        {
            return field.GetMetadata(ComplexityMetaName, DefaultComplexity)(childrenComplexity);
        }
    }
}