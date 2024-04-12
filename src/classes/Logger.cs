namespace G3Archive
{
    public class Logger
    {
        public static bool Enabled = true;
        public static void Log(string message)
        {
            if (!Enabled) return;
            Console.WriteLine(message);
        }
    }
}
