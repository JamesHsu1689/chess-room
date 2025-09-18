namespace ChessRoom.Server.Models
{
    public class Ids
    {
        private const string Alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789"; // no 0/O/1/I
        private static readonly Random Rand = new();

        public static string Short(int len = 6)
        {
            var chars = new char[len];
            for(int i = 0; i < len; i++) chars[i] = Alphabet[Rand.Next(Alphabet.Length)];
            return new string(chars);
        }
        public static string Token() => Convert.ToHexString(Guid.NewGuid().ToByteArray());
    }
}
