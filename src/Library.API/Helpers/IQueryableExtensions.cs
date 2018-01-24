using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using Library.API.Services;

namespace Library.API.Helpers
{
    public static class IQueryableExtensions
    {
        public static IQueryable<T> ApplySort<T>(this IQueryable<T> source, string orderBy,
            Dictionary<string, PropertyMappingValue> mappings)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (mappings == null)
            {
                throw new ArgumentNullException(nameof(mappings));
            }

            if (string.IsNullOrWhiteSpace(orderBy))
            {
                return source;
            }

            var orderByAfterSplit = orderBy.Split(',');

            foreach (var orderByClause in orderByAfterSplit.Reverse())
            {
                var trimmedClause = orderByClause.Trim();

                var isDesc = trimmedClause.EndsWith(" desc");

                var indexOfFirstSpace = trimmedClause.IndexOf(" ");

                var propertyName = indexOfFirstSpace == -1 ? trimmedClause : trimmedClause.Remove(indexOfFirstSpace);

                if (!mappings.ContainsKey(propertyName))
                {
                    throw new ArgumentException($"Key mapping for {propertyName} is missing");
                }

                var propertyMappingValue = mappings[propertyName];

                if (propertyMappingValue == null)
                {
                    throw new ArgumentNullException(nameof(propertyMappingValue));
                }

                foreach (var destinationProperty in propertyMappingValue.DestinationProperties.Reverse())
                {
                    if (propertyMappingValue.Revert)
                    {
                        isDesc = !isDesc;
                    }

                    source = source.OrderBy(destinationProperty + (isDesc ? " descending" : " ascending"));
                }
            }

            return source;
        }
    }
}