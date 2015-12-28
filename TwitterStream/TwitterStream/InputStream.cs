using Microsoft.ComplexEventProcessing;
using Microsoft.ComplexEventProcessing.Adapters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tweetinvi;
using Tweetinvi.Core.Enum;

namespace TwitterStream
{
    public class Input : TypedPointInputAdapter<TweetsType>
    {
        public Input(InputConfig config)
        {
        }

        public override void Resume()
        {
            ProduceEvent();
        }

        public override void Start()
        {
            ProduceEvent();
        }

        public void ProduceEvent()
        {

            PointEvent<TweetsType> currEvent = default(PointEvent<TweetsType>);
            EnqueueOperationResult result = EnqueueOperationResult.Full;
            Random rand = new Random();

            while (true)
            {

                if (AdapterState.Stopping == AdapterState)
                {
                    Stopped();
                    return;
                }

                Auth.SetUserCredentials("JpNPcJ09Uk3Dzn7Ss9s1hxA02", "A2jLu90Y6yOK7sGygGE2UEZANaiL1XMiHYDCUSs3rsgz6nahKQ", "140868440-Y5Fe0jryPMTbXWXW6QlOt1cx23dSy8AuysGjyT0o", "vAJ60rYYe71Ee054hU4kduLJJ9qiK0ZqJ1HW3mbpdmLT6");

                var stream = Stream.CreateFilteredStream();
                stream.AddTrack("flu");
                stream.AddTrack("cancer");

                stream.MatchingTweetAndLocationReceived += (sender, args) =>
                {
                    

                    currEvent = CreateInsertEvent();
                    currEvent.StartTime = DateTime.Now;

                    //Setting the payLoad for Event
                    TweetsType obj = new TweetsType();
                    obj.Text = args.Tweet.Text;
                    if (obj.Text == null) obj.Text = "";
                    if (args.Tweet.Coordinates != null)
                    {
                        obj.Latitude = args.Tweet.Coordinates.Latitude;
                        obj.Longitude = args.Tweet.Coordinates.Longitude;
                    }
                    else
                    {
                        obj.Latitude = 0;
                        obj.Longitude = 0;
                    }
                    obj.CreatedAt = args.Tweet.CreatedAt;                    
                    obj.UserName = args.Tweet.CreatedBy.Name;
                    if (obj.UserName == null) obj.UserName = "";
                    //obj.Country = args.Tweet.Place.Country;                    
                    obj.Location = args.Tweet.CreatedBy.Location;                   
                    if (obj.Location == null) obj.Location = "";
                    obj.Lang = args.Tweet.Language.ToString();
                    if (obj.Lang == null) obj.Lang = "";
                    obj.TweetID = args.Tweet.Id;
                    obj.FollowersCount = args.Tweet.CreatedBy.FollowersCount;                    
                    obj.FriendsCount = args.Tweet.CreatedBy.FriendsCount;                    
                    obj.HashTag = args.MatchingTracks.FirstOrDefault();
                    if (obj.HashTag == null) obj.HashTag = "";
                    obj.TimeZone = args.Tweet.CreatedBy.TimeZone;
                    if (obj.TimeZone == null) obj.TimeZone = "";
                    
                    //obj.Init();
                    //Thread.Sleep(1);
                    currEvent.Payload = obj;

                    //Enqueue the Event
                    result = Enqueue(ref currEvent);

                    if (EnqueueOperationResult.Full == result)
                    {
                        ReleaseEvent(ref currEvent);
                        Ready();
                        return;
                    }

                    // Insert CTI Event into stream
                    EnqueueCtiEvent(DateTime.Now);                    
                    
                };
                stream.StartStreamMatchingAllConditions();

            }
        
        }
    }
}
