using DeskBooker.Core.DataInterface;
using DeskBooker.Core.Domain;

namespace DeskBooker.Core.Processor
{
    public class DeskBookingRequestProcessor : IDeskBookingRequestProcessor
    {
        private readonly IDeskBookingRepository deskBookingRepository;
        private readonly IDeskRepository deskRepository;

        public DeskBookingRequestProcessor()
        {
        }

        public DeskBookingRequestProcessor(IDeskBookingRepository deskBookingRepository, IDeskRepository deskRepository)
        {
            this.deskBookingRepository = deskBookingRepository;
            this.deskRepository = deskRepository;
        }

        public DeskBookingResult BookDesk(DeskBookingRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var result = Create<DeskBookingResult>(request);
            var availableDesks = deskRepository.GetAvailableDesks(request.Date);
            if (availableDesks.Any())
            {
                var availableDesk = availableDesks.First();
                var deskBooking = Create<DeskBooking>(request);
                deskBooking.DeskId = availableDesk.Id;

                deskBookingRepository.Save(deskBooking);

                result.DeskBookingId = deskBooking.Id;
                result.Code = DeskBookingResultCode.Success;
            }
            else
            {
                result.Code = DeskBookingResultCode.NoDeskAvailable;
            }

            return result;
        }

        private T Create<T>(DeskBookingRequest request) where T : DeskBookingBase, new()
        {
            return new T
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                Date = request.Date
            };
        }
    }
}