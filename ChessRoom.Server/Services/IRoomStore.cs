using ChessRoom.Server.Models;

namespace ChessRoom.Server.Services
{
    public interface IRoomStore
    {
        Room Add(Room room);
        bool TryGet(string id, out Room room);
        void Remove(string id);
        (string RoomId, object Presence)? DropConnection(string connectionId);
    }
}
