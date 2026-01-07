using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Core.Models;

namespace CoreTests.Models
{
    [TestClass]
    public sealed class PersonalAnalyticsFeedbackTests
    {
        [TestMethod]
        public void TestIsGap()
        {
            var feedback = new PersonalAnalyticsFeedback(0, 0, 0, 0);

            Assert.IsTrue(feedback.IsGap());
        }

        [TestMethod]
        public void TestNotGap()
        {
            var feedback = new PersonalAnalyticsFeedback(1, 0, 0, 0);
            Assert.IsFalse(feedback.IsGap());
        }

        [TestMethod]
        public void TestUninitialized()
        {
            var feedback = new PersonalAnalyticsFeedback();

            Assert.ThrowsException<InvalidOperationException>(() => feedback.IsGap());
        }
    }
}
