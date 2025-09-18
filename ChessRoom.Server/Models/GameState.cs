namespace ChessRoom.Server.Models
{
    public record GameState(
    string Fen,
    string Turn,     // "w" or "b"
    long WhiteMs,
    long BlackMs,
    long LastMoveUnix // unix ms
    );
}
