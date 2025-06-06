using System;
using System.Runtime.InteropServices;

static class WindowConsoleHandler {
    [DllImport("libstdin_handler", SetLastError = true)]
    private static extern int init();

    [DllImport("libstdin_handler", SetLastError = true)]
    private static extern byte read_stdin();

    [DllImport("libstdin_handler", SetLastError = true)]
    private static extern int reset();
    
    public static int readStdin() => (int)read_stdin();

    public static void Setup() {
        init();
    }

    public static void Reset() {
        reset();
    }
}

static class PosixConsoleHandler {
    [DllImport("libstdin_handler", SetLastError = true)]
    private static extern int init();

    [DllImport("libstdin_handler", SetLastError = true)]
    private static extern byte read_stdin();

    [DllImport("libstdin_handler", SetLastError = true)]
    private static extern int reset();

    public static int readStdin() => (int)read_stdin();

    private static void addPath(string name) {
        string content = Environment.GetEnvironmentVariable(name);
        if (content == null) content = "";
        string path = Environment.CurrentDirectory;
        content += $":{path}";
        Environment.SetEnvironmentVariable(name, content);
    }

    public static void Setup() {
        addPath("LD_LIBRARY_PATH"); // linux
        addPath("DYLD_LIBRARY_PATH"); //macbook
        addPath("DYLD_FRAMEWORK_PATH");
        addPath("DYLD_FALLBACK_FRAMEWORK_PATH");
        addPath("DYLD_FALLBACK_LIBRARY_PATH");
        init();
    }

    public static void Reset() {
        reset();
    }
}

static class ConsoleIntermediateHandler {
    public static void Setup() {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
            WindowConsoleHandler.Setup();
        } else {
            PosixConsoleHandler.Setup();
        }
    }

    public static int Read() {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
            return WindowConsoleHandler.readStdin();
        } else {
            return PosixConsoleHandler.readStdin();
        }
    }

    public static void Reset() {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
            WindowConsoleHandler.Reset();
        } else {
            PosixConsoleHandler.Reset();
        }
    }
}

internal class Program {
    public static string ToANSI(string content, string control = "[", string special = "\x1b") => special + control + content;
    public static void Main(string[] _) {
        ConsoleIntermediateHandler.Setup();
        Console.Write($"{ToANSI("?1049h")}{ToANSI("5;5H")}{ToANSI("=19h")}{ToANSI("=7l")}{ToANSI("?25l")}{ToANSI("38;2;128;130;155m")}abc{ToANSI("0m")}{ToANSI("6n")}{ToANSI("?1004h")}{ToANSI("?9h")}{ToANSI("?1001h")}{ToANSI("?1000h")}{ToANSI("?1003h", "[", "\x1b")}{ToANSI("?25h")}{ToANSI("?40l")}{ToANSI("?3l")}{ToANSI("?1006h")}");
        while (true) {
            int v = (int)(ConsoleIntermediateHandler.Read());
            switch (v)
            {
                case 3:
                  ConsoleIntermediateHandler.Reset();
                  return ;
                case 97:
                    Console.Write(ToANSI("6n"));
                    break;
            }
            Console.Write($" {(int)v}");
            if (v >= 32 && v < 128) Console.Write($"({(char)v})");
            Console.Out.Flush();
        }
    }
}