using Godot;
using Engine = twodog.Engine;

internal static class Program
{
    // STA matches how godot.exe runs its main thread on Windows: OLE (drag & drop,
    // IME, native dialogs) fails to initialize on the MTA thread .NET uses by default.
    // No effect on Linux/macOS.
    [STAThread]
    private static void Main()
    {
        // Create and start the Godot engine with your project. Start() runs the
        // main scene configured in project.godot (run/main_scene), exactly like
        // launching godot.exe would - no manual scene loading needed.
        using var engine = new Engine("fennecs.demos.godot", Engine.ResolveProjectDir());
        using var godot = engine.Start();

        if (engine.Tree.CurrentScene is { } scene)
            GD.Print($"2dog is running '{scene.Name}'!");
        else
            GD.Print("2dog is running (no run/main_scene set in project.godot).");
        Console.WriteLine("Close the window or press 'Q' to quit.");

        // Key polling requires a real console; skip it when input is redirected
        // (piped, CI) - Console.KeyAvailable throws there.
        var interactive = !Console.IsInputRedirected;

        // Main game loop - runs until the window closes or 'Q' is pressed
        while (!godot.Iteration())
        {
            if (interactive && Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Q)
                break;

            // Your per-frame logic here
        }

        Console.WriteLine("Shutting down...");
    }
}
