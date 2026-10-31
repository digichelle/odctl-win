/*
odctl - OneDrive command-line control for Windows
Copyright © 2026 Michelle Catherine Marcó
MIT License: https://opensource.org/license/MIT
*/

using System.CommandLine;
using System.CommandLine.Invocation;
using System.Diagnostics;

namespace ODCtl;

// Initialize "global" variables and implement functions
internal class ODCtl {

    internal const string ProgramName = "ODCtl";
    internal const string Description = "OneDrive command-line control for Windows";
    internal const string Copyright = "Copyright © 2026 Michelle Catherine Marcó";
    internal const string License = "MIT License: https://opensource.org/licenses/MIT";
    internal const string OneDriveExe = "OneDrive.exe";
    internal const string OneDriveSyncServiceExe = "OneDrive.Sync.Service.exe";
    internal const string FileCoAuthExe = "FileCoAuth.exe";
    internal const string FileSyncHelperExe = "FileSyncHelper.exe";
    internal static string OneDrivePath = "";
    internal static string LocalAppDataPath = Environment.GetEnvironmentVariable("LocalAppData") + "\\Microsoft\\OneDrive\\";
    internal static string ProgramFiles32Path = Environment.GetEnvironmentVariable("ProgramFiles(x86)") + "\\Microsoft OneDrive\\";
    internal static string ProgramFiles64Path = Environment.GetEnvironmentVariable("ProgramW6432") + "\\Microsoft OneDrive\\";
    internal const string StartOpts = "/background";
    internal const string StopOpts = "/background /shutdown";
    internal const int RestartWait = 5000;
    internal static bool CustomPathEnabled = false;
    internal static bool DryRunMode = false;
    internal static bool QuietMode = false;
    internal static bool VerboseMode = false;

    // Function to write verbose messages when verbose mode is on
    internal static void VerboseMessageLine(string Message = "") {
        if (VerboseMode) {
            Console.WriteLine(Message);
        }
        return;
    }

    // Function to write optional messages when quiet mode is off
    // If both quiet and verbose modes are enabled, the latter takes priority
    internal static void OptionalMessageLine(string Message = "") {
        if (VerboseMode || !QuietMode) {
            Console.WriteLine(Message);
        }
        return;
    }

    // Function to set OneDrive path
    internal static int SetOneDrivePath(string @CustomPath = "") {
        // If no path specified, trigger automatic detection
        if (@CustomPath == "") {
            VerboseMessageLine("Searching for OneDrive executable...");
            // Look for OneDrive.exe in known installation directories
            // In descending order of preference: LocalAppData, ProgramFiles64, ProgramFiles32
            // If not found, exit with error
            if (File.Exists(@LocalAppDataPath + OneDriveExe)) {
                VerboseMessageLine("Found: " + @LocalAppDataPath + OneDriveExe + Environment.NewLine);
                OneDrivePath = @LocalAppDataPath;
            }
            else if (File.Exists(@ProgramFiles64Path + OneDriveExe)) {
                VerboseMessageLine("Found: " + @ProgramFiles64Path + OneDriveExe + Environment.NewLine);
                OneDrivePath = @ProgramFiles64Path;
            }
            else if (File.Exists(@ProgramFiles32Path + OneDriveExe)) {
                VerboseMessageLine("Found: " + @ProgramFiles32Path + OneDriveExe + Environment.NewLine);
                OneDrivePath = @ProgramFiles32Path;
            }
            else {
                Console.Error.WriteLine("Error: OneDrive not found");
                Environment.Exit(1);
                return 1;
            }
        }
        // If path specified, set custom path
        else {
            VerboseMessageLine("Custom path specified: " + @CustomPath);
            // Ensure that the custom path is a valid and full (not relative) path
            if (!Path.IsPathFullyQualified(@CustomPath)) {
                Console.Error.WriteLine("Error: specified path is not fully qualified");
                Environment.Exit(1);
            }
            // If path doesn't end with a backslash, add one
            if (!@CustomPath.EndsWith('\\')) {
                @CustomPath += '\\';
            }
            // If OneDrive is not found in path, exit with error
            if (!File.Exists(@CustomPath + OneDriveExe)) {
                Console.Error.WriteLine("Error: OneDrive not found in specified path");
                Environment.Exit(1);
            }
            // Set custom OneDrive path
            VerboseMessageLine("Custom path set" + Environment.NewLine);
            OneDrivePath = @CustomPath;
            CustomPathEnabled = true;
        }
        return 0;
    }

    // Function to display OneDrive executable
    internal static int Where() {
        // If OneDrive.exe exists, show custom/detected path, else return 1 (error)
        if (@OneDrivePath != "") {
            Console.WriteLine(@OneDrivePath + OneDriveExe);
            if (CustomPathEnabled) {
                VerboseMessageLine("Path type: custom");
            }
            else {
                VerboseMessageLine("Path type: detected");
            }
            return 0;
        }
        return 1;
    }

