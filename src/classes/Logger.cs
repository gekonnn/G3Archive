namespace G3Archive
{
    public class Logger
    {
        public static bool Enabled = false;
        public static void Log(string message)
        {
            if (!Enabled) return;
            Console.WriteLine(message);
        }
    }
}
