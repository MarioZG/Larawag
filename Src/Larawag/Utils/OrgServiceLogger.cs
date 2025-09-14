using Larawag.Services;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Larawag.Utils
{
    public class OrgServiceLogger : IOrganizationService
    {

        private readonly IOrganizationService innerService;
        private readonly QueryToFetchXmlConverter queryToFetchXmlConverter;
        private TextWriter outputWriter;

        public OrgServiceLogger(IOrganizationService innerService, QueryToFetchXmlConverter queryToFetchXmlConverter)
        {
            this.innerService = innerService;
            this.queryToFetchXmlConverter = queryToFetchXmlConverter;
        }

        public void SetWriter(TextWriter outputWriter)
        {
            this.outputWriter = outputWriter;

        }

        public void Associate(string entityName, Guid entityId, Relationship relationship, EntityReferenceCollection relatedEntities)
        {
            innerService.Associate(entityName, entityId, relationship, relatedEntities);
        }

        public Guid Create(Entity entity)
        {
            return innerService.Create(entity);
        }

        public void Delete(string entityName, Guid id)
        {
            innerService.Delete(entityName, id);
        }

        public void Disassociate(string entityName, Guid entityId, Relationship relationship, EntityReferenceCollection relatedEntities)
        {
            innerService.Disassociate(entityName, entityId, relationship, relatedEntities);
        }

        public OrganizationResponse Execute(OrganizationRequest request)
        {
            outputWriter?.WriteLine("Execute: "+ request.RequestName);

            if(request is RetrieveMultipleRequest)
            {
                outputWriter?.WriteLine($"Query type: {((RetrieveMultipleRequest)request).Query.GetType().FullName}");
                try
                {
                    var fetchxml = queryToFetchXmlConverter.Convert(((RetrieveMultipleRequest)request).Query);
                    outputWriter?.WriteLine(XDocument.Parse(fetchxml).ToString());
                }
                catch(Exception ex)
                {
                    outputWriter?.WriteLine("Error translating to fetchXML:"+ex.ToString());

                }
            }

            return innerService.Execute(request);
        }

        public Entity Retrieve(string entityName, Guid id, ColumnSet columnSet)
        {
            outputWriter?.WriteLine("Retrieve");
            return innerService.Retrieve(entityName, id, columnSet);
        }

        public EntityCollection RetrieveMultiple(QueryBase query)
        {
            outputWriter?.WriteLine("RetrieveMultiple");
            return innerService.RetrieveMultiple(query);
        }

        public void Update(Entity entity)
        {
            innerService.Update(entity);
        }
    }
}