    // Function to check if a process is running
    internal static bool IsProcessRunning(string ProcessExe) {
        // Strip ".exe" from name, if present
        if (ProcessExe.EndsWith(".exe")) {
            ProcessExe = ProcessExe[..^4];
        }
        // Pass ProcessExe to Process.GetProcessesByName()
        Process[] ProcessName = Process.GetProcessesByName(ProcessExe);
        // If ProcessExe not running, return false, else return true
        if (ProcessName.Length == 0) {
            return false;
        }
        else {
            return true;
        }
    }

    // Function to kill process
    internal static int KillProcess(string ProcessExe) {
        // Strip ".exe" from name, if present
        if (ProcessExe.EndsWith(".exe")) {
            ProcessExe = ProcessExe[..^4];
        }
        // Kill process
        foreach (var P in Process.GetProcessesByName(ProcessExe)) {
            P.Kill();
            P.WaitForExit();
            P.Dispose();
        }
        return 0;
    }

    // Function to display OneDrive process status
    internal static void Status() {
        Console.WriteLine("OneDrive Process Status");
        Console.WriteLine();
        // Table header
        Console.Write("Process                     Running" + Environment.NewLine);
        Console.Write("-----------------------------------" + Environment.NewLine);
        // OneDrive status
        Console.Write("OneDrive.exe                ");
        if (IsProcessRunning(OneDriveExe)) {
            Console.Write("yes");
        }
        else {
            Console.Write("no");
        }
        Console.Write(Environment.NewLine);
        // OneDrive Sync Service status
        Console.Write("OneDrive.Sync.Service.exe   ");
        if (IsProcessRunning(OneDriveSyncServiceExe)) {
            Console.Write("yes");
        }
        else {
            Console.Write("no");
        }
        Console.Write(Environment.NewLine);
        Console.Write("FileCoAuth.exe              ");
        if (IsProcessRunning(FileCoAuthExe)) {
            Console.Write("yes");
        }
        else {
            Console.Write("no");
        }
        Console.Write(Environment.NewLine);
        Console.Write("FileSyncHelper.exe          ");
        if (IsProcessRunning(FileSyncHelperExe)) {
            Console.Write("yes");
        }
        else {
            Console.Write("no");
        }
        Console.Write(Environment.NewLine);
        return;
    }

    // Function to define start commands
    internal static int StartCmd() {
        // If dry run mode enabled, bypass command execution
        if (DryRunMode) {
            Console.WriteLine("DRY RUN: START COMMAND DISABLED");
            return 0;
        }
        // If OneDrive running, exit with error, else start
        if (IsProcessRunning(OneDriveExe)) {
            Console.Error.WriteLine("Error: OneDrive is already running");
            Environment.Exit(1);
            return 1;
        }
        else {
            string Cmd = @OneDrivePath + OneDriveExe;
            VerboseMessageLine("Running start command: " + Cmd + ' ' + StartOpts);
            Process.Start(Cmd, StartOpts);
            return 0;
        }
    }

    // Function to define stop commands
    internal static int StopCmd() {
        // If dry run mode enabled, bypass command execution
        if (DryRunMode) {
            Console.WriteLine("DRY RUN: STOP COMMAND DISABLED");
            return 0;
        }
        // If OneDrive not running, exit with error, else stop
        if (!IsProcessRunning(OneDriveExe)) {
            Console.Error.WriteLine("Error: OneDrive is not running");
            Environment.Exit(1);
            return 1;
        }
        else {
            string Cmd = @OneDrivePath + OneDriveExe;
            VerboseMessageLine("Running stop command: " + Cmd + ' ' + StopOpts);
            Process.Start(Cmd, StopOpts);
            if (IsProcessRunning(OneDriveSyncServiceExe)) {
                VerboseMessageLine("Killing process: " + OneDriveSyncServiceExe);
                KillProcess(OneDriveSyncServiceExe);
            }
            return 0;
        }
    }

    // Function to start OneDrive
    internal static int Start() {
        OptionalMessageLine("Starting OneDrive...");
        StartCmd();
        OptionalMessageLine("OneDrive started!");
        return 0;
    }

    // Function to stop OneDrive
    internal static int Stop() {
        OptionalMessageLine("Stopping OneDrive...");
        StopCmd();
        OptionalMessageLine("OneDrive stopped!");
        return 0;
    }

    // Function to restart (start, and then stop) OneDrive
    internal static int Restart() {
        OptionalMessageLine("Restarting OneDrive...");
        StopCmd();
        VerboseMessageLine("Waiting " + RestartWait + " ms before start...");
        Thread.Sleep(RestartWait);
        StartCmd();
        OptionalMessageLine("OneDrive restarted!");
        return 0;
    }

}

