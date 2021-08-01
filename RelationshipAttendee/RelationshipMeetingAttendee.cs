using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelationshipAttendee
{
    public class RelationshipMeetingAttendee : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            // Obtain the tracing service
            ITracingService tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            // Obtain the execution context from the service provider.  
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);
            if (tracingService == null)
            {
                throw new InvalidPluginExecutionException("Failed to retrieve the tracing service.");
            }

            try
            {
                if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                {
                    Entity relationshipAttendee = (Entity)context.InputParameters["Target"];
                    tracingService.Trace("Meeting");
                    if (relationshipAttendee!= null &&  relationshipAttendee.Attributes.Contains("new_contact") && relationshipAttendee.Attributes["new_contact"]!= null)
                    {
                        tracingService.Trace("Meeting 1:"+ ((EntityReference)relationshipAttendee.Attributes["new_contact"]).Name);
                        relationshipAttendee.Attributes["new_name"] = ((EntityReference)relationshipAttendee.Attributes["new_contact"]).Name;
                    }

                    if (relationshipAttendee != null && relationshipAttendee.Attributes.Contains("new_user") && relationshipAttendee.Attributes["new_user"] != null)
                    {
                        tracingService.Trace("Meeting 2:" + ((EntityReference)relationshipAttendee.Attributes["new_user"]).Name);
                        relationshipAttendee.Attributes["new_name"] = ((EntityReference)relationshipAttendee.Attributes["new_user"]).Name;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException(ex.ToString());
            }
        }
    }
}
