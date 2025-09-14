using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Larawag.Services
{
    public class QueryToFetchXmlConverter
    {
        private readonly IOrganizationService innerService;

        public QueryToFetchXmlConverter(IOrganizationService innerService)
        {
            this.innerService = innerService;
        }

        public string Convert(QueryBase query)
        {
            var fetchXmlConvertRequest = new QueryExpressionToFetchXmlRequest()
            {
                Query = query
            };
            var fetchXmlResp = innerService.Execute(fetchXmlConvertRequest) as QueryExpressionToFetchXmlResponse;

            return fetchXmlResp.FetchXml;
        }
    }
}
