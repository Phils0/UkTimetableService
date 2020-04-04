using System.Collections.Generic;

namespace Timetable
{
    public class Activities
    {
        public const string SetDownOnlyStop = "D";
        public const string DetachVehicles = "-D";
        public const string StopNotAdvertised = "N";
        public const string RequestStop = "R";
        public const string PassengerStop = "T";
        public const string TrainBegins = "TB";
        public const string TrainFinishes = "TF";
        public const string TrainSplitsOrJoins = "-T";
        public const string PickUpOnlyStop = "U";
        public const string AttachVehicles = "-U";
        
        public string Value { get; }
        
        public PublicStop AdvertisedStop { get; private set; } = PublicStop.NotSet;
        public Activities(string activities)
        {
            Value = activities;
            UpdateAdvertisedStop();
        }
        
        private void UpdateAdvertisedStop()
        {
            if (string.IsNullOrEmpty(Value))
            {
                AdvertisedStop = PublicStop.No;
                return;
            }

            var activities = Split(Value);
            
            if (activities.Contains(StopNotAdvertised))
            {
                AdvertisedStop = PublicStop.No;
                return;
            }
            
            if (activities.Contains(TrainBegins) ||
                activities.Contains(PickUpOnlyStop))
            {
                AdvertisedStop = PublicStop.PickUpOnly;                           
            }
            else if (activities.Contains(TrainFinishes) ||
                     activities.Contains(SetDownOnlyStop))
            {
                AdvertisedStop = PublicStop.SetDownOnly;                           
            }
            else if (activities.Contains(RequestStop))
            {
                AdvertisedStop = PublicStop.Request;                           
            }
            else if (activities.Contains(PassengerStop))
            {
                AdvertisedStop = PublicStop.Yes;
            }
            else
            {
                AdvertisedStop = PublicStop.No;
            }
        }
        
        internal static ISet<string> Split(string s)
        {
            var activities = new HashSet<string>();
            if (string.IsNullOrEmpty(s))
                return activities;

            var chars = s.ToCharArray();
            
            for(int i = 0; i < chars.Length; i+=2)
            {
                string a;
                
                if (IsLastChar(i, chars) || IsSingleCharActivity(chars, i))
                    a = new string(chars, i, 1);                    
                else
                    a = new string(chars, i, 2);
                
                activities.Add(a);
            }

            return activities;
        }

        private static bool IsLastChar(int i, char[] chars)
        {
            return i == chars.Length - 1;
        }
        
        private static bool IsSingleCharActivity(char[] chars, int i)
        {
            return chars[i+1] == ' ';
        }
    }
}