﻿using System.Collections.Generic;
using System.Linq;
using GRomash.CrmWebApiEarlyBoundGenerator.Infrastructure.Builders;
using GRomash.CrmWebApiEarlyBoundGenerator.Infrastructure.Factory;
using GRomash.CrmWebApiEarlyBoundGenerator.Infrastructure.Repository;
using Microsoft.Xrm.Sdk.Metadata;

namespace GRomash.CrmWebApiEarlyBoundGenerator.Infrastructure.Service
{
    public class GenerationService
    {
        /// <summary>
        /// The metadata repository
        /// </summary>
        private readonly MetadataRepository _metadataRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="GenerationService"/> class.
        /// </summary>
        /// <param name="metadataRepository">The metadata repository.</param>
        public GenerationService(MetadataRepository metadataRepository)
        {
            _metadataRepository = metadataRepository;
        }

        /// <summary>
        /// Generates the entities.
        /// </summary>
        /// <param name="nameSpace">The name space.</param>
        /// <param name="outFolder">The out folder.</param>
        /// <param name="entities">The entities.</param>
        /// <param name="entityMetadatas">The entity metadatas.</param>
        public void GenerateEntities(string nameSpace, string outFolder, string[] entities, IEnumerable<EntityMetadata> entityMetadatas)
        {
            var classBuilder = new ClassBuilder();
            var fileBuilder = new FileBuilder(outFolder);
            var fieldsFactory = new FieldsFactory();
            var propsFactory = new PropertiesFactory(entities, _metadataRepository);

            fileBuilder.BuildBaseClass(nameSpace);

            foreach (var entityMetadata in entityMetadatas)
            {
                var classModel = classBuilder.GetClassModel(entityMetadata, nameSpace);

                var relationshipMetadatas = entityMetadata.OneToManyRelationships.Union(entityMetadata.ManyToOneRelationships).ToArray();

                classModel.Properties =
                    propsFactory.GetPropertyModels(entityMetadata.Attributes, relationshipMetadatas);
                classModel.Fields =
                    fieldsFactory.GetFields(entityMetadata.Attributes);
                classModel.Schemas = fieldsFactory.GetSchemaNames(entityMetadata.Attributes);
                classModel.PropertiesFields = fieldsFactory.GetProperties(entityMetadata.ManyToManyRelationships, entityMetadata.ManyToOneRelationships);


                fileBuilder.BuildClass(classModel);
            }
        }
    }
}