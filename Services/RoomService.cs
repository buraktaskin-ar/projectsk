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
            return _rooms.Where(r => r.Hotel?.Id == hotelId).ToList();
        }

        public Room? GetRoomById(int id)
        {
            return _rooms.FirstOrDefault(r => r.Id == id);
        }

        public List<Room> SearchRoomsByCapacity(int minCapacity)
        {
            return _rooms.Where(r => r.Capacity >= minCapacity).ToList();
        }

        public List<Room> SearchRoomsBySeaView(bool hasSeaView)
        {
            return _rooms.Where(r => r.IsSeaView == hasSeaView).ToList();
        }

        public List<Room> SearchRoomsByType(RoomType roomType)
        {
            return _rooms.Where(r => r.RoomType == roomType).ToList();
        }

        public List<Room> SearchRoomsByPriceRange(decimal minPrice, decimal maxPrice)
        {
            return _rooms.Where(r => r.Price >= minPrice && r.Price <= maxPrice).ToList();
        }

        public bool IsRoomAvailable(int roomId, DateTime checkIn, DateTime checkOut)
        {
            var room = GetRoomById(roomId);
            if (room == null)
                return false;

            // Check if there are any conflicting reservations in the room's availability list
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

            // Also add to the room's availability list
            room.RoomAvailabilities.Add(availability);
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

                // Also remove from room's list
                var room = GetRoomById(roomId);
                if (room != null)
                {
                    room.RoomAvailabilities.Remove(availability);
                }
            }
        }

        public List<RoomAvailability> GetRoomAvailabilities(int roomId)
        {
            return _roomAvailabilities.Where(ra => ra.Room.Id == roomId).ToList();
        }

        public Room AddRoom(string roomNumber, int floor, int capacity, bool isSeaView, RoomType roomType, decimal price)
        {
            var room = new Room
            {
                Id = _rooms.Count > 0 ? _rooms.Max(r => r.Id) + 1 : 1,
                RoomNumber = roomNumber,
                Floor = floor,
                Capacity = capacity,
                IsSeaView = isSeaView,
                RoomType = roomType,
                Price = price
            };

            _rooms.Add(room);
            return room;
        }
    }
}