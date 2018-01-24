using System.Collections.Generic;

namespace Library.API.Services
{
    public class PropertyMapping<TSource, TDestination> : IPropertyMapping
    {
        public PropertyMapping(Dictionary<string, PropertyMappingValue> mappingDictionary)
        {
            MappingDictionary = mappingDictionary;
        }

        public Dictionary<string, PropertyMappingValue> MappingDictionary { get; private set; }
    }
}