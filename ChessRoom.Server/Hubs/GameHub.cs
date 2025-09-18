using ChessRoom.Server.Services;
using Microsoft.AspNetCore.SignalR;
using System.Text.RegularExpressions;

namespace ChessRoom.Server.Hubs
{

    public record MoveDto(string From, string To, string? Promotion, string FenAfter, string San);
    public record JoinAck(string Fen, string Turn, long WhiteMs, long BlackMs, long LastMoveUnix, string? Role, object Presence);

    public class GameHub : Hub
    {
        private readonly IRoomStore _store;
        public GameHub(IRoomStore store) { _store = store; }

        public async Task JoinRoom(string roomId, string? desiredRole)
        {
            if(!_store.TryGet(roomId, out var room)) throw new HubException("Room not found");

            await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
            (_store as InMemoryRoomStore)!.TrackConnection(roomId, Context.ConnectionId);

            var sessionId = room.EnsureSession(Context.ConnectionId);
            string? role = null;
            if(desiredRole is "white" or "black")
                if(room.TrySeat(desiredRole, sessionId)) role = desiredRole;

            var s = room.State;
            await Clients.Caller.SendAsync("Joined", new JoinAck(
                s.Fen, s.Turn, s.WhiteMs, s.BlackMs, s.LastMoveUnix, role, room.Presence()
            ));
            await Clients.Group(roomId).SendAsync("PresenceUpdated", room.Presence());
        }

        public async Task MakeMove(string roomId, MoveDto m)
        {
            if(!_store.TryGet(roomId, out var room)) throw new HubException("Room not found");
            var sess = room.GetSessionForConnection(Context.ConnectionId);
            if(sess is null || !room.IsPlayersTurn(sess)) throw new HubException("Not your turn");

            // MVP: trust client FEN/SAN (fast path). Phase 2: validate on server with a chess engine.
            var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var elapsed = now - room.State.LastMoveUnix;
            if(room.State.Turn == "w") room.State = room.State with { WhiteMs = Math.Max(0, room.State.WhiteMs - elapsed) };
            else room.State = room.State with { BlackMs = Math.Max(0, room.State.BlackMs - elapsed) };

            var nextTurn = room.State.Turn == "w" ? "b" : "w";
            room.State = room.State with { Fen = m.FenAfter, Turn = nextTurn, LastMoveUnix = now };

            await Clients.Group(roomId).SendAsync("GameUpdated", new
            {
                fen = room.State.Fen,
                turn = room.State.Turn,
                whiteMs = room.State.WhiteMs,
                blackMs = room.State.BlackMs,
                lastMoveUnix = room.State.LastMoveUnix,
                lastSan = m.San,
                lastFrom = m.From,
                lastTo = m.To,
                promotion = m.Promotion
            });
        }

        public override async Task OnDisconnectedAsync(Exception? ex)
        {
            var drop = _store.DropConnection(Context.ConnectionId);
            if(drop is not null)
                await Clients.Group(drop.Value.RoomId).SendAsync("PresenceUpdated", drop.Value.Presence);
            await base.OnDisconnectedAsync(ex);
        }
    }
}
