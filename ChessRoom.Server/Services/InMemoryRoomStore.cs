using ChessRoom.Server.Models;
using System.Collections.Concurrent;

namespace ChessRoom.Server.Services
{
    public class InMemoryRoomStore : IRoomStore
    {
        private readonly ConcurrentDictionary<string, Room> _rooms = new();
        private readonly ConcurrentDictionary<string, string> _connToRoom = new();

        public Room Add(Room room) { _rooms[room.Id] = room; return room; }
        public bool TryGet(string id, out Room room) => _rooms.TryGetValue(id, out room);
        public void Remove(string id) { _rooms.TryRemove(id, out _); }

        public (string RoomId, object Presence)? DropConnection(string connectionId)
        {
            if(!_connToRoom.TryRemove(connectionId, out var roomId)) return null;
            if(!_rooms.TryGetValue(roomId, out var room)) return null;
            // We don’t clear seats on disconnect for MVP (helps reconnect); can add timeouts later
            return (roomId, room.Presence());
        }

        public void TrackConnection(string roomId, string connectionId)
            => _connToRoom[connectionId] = roomId;
    }
}
