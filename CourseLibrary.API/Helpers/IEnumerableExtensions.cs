using System.Collections;
using System.Dynamic;
using System.Reflection;

namespace CourseLibrary.API.Helpers;

    public static class IEnumerableExtensions
    {
    public static IEnumerable<ExpandoObject> ShapeData<TSource>(
        this IEnumerable<TSource> source,
        string fields)
    {
        if (source == null || !source.Any())
        {
            return Enumerable.Empty<ExpandoObject>();
        }
        var expandoObjectList = new List<ExpandoObject>();


        var propertyInfoList = new List<PropertyInfo>();

        if(string.IsNullOrWhiteSpace(fields))
        { 
            // Get the properties to include
            var propertyInfos = typeof(TSource)
                .GetProperties(BindingFlags.IgnoreCase | BindingFlags.Public
                | BindingFlags.Instance);

            
            propertyInfoList.AddRange(propertyInfos);
        }
        else
        {
            var fieldsAfterSplit = fields.Split(",");
            foreach (var field in fieldsAfterSplit)
            {
                var propertyName = field.Trim();

                var propertyInfo = typeof(TSource)
                    .GetProperty(propertyName, BindingFlags.IgnoreCase |
                                               BindingFlags.Public | BindingFlags.Instance);

                if (propertyInfo == null)
                {
                    throw new Exception($"Property " +
                                        $"{propertyName} not found on type {typeof(TSource).Name}");
                }

                propertyInfoList.Add(propertyInfo);
            }
        }

        foreach (var sourceObject in source)
        {
            var dataShapedObject = new ExpandoObject() as IDictionary<string, object?>;

            foreach (var propertyInfo in propertyInfoList)
            {
                var propertyValue = propertyInfo.GetValue(sourceObject);

                dataShapedObject.Add(propertyInfo.Name, propertyValue);
            }

            expandoObjectList.Add((ExpandoObject)dataShapedObject);


        }

        return expandoObjectList;
    }
}
