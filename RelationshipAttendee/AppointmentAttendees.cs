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
    public class AppointmentAttendees
    {
        public AppointmentAttendees()
        {
            this.Contacts = new List<Contact>();
            this.Users = new List<User>();
        }
        public List<Contact> Contacts { get; set; }
        public List<User> Users { get; set; }

        public void AddToList(Contact contact)
        {
            this.Contacts.Add(contact);
        }

        public void AddToList(User user)
        {
            this.Users.Add(user);
        }
        
        public void AddToList(EntityCollection relationshipAttendeeEntityCollection,ITracingService tracingService)
        {
            foreach (var attendee in relationshipAttendeeEntityCollection.Entities)
            {
                if (attendee != null && attendee.Attributes.Contains(Constants.RelationshipAttendee.ContactFieldName) && attendee.Attributes[Constants.RelationshipAttendee.ContactFieldName] != null)
                {
                    EntityReference entityReference = (EntityReference)attendee.Attributes[Constants.RelationshipAttendee.ContactFieldName];
                    int contactCount = this.Contacts.Where(item => item.Id == entityReference.Id).Count();
                    tracingService.Trace("Contact Count:" + contactCount);
                    if (contactCount == 0)
                    {
                        tracingService.Trace("Contact Added:" + entityReference.Id.ToString() + " " + entityReference.Name);
                        this.AddToList(new Contact { Id = entityReference.Id, Name = entityReference.Name,RelationshipAttendeeId = attendee.Id });
                    }
                }
                else if (attendee != null && attendee.Attributes.Contains(Constants.RelationshipAttendee.UserFieldName) && attendee.Attributes[Constants.RelationshipAttendee.UserFieldName] != null)
                {
                    EntityReference entityReference = (EntityReference)attendee.Attributes[Constants.RelationshipAttendee.UserFieldName];

                    int usersCount = this.Users.Where(item => item.Id == entityReference.Id).Count();
                    tracingService.Trace("User Count:" + usersCount);
                    if (usersCount == 0)
                    {
                        tracingService.Trace("User Added:" + entityReference.Id.ToString() + " " + entityReference.Name);
                        this.AddToList(new User { Id = entityReference.Id, Name = entityReference.Name, RelationshipAttendeeId = attendee.Id });
                    }
                }
            }
        }
        
    }
}
