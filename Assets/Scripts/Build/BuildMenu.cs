using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;

public class BuildManager
{
#if UNITY_EDITOR
    [MenuItem("PongBuild/Build Server")]
    public static void BuildServer()
    {
        BuildPlayerOptions buildOptions = new BuildPlayerOptions
        {
            scenes = new[] { "Assets/Scenes/Game.unity" },
            locationPathName = "Builds/Server/PongServer.exe",
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.None,
            subtarget = (int)StandaloneBuildSubtarget.Server
        };

        // Add server argument
        buildOptions.options |= BuildOptions.Development; // Optional
        PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, "SERVER");

        BuildPipeline.BuildPlayer(buildOptions);
    }

    [MenuItem("PongBuild/Build Client")]
    public static void BuildClient()
    {
        BuildPlayerOptions buildOptions = new BuildPlayerOptions
        {
            scenes = new[] { "Assets/Scenes/MainMenu.unity", "Assets/Scenes/Game.unity" },
            locationPathName = "Builds/Client/PongClient.exe",
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.None,
            subtarget = (int)StandaloneBuildSubtarget.Player
        };
        PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
        // Add client argument
        PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, "CLIENT");
        //EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64);
        BuildPipeline.BuildPlayer(buildOptions);
    }
    
    [MenuItem("PongBuild/Launch Server")]
    public static void LaunchServer()
    {
        // Correct the path to be outside of the Assets folder
        string serverPath = Path.Combine(Application.dataPath, "../Builds/Server/PongServer.exe");
        
        // Normalize the path
        serverPath = Path.GetFullPath(serverPath);

        // Check if the file exists
        if (System.IO.File.Exists(serverPath))
        {
            try
            {
                UnityEngine.Debug.Log("Launching server... fileName: " + serverPath);
                string timestamp = System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
                Process.Start(new ProcessStartInfo
                {
                    FileName = serverPath,
                    Arguments = $"-batchmode -nographics -logFile ./Logs/{timestamp}_server_log.txt",
                    RedirectStandardOutput = false,
                    RedirectStandardError = false,
                    UseShellExecute = true,
                    CreateNoWindow = false 
                });
                UnityEngine.Debug.Log("Server launched.");
            }
            catch (System.Exception ex)
            {
                UnityEngine.Debug.LogError("Failed to launch server: " + ex.Message);
            }
        }
        else
        {
            UnityEngine.Debug.LogError($"Server executable not found at: {serverPath}");
        }
    }
    
    [MenuItem("PongBuild/Launch Client")]
    public static void LaunchClient()
    {
        string clientPath = Path.Combine(Application.dataPath, "../Builds/Client/PongClient.exe");
        clientPath = Path.GetFullPath(clientPath);

        if (System.IO.File.Exists(clientPath))
        {
            string timestamp = System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            var startInfo = new ProcessStartInfo
            {
                FileName = clientPath,
                Arguments = "-popupwindow",
                UseShellExecute = true,
                CreateNoWindow = false,
                RedirectStandardError = false,
                RedirectStandardOutput = false
            };

            Process.Start(startInfo);
            UnityEngine.Debug.Log("Client launched.");
        }
        else
        {
            UnityEngine.Debug.LogError($"Client executable not found at: {clientPath}");
        }
    }
#endif
}