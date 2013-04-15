// -----------------------------------------------------------------------
// <copyright file="DateTimeUTC.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace SMS.ServiceStack.Types
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public struct DateTimeUTC
    {
        private readonly DateTime dateTime;

        public DateTimeUTC(DateTime dateTime)
        {
            this.dateTime = dateTime.Kind == DateTimeKind.Utc ? dateTime : new DateTime(dateTime.Ticks, DateTimeKind.Utc);
        }

        public static DateTimeUTC Now
        {
            get
            {
                return new DateTimeUTC(DateTime.UtcNow);
            }
        }

        public static bool operator <(DateTimeUTC left, DateTimeUTC right)
        {
            return left.dateTime < right.dateTime;
        }

        public static bool operator >(DateTimeUTC left, DateTimeUTC right)
        {
            return left.dateTime > right.dateTime;
        }

        public static bool operator <=(DateTimeUTC left, DateTimeUTC right)
        {
            return left.dateTime <= right.dateTime;
        }

        public static bool operator >=(DateTimeUTC left, DateTimeUTC right)
        {
            return left.dateTime >= right.dateTime;
        }

        public static bool operator ==(DateTimeUTC left, DateTimeUTC right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(DateTimeUTC left, DateTimeUTC right)
        {
            return !left.Equals(right);
        }

        public static DateTimeUTC FromLocalTime(DateTime dateTime)
        {
            return new DateTimeUTC(new DateTime(dateTime.Ticks, DateTimeKind.Local).ToUniversalTime());
        }

        public static DateTimeUTC FromUTCTime(DateTime dateTime)
        {
            return new DateTimeUTC(dateTime);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return this.dateTime.GetHashCode();
            }
        }

        public bool Equals(DateTimeUTC other)
        {
            return other.dateTime.Equals(this.dateTime);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (obj.GetType() != typeof(DateTimeUTC))
            {
                return false;
            }

            return this.Equals((DateTimeUTC)obj);
        }

        public DateTime ToDateTime()
        {
            return this.dateTime;
        }

        public new string ToString()
        {
            return this.dateTime.ToString();
        }

        public string ToString(string format)
        {
            return this.dateTime.ToString(format);
        }
    }
}
