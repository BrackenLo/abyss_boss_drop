using System;

using System.Runtime.InteropServices;

namespace VimJam_2_Boss_8_Bits_to_Infinity__Code_Sortout
{
    class Program
    {
        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        const int SW_HIDE = 0;
        const int SW_SHOW = 5;

        static void Main(string[] args)
        {
             var handle = GetConsoleWindow();

            // Hide
            ShowWindow(handle, SW_HIDE);

            //// Show

            Console.WriteLine("Hello World!");
            Game.Instance.start();
        }
    }
}
