using Aml.Domain;


namespace Aml.Tests
{
    public class CounterpartyNameTests
    {
        [Theory]
        [InlineData(" Example Name ", "EXAMPLE NAME")]
        [InlineData("  Example Name  ", "EXAMPLE NAME")]
        [InlineData("eXamPle naME", "EXAMPLE NAME")]
        [InlineData(null, "")]
        [InlineData("", "")]
        [InlineData("    ", "")]
        [InlineData("example    name", "EXAMPLE NAME")]
        [InlineData("   example    \n name", "EXAMPLE NAME")]
        [InlineData("EXAMPLE NAME", "EXAMPLE NAME")]
        
        public void Normalize_VariousInputs_ReturnsExpected(string? raw, string expected)
        {
            var result = CounterpartyName.Normalize(raw);

            Assert.Equal(expected, result);
        }

    }
}

