using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Reflection;
using Highway.Data.EntityFramework;

namespace Highway.Data
{
    /// <summary>
    /// Creates Mapping Details from the context metadata
    /// </summary>
    public class MappingDetailFactory
    {
        readonly IDictionary<Type, MappingDetail> _mappingDetails = new Dictionary<Type, MappingDetail>();
        readonly ICollection<EntitySet> _sets = new Collection<EntitySet>();
        public MappingDetail CreateDetails(IDataContext context, Type entityType)
        {
            MappingDetail mappingDetail;
            if (_mappingDetails.TryGetValue(entityType, out mappingDetail))
            {
                return mappingDetail;
            }
            mappingDetail = new MappingDetail();
            var typedContext = context as IObjectContextAdapter;
            if (typedContext == null)
            {
                return null;
            }


            var metadata = GetObjectMetadata(entityType, typedContext);
            
            if (!_sets.Any())
            {
                var storageMetadata = GetStorageMetadata(typedContext);
                GetEntitySets(storageMetadata);
                EntitySet setForEntity;
                if (_sets.Count(x => x.ElementType.Name == entityType.Name) > 1)
                {
                    //TODO Handle multiple objects with the same name
                }
                else
                {
                    setForEntity = _sets.Single(x => x.ElementType.Name == entityType.Name);
                    mappingDetail.Table = string.IsNullOrWhiteSpace(setForEntity.Table) ? setForEntity.Name : setForEntity.Table;
                    mappingDetail.Schema = string.IsNullOrWhiteSpace(setForEntity.Schema) ? "dbo" : setForEntity.Schema;
                }


                mappingDetail.Properties = metadata.Properties
                    .Select(k => entityType.GetProperty(k.Name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
                    .Select(x => new PropertyDetail { Property = x}).ToList();

                
                var setMaps = (storageMetadata.GetPropertyValue("AllSetMaps") as IEnumerable);
                foreach (var map in setMaps)
                {
                    if (map.GetType().Name != "StorageEntitySetMapping") continue;
                    ExtractTypeMappings(entityType, map, mappingDetail);
                }

                
            }
            
            _mappingDetails.Add(entityType, mappingDetail);
            return mappingDetail;
        }

        private void GetEntitySets(GlobalItem storageMetadata)
        {
            var entitySets = storageMetadata.GetPropertyValue("EntitySetMaps") as IEnumerable;

            foreach (var entitySet in entitySets)
            {
                var set = entitySet.GetPropertyValue("Set") as EntitySet;
                _sets.Add(set);
            }
        }

        private static void ExtractTypeMappings(Type entityType, object map, MappingDetail mappingDetail)
        {
            var typeMappings = map.GetPropertyValue("TypeMappings") as IEnumerable;
            foreach (var typeMap in typeMappings)
            {
                var types = typeMap.GetPropertyValue("Types") as IEnumerable;
                var mappingIsForType = false;
                foreach (var type in types)
                {
                    if (type.GetPropertyValue("Name") != entityType.Name) continue;
                    mappingIsForType = true;
                }
                if (!mappingIsForType) continue;
                ExtractPropertyMappings(typeMap, mappingDetail);
            }
        }

        private static void ExtractPropertyMappings(object typeMap, MappingDetail mappingDetail)
        {
            var fragments = typeMap.GetPropertyValue("MappingFragments") as IEnumerable;
            foreach (var fragment in fragments)
            {
                var properties = fragment.GetPropertyValue("Properties") as IEnumerable;
                foreach (var property in properties)
                {
                    var name = property.GetPropertyValue("EdmProperty").GetPropertyValue("Name");
                    var propertyDetail = mappingDetail.Properties.Single(x => x.Property.Name == name);
                    propertyDetail.IsGenerated = property.GetPropertyValue("ColumnProperty").GetPropertyValue("StoreGeneratedPattern").ToString() != "None";
                    propertyDetail.ColumnName = property.GetPropertyValue("ColumnProperty").GetPropertyValue("Name").ToString();
                }
            }
        }

        private static EntityType GetObjectMetadata(Type entityType, IObjectContextAdapter typedContext)
        {
            var metadata = typedContext.ObjectContext.MetadataWorkspace
                .GetItems<EntityType>(DataSpace.OSpace)
                .SingleOrDefault(p => p.FullName == entityType.FullName);

            if (metadata == null)
            {
                throw new InvalidOperationException(String.Format("The type {0} is not known to the DbContext.",
                    entityType.FullName));
            }
            return metadata;
        }

        private static GlobalItem GetStorageMetadata(IObjectContextAdapter typedContext)
        {
            var storageMetaData = typedContext.ObjectContext.MetadataWorkspace
                .GetItems(DataSpace.CSSpace)
                .SingleOrDefault();

            if (storageMetaData == null)
            {
                throw new InvalidOperationException(
                    string.Format("Cannot find Mapping information from an uninitialized context"));
            }
            return storageMetaData;
        }
    }
}