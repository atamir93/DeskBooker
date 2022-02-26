using DeskBooker.Core.Domain;
using System;
using Xunit;

namespace DeskBooker.Core.Processor
{
    public class DeskBookingRequestProcessorTests
    {
        private readonly DeskBookingRequestProcessor processor;

        // Setup
        public DeskBookingRequestProcessorTests()
        {
            processor = new DeskBookingRequestProcessor();
        }

        [Fact]
        public void RequestIsNotNull_ReturnDeskBookingResultWithRequestValues()
        {
            // Arrange
            var request = new DeskBookingRequest
            {
                FirstName = "Ataev",
                LastName = "Daler",
                Email = "test@gmail.com",
                Date = new DateTime(2020, 04, 08)
            };

            // Act
            DeskBookingResult result = processor.BookDesk(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(request.FirstName, result.FirstName);
            Assert.Equal(request.LastName, result.LastName);
            Assert.Equal(request.Email, result.Email);
            Assert.Equal(request.Date, result.Date);
        }

        [Fact]
        public void RequestIsNull_ThrowException()
        {
            var exception = Assert.Throws<ArgumentNullException>(() => processor.BookDesk(null));
            Assert.Equal("request", exception.ParamName);
        }
    }
}
