using GraphQL.Validation.Complexity;
using Shouldly;
using Xunit;

namespace GraphQL.ComplexityAnalizer.Tests
{
    public class Tests : ComplexityTestBase<ComplexityRule, ValidationSchema>
    {
        [Fact]
        public void one_field()
        {
            ShouldPassRule("query { dog { name } }");
            this.Rule.CalculatedComplexity.ShouldBe(2);
        }

        [Fact]
        public void many_fields()
        {
            ShouldPassRule("query { dog { name, nickname, barks, barkVolume } }");
            this.Rule.CalculatedComplexity.ShouldBe(5);
        }

        [Fact]
        public void duplicated_fields()
        {
            ShouldPassRule(@"
query {
    human(id: ""test"") {
        name
        iq
        ...F0
        ...F1
    }
}

fragment F0 on Human {
    relatives {
        name
    }
}

fragment F1 on Human {
    relatives {
        iq
    }
}
");
            this.Rule.CalculatedComplexity.ShouldBe(15);
        }

        [Fact]
        public void nested_lists()
        {
            ShouldPassRule(@"
query {
    human(id: ""test"") {
        name
        iq
        pets {
            name
        }
        relatives {
            name
            iq
        }
    }
}
");
            this.Rule.CalculatedComplexity.ShouldBe(16);
        }

        [Fact]
        public void too_complex_nested_lists()
        {
            this.Rule.Configuration = new ComplexityConfiguration
            {
                MaxComplexity = 7
            };

            ShouldFailRule(_ =>
            {
                _.Query = @"
query {
    human(id: ""test"") {
        name
        iq
        pets {
            name
        }
        relatives {
            name
            iq
        }
    }
}
";
                _.Error(
                    message: Rule.TooComplexMessage("relatives"),
                    line: 9,
                    column: 9);
                _.Error(
                    message: Rule.TooComplexMessage("human"),
                    line: 3,
                    column: 5);
            });
        }
    }
}