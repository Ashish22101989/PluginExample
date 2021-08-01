using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System.Net.Http.Headers;
using Microsoft.Xrm.Sdk;

using Microsoft.Xrm.Sdk.Query;
using Microsoft.Crm.Sdk.Messages;

namespace RelationshipAttendee
{
    public class RelationshipAttendee
    {

        /// <summary>
        /// Create Required Attendee in Relationship Attendee Entity
        /// </summary>
        /// <param name="attendees"></param>
        /// <param name="appointmentId"></param>
        /// <param name="service"></param>
        public void Create(EntityCollection attendees, Guid appointmentId, IOrganizationService service,ITracingService tracingService)
        {
            if (attendees != null && attendees.Entities.Count > 0)
            {
                foreach (var attendee in attendees.Entities)
                {
                    if (attendee != null && attendee.Contains("partyid") && attendee.Attributes["partyid"] != null)
                    {
                        var entRef = (EntityReference)attendee.Attributes["partyid"];
                        if (entRef.LogicalName == Constants.ContactSchemaName)
                        {
                            Entity ent = new Entity(Constants.RelationshipAttendeeSchemaName);
                            tracingService.Trace(entRef.LogicalName + "Name:" + entRef.Name);
                            ent.Attributes[Constants.RelationshipAttendee.NameField] = entRef.Name;
                            ent.Attributes[Constants.RelationshipAttendee.ContactFieldName] = new EntityReference(Constants.ContactSchemaName, entRef.Id);
                            ent.Attributes[Constants.RelationshipAttendee.AppointmentFieldName] = new EntityReference(Constants.AppointmentSchemaName, appointmentId);
                            service.Create(ent);
                        }
                        else if (entRef.LogicalName == Constants.UserSchemaName)
                        {
                            tracingService.Trace(entRef.LogicalName + "Name:" + entRef.Name);
                            Entity ent = new Entity(Constants.RelationshipAttendeeSchemaName);
                            ent.Attributes[Constants.RelationshipAttendee.NameField] = entRef.Name;
                            ent.Attributes[Constants.RelationshipAttendee.UserFieldName] = new EntityReference(Constants.UserSchemaName, entRef.Id);
                            ent.Attributes[Constants.RelationshipAttendee.AppointmentFieldName] = new EntityReference(Constants.AppointmentSchemaName, appointmentId);
                            service.Create(ent);
                        }
                    }
                }
            }
        }
        
        public EntityCollection GetRecords(Guid appointmentId, IOrganizationService organizationService,ITracingService tracingService)
        {
            if (appointmentId != null && appointmentId != Guid.Empty)
            {
                string appointmentIdStr = appointmentId.ToString();
                //appointmentIdStr = "F6682D3E-68E1-EB11-BACB-0022486EAE28";
                //new_relationshipattendees
                #region fetch
                string str = "<fetch version=\"1.0\" output-format=\"xml-platform\" mapping=\"logical\" distinct=\"false\">";
                str += "  <entity name=\""+ Constants.RelationshipAttendeeSchemaName + "\">";
                str += "    <attribute name=\""+ Constants.RelationshipAttendeeSchemaName + "id"+"\" />";
                str += "    <attribute name=\""+ Constants.RelationshipAttendee.NameField +"\" />";
                str += "    <attribute name=\""+Constants.RelationshipAttendee.ContactFieldName +"\" />";
                str += "    <attribute name=\""+Constants.RelationshipAttendee.UserFieldName+"\" />";
                str += "    <attribute name=\""+ Constants.RelationshipAttendee.AppointmentFieldName + "\" />";
                str += "    <attribute name=\""+Constants.RelationshipAttendee.AttendanceStatusField+"\" />";
                str += "    <order attribute=\""+ Constants.RelationshipAttendee.NameField + "\" descending=\"false\" />";
                str += "    <filter type=\"and\">";
                str += "      <condition attribute=\""+Constants.RelationshipAttendee.AppointmentFieldName +"\" operator=\"eq\" uitype=\"appointment\" value=\"{" + appointmentIdStr + "}\" />";
                str += "    </filter>";
                str += "  </entity>";
                str += "</fetch>";
                #endregion fetch
                tracingService.Trace("Fetch:" + str);

                return organizationService.RetrieveMultiple(new FetchExpression(str));
            }
            return null;
        }

    }
}