// Implement custom version action
public sealed class CustomVersion : SynchronousCommandLineAction {
    public override int Invoke(ParseResult PR) {
        PR.InvocationConfiguration.Output.WriteLine(ODCtl.ProgramName + " - " + ODCtl.Description);
        PR.InvocationConfiguration.Output.WriteLine("Version " + Application.ProductVersion);
        PR.InvocationConfiguration.Output.WriteLine(ODCtl.Copyright);
        PR.InvocationConfiguration.Output.WriteLine(ODCtl.License);
        return 0;
    }
}

// Program execution begins here
internal class Program {
    private static int Main(string[] args) {

        // Command/option parsing begins here

        // Declare root command
        RootCommand RootCmd = new(ODCtl.Description);

        // Declare other commands
        Command Status = new("status") {
            Description = "Show OneDrive process status"
        };
        Command Where = new("where") {
            Description = "Show OneDrive path"
        };
        Command Which = new("which") {
            Description = "Alias of where (hidden)",
            Hidden = true
        };
        Command Start = new("start") {
            Description = "Start OneDrive"
        };
        Command Stop = new("stop") {
            Description = "Stop OneDrive"
        };
        Command Restart = new("restart") {
            Description = "Restart OneDrive"
        };

        // Add other commands as subcommands of root command
        RootCmd.Subcommands.Add(Status);
        RootCmd.Subcommands.Add(Where);
        RootCmd.Subcommands.Add(Which);
        RootCmd.Subcommands.Add(Start);
        RootCmd.Subcommands.Add(Stop);
        RootCmd.Subcommands.Add(Restart);

        // Set actions based on command
        Status.SetAction(ParseResult => {
            ODCtl.Status();
        });
        Where.SetAction(ParseResult => {
            ODCtl.Where();
        });
        Which.SetAction(ParseResult => {
            ODCtl.Where();
        });
        Start.SetAction(ParseResult => {
            ODCtl.Start();
        });
        Stop.SetAction(ParseResult => {
            ODCtl.Stop();
        });
        Restart.SetAction(ParseResult => {
            ODCtl.Restart();
        });

        // Override default --version handler and add /version alias
        var VersionOpt = RootCmd.Options.OfType<VersionOption>().Single();
        VersionOpt.Action = new CustomVersion();
        VersionOpt.Description = "Show version and license information";
        VersionOpt.Aliases.Add("/version");

        // Declare options
        Option<bool> DryRunOpt = new("--dry-run", "/dry-run") {
            Description = "Test command functions without actually running them (hidden)",
            Recursive = true,
            Hidden = true
        };
        Option<string> PathOpt = new("--path", "/path") {
            Description = "Set custom OneDrive path (overrides automatic detection)",
            Recursive = true,
            // Customize help display for PathOpt
            HelpName = "PATH"
        };
        Option<bool> QuietOpt = new("--quiet", "/quiet", "-q", "/q") {
            Description = "Suppress optional messages",
            Recursive = true
        };
        Option<bool> VerboseOpt = new("--verbose", "/verbose", "-v", "/v") {
            Description = "Show additional messages",
            Recursive = true
        };

        // Add options to root command
        RootCmd.Options.Add(DryRunOpt);
        RootCmd.Options.Add(PathOpt);
        RootCmd.Options.Add(QuietOpt);
        RootCmd.Options.Add(VerboseOpt);

        // Parse options and arguments
        ParseResult ParseResult = RootCmd.Parse(args);

        // Process selected options, part 1
        if (ParseResult.Errors.Count == 0) {

            // If --dry-run is specified, enable dry run mode
            if (ParseResult.GetValue(DryRunOpt) is true) {
                ODCtl.DryRunMode = true;
            }

            // If --quiet is specified, enable quiet mode
            if (ParseResult.GetValue(QuietOpt) is true) {
                ODCtl.QuietMode = true;
            }

            // If --verbose is specified, enable verbose mode
            if (ParseResult.GetValue(VerboseOpt) is true) {
                ODCtl.VerboseMode = true;
            }

        }

        // Set console output encoding to UTF-8
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        // If verbose mode enabled, write program information
        ODCtl.VerboseMessageLine(ODCtl.ProgramName + " - " + ODCtl.Description);
        ODCtl.VerboseMessageLine("Version " + Application.ProductVersion);
        ODCtl.VerboseMessageLine(ODCtl.Copyright);
        ODCtl.VerboseMessageLine(ODCtl.License);
        ODCtl.VerboseMessageLine();

        // Automatically detect and set OneDrive path (can be overriden with --path)
        ODCtl.SetOneDrivePath();

        // Process selected options, part 2
        if (ParseResult.Errors.Count == 0) {

            // If --path is specified, set custom path
            if (ParseResult.GetValue(PathOpt) is string @CustomPath) {
                ODCtl.SetOneDrivePath(@CustomPath);
            }

        }

        // Invoke specified command
        return RootCmd.Parse(args).Invoke();
    }
}