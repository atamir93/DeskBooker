using DeskBooker.Core.DataInterface;
using DeskBooker.Core.Domain;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace DeskBooker.Core.Processor
{
    public class DeskBookingRequestProcessorTests
    {
        private readonly DeskBookingRequest request;
        private readonly List<Desk> availableDesks;
        private readonly Mock<IDeskBookingRepository> deskBookingRepositoryMock;
        private readonly Mock<IDeskRepository> deskRepositoryMock;
        private readonly DeskBookingRequestProcessor processor;

        // Setup
        public DeskBookingRequestProcessorTests()
        {
            request = new DeskBookingRequest
            {
                FirstName = "Ataev",
                LastName = "Daler",
                Email = "test@gmail.com",
                Date = new DateTime(2020, 1, 28)
            };
            availableDesks = new List<Desk> { new Desk { Id = 7 } };
            deskBookingRepositoryMock = new Mock<IDeskBookingRepository>();
            deskRepositoryMock = new Mock<IDeskRepository>();
            deskRepositoryMock.Setup(x => x.GetAvailableDesks(request.Date)).Returns(availableDesks);
            processor = new DeskBookingRequestProcessor(deskBookingRepositoryMock.Object, deskRepositoryMock.Object);
        }

        [Fact]
        public void RequestIsNotNull_ReturnDeskBookingResultWithRequestValues()
        {
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

        [Fact]
        public void DeskAvailable_SaveDeskBooking()
        {
            DeskBooking savedDeskBooking = null;
            deskBookingRepositoryMock.Setup(x => x.Save(It.IsAny<DeskBooking>())).Callback<DeskBooking>(deskBooking =>
            {
                savedDeskBooking = deskBooking;
            });
            processor.BookDesk(request);

            deskBookingRepositoryMock.Verify(x => x.Save(It.IsAny<DeskBooking>()), Times.Once);
            Assert.NotNull(savedDeskBooking);
            Assert.Equal(request.FirstName, savedDeskBooking.FirstName);
            Assert.Equal(request.LastName, savedDeskBooking.LastName);
            Assert.Equal(request.Date, savedDeskBooking.Date);
            Assert.Equal(request.Email, savedDeskBooking.Email);
            Assert.Equal(availableDesks.First().Id, savedDeskBooking.DeskId);
        }

        [Fact]
        public void NoDeskAvailable_DoNotSaveDeskBooking()
        {
            availableDesks.Clear();
            processor.BookDesk(request);
            deskBookingRepositoryMock.Verify(x => x.Save(It.IsAny<DeskBooking>()), Times.Never);
        }

        //Data driven test
        [Theory]
        [InlineData(DeskBookingResultCode.Success, true)]
        [InlineData(DeskBookingResultCode.NoDeskAvailable, false)]
        public void ShouldReturnExpectedResultCode(DeskBookingResultCode expectedResultCode, bool isDeskAvailable)
        {
            if (!isDeskAvailable)
            {
                availableDesks.Clear();
            }

            var result = processor.BookDesk(request);
            Assert.Equal(expectedResultCode, result.Code);
        }

        //Data driven test
        [Theory]
        [InlineData(5, true)]
        [InlineData(null, false)]
        public void ShouldReturnExpectedDeskBookingId(int? expectedDeskBookingId, bool isDeskAvailable)
        {
            if (!isDeskAvailable)
            {
                availableDesks.Clear();
            }
            else
            {
                deskBookingRepositoryMock.Setup(x => x.Save(It.IsAny<DeskBooking>())).Callback<DeskBooking>(deskBooking => 
                {
                    deskBooking.Id = expectedDeskBookingId.Value;
                });
            }

            var result = processor.BookDesk(request);
            Assert.Equal(expectedDeskBookingId, result.DeskBookingId);
        }
    }
}
