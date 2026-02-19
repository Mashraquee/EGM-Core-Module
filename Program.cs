using System;
using System.IO;
using System.Text.Json;
using System.Diagnostics;
namespace Implementation
{
    internal class Program
    {
        static string GameRoot = Path.Combine(Directory.GetCurrentDirectory(), "GameRoot");
        static string VersionsDir = Path.Combine(GameRoot, "versions");
        static string CurrentVersionFile = Path.Combine(GameRoot, "current.txt");
        static string LastKnownGoodFile = Path.Combine(GameRoot, "last_known_good.txt");
        static string InstallHistoryLog = Path.Combine(GameRoot, "install_history.log");
        static void Main(string[] args)
        {
            if (args.Length < 2 || args[0] != "update" || args[1] != "--package")
            {
                Console.WriteLine("Usage: update --package <path>");
                return;
            }

            string packagePath = args[2];

            try
            {
                UpdateGame(packagePath);
            }
            catch (Exception ex)
            {
                Log($"FATAL ERROR: {ex.Message}");
            }
        }

        static void UpdateGame(string packagePath)
        {
            Log("----- Starting Update Process -----");

            if (!Directory.Exists(packagePath))
                throw new Exception("Update package not found.");

            string manifestPath = Path.Combine(packagePath, "manifest.json");

            if (!File.Exists(manifestPath))
                throw new Exception("Manifest file missing.");

            var manifestJson = File.ReadAllText(manifestPath);
            var manifest = JsonSerializer.Deserialize<Manifest>(manifestJson);

            if (string.IsNullOrWhiteSpace(manifest.Version))
                throw new Exception("Invalid manifest: version missing.");

            Log($"Validated package for version: {manifest.Version}");

            string newVersionPath = Path.Combine(VersionsDir, manifest.Version);

            // Run Pre-Install Script
            if (!RunPreInstall(packagePath))
            {
                Log("Pre-install failed. Rolling back...");
                Rollback();
                return;
            }

            // Copy files
            string sourceFiles = Path.Combine(packagePath, "files");
            if (!Directory.Exists(sourceFiles))
                throw new Exception("Package files directory missing.");

            if (Directory.Exists(newVersionPath))
                Directory.Delete(newVersionPath, true);

            CopyDirectory(sourceFiles, newVersionPath);
            Log($"Copied files to {newVersionPath}");

            // Update version pointers
            string currentVersion = GetCurrentVersion();

            File.WriteAllText(CurrentVersionFile, manifest.Version);
            File.WriteAllText(LastKnownGoodFile, manifest.Version);

            Log($"Update successful. Active version: {manifest.Version}");
            Log("----- Update Complete -----\n");
        }

        static bool RunPreInstall(string packagePath)
        {
            try
            {
                string scriptFile = Path.Combine(packagePath, "preinstall.txt");

                if (!File.Exists(scriptFile))
                {
                    Log("No pre-install script found. Skipping.");
                    return true;
                }

                string scriptCommand = File.ReadAllText(scriptFile);

                Log($"Running pre-install: {scriptCommand}");

                Process process = new Process();
                process.StartInfo.FileName = "cmd.exe";
                process.StartInfo.Arguments = $"/c {scriptCommand}";
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.UseShellExecute = false;
                process.Start();

                process.WaitForExit();

                Log($"Pre-install exit code: {process.ExitCode}");

                return process.ExitCode == 0;
            }
            catch (Exception ex)
            {
                Log($"Pre-install exception: {ex.Message}");
                return false;
            }
        }

        static void Rollback()
        {
            string lastGood = GetLastKnownGoodVersion();

            if (string.IsNullOrEmpty(lastGood))
            {
                Log("No last known good version found. Cannot rollback.");
                return;
            }

            File.WriteAllText(CurrentVersionFile, lastGood);
            Log($"Rolled back to version: {lastGood}");
            Log("----- Rollback Complete -----\n");
        }

        static string GetCurrentVersion()
        {
            if (!File.Exists(CurrentVersionFile))
                return null;

            return File.ReadAllText(CurrentVersionFile).Trim();
        }

        static string GetLastKnownGoodVersion()
        {
            if (!File.Exists(LastKnownGoodFile))
                return null;

            return File.ReadAllText(LastKnownGoodFile).Trim();
        }

        static void Log(string message)
        {
            string logEntry = $"[{DateTime.Now}] {message}";
            Console.WriteLine(logEntry);
            Directory.CreateDirectory(GameRoot);
            File.AppendAllText(InstallHistoryLog, logEntry + Environment.NewLine);
        }

        static void CopyDirectory(string sourceDir, string destDir)
        {
            Directory.CreateDirectory(destDir);

            foreach (string file in Directory.GetFiles(sourceDir))
            {
                string destFile = Path.Combine(destDir, Path.GetFileName(file));
                File.Copy(file, destFile, true);
            }

            foreach (string directory in Directory.GetDirectories(sourceDir))
            {
                string destSubDir = Path.Combine(destDir, Path.GetFileName(directory));
                CopyDirectory(directory, destSubDir);
            }
        }
    }
}
class Manifest
{
    public string Version { get; set; }
}
