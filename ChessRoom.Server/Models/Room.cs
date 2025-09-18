using System.Collections.Concurrent;

namespace ChessRoom.Server.Models
{

    public enum RoomStatus { Open, Playing, Ended }

    public class Room
    {
        public string Id { get; init; } = Ids.Short();
        public string HostToken { get; init; } = Ids.Token();

        public string? WhiteSession { get; private set; }
        public string? BlackSession { get; private set; }

        public RoomStatus Status { get; set; } = RoomStatus.Open;
        public bool AllowSpectators { get; init; } = true;

        public GameState State { get; set; } = new("startpos", "w",
            WhiteMs: 10 * 60_000, BlackMs: 10 * 60_000, LastMoveUnix: DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());

        // Very light "sessions" so reconnects can keep your seat
        private readonly ConcurrentDictionary<string, string> _connToSession = new();
        public string EnsureSession(string connectionId)
        {
            return _connToSession.GetOrAdd(connectionId, _ => Guid.NewGuid().ToString("N"));
        }
        public string? GetSessionForConnection(string connectionId)
            => _connToSession.TryGetValue(connectionId, out var s) ? s : null;

        public bool TrySeat(string role, string sessionId)
        {
            if(role == "white" && WhiteSession is null) { WhiteSession = sessionId; return true; }
            if(role == "black" && BlackSession is null) { BlackSession = sessionId; return true; }
            return false;
        }

        public bool IsPlayersTurn(string sessionId)
            => (State.Turn == "w" && WhiteSession == sessionId) ||
               (State.Turn == "b" && BlackSession == sessionId);

        public object Presence() => new
        {
            players = new { white = WhiteSession is not null, black = BlackSession is not null },
        };
    }
}
