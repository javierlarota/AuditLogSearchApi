using AuditLogSearchApi.Services;

namespace AuditLogSearchApi.Tests
{
    public class SearchQueryParserTests
    {
        private readonly SearchQueryParser _parser;

        public SearchQueryParserTests()
        {
            _parser = new SearchQueryParser();
        }

        [Fact]
        public void Parse_SimpleQuery_ReturnsFullTextSearch()
        {
            // Arrange
            var query = "login";

            // Act
            var result = _parser.Parse(query);

            // Assert
            Assert.False(result.IsColumnSpecific);
            Assert.Equal("login:*", result.PostgresQuery);
            Assert.Empty(result.Conditions);
        }

        [Fact]
        public void Parse_AndOperator_ReturnsCorrectQuery()
        {
            // Arrange
            var query = "login AND failed";

            // Act
            var result = _parser.Parse(query);

            // Assert
            Assert.False(result.IsColumnSpecific);
            Assert.Equal("login:* & failed:*", result.PostgresQuery);
            Assert.Empty(result.Conditions);
        }

        [Fact]
        public void Parse_OrOperator_ReturnsCorrectQuery()
        {
            // Arrange
            var query = "login OR logout";

            // Act
            var result = _parser.Parse(query);

            // Assert
            Assert.False(result.IsColumnSpecific);
            Assert.Equal("login:* | logout:*", result.PostgresQuery);
            Assert.Empty(result.Conditions);
        }

        [Fact]
        public void Parse_NotOperator_ReturnsCorrectQuery()
        {
            // Arrange
            var query = "login NOT failed";

            // Act
            var result = _parser.Parse(query);

            // Assert
            Assert.False(result.IsColumnSpecific);
            Assert.Equal("login:* & !failed:*", result.PostgresQuery);
            Assert.Empty(result.Conditions);
        }

        [Fact]
        public void Parse_ComplexQuery_WithMultipleOperators()
        {
            // Arrange
            var query = "login AND (failed OR error)";

            // Act
            var result = _parser.Parse(query);

            // Assert
            Assert.False(result.IsColumnSpecific);
            Assert.Contains("&", result.PostgresQuery);
            Assert.Contains("|", result.PostgresQuery);
        }

        [Fact]
        public void Parse_ColumnSpecificQuery_ReturnsConditions()
        {
            // Arrange
            var query = "user_name:John";

            // Act
            var result = _parser.Parse(query);

            // Assert
            Assert.True(result.IsColumnSpecific);
            Assert.Single(result.Conditions);
            Assert.Equal("user_name", result.Conditions[0].Column);
            Assert.Equal("John", result.Conditions[0].SearchTerm);
            Assert.Equal("AND", result.Conditions[0].Operator);
        }

        [Fact]
        public void Parse_ColumnSpecificWithAnd_ReturnsMultipleConditions()
        {
            // Arrange
            var query = "user_name:John AND action:LOGIN";

            // Act
            var result = _parser.Parse(query);

            // Assert
            Assert.True(result.IsColumnSpecific);
            Assert.Equal(2, result.Conditions.Count);
            Assert.Equal("user_name", result.Conditions[0].Column);
            Assert.Equal("John", result.Conditions[0].SearchTerm);
            Assert.Equal("action", result.Conditions[1].Column);
            Assert.Equal("LOGIN", result.Conditions[1].SearchTerm);
            Assert.Equal("AND", result.Conditions[1].Operator);
        }

        [Fact]
        public void Parse_ColumnSpecificWithOr_ReturnsMultipleConditions()
        {
            // Arrange
            var query = "status:SUCCESS OR status:FAILED";

            // Act
            var result = _parser.Parse(query);

            // Assert
            Assert.True(result.IsColumnSpecific);
            Assert.Equal(2, result.Conditions.Count);
            Assert.Equal("status", result.Conditions[0].Column);
            Assert.Equal("SUCCESS", result.Conditions[0].SearchTerm);
            Assert.Equal("status", result.Conditions[1].Column);
            Assert.Equal("FAILED", result.Conditions[1].SearchTerm);
            Assert.Equal("OR", result.Conditions[1].Operator);
        }

        [Fact]
        public void Parse_MixedQuery_WithColumnAndFullText()
        {
            // Arrange
            var query = "user_name:admin AND login";

            // Act
            var result = _parser.Parse(query);

            // Assert
            Assert.True(result.IsColumnSpecific);
            Assert.Equal(2, result.Conditions.Count);
        }

        [Fact]
        public void Parse_EmptyQuery_ReturnsEmptyResult()
        {
            // Arrange
            var query = "";

            // Act
            var result = _parser.Parse(query);

            // Assert
            Assert.False(result.IsColumnSpecific);
            Assert.Empty(result.PostgresQuery);
            Assert.Empty(result.Conditions);
        }

        [Fact]
        public void Parse_WhitespaceQuery_ReturnsEmptyResult()
        {
            // Arrange
            var query = "   ";

            // Act
            var result = _parser.Parse(query);

            // Assert
            Assert.False(result.IsColumnSpecific);
            Assert.Empty(result.PostgresQuery.Trim());
            Assert.Empty(result.Conditions);
        }

        [Theory]
        [InlineData("user_name:John", "user_name", "John")]
        [InlineData("action:LOGIN", "action", "LOGIN")]
        [InlineData("status:SUCCESS", "status", "SUCCESS")]
        [InlineData("ip_address:192.168.1.1", "ip_address", "192.168.1.1")]
        public void Parse_VariousColumnQueries_ParsedCorrectly(string query, string expectedColumn, string expectedTerm)
        {
            // Act
            var result = _parser.Parse(query);

            // Assert
            Assert.True(result.IsColumnSpecific);
            Assert.Single(result.Conditions);
            Assert.Equal(expectedColumn, result.Conditions[0].Column);
            Assert.Equal(expectedTerm, result.Conditions[0].SearchTerm);
        }

        [Fact]
        public void Parse_QuotedPhrase_HandledCorrectly()
        {
            // Arrange
            var query = "\"user login\"";

            // Act
            var result = _parser.Parse(query);

            // Assert
            Assert.False(result.IsColumnSpecific);
            Assert.NotEmpty(result.PostgresQuery);
        }

        [Fact]
        public void Parse_CaseSensitivity_OperatorsNotCaseSensitive()
        {
            // Arrange
            var query1 = "login AND failed";
            var query2 = "login and failed";

            // Act
            var result1 = _parser.Parse(query1);
            var result2 = _parser.Parse(query2);

            // Assert
            Assert.Equal(result1.PostgresQuery, result2.PostgresQuery);
        }
    }
}
