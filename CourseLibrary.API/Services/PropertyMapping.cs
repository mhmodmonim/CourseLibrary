namespace CourseLibrary.API.Services;

public class PropertyMapping<TSource, TDestination> : IPropertyMapping
{
    public Dictionary<string, PropertyMappingValue> MappingDictionary { get; private set; }




    public PropertyMapping(Dictionary<string, PropertyMappingValue> mappingDictionary)
    {
        if (mappingDictionary == null || !mappingDictionary.Any())
        {
            throw new ArgumentNullException(nameof(mappingDictionary), "Mapping dictionary cannot be null or empty.");
        }
        MappingDictionary = mappingDictionary;
    }

}

       
