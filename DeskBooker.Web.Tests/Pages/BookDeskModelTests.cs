using DeskBooker.Core.Domain;
using DeskBooker.Core.Processor;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;

namespace DeskBooker.Web.Pages
{
    [TestClass]
    public class BookDeskModelTests
    {
        private Mock<IDeskBookingRequestProcessor> processorMock;
        private BookDeskModel bookDeskModel;
        private DeskBookingResult deskBookingResult;

        [TestInitialize]
        public void Initialize()
        {
            processorMock = new Mock<IDeskBookingRequestProcessor>();

            bookDeskModel = new BookDeskModel(processorMock.Object)
            {
                DeskBookingRequest = new DeskBookingRequest()
            };

            deskBookingResult = new DeskBookingResult
            {
                Code = DeskBookingResultCode.Success
            };

            processorMock.Setup(x => x.BookDesk(bookDeskModel.DeskBookingRequest))
             .Returns(deskBookingResult);
        }

        [DataTestMethod]
        [DataRow(1, true)]
        [DataRow(0, false)]
        public void ShouldCallBookDeskMethodOfProcessorIfModelIsValid(int expectedBookDeskCalls, bool isModelValid)
        {
            if (!isModelValid)
            {
                bookDeskModel.ModelState.AddModelError("Key", "Message");
            }
            bookDeskModel.OnPost();

            processorMock.Verify(x => x.BookDesk(bookDeskModel.DeskBookingRequest), Times.Exactly(expectedBookDeskCalls));
        }

        [TestMethod]
        public void ShouldAddModelErrorIfNoDeskIsAvailable()
        {
            deskBookingResult.Code = DeskBookingResultCode.NoDeskAvailable;

            bookDeskModel.OnPost();

            Assert.IsTrue(bookDeskModel.ModelState.TryGetValue("DeskBookingRequest.Date", out ModelStateEntry modelStateEntry));
            Assert.AreEqual(1, modelStateEntry.Errors.Count);
            var modelError = modelStateEntry.Errors[0];
            Assert.AreEqual("No desk available for selected date", modelError.ErrorMessage);
        }

        [TestMethod]
        public void ShouldNotAddModelErrorIfDeskIsAvailable()
        {
            deskBookingResult.Code = DeskBookingResultCode.Success;

            bookDeskModel.OnPost();

            Assert.IsFalse(bookDeskModel.ModelState.TryGetValue("DeskBookingRequest.Date", out ModelStateEntry modelStateEntry));
        }

        [DataTestMethod]
        [DataRow(typeof(PageResult), false, null)]
        [DataRow(typeof(PageResult), true, DeskBookingResultCode.NoDeskAvailable)]
        [DataRow(typeof(RedirectToPageResult), true, DeskBookingResultCode.Success)]
        public void ShoudlReturnExpectedActionResult(Type expectedActionResultType, bool isModelValid, DeskBookingResultCode? deskBookingResultCode)
        {
            if (!isModelValid)
            {
                bookDeskModel.ModelState.AddModelError("Key", "Message");
            }

            if (deskBookingResultCode.HasValue)
            {
                deskBookingResult.Code = deskBookingResultCode.Value;
            }

            IActionResult actionResult = bookDeskModel.OnPost();

            // Assert
            Assert.IsInstanceOfType(actionResult, expectedActionResultType);
        }


        [TestMethod]
        public void ShouldRedirectToBookDeskConfirmationPage()
        {
            // Arrange
            deskBookingResult.Code = DeskBookingResultCode.Success;
            deskBookingResult.DeskBookingId = 13;
            deskBookingResult.FirstName = "Atamir";
            deskBookingResult.Date = new DateTime(2020, 1, 28);

            // Act
            IActionResult actionResult = bookDeskModel.OnPost();

            // Assert
            var redirectToPageResult = (RedirectToPageResult)actionResult;
            Assert.AreEqual("BookDeskConfirmation", redirectToPageResult.PageName);

            var routeValues = redirectToPageResult.RouteValues;
            Assert.AreEqual(3, routeValues.Count);

            Assert.IsTrue(routeValues.TryGetValue("DeskBookingId", out object deskBookingId));
            Assert.AreEqual(deskBookingResult.DeskBookingId, deskBookingId);

            Assert.IsTrue(routeValues.TryGetValue("FirstName", out object firstName));
            Assert.AreEqual(deskBookingResult.FirstName, firstName);

            Assert.IsTrue(routeValues.TryGetValue("Date", out object date));
            Assert.AreEqual(deskBookingResult.Date, date);
        }
    }
}
