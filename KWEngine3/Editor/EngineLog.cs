namespace KWEngine3.Editor
{
    internal static class EngineLog
    {
        private const int MAXMESSAGES = 50;
        public static Queue<string> _messages = new Queue<string>(MAXMESSAGES);

        public static void AddMessage(string message)
        {
            if (message != null && message.Length > 0)
            {
                if (_messages.Count >= MAXMESSAGES)
                    _messages.Dequeue();
                _messages.Enqueue(message);

                ConsoleColor bak = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(message);
                Console.ForegroundColor = bak;
            }
        }

        public static void Clear()
        {
            _messages.Clear();
        }
    }
}
