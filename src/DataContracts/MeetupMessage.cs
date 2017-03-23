using System;
using System.Runtime.Serialization;

namespace DataContracts
{
    [DataContract]
    public class MeetupMessage
    {
        [DataMember(Name = "rsvp_id")]
        public long EventId { get; set; }
        [DataMember(Name = "mtime")]
        public long UnixTimestamp { get; set; }

        [DataMember(Name = "group")]
        public Group Group { get; set; }

        public DateTime Timestamp { get { return UnixTimeStampToDateTime(UnixTimestamp); } }


        public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }
    }

    [DataContract]
    public class Group
    {
        [DataMember(Name = "group_city")]
        public string City { get; set; }
        [DataMember(Name = "group_country")]
        public string Country { get; set; }
    }
}
