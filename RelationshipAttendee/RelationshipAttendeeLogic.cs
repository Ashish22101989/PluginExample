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
using System.ServiceModel;

namespace RelationshipAttendee
{
    public class RelationshipAttendeeLogic : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {

            // Obtain the tracing service
            ITracingService tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            // Obtain the execution context from the service provider.  
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));

            // The InputParameters collection contains all the data passed in the message request. 
            try
            {
                if (context.MessageName.ToUpper() == "CREATE")
                {
                    tracingService.Trace("Create Condition");
                    OnCreate(serviceProvider, context);
                }
                else if (context.MessageName.ToUpper() == "UPDATE")
                {
                    tracingService.Trace("Update Condition");
                    OnUpdate(serviceProvider, context);
                }
            }
            catch (FaultException<OrganizationServiceFault> ex)
            {
                throw new InvalidPluginExecutionException("An error occurred in Plugin.", ex);
            }

            catch (Exception ex)
            {
                tracingService.Trace("FollowUpPlugin: {0}", ex.ToString());
                throw;
            }

        }

        /// <summary>
        /// On Create Plugin
        /// </summary>
        /// <param name="serviceProvider"></param>
        public void OnCreate(IServiceProvider serviceProvider, IPluginExecutionContext context)
        {
            // Obtain the execution context from the service provider.
            //IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            // Obtain the organization service reference.
            IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);
            ITracingService tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            if (tracingService == null)
            {
                throw new InvalidPluginExecutionException("Failed to retrieve the tracing service.");
            }

            try
            {
                if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                {
                    Entity appointment = (Entity)context.InputParameters["Target"];
                    OnCreate(appointment, service,tracingService);
                }
            }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException(ex.ToString());
            }
        }

        /// <summary>
        /// On Create Logic
        /// </summary>
        /// <param name="appointment"></param>
        /// <param name="service"></param>
        public void OnCreate(Entity appointment, IOrganizationService service,ITracingService tracingService)
        {
            try
            {
                if (appointment != null)
                {
                    Guid appointmentId = appointment.Id;
                    RelationshipAttendee relationshipAttendee = new RelationshipAttendee();
                    if (appointment.Contains("requiredattendees") && appointment.Attributes["requiredattendees"] != null)
                    {
                        EntityCollection requiredAttendees = (EntityCollection)appointment.Attributes["requiredattendees"];
                        relationshipAttendee.Create(requiredAttendees, appointmentId, service,tracingService);
                        if (appointment.Contains("optionalattendees") && appointment.Attributes["optionalattendees"] != null)
                        {
                            EntityCollection optionalAttendees = (EntityCollection)appointment.Attributes["optionalattendees"];
                            if (optionalAttendees != null && optionalAttendees.Entities.Count > 0)
                            {
                                //Remove duplicates from Optional Attendee entity collection
                                var optionalAttendeeCollection = (from entity in optionalAttendees.Entities
                                                                  where !requiredAttendees.Entities.Any(x => ((EntityReference)x.Attributes["partyid"]).Id == (((EntityReference)entity.Attributes["partyid"]).Id))
                                                                  select entity).ToList();

                                optionalAttendees = new EntityCollection(optionalAttendeeCollection);
                                relationshipAttendee.Create(optionalAttendees, appointmentId, service,tracingService);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// On Update Plugin
        /// </summary>
        /// <param name="serviceProvider"></param>
        public void OnUpdate(IServiceProvider serviceProvider, IPluginExecutionContext context)
        {
            
            // Obtain the execution context from the service provider.
            //IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            // Obtain the organization service reference.
            IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);
            ITracingService tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            tracingService.Trace("On Update Called 1");
            if (tracingService == null)
            {
                throw new InvalidPluginExecutionException("Failed to retrieve the tracing service.");
            }

            try
            {
                if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                {
                    tracingService.Trace("On Update Called");
                    Entity appointment = (Entity)context.InputParameters["Target"];
                    Entity preAttendeesImage = null;
                    Entity postAttendeesImage = null;
                    if (context.PreEntityImages.Contains("AttendeesImage") && context.PreEntityImages["AttendeesImage"] is Entity)
                    {
                        preAttendeesImage = (Entity)context.PreEntityImages["AttendeesImage"];
                        tracingService.Trace("preAttendeesImage");
                    }
                    if (context.PostEntityImages.Contains("AttendeesImage") && context.PostEntityImages["AttendeesImage"] is Entity)
                    {
                        postAttendeesImage = (Entity)context.PostEntityImages["AttendeesImage"];
                        tracingService.Trace("postAttendeesImage");
                    }
                    tracingService.Trace("Appointment ID:"+ appointment.Id.ToString());
                    OnUpdate(appointment, preAttendeesImage, postAttendeesImage, service, tracingService);
                }
            }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException(ex.ToString());
            }
        }

        /// <summary>
        /// On Update Logic
        /// </summary>
        /// <param name="appointment"></param>
        /// <param name="preImage"></param>
        /// <param name="postImage"></param>
        /// <param name="service"></param>
        public void OnUpdate(Entity appointment, Entity preImage, Entity postImage, IOrganizationService service,ITracingService tracingService)
        {
            try
            {
                if (appointment != null)
                {
                    tracingService.Trace("Appointment ID:" + appointment.Id.ToString());
                    Guid appointmentId = appointment.Id;
                    RelationshipAttendee relationshipAttendee = new RelationshipAttendee();
                    Attendee attendee = new Attendee(preImage, postImage);
                    //get records from relationship attendee entity associated to appointment
                    EntityCollection relationshipAttendeeEntityCollection = relationshipAttendee.GetRecords(appointmentId, service,tracingService);

                    tracingService.Trace("relationshipAttendeeEntityCollection:" + relationshipAttendeeEntityCollection.Entities.Count);
                    //Add records retrieved from relationship attendee entity to AppointmentAttendees object
                    AppointmentAttendees appointmentAttendees = new AppointmentAttendees();
                    appointmentAttendees.AddToList(relationshipAttendeeEntityCollection,tracingService);
                    //add or remove required attendees from relationship attendee record associated to appointment
                    attendee.RequiredAttendeesLogic(relationshipAttendeeEntityCollection, appointment, appointmentAttendees, service,tracingService);
                    //add or remove optional attendees from relationship attendee record associated to appointment
                    attendee.OptionalAttendeesLogic(relationshipAttendeeEntityCollection, appointment, appointmentAttendees, service,tracingService);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
