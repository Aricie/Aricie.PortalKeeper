using System.Collections.Generic;
using System.Xml.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace RedditSharp
{
    [Serializable()]
    public abstract class Thing
    {




        public static Thing Parse(Reddit reddit, JToken json, IWebAgent webAgent)
        {
            var kind = json["kind"].ValueOrDefault<string>();
            switch (kind)
            {
                case "t1":
                    return new Comment(reddit, json, webAgent, null);
                case "t2":
                    return new RedditUser(reddit, json, webAgent);
                case "t3":
                    return new Post(reddit, json, webAgent);
                case "t4":
                    return new PrivateMessage(reddit, json, webAgent);
                case "t5":
                    return new Subreddit(reddit, json, webAgent);
                default:
                    return null;
            }
        }

        // if we can't determine the type of thing by "kind", try by type
        public static Thing Parse<T>(Reddit reddit, JToken json, IWebAgent webAgent) where T : Thing
        {
            Thing result = Parse(reddit, json, webAgent);
            if (result == null)
            {
                if (typeof (T) == typeof (WikiPageRevision))
                {
                    return new WikiPageRevision(reddit, json, webAgent);
                }
            }
            return result;
        }

        public Thing()
        {

        }

        internal Thing(JToken json)
        {
            if (json == null)
                return;
            var data = json["name"] == null ? json["data"] : json;
            FullName = data["name"].ValueOrDefault<string>();
            Id = data["id"].ValueOrDefault<string>();
            Kind = json["kind"].ValueOrDefault<string>();
            FetchedAt = DateTime.Now;
        }

        public virtual string Shortlink
        {
            get { return "http://redd.it/" + Id; }
        }


        [XmlIgnore()]
        [JsonIgnore]
        public Reddit Reddit { get; set; }

        public string Id { get; set; }
        public string FullName { get; set; }
        public string Kind { get; set; }

        [JsonProperty("author")]
        public string AuthorName { get; set; }

        [JsonIgnore]
        public RedditUser Author
        {
            get { return Reddit.GetUser(AuthorName); }
        }

        [JsonIgnore]
        public abstract Thing ParentThing { get; }

        [JsonIgnore]
        public abstract IEnumerable<Thing> Children { get; }


        /// <summary>
        /// The time at which this object was fetched from reddit servers.
        /// </summary>
        public DateTime FetchedAt { get;  set; }

        /// <summary>
        /// Gets the time since last fetch from reddit servers.
        /// </summary>
        public TimeSpan TimeSinceFetch
        {
            get
            {
                return DateTime.Now - FetchedAt;
            }
        }
    }
}
