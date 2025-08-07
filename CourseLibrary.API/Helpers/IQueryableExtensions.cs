using System.Linq.Expressions;
using CourseLibrary.API.Services;
using System.Linq.Dynamic.Core;

namespace CourseLibrary.API.Helpers;


public static class IQueryableExtensions
{
    public static IQueryable<T> ApplySort<T>(
        this IQueryable<T> source,
        string orderBy,
        Dictionary<string, PropertyMappingValue> propertyMapping)
    {
        if (source == null)
        {
            throw new ArgumentNullException(nameof(source));
        }
        if (string.IsNullOrWhiteSpace(orderBy))
        {
            return source;
        }
        if (propertyMapping == null || !propertyMapping.Any())
        {
            throw new ArgumentNullException(nameof(propertyMapping), "Property mapping cannot be null or empty.");
        }

        var orderByString = string.Empty;
        var orderBySplit = orderBy.Split(',');

        // Loop through each order by clause
        foreach (var orderByClause in orderBySplit)
        {
            var trimmedOrderByClause = orderByClause.Trim();
            var orderDescending = trimmedOrderByClause.EndsWith(" desc");
            var indexOfFirstSpace = trimmedOrderByClause.IndexOf(" ");
            var propertyName = indexOfFirstSpace == -1
                ? trimmedOrderByClause
                : trimmedOrderByClause.Remove(indexOfFirstSpace);

            if (!propertyMapping.ContainsKey(propertyName))
            {
                throw new ArgumentNullException($"key name {propertyName} is missing");
            }

            var propertyMappingValue = propertyMapping[propertyName];

            if (propertyMappingValue == null)
            {
                throw new ArgumentNullException(nameof(propertyMappingValue));
            }

            foreach (var destinationProperty in propertyMappingValue.DestinationProperties)
            {
                    orderByString = orderByString +
                                (string.IsNullOrWhiteSpace(orderByString) ? string.Empty : ", ")
                                + destinationProperty
                                + (orderDescending ? " descending" : " ascending");
            }
        }

        // Use System.Linq.Dynamic.Core to support string-based OrderBy
        return source.OrderBy(orderByString);
    }
}
