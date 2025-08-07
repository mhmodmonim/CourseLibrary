namespace CourseLibrary.API.Services;

public class PropertyMappingValue
{
    public IEnumerable<string> DestinationProperties { get; private set; }
    public bool Revert { get; private set; }


    public PropertyMappingValue(IEnumerable<string> destinationProperties, bool revert = false)
    {
        if (destinationProperties == null || !destinationProperties.Any())
        {
            throw new ArgumentNullException(nameof(destinationProperties), "Destination properties cannot be null or empty.");
        }
        DestinationProperties = destinationProperties;
        Revert = revert;
    }



}
