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
    public class Contact
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string EntityName { get { return "contact"; } }
        public string EmailAddress { get; set; }
        public Guid RelationshipAttendeeId { get; set; }

    }
}
