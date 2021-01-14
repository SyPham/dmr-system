using DMR_API._Services.Interface;
using DMR_API._Services.Services;
using NUnit.Framework;
using System;
using System.Linq;

namespace DMR_Testing
{
    public class Time
    {
        private ITimeService _timeService;
        [SetUp]
        public void Setup()
        {
            _timeService = new TimeService();
        }

        [Test]
        public void Test()
        {
            var ct = DateTime.Now;
            var s = new DateTime(ct.Year, ct.Month, ct.Day, 7, 00, 0); // 7, 9, 11, 13
            var t = new DateTime(ct.Year, ct.Month, ct.Day, 16, 30, 0);
            var dueDate = new DateTime(ct.Year, ct.Month, ct.Day);
            var result = _timeService.TimeRange(s, t, dueDate).ToList();

            Assert.IsTrue(result.Count == 5);
            Assert.Pass();
        }
    }
}