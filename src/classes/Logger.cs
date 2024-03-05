namespace G3Archive
{
    public class Logger
    {
        public static bool Quiet = false;
        public static void Log(string message)
        {
            if(Quiet) { return; }
            Console.WriteLine(message);
        }
    }
}
