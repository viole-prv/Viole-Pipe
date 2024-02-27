using System.IO;
using System.IO.Pipes;
using System.Runtime.InteropServices;

namespace Viole_Pipe
{
    public class Pipe
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool WaitNamedPipe(string Name, int Timeout);

        public readonly string Name;

        public static string[] List() => Directory.GetFiles(@"\\.\pipe\");

        public Pipe(string Name)
        {
            this.Name = Name;
        }

        public bool Any() => Any(Name);

        public void Set(string Command) => Set(Name, Command);

        public static bool Any(string Name)
        {
            if (!WaitNamedPipe(Path.GetFullPath(string.Format("\\\\.\\pipe\\{0}", Name)), 0))
            {
                int LastWin32Error = Marshal.GetLastWin32Error();

                switch (LastWin32Error)
                {
                    case 0:
                    case 2:
                        return false;
                }
            }

            return true;
        }

        public static void Set(string Name, string Command)
        {
            using (var NamedPipeClientStream = new NamedPipeClientStream(".", Name, PipeDirection.InOut))
            {
                using (var StreamWriter = new StreamWriter(NamedPipeClientStream))
                {
                    NamedPipeClientStream.Connect();
                    StreamWriter.Write(Command);

                    StreamWriter.Dispose();
                    NamedPipeClientStream.Dispose();
                }
            }
        }
    }
}
