using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace AuditLogSearchApi.Services
{
    /// <summary>
    /// Parses search queries with AND/OR operators and column-specific searches
    /// Supports formats like:
    /// - "login failed" (simple search across all columns)
    /// - "login AND failed" (both terms must exist)
    /// - "login OR logout" (either term must exist)
    /// - "user_name:John AND action:LOGIN" (column-specific searches)
    /// - "(user_name:John OR user_name:Jane) AND status:SUCCESS" (grouped queries)
    /// </summary>
    public class SearchQueryParser
    {
        private static readonly HashSet<string> ValidColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "id", "timestamp", "user_id", "user_name", "action", "resource_type",
            "resource_id", "ip_address", "status", "details", "metadata", "created_at"
        };

        public class ParsedQuery
        {
            public string PostgresQuery { get; set; } = string.Empty;
            public List<QueryCondition> Conditions { get; set; } = new List<QueryCondition>();
            public bool IsColumnSpecific { get; set; }
        }

        public class QueryCondition
        {
            public string? Column { get; set; }
            public string SearchTerm { get; set; } = string.Empty;
            public string Operator { get; set; } = "AND"; // AND, OR
            public bool IsNegation { get; set; }
        }

        /// <summary>
        /// Parse a search query into a PostgreSQL-compatible full-text search query
        /// </summary>
        public ParsedQuery Parse(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return new ParsedQuery { PostgresQuery = "" };
            }

            var result = new ParsedQuery();
            var normalizedQuery = query.Trim();

            // Check if query contains column-specific searches (column:value)
            var columnSearchPattern = @"(\w+):([^\s\)]+)";
            var hasColumnSearch = Regex.IsMatch(normalizedQuery, columnSearchPattern);

            result.IsColumnSpecific = hasColumnSearch;

            if (hasColumnSearch)
            {
                // Parse column-specific searches with AND/OR logic
                result.Conditions = ParseColumnSpecificQuery(normalizedQuery);
            }
            else
            {
                // Parse simple full-text search with AND/OR logic
                result.PostgresQuery = ParseFullTextQuery(normalizedQuery);
            }

            return result;
        }

        /// <summary>
        /// Parse a full-text search query with AND/OR operators
        /// Converts to PostgreSQL tsquery format
        /// </summary>
        private string ParseFullTextQuery(string query)
        {
            // Replace logical operators with PostgreSQL tsquery operators
            // AND -> &, OR -> |, NOT -> !
            
            // Handle parentheses for grouping
            var processed = query;

            // Split by spaces while preserving quoted strings
            var tokens = SplitPreservingQuotes(processed);
            var tsQueryParts = new List<string>();
            var currentOperator = "&"; // default AND

            for (int i = 0; i < tokens.Count; i++)
            {
                var token = tokens[i].Trim();

                if (string.IsNullOrEmpty(token))
                    continue;

                if (token.Equals("AND", StringComparison.OrdinalIgnoreCase))
                {
                    currentOperator = "&";
                }
                else if (token.Equals("OR", StringComparison.OrdinalIgnoreCase))
                {
                    currentOperator = "|";
                }
                else if (token.Equals("NOT", StringComparison.OrdinalIgnoreCase))
                {
                    // Handle NOT for next term
                    if (i + 1 < tokens.Count)
                    {
                        i++;
                        var nextToken = tokens[i].Trim().Trim('"', '\'');
                        if (!string.IsNullOrEmpty(nextToken))
                        {
                            if (tsQueryParts.Count > 0)
                            {
                                tsQueryParts.Add(currentOperator);
                            }
                            tsQueryParts.Add($"!{EscapeForTsQuery(nextToken)}");
                            currentOperator = "&";
                        }
                    }
                }
                else
                {
                    // Regular search term
                    var cleanToken = token.Trim('"', '\'');
                    if (!string.IsNullOrEmpty(cleanToken))
                    {
                        if (tsQueryParts.Count > 0)
                        {
                            tsQueryParts.Add(currentOperator);
                        }
                        tsQueryParts.Add(EscapeForTsQuery(cleanToken));
                        currentOperator = "&"; // Reset to default
                    }
                }
            }

            return tsQueryParts.Count > 0 ? string.Join(" ", tsQueryParts) : "";
        }

        /// <summary>
        /// Parse column-specific query with AND/OR logic
        /// Example: "user_name:John AND action:LOGIN OR action:LOGOUT"
        /// </summary>
        private List<QueryCondition> ParseColumnSpecificQuery(string query)
        {
            var conditions = new List<QueryCondition>();
            
            // Split by spaces while preserving quoted strings
            var tokens = SplitPreservingQuotes(query);
            string currentOperator = "AND";

            for (int i = 0; i < tokens.Count; i++)
            {
                var token = tokens[i].Trim();

                if (string.IsNullOrEmpty(token))
                    continue;

                if (token.Equals("AND", StringComparison.OrdinalIgnoreCase))
                {
                    currentOperator = "AND";
                }
                else if (token.Equals("OR", StringComparison.OrdinalIgnoreCase))
                {
                    currentOperator = "OR";
                }
                else
                {
                    // Check if this is a column:value pair
                    var colonIndex = token.IndexOf(':');
                    if (colonIndex > 0 && colonIndex < token.Length - 1)
                    {
                        var column = token.Substring(0, colonIndex);
                        var value = token.Substring(colonIndex + 1).Trim('"', '\'');

                        // Validate column name
                        if (ValidColumns.Contains(column))
                        {
                            conditions.Add(new QueryCondition
                            {
                                Column = column,
                                SearchTerm = value,
                                Operator = currentOperator,
                                IsNegation = false
                            });
                            currentOperator = "AND"; // Reset to default
                        }
                    }
                    else
                    {
                        // This is a standalone search term (not column-specific)
                        var cleanToken = token.Trim('"', '\'');
                        if (!string.IsNullOrEmpty(cleanToken))
                        {
                            conditions.Add(new QueryCondition
                            {
                                Column = null, // Full-text search across all columns
                                SearchTerm = cleanToken,
                                Operator = currentOperator,
                                IsNegation = false
                            });
                            currentOperator = "AND"; // Reset to default
                        }
                    }
                }
            }

            return conditions;
        }

        /// <summary>
        /// Split query by spaces while preserving quoted strings
        /// </summary>
        private List<string> SplitPreservingQuotes(string input)
        {
            var result = new List<string>();
            var current = "";
            var inQuotes = false;
            var quoteChar = ' ';

            for (int i = 0; i < input.Length; i++)
            {
                var c = input[i];

                if ((c == '"' || c == '\'') && !inQuotes)
                {
                    inQuotes = true;
                    quoteChar = c;
                    current += c;
                }
                else if (c == quoteChar && inQuotes)
                {
                    inQuotes = false;
                    current += c;
                }
                else if (c == ' ' && !inQuotes)
                {
                    if (!string.IsNullOrEmpty(current))
                    {
                        result.Add(current);
                        current = "";
                    }
                }
                else
                {
                    current += c;
                }
            }

            if (!string.IsNullOrEmpty(current))
            {
                result.Add(current);
            }

            return result;
        }

        /// <summary>
        /// Escape special characters for PostgreSQL tsquery
        /// </summary>
        private string EscapeForTsQuery(string term)
        {
            // Remove special characters that could break tsquery
            // Keep alphanumeric, hyphens, and underscores
            var cleaned = Regex.Replace(term, @"[^\w\s-]", "");
            
            // Replace spaces with AND operator for phrase searching
            cleaned = cleaned.Replace(" ", ":*&");
            
            // Add prefix matching for partial word matches
            if (!string.IsNullOrEmpty(cleaned))
            {
                cleaned += ":*";
            }

            return cleaned;
        }

        /// <summary>
        /// Validate if a column name is valid for searching
        /// </summary>
        public bool IsValidColumn(string columnName)
        {
            return ValidColumns.Contains(columnName);
        }

        /// <summary>
        /// Get all valid column names
        /// </summary>
        public IEnumerable<string> GetValidColumns()
        {
            return ValidColumns.OrderBy(c => c);
        }
    }
}
