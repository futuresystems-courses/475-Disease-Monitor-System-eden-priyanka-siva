using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitterStream
{
    public class TweetsType
    {
        public DateTime CreatedAt;
        public long TweetID;
        public string Text;
        public string UserId;
        public string UserName;
        public string Location;
        public int FollowersCount;
        public int FriendsCount;
        public string Lang;
        public double Longitude;
        public double Latitude;
        public string Country;
        public string HashTag;
        public string TimeZone;
    }
}

