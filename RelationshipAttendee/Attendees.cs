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
    public class Attendee
    {
        public Attendee(Entity preImage, Entity postImage)
        {
            #region get pre and post image required and optional attendees and their count

            if (preImage != null)
            {
                //Entity preAttendeesImage = (Entity)context.PreEntityImages["AttendeesImage"];
                this.PreImageRequiredAttendees = (EntityCollection)preImage.Attributes["requiredattendees"];
                this.PreImageOptionalAttendees = (EntityCollection)preImage.Attributes["optionalattendees"];

                if (this.PreImageRequiredAttendees != null && this.PreImageRequiredAttendees.Entities.Count > 0)
                {
                    this.PreImageRequiredAttendeesCount = this.PreImageRequiredAttendees.Entities.Count;
                }

                if (this.PreImageOptionalAttendees != null && this.PreImageOptionalAttendees.Entities.Count > 0)
                {
                    this.PreImageOptionalAttendeesCount = this.PreImageOptionalAttendees.Entities.Count;
                }
            }

            if (postImage != null)
            {
                //Entity postAttendeesImage = (Entity)context.PostEntityImages["AttendeesImage"];
                this.PostImageRequiredAttendees = (EntityCollection)postImage.Attributes["requiredattendees"];
                this.PostImageOptionalAttendees = (EntityCollection)postImage.Attributes["optionalattendees"];

                if (this.PostImageRequiredAttendees != null && this.PostImageRequiredAttendees.Entities.Count > 0)
                {
                    this.PostImageRequiredAttendeesCount = this.PostImageRequiredAttendees.Entities.Count;
                }
                if (this.PostImageOptionalAttendees != null && this.PostImageOptionalAttendees.Entities.Count > 0)
                {
                    this.PostImageOptionalAttendeesCount = this.PostImageOptionalAttendees.Entities.Count;
                }
            }

            #endregion get pre and post image required and optional attendees and their count
        }

        private EntityCollection PreImageRequiredAttendees { get; set; }
        private EntityCollection PostImageRequiredAttendees { get; set; }
        private EntityCollection PreImageOptionalAttendees { get; set; }
        private EntityCollection PostImageOptionalAttendees { get; set; }
        private int PreImageRequiredAttendeesCount { get; set; }
        private int PostImageRequiredAttendeesCount { get; set; }
        private int PreImageOptionalAttendeesCount { get; set; }
        private int PostImageOptionalAttendeesCount { get; set; }

        private void CreateRelationshipAteendeeRecords(EntityCollection newlyAddedRequiredAttendees, Entity appointment, AppointmentAttendees appointmentAttendees, IOrganizationService service,ITracingService tracingService)
        {

            tracingService.Trace("CreateRelationshipAteendeeRecords:" + newlyAddedRequiredAttendees.Entities.Count);
            foreach (var attendee in newlyAddedRequiredAttendees.Entities)
            {
                if (attendee != null && attendee.Contains("partyid") && attendee.Attributes["partyid"] != null)
                {
                    var entRef = (EntityReference)attendee.Attributes["partyid"];
                    if (entRef.LogicalName == Constants.ContactSchemaName)
                    {
                        int contactCount = appointmentAttendees.Contacts.Where(item => item.Id == entRef.Id).Count();
                        tracingService.Trace("CreateRelationshipAteendeeRecords Contact Count:" + contactCount);
                        if (contactCount == 0)
                        {
                            Entity ent = new Entity(Constants.RelationshipAttendeeSchemaName);
                            tracingService.Trace("CreateRelationshipAteendeeRecords Contact Name and Id:" + entRef.Name + " "+ entRef.Id.ToString());
                            ent.Attributes["new_name"] = entRef.Name;
                            ent.Attributes[Constants.RelationshipAttendee.ContactFieldName] = new EntityReference(Constants.ContactSchemaName, entRef.Id);
                            ent.Attributes[Constants.RelationshipAttendee.AppointmentFieldName] = new EntityReference(Constants.AppointmentSchemaName, appointment.Id);
                            service.Create(ent);
                        }
                    }
                    else if (entRef.LogicalName == Constants.UserSchemaName)
                    {

                        int userCount = appointmentAttendees.Users.Where(item => item.Id == entRef.Id).Count();
                        tracingService.Trace("CreateRelationshipAteendeeRecords User Count:" + userCount);
                        if (userCount == 0)
                        {
                            Entity ent = new Entity(Constants.RelationshipAttendeeSchemaName);
                            tracingService.Trace("CreateRelationshipAteendeeRecords User Name and Id:" + entRef.Name + " " + entRef.Id.ToString());
                            ent.Attributes["new_name"] = entRef.Name;
                            ent.Attributes[Constants.RelationshipAttendee.ContactFieldName] = new EntityReference(Constants.ContactSchemaName, entRef.Id);
                            ent.Attributes[Constants.RelationshipAttendee.AppointmentFieldName] = new EntityReference(Constants.AppointmentSchemaName, appointment.Id);
                            service.Create(ent);
                        }
                    }
                }
            }
        }

        private void DeleteRelationshipAttendeeRecords(EntityCollection newlyRemovedRequiredAttendees, Entity appointment, AppointmentAttendees appointmentAttendees, IOrganizationService service,ITracingService tracingService)
        {
            tracingService.Trace("DeleteRelationshipAttendeeRecords:newlyRemovedRequiredAttendees " + newlyRemovedRequiredAttendees.Entities.Count);
            foreach (var attendee in newlyRemovedRequiredAttendees.Entities)
            {
                if (attendee != null && attendee.Contains("partyid") && attendee.Attributes["partyid"] != null)
                {
                    var entRef = (EntityReference)attendee.Attributes["partyid"];
                    if (entRef.LogicalName == Constants.ContactSchemaName)
                    {
                        var relationShipAttendee = appointmentAttendees.Contacts.Where(item => item.Id == entRef.Id).FirstOrDefault();
                        if (relationShipAttendee != null)
                        {
                            tracingService.Trace("Contact Deleted: " + entRef.Id.ToString());
                            service.Delete(Constants.RelationshipAttendeeSchemaName, relationShipAttendee.RelationshipAttendeeId);
                        }
                    }
                    else if (entRef.LogicalName == Constants.UserSchemaName)
                    {
                        var relationShipAttendee = appointmentAttendees.Users.Where(item => item.Id == entRef.Id).FirstOrDefault();
                        if (relationShipAttendee != null)
                        {
                            tracingService.Trace("User Deleted: " + entRef.Id.ToString());
                            service.Delete(Constants.RelationshipAttendeeSchemaName, relationShipAttendee.RelationshipAttendeeId);
                        }
                    }
                }
            }
        }

        private EntityCollection GetDistinctAttendees(EntityCollection entityCollection1, EntityCollection entityCollection2)
        {
            if (entityCollection1 != null && entityCollection1.Entities.Count > 0 && entityCollection2 != null && entityCollection2.Entities.Count > 0)
            {
                var attendeesCollection = (from entity in entityCollection2.Entities
                                           where !entityCollection1.Entities.Any(x => ((EntityReference)x.Attributes["partyid"]).Id == (((EntityReference)entity.Attributes["partyid"]).Id))
                                           select entity).ToList();
                EntityCollection entityCollection = new EntityCollection(attendeesCollection);
                return entityCollection;
            }
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="relationshipAttendeeEntityCollection"></param>
        /// <param name="appointment"></param>
        /// <param name="appointmentAttendees"></param>
        /// <param name="service"></param>
        public void RequiredAttendeesLogic(EntityCollection relationshipAttendeeEntityCollection, Entity appointment, AppointmentAttendees appointmentAttendees, IOrganizationService service,ITracingService tracingService)
        {
            #region Check conditions for Required Attendees

            //condition checks that records are added to the required attendee field on apponitment entity and therfore we need to create records in RA entity   
            if ((this.PostImageRequiredAttendeesCount > this.PreImageRequiredAttendeesCount)
                && (this.PostImageRequiredAttendeesCount != 0 || this.PreImageRequiredAttendeesCount != 0)
                && relationshipAttendeeEntityCollection != null && relationshipAttendeeEntityCollection.Entities.Count > 0
                && appointment.Attributes.Contains("requiredattendees") && appointment.Attributes["requiredattendees"] != null)
            {
                tracingService.Trace("RequiredAttendeesLogic Condition 1:");
                //get records which needs to be added to Relationship Attendee entity
                //it gets the record that are newly added to required attendee field
                EntityCollection newlyAddedRequiredAttendees = GetDistinctAttendees(this.PreImageRequiredAttendees, this.PostImageRequiredAttendees);
                tracingService.Trace("RequiredAttendeesLogic Condition:"+ newlyAddedRequiredAttendees.Entities.Count);
                if (newlyAddedRequiredAttendees != null && newlyAddedRequiredAttendees.Entities.Count > 0)
                {
                    CreateRelationshipAteendeeRecords(newlyAddedRequiredAttendees, appointment, appointmentAttendees, service,tracingService);
                }
            }
            //condition checks that records are removed the required attendee field on apponitment entity and therfore we need to remove records in RA entity
            else if ((this.PostImageRequiredAttendeesCount < this.PreImageRequiredAttendeesCount) 
                && (this.PostImageRequiredAttendeesCount != 0 || this.PreImageRequiredAttendeesCount != 0)
                && relationshipAttendeeEntityCollection != null && relationshipAttendeeEntityCollection.Entities.Count > 0
                && appointment.Attributes.Contains("requiredattendees") && appointment.Attributes["requiredattendees"] != null)
            {
                tracingService.Trace("RequiredAttendeesLogic Condition 2:");
                //get records which needs to be removed to Relationship Attendee entity
                EntityCollection newlyRemovedRequiredAttendees = GetDistinctAttendees(this.PostImageRequiredAttendees, this.PreImageRequiredAttendees);
                tracingService.Trace("RequiredAttendeesLogic Condition:" + newlyRemovedRequiredAttendees.Entities.Count);
                if (newlyRemovedRequiredAttendees != null && newlyRemovedRequiredAttendees.Entities.Count > 0)
                {
                    DeleteRelationshipAttendeeRecords(newlyRemovedRequiredAttendees, appointment, appointmentAttendees, service,tracingService);
                }
            }
            //condition checks that records are added or removed the required attendee field on apponitment entity and therfore we need to remove records in RA entity
            else if ((this.PostImageRequiredAttendeesCount == this.PreImageRequiredAttendeesCount) 
                && (this.PostImageRequiredAttendeesCount != 0 || this.PreImageRequiredAttendeesCount != 0)
                && relationshipAttendeeEntityCollection != null && relationshipAttendeeEntityCollection.Entities.Count > 0
                && appointment.Attributes.Contains("requiredattendees") && appointment.Attributes["requiredattendees"] != null)
            {
                tracingService.Trace("RequiredAttendeesLogic Condition 3:");
                //get records which needs to be added to Relationship Attendee entity
                EntityCollection newlyAddedRequiredAttendees = GetDistinctAttendees(this.PreImageRequiredAttendees, this.PostImageRequiredAttendees);
                tracingService.Trace("RequiredAttendeesLogic Condition:" + newlyAddedRequiredAttendees.Entities.Count);
                if (newlyAddedRequiredAttendees != null && newlyAddedRequiredAttendees.Entities.Count > 0)
                {
                    CreateRelationshipAteendeeRecords(newlyAddedRequiredAttendees, appointment, appointmentAttendees, service,tracingService);
                }
                //get records which needs to be removed to Relationship Attendee entity
                EntityCollection newlyRemovedRequiredAttendees = GetDistinctAttendees(this.PostImageRequiredAttendees, this.PreImageRequiredAttendees);
                if (newlyRemovedRequiredAttendees != null && newlyRemovedRequiredAttendees.Entities.Count > 0)
                {
                    DeleteRelationshipAttendeeRecords(newlyRemovedRequiredAttendees, appointment, appointmentAttendees, service,tracingService);
                }
            }

            #endregion Check conditions for Required Attendees
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="relationshipAttendeeEntityCollection"></param>
        /// <param name="appointment"></param>
        /// <param name="appointmentAttendees"></param>
        /// <param name="service"></param>
        public void OptionalAttendeesLogic(EntityCollection relationshipAttendeeEntityCollection, Entity appointment, AppointmentAttendees appointmentAttendees, IOrganizationService service,ITracingService tracingService)
        {

            #region Check conditions for Optional Attendees
            tracingService.Trace("OptionalAttendeesLogic function called");
            //condition checks that records are added to the required attendee field on apponitment entity and therfore we need to create records in RA entity   
            if ((this.PostImageOptionalAttendeesCount > this.PreImageOptionalAttendeesCount)
                && (this.PostImageOptionalAttendeesCount != 0 || this.PreImageOptionalAttendeesCount != 0)
                && relationshipAttendeeEntityCollection != null && relationshipAttendeeEntityCollection.Entities.Count > 0
                && appointment.Attributes.Contains("optionalattendees") && appointment.Attributes["optionalattendees"] != null)
            {
                tracingService.Trace("OptionalAttendeesLogic function condition 1");
                //get records which needs to be added to Relationship Attendee entity
                //it gets the record that are newly added to required attendee field
                EntityCollection newlyAddedOptionalAttendees = GetDistinctAttendees(this.PreImageOptionalAttendees, this.PostImageOptionalAttendees);
                tracingService.Trace("OptionalAttendeesLogic function condition 1:newlyAddedOptionalAttendees"+ newlyAddedOptionalAttendees.Entities.Count);
                if (newlyAddedOptionalAttendees != null && newlyAddedOptionalAttendees.Entities.Count > 0)
                {
                    CreateRelationshipAteendeeRecords(newlyAddedOptionalAttendees, appointment, appointmentAttendees, service,tracingService);
                }
            }
            //condition checks that records are removed the required attendee field on apponitment entity and therfore we need to remove records in RA entity
            else if ((this.PostImageOptionalAttendeesCount < this.PreImageOptionalAttendeesCount)
                && (this.PostImageOptionalAttendeesCount != 0 || this.PreImageOptionalAttendeesCount != 0)
                && relationshipAttendeeEntityCollection != null && relationshipAttendeeEntityCollection.Entities.Count > 0
                && appointment.Attributes.Contains("optionalattendees") && appointment.Attributes["optionalattendees"] != null)
            {
                //get records which needs to be removed to Relationship Attendee entity
                EntityCollection newlyRemovedOptionalAttendees = GetDistinctAttendees(this.PostImageOptionalAttendees, this.PreImageOptionalAttendees);
                tracingService.Trace("OptionalAttendeesLogic function condition 2:newlyAddedOptionalAttendees" + newlyRemovedOptionalAttendees.Entities.Count);
                if (newlyRemovedOptionalAttendees != null && newlyRemovedOptionalAttendees.Entities.Count > 0)
                {
                    DeleteRelationshipAttendeeRecords(newlyRemovedOptionalAttendees, appointment, appointmentAttendees, service,tracingService);
                }
            }
            //condition checks that records are added or removed the required attendee field on apponitment entity and therfore we need to remove records in RA entity
            else if ((this.PostImageOptionalAttendeesCount == this.PreImageOptionalAttendeesCount)
                && (this.PostImageOptionalAttendeesCount != 0 || this.PreImageOptionalAttendeesCount != 0)
                && relationshipAttendeeEntityCollection != null && relationshipAttendeeEntityCollection.Entities.Count > 0
                && appointment.Attributes.Contains("optionalattendees") && appointment.Attributes["optionalattendees"] != null)
            {
                
                //get records which needs to be added to Relationship Attendee entity
                EntityCollection newlyAddedOptionalAttendees = GetDistinctAttendees(this.PreImageOptionalAttendees, this.PostImageOptionalAttendees);
                tracingService.Trace("OptionalAttendeesLogic function condition 3:newlyAddedOptionalAttendees" + newlyAddedOptionalAttendees.Entities.Count);
                if (newlyAddedOptionalAttendees != null && newlyAddedOptionalAttendees.Entities.Count > 0)
                {
                    CreateRelationshipAteendeeRecords(newlyAddedOptionalAttendees, appointment, appointmentAttendees, service,tracingService);
                }
                //get records which needs to be removed to Relationship Attendee entity
                EntityCollection newlyRemovedOptionalAttendees = GetDistinctAttendees(this.PostImageOptionalAttendees, this.PreImageOptionalAttendees);
                tracingService.Trace("OptionalAttendeesLogic function condition 3:newlyAddedOptionalAttendees" + newlyRemovedOptionalAttendees.Entities.Count);
                if (newlyRemovedOptionalAttendees != null && newlyRemovedOptionalAttendees.Entities.Count > 0)
                {
                    DeleteRelationshipAttendeeRecords(newlyRemovedOptionalAttendees, appointment, appointmentAttendees, service,tracingService);
                }
            }

            #endregion Check conditions for Required Attendees
        }
      
    }
}
