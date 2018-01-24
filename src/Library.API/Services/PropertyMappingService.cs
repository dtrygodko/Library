using System;
using System.Collections.Generic;
using System.Linq;
using Library.API.Entities;
using Library.API.Models;

namespace Library.API.Services
{
    public class PropertyMappingService : IPropertyMappingService
    {
        private readonly Dictionary<string, PropertyMappingValue> _authorPropertyMapping =
            new Dictionary<string, PropertyMappingValue>
            {
                {"id", new PropertyMappingValue(new List<string> {"Id"})},
                {"genre", new PropertyMappingValue(new List<string> {"Genre"})},
                {"age", new PropertyMappingValue(new List<string> {"DateOfBirth"}, true)},
                {"name", new PropertyMappingValue(new List<string> {"FirstName", "LastName"})}
            };

        private readonly IList<IPropertyMapping> _propertyMappings = new List<IPropertyMapping>();

        public PropertyMappingService()
        {
            _propertyMappings.Add(new PropertyMapping<AuthorDto, Author>(_authorPropertyMapping));
        }

        public Dictionary<string, PropertyMappingValue> GetPropertyMapping<TSource, TDestination>()
        {
            var matchingMapping = _propertyMappings.OfType<PropertyMapping<TSource, TDestination>>();

            if (matchingMapping.Count() == 1)
            {
                return matchingMapping.First().MappingDictionary;
            }

            throw new Exception($"Cannot find property mapping for <{typeof(TSource)}, {typeof(TDestination)}>");
        }
    }
}