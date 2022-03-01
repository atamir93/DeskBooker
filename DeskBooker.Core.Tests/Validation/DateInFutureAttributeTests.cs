using System;
using Xunit;

namespace DeskBooker.Core.Validation
{
    public class DateInFutureAttributeTests
    {
        [Theory]
        [InlineData(false, -1)]
        [InlineData(false, 0)]
        [InlineData(true, 1)]
        public void ValidateDateIsInTheFuture(bool expectedIsValid, int secondsToAdd)
        {
            var dateTimeNow = new DateTime(2020, 1, 13);
            var attribute = new DateInFutureAttribute(() => dateTimeNow);
            var isValid = attribute.IsValid(dateTimeNow.AddSeconds(secondsToAdd));
            Assert.Equal(expectedIsValid, isValid);
        }

        [Fact]
        public void ShouldHaveExpectedErrorMessage()
        {
            var attribute = new DateInFutureAttribute();
            Assert.Equal("Date must be in the future", attribute.ErrorMessage);
        }
    }
}
