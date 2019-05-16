using System.Collections.Generic;
using BlogDemo.Core.Entites;
using BlogDemo.Core.Interfaces;

namespace BlogDemo.Infrastructure.Services
{
    //TSource到 TDestination的映射关系，字典中结构   属性名-list<MappedProperty>。   MappedProperty存放值和是否需要反转
    public abstract class PropertyMapping<TSource, TDestination> : IPropertyMapping 
        where TDestination : Entity
    {
        public Dictionary<string, List<MappedProperty>> MappingDictionary { get; }

        protected PropertyMapping(Dictionary<string, List<MappedProperty>> mappingDictionary)
        {
            MappingDictionary = mappingDictionary;

            MappingDictionary[nameof(Entity.Id)] = new List<MappedProperty>
            {
                new MappedProperty { Name = nameof(Entity.Id), Revert = false}
            };
        }
    }
}
