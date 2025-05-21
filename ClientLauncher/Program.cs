using System.Diagnostics;

class Program
{
    static void Main(string[] args)
    {
        if (args.Length < 3)
        {
            Console.WriteLine("Usage: ClientLauncher <clientExePath> <pipeOutHandle> <pipeInHandle>");
            return;
        }

        string clientExe = args[0];
        string pipeOut = args[1];
        string pipeIn = args[2];

        ProcessStartInfo psi = new ProcessStartInfo
        {
            FileName = clientExe,
            Arguments = $"{pipeOut} {pipeIn}",
            UseShellExecute = false
        };

        Process.Start(psi);
    }
}
