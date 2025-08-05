using ChatWithAPIDemo.Models;

namespace ChatWithAPIDemo.Services
{
    public class RoomService
    {
        private readonly List<Room> _rooms;
        private readonly List<RoomAvailability> _roomAvailabilities;

        public RoomService(List<Room> rooms, List<RoomAvailability> roomAvailabilities)
        {
            _rooms = rooms;
            _roomAvailabilities = roomAvailabilities;
        }

        public List<Room> GetAllRooms() => _rooms;

        public List<Room> GetRoomsByHotelId(int hotelId)
        {
            // Burada hotel ile room arasındaki ilişki yokmuş gibi görünüyor
            // Eğer Room modeline HotelId eklenmişse kullanabilirsiniz
            return _rooms.Where(r => r.IsAvailable).ToList();
        }

        public Room? GetRoomById(int id)
        {
            return _rooms.FirstOrDefault(r => r.Id == id);
        }

        public List<Room> SearchRoomsByCapacity(int minCapacity)
        {
            return _rooms.Where(r => r.Capacity >= minCapacity && r.IsAvailable).ToList();
        }

        public List<Room> SearchRoomsBySeaView(bool hasSeaView)
        {
            return _rooms.Where(r => r.IsSeaView == hasSeaView && r.IsAvailable).ToList();
        }

        public List<Room> SearchRoomsBySmokingAllowed(bool smokingAllowed)
        {
            return _rooms.Where(r => r.IsSmokingAllowed == smokingAllowed && r.IsAvailable).ToList();
        }

        public List<Room> SearchRoomsByPriceRange(decimal minPrice, decimal maxPrice)
        {
            return _rooms.Where(r => r.Price >= minPrice && r.Price <= maxPrice && r.IsAvailable).ToList();
        }

        public bool IsRoomAvailable(int roomId, DateTime checkIn, DateTime checkOut)
        {
            var room = GetRoomById(roomId);
            if (room == null || !room.IsAvailable)
                return false;

            // Belirtilen tarih aralığında oda müsaitliğini kontrol et
            var conflictingAvailabilities = _roomAvailabilities.Where(ra =>
                ra.Room.Id == roomId &&
                ra.AvailabilitySlot.Status != AvailabilityStatus.Available &&
                (
                    (ra.AvailabilitySlot.Start <= checkIn && ra.AvailabilitySlot.End > checkIn) ||
                    (ra.AvailabilitySlot.Start < checkOut && ra.AvailabilitySlot.End >= checkOut) ||
                    (ra.AvailabilitySlot.Start >= checkIn && ra.AvailabilitySlot.End <= checkOut)
                )
            ).ToList();

            return !conflictingAvailabilities.Any();
        }

        public void BlockRoomAvailability(int roomId, DateTime checkIn, DateTime checkOut, string note = "")
        {
            var room = GetRoomById(roomId);
            if (room == null) return;

            var availability = new RoomAvailability
            {
                Id = _roomAvailabilities.Count > 0 ? _roomAvailabilities.Max(ra => ra.Id ?? 0) + 1 : 1,
                Room = room,
                AvailabilitySlot = new AvailabilitySlot
                {
                    Start = checkIn,
                    End = checkOut,
                    Status = AvailabilityStatus.Reserved,
                    Note = note
                }
            };

            _roomAvailabilities.Add(availability);
        }

        public void FreeRoomAvailability(int roomId, DateTime checkIn, DateTime checkOut)
        {
            var availabilitiesToRemove = _roomAvailabilities.Where(ra =>
                ra.Room.Id == roomId &&
                ra.AvailabilitySlot.Start == checkIn &&
                ra.AvailabilitySlot.End == checkOut &&
                ra.AvailabilitySlot.Status == AvailabilityStatus.Reserved
            ).ToList();

            foreach (var availability in availabilitiesToRemove)
            {
                _roomAvailabilities.Remove(availability);
            }
        }

        public List<RoomAvailability> GetRoomAvailabilities(int roomId)
        {
            return _roomAvailabilities.Where(ra => ra.Room.Id == roomId).ToList();
        }

        public Room AddRoom(string roomNumber, int floor, int capacity, bool isSeaView, bool isSmokingAllowed, decimal price)
        {
            var room = new Room
            {
                Id = _rooms.Count > 0 ? _rooms.Max(r => r.Id) + 1 : 1,
                RoomNumber = roomNumber,
                Floor = floor,
                Capacity = capacity,
                IsSeaView = isSeaView,
                IsSmokingAllowed = isSmokingAllowed,
                IsAvailable = true,
                Price = price
            };

            _rooms.Add(room);
            return room;
        }

        public bool UpdateRoomAvailability(int roomId, bool isAvailable)
        {
            var room = GetRoomById(roomId);
            if (room == null) return false;

            room.IsAvailable = isAvailable;
            return true;
        }
    }

}
