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
        private DateTime dateTime;

        DateTimeUTC(DateTime dateTime)
        {
            this.dateTime = dateTime.Kind == DateTimeKind.Utc ? dateTime : new DateTime(dateTime.Ticks, DateTimeKind.Utc);
        }

        public bool Equals(DateTimeUTC other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }
            if (ReferenceEquals(this, other))
            {
                return true;
            }
            return other.dateTime.Equals(this.dateTime);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            if (obj.GetType() != typeof(DateTimeUTC))
            {
                return false;
            }
            return this.Equals((DateTimeUTC)obj);
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
            return Equals(left, right);
        }

        public static bool operator !=(DateTimeUTC left, DateTimeUTC right)
        {
            return !Equals(left, right);
        }

        public DateTime ToDateTime()
        {
            return dateTime;
        }

        public string ToString()
        {
            return dateTime.ToString();
        }

        public string ToString(string format)
        {
            return dateTime.ToString(format);
        }

        public static DateTimeUTC Now
        {
            get
            {
                return new DateTimeUTC(DateTime.UtcNow);
            }
        }

        public static DateTimeUTC FromLocalTime(DateTime dateTime)
        {
            return new DateTimeUTC(dateTime.ToUniversalTime());
        }

        public static DateTimeUTC FromUTCTime(DateTime dateTime)
        {
            return new DateTimeUTC(dateTime);
        }
    }
}
