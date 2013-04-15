using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMS.ServiceStack.Tests
{
    using NUnit.Framework;

    using SMS.ServiceStack.Types;

    public class DateTimeUTCTests
    {
        [Test]
        public void GivenConstructorWithUnspecifedTime_ShouldConvertToUTC()
        {
            var testDate = new DateTime(2013, 4, 27, 0, 0, 0, DateTimeKind.Unspecified);
            var utcDate = new DateTimeUTC(testDate);

            DateIsTheSame(testDate, utcDate.ToDateTime());
            Assert.AreEqual(DateTimeKind.Utc, utcDate.ToDateTime().Kind);
        }

        [Test]
        public void GivenConstructorWithUTCTime_ShouldConvertToUTC()
        {
            var testDate = new DateTime(2013, 4, 27, 0, 0, 0, DateTimeKind.Utc);
            var utcDate = new DateTimeUTC(testDate);

            DateIsTheSame(testDate, utcDate.ToDateTime());
            Assert.AreEqual(DateTimeKind.Utc, utcDate.ToDateTime().Kind);
        }

        [Test]
        public void GivenConstructorWithLocalTime_ShouldConvertToUTC()
        {
            var testDate = new DateTime(2013, 4, 27, 0, 0, 0, DateTimeKind.Local);
            var utcDate = new DateTimeUTC(testDate);

            DateIsTheSame(testDate, utcDate.ToDateTime());
            Assert.AreEqual(DateTimeKind.Utc, utcDate.ToDateTime().Kind);
        }

        [Test]
        public void GivenNow_ShouldReturnUTCNow()
        {
            var testDate = DateTime.UtcNow;
            var utcDate = DateTimeUTC.Now;

            DateIsTheSameDate(testDate, utcDate.ToDateTime());
            Assert.AreEqual(testDate.Hour, utcDate.ToDateTime().Hour);
            Assert.AreEqual(testDate.Minute, utcDate.ToDateTime().Minute);
            Assert.AreEqual(DateTimeKind.Utc, utcDate.ToDateTime().Kind);
        }

        [Test]
        public void GivenFromUTCTimeWithUnspecifedTime_ShouldConvertToUTC()
        {
            var testDate = new DateTime(2013, 4, 27, 0, 0, 0, DateTimeKind.Unspecified);
            var utcDate = DateTimeUTC.FromUTCTime(testDate);

            DateIsTheSame(testDate, utcDate.ToDateTime());
            Assert.AreEqual(DateTimeKind.Utc, utcDate.ToDateTime().Kind);
        }

        [Test]
        public void GivenFromUTCTimeWithUTCTime_ShouldConvertToUTC()
        {
            var testDate = new DateTime(2013, 4, 27, 0, 0, 0, DateTimeKind.Utc);
            var utcDate = DateTimeUTC.FromUTCTime(testDate);

            DateIsTheSame(testDate, utcDate.ToDateTime());
            Assert.AreEqual(DateTimeKind.Utc, utcDate.ToDateTime().Kind);
        }

        [Test]
        public void GivenFromUTCTimeWithLocalTime_ShouldConvertToUTC()
        {
            var testDate = new DateTime(2013, 4, 27, 0, 0, 0, DateTimeKind.Local);
            var utcDate = DateTimeUTC.FromUTCTime(testDate);

            DateIsTheSame(testDate, utcDate.ToDateTime());
        }

        [Test]
        public void GivenFromLocalTimeWithUnspecifedTime_ShouldConvertToUTC()
        {
            var testDate = new DateTime(2013, 4, 27, 12, 0, 0, DateTimeKind.Unspecified);
            var utcDate = DateTimeUTC.FromLocalTime(testDate);

            DateIsTheSameDate(testDate.AddHours(-1).Subtract(TimeZoneInfo.Local.BaseUtcOffset), utcDate.ToDateTime());
            Assert.AreEqual(testDate.AddHours(-1).Subtract(TimeZoneInfo.Local.BaseUtcOffset).Hour, utcDate.ToDateTime().Hour);
            Assert.AreEqual(testDate.AddHours(-1).Subtract(TimeZoneInfo.Local.BaseUtcOffset).Minute, utcDate.ToDateTime().Minute);
        }

        [Test]
        public void GivenFromLocalTimeWithUTCTime_ShouldConvertToUTC()
        {
            var testDate = new DateTime(2013, 4, 27, 12, 0, 0, DateTimeKind.Utc);
            var utcDate = DateTimeUTC.FromLocalTime(testDate);

            DateIsTheSame(testDate.AddHours(-1).Subtract(TimeZoneInfo.Local.BaseUtcOffset), utcDate.ToDateTime());
            Assert.AreEqual(testDate.AddHours(-1).Subtract(TimeZoneInfo.Local.BaseUtcOffset).Hour, utcDate.ToDateTime().Hour);
            Assert.AreEqual(testDate.AddHours(-1).Subtract(TimeZoneInfo.Local.BaseUtcOffset).Minute, utcDate.ToDateTime().Minute);
        }

        [Test]
        public void GivenFromLocalTimeWithLocalTime_ShouldConvertToUTC()
        {
            var testDate = new DateTime(2013, 4, 27, 12, 0, 0, DateTimeKind.Local);
            var utcDate = DateTimeUTC.FromLocalTime(testDate);

            DateIsTheSame(testDate.AddHours(-1).Subtract(TimeZoneInfo.Local.BaseUtcOffset), utcDate.ToDateTime());
            Assert.AreEqual(testDate.AddHours(-1).Subtract(TimeZoneInfo.Local.BaseUtcOffset).Hour, utcDate.ToDateTime().Hour);
            Assert.AreEqual(testDate.AddHours(-1).Subtract(TimeZoneInfo.Local.BaseUtcOffset).Minute, utcDate.ToDateTime().Minute);
        }

        [Test]
        public void GivenGetHashCode_ShouldReturnTheSameAsDateTime()
        {
            var testDate = new DateTime(2013, 4, 27, 12, 0, 0, DateTimeKind.Utc);
            var utcDate = new DateTimeUTC(testDate);

            Assert.AreEqual(testDate.GetHashCode(), utcDate.GetHashCode());
        }

        [Test]
        public void GivenEqualsGetsSameValues_ShouldReturnTrue()
        {
            var first = new DateTimeUTC(new DateTime(2013, 4, 27, 12, 0, 0, DateTimeKind.Utc));
            var second = new DateTimeUTC(new DateTime(2013, 4, 27, 12, 0, 0, DateTimeKind.Utc));

            Assert.IsTrue(first.Equals(second));
        }

        [Test]
        public void GivenEqualsGetsDifferentValuesValues_ShouldReturnTrue()
        {
            var first = new DateTimeUTC(new DateTime(2013, 4, 27, 12, 0, 0, DateTimeKind.Utc));
            var second = new DateTimeUTC(new DateTime(2013, 4, 28, 12, 0, 0, DateTimeKind.Utc));

            Assert.IsFalse(first.Equals(second));
        }

        [Test]
        public void GivenEqualsOperatorGetsSameValues_ShouldBehaveAsDateTime()
        {
            var firstDateTime = new DateTime(2013, 4, 27, 12, 0, 0, DateTimeKind.Utc);
            var secondDateTime = new DateTime(2013, 4, 27, 12, 0, 0, DateTimeKind.Utc);
            var firstDateTimeUTC = new DateTimeUTC(firstDateTime);
            var secondDateTimeUTC = new DateTimeUTC(secondDateTime);

            Assert.AreEqual(firstDateTime == secondDateTime, firstDateTimeUTC == secondDateTimeUTC);
        }

        [Test]
        public void GivenEqualsOperatorGetsDifferentValues_ShouldBehaveAsDateTime()
        {
            var firstDateTime = new DateTime(2013, 4, 27, 12, 0, 0, DateTimeKind.Utc);
            var secondDateTime = new DateTime(2013, 4, 28, 12, 0, 0, DateTimeKind.Utc);
            var firstDateTimeUTC = new DateTimeUTC(firstDateTime);
            var secondDateTimeUTC = new DateTimeUTC(secondDateTime);

            Assert.AreEqual(firstDateTime == secondDateTime, firstDateTimeUTC == secondDateTimeUTC);
        }

        [Test]
        public void GivenNotEqualsOperatorGetsSameValues_ShouldBehaveAsDateTime()
        {
            var firstDateTime = new DateTime(2013, 4, 27, 12, 0, 0, DateTimeKind.Utc);
            var secondDateTime = new DateTime(2013, 4, 27, 12, 0, 0, DateTimeKind.Utc);
            var firstDateTimeUTC = new DateTimeUTC(firstDateTime);
            var secondDateTimeUTC = new DateTimeUTC(secondDateTime);

            Assert.AreEqual(firstDateTime != secondDateTime, firstDateTimeUTC != secondDateTimeUTC);
        }

        [Test]
        public void GivenNotEqualsOperatorGetsDifferentValues_ShouldBehaveAsDateTime()
        {
            var firstDateTime = new DateTime(2013, 4, 27, 12, 0, 0, DateTimeKind.Utc);
            var secondDateTime = new DateTime(2013, 4, 28, 12, 0, 0, DateTimeKind.Utc);
            var firstDateTimeUTC = new DateTimeUTC(firstDateTime);
            var secondDateTimeUTC = new DateTimeUTC(secondDateTime);

            Assert.AreEqual(firstDateTime != secondDateTime, firstDateTimeUTC != secondDateTimeUTC);
        }

        [Test]
        public void GivenGreaterOperatorGetsSameValues_ShouldBehaveAsDateTime()
        {
            var firstDateTime = new DateTime(2013, 4, 27, 12, 0, 0, DateTimeKind.Utc);
            var secondDateTime = new DateTime(2013, 4, 27, 12, 0, 0, DateTimeKind.Utc);
            var firstDateTimeUTC = new DateTimeUTC(firstDateTime);
            var secondDateTimeUTC = new DateTimeUTC(secondDateTime);

            Assert.AreEqual(firstDateTime > secondDateTime, firstDateTimeUTC > secondDateTimeUTC);
        }

        [Test]
        public void GivenGreaterOperatorGetsDifferentValues_ShouldBehaveAsDateTime()
        {
            var firstDateTime = new DateTime(2013, 4, 27, 12, 0, 0, DateTimeKind.Utc);
            var secondDateTime = new DateTime(2013, 4, 28, 12, 0, 0, DateTimeKind.Utc);
            var firstDateTimeUTC = new DateTimeUTC(firstDateTime);
            var secondDateTimeUTC = new DateTimeUTC(secondDateTime);

            Assert.AreEqual(firstDateTime > secondDateTime, firstDateTimeUTC > secondDateTimeUTC);
        }

        [Test]
        public void GivenSmallerOperatorGetsSameValues_ShouldBehaveAsDateTime()
        {
            var firstDateTime = new DateTime(2013, 4, 27, 12, 0, 0, DateTimeKind.Utc);
            var secondDateTime = new DateTime(2013, 4, 27, 12, 0, 0, DateTimeKind.Utc);
            var firstDateTimeUTC = new DateTimeUTC(firstDateTime);
            var secondDateTimeUTC = new DateTimeUTC(secondDateTime);

            Assert.AreEqual(firstDateTime < secondDateTime, firstDateTimeUTC < secondDateTimeUTC);
        }

        [Test]
        public void GivenSmallerOperatorGetsDifferentValues_ShouldBehaveAsDateTime()
        {
            var firstDateTime = new DateTime(2013, 4, 27, 12, 0, 0, DateTimeKind.Utc);
            var secondDateTime = new DateTime(2013, 4, 28, 12, 0, 0, DateTimeKind.Utc);
            var firstDateTimeUTC = new DateTimeUTC(firstDateTime);
            var secondDateTimeUTC = new DateTimeUTC(secondDateTime);

            Assert.AreEqual(firstDateTime < secondDateTime, firstDateTimeUTC < secondDateTimeUTC);
        }

        [Test]
        public void GivenGreaterThenOperatorGetsSameValues_ShouldBehaveAsDateTime()
        {
            var firstDateTime = new DateTime(2013, 4, 27, 12, 0, 0, DateTimeKind.Utc);
            var secondDateTime = new DateTime(2013, 4, 27, 12, 0, 0, DateTimeKind.Utc);
            var firstDateTimeUTC = new DateTimeUTC(firstDateTime);
            var secondDateTimeUTC = new DateTimeUTC(secondDateTime);

            Assert.AreEqual(firstDateTime >= secondDateTime, firstDateTimeUTC >= secondDateTimeUTC);
        }

        [Test]
        public void GivenGreaterThenOperatorGetsDifferentValues_ShouldBehaveAsDateTime()
        {
            var firstDateTime = new DateTime(2013, 4, 27, 12, 0, 0, DateTimeKind.Utc);
            var secondDateTime = new DateTime(2013, 4, 28, 12, 0, 0, DateTimeKind.Utc);
            var firstDateTimeUTC = new DateTimeUTC(firstDateTime);
            var secondDateTimeUTC = new DateTimeUTC(secondDateTime);

            Assert.AreEqual(firstDateTime >= secondDateTime, firstDateTimeUTC >= secondDateTimeUTC);
        }

        [Test]
        public void GivenSmallerThenOperatorGetsSameValues_ShouldBehaveAsDateTime()
        {
            var firstDateTime = new DateTime(2013, 4, 27, 12, 0, 0, DateTimeKind.Utc);
            var secondDateTime = new DateTime(2013, 4, 27, 12, 0, 0, DateTimeKind.Utc);
            var firstDateTimeUTC = new DateTimeUTC(firstDateTime);
            var secondDateTimeUTC = new DateTimeUTC(secondDateTime);

            Assert.AreEqual(firstDateTime <= secondDateTime, firstDateTimeUTC <= secondDateTimeUTC);
        }

        [Test]
        public void GivenSmallerThenOperatorGetsDifferentValues_ShouldBehaveAsDateTime()
        {
            var firstDateTime = new DateTime(2013, 4, 27, 12, 0, 0, DateTimeKind.Utc);
            var secondDateTime = new DateTime(2013, 4, 28, 12, 0, 0, DateTimeKind.Utc);
            var firstDateTimeUTC = new DateTimeUTC(firstDateTime);
            var secondDateTimeUTC = new DateTimeUTC(secondDateTime);

            Assert.AreEqual(firstDateTime <= secondDateTime, firstDateTimeUTC <= secondDateTimeUTC);
        }

        private void DateIsTheSame(DateTime a, DateTime b)
        {
            DateIsTheSameDate(a, b);
            DateIsTheSameTime(a, b);
        }

        private void DateIsTheSameDate(DateTime a, DateTime b)
        {
            Assert.AreEqual(a.Year, b.Year);
            Assert.AreEqual(a.Month, b.Month);
            Assert.AreEqual(a.Day, b.Day);
        }

        private void DateIsTheSameTime(DateTime a, DateTime b)
        {
            Assert.AreEqual(a.Hour, b.Hour);
            Assert.AreEqual(a.Minute, b.Minute);
            Assert.AreEqual(a.Second, b.Second);
            Assert.AreEqual(a.Millisecond, b.Millisecond);
        }
    }
}
