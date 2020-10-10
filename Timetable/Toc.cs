using System;

namespace Timetable
{
    public class Toc : IEquatable<string>, IEquatable<Toc>
    {
        public static readonly Toc Unknown = new Toc("??")
        {
            Name = "Unknown"
        };

        public string Code { get; }

        public string Name { get; set; }

        public Toc(string code, string name = "")
        {
            Code = code;
            Name = name;
        }
        
        public bool Equals(string other)
        {
            return Code.Equals(other);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Toc) obj);
        }

        public bool Equals(Toc other)
        {
            return Code == other.Code;
        }

        public override int GetHashCode()
        {
            return (Code != null ? Code.GetHashCode() : 0);
        }

        public override string ToString()
        {
            return Code;
        }
    }
}