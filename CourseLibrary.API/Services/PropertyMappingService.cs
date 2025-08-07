namespace CourseLibrary.API.Services;

public class PropertyMappingService : IPropertyMappingService
{

    private readonly Dictionary<string, PropertyMappingValue> _authorPropertyMappings = 
        new(StringComparer.OrdinalIgnoreCase)
        {
            {"Id", new(["Id"]) },
            {"MainCategory", new(["MainCategory"]) },
            {"Age", new(["DateOfBirth"], true) },
            {"Name", new(["FirstName", "LastName"]) }
        };    


    private readonly IList<IPropertyMapping> _propertyMappings = [];

    public PropertyMappingService()
    {
        _propertyMappings.Add(
            new PropertyMapping<Models.AuthorDto,Entities.Author>(_authorPropertyMappings));
    }

    public Dictionary<string, PropertyMappingValue> GetPropertyMapping<TSource, TDestination>()
    {
        // get matching property mapping
        var matchingMapping = _propertyMappings
            .OfType<PropertyMapping<TSource, TDestination>>();

        if (matchingMapping.Count() == 1)
        {
            return matchingMapping.First().MappingDictionary;
        }

        throw new Exception($"Cannot find property mapping for " +
                            $"{typeof(TSource)} to {typeof(TDestination)}");
    }

    public bool ValidMappingExistsFor<TSource, TDestination>(string fields)
    {
        if (string.IsNullOrWhiteSpace(fields))
        {
            return true; // no fields specified, so valid by default
        }
        var fieldAfterSplit = fields.Split(',');
        foreach (var field in fieldAfterSplit)
        {
            var trimmedField = field.Trim();
            if (string.IsNullOrWhiteSpace(trimmedField))
            {
                continue; // skip empty fields
            }
            var propertyMapping = GetPropertyMapping<TSource, TDestination>();
            if (!propertyMapping.ContainsKey(trimmedField))
            {
                return false; // invalid mapping found
            }
        }
        return true; // all fields are valid
    }

}
