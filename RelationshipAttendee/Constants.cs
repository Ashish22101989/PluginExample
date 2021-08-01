using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelationshipAttendee
{
    public static class Constants
    {
        public static string RelationshipAttendeeSchemaName = "new_relationshipattendees";
        public static string ContactSchemaName = "contact";
        public static string UserSchemaName = "systemuser";
        public static string AccountSchemaName = "account";
        public static string AppointmentSchemaName = "appointment";

        public static class RelationshipAttendee
        {
            public static string AppointmentFieldName = "new_attendeesid";
            public static string ContactFieldName = "new_contact";
            public static string UserFieldName = "new_user";
            public static string NameField = "new_name";
            public static string AttendanceStatusField = "new_attendeestatus";
            //new_attendeesid

        }
    }
}
