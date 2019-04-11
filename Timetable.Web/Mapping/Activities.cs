using System.Collections.Generic;

namespace Timetable.Web.Mapping
{
    internal static class Activities
    {
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