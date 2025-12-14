using System;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

public class BuildVersionProcessor : IPreprocessBuildWithReport {
  const string DEFAULT_VERSION = "24.11.9";

  // Keep in sync with VersionShower.cs
  static readonly Regex VERSION_EXTRACTOR_REGEXP = new(@"((\d{2}.\d{2}).(\d+))(?:-(\w{4}))?");

  public int callbackOrder => 0;

  public void OnPreprocessBuild(BuildReport report) {
    #if UNITY_EDITOR_OSX
      return;
    #endif
    
    #if UNITY_STANDALONE_LINUX
      return;
    #endif
    
    var isGitClean = IsGitClean();
    var truncatedSha = LastFilteredTruncatedGitSha();

    var newVersion = NewVersion(PlayerSettings.bundleVersion, DateTime.Now, truncatedSha, isGitClean);
    if (!newVersion.Equals(PlayerSettings.bundleVersion)) {
      PlayerSettings.bundleVersion = newVersion;
      // PlayerSettings.Android.bundleVersionCode = PlayerSettings.Android.bundleVersionCode + 1;
    }
  }

  public static String NewVersion(
      string currentVersion,
      DateTime now) {
    return NewVersion(currentVersion, now, "", false);
  }

  /**
   * Returns a new version string to use in the application based on
   * the `currentVersion` and the what time it is right `now`.
   *
   * The version is in the form `vYYYY.MM-deduper`. The deduper is
   * "0" unless the (old) current version has the same year and month
   * as the one we are trying to generate.
   */
  public static String NewVersion(
      string lastVersion,
      DateTime now,
      string? truncatedSha,
      bool isGitClean
      ) {

    // This gets printed to ~/Library/Logs/Unity/Editor.log
    Debug.Log($"Last version: '{lastVersion}'. Client's truncated sha: {truncatedSha} -- is clean: {isGitClean}");
    var nowString = now.ToString("yy.MM");

    var lastVersionMatch = VERSION_EXTRACTOR_REGEXP.Match(lastVersion);
    if (lastVersionMatch.Success) {
      if (int.TryParse(lastVersionMatch.Groups[3].Value, out int currentDeduper)) {

        var lastVersionSha = lastVersionMatch.Groups[4].Value;
        if (isGitClean && lastVersionSha.Equals(truncatedSha)) {
          return lastVersion;
        }

        var newDeduper = lastVersionMatch.Groups[2].Value.Equals(nowString)
          ? currentDeduper + 1
          : 0;
        var maybeDashTruncatedSha = isGitClean
          ? $"-{truncatedSha}"
          : "";
        return $"{nowString}.{newDeduper}{maybeDashTruncatedSha}";
      };
    }
    return DEFAULT_VERSION;
  }

  public record CommandOutput(string stdOut, string stdErr) {
    public string stdOut { get; } = stdOut;
    public string stdErr { get; } = stdErr;
  }

  // count the commits from this month
  // git rev-list --count --since="$(date +%Y-%m-01)" HEAD

  public static string? LastFilteredTruncatedGitSha() {

    var gitLog = RunGitCommand(
      // This does not actually work -- grep operates on the commit message, not the content
      // @"log -1 --format='%H' --no-merges "+
      // @"--invert-grep --grep='^\s*bundleVersion:\s[a-zA-Z0-9.]+$' " +
      // @"--invert-grep --grep='^\s*AndroidBundleVersionCode:\s*[0-9]+$'");

      // Fow now, at least, let's just exclude any commit that only touched
      // the `ProjectSettings.asset`. This is far from perfect...
      @"log -1 --format='%H' --no-merges -- ':(exclude)ProjectSettings/ProjectSettings.asset'");

    if (gitLog.stdErr?.Length > 0) {
      Debug.LogWarning($"git command error: {gitLog.stdErr}");
    }

    if (string.IsNullOrEmpty(gitLog.stdOut)) {
      return null;
    }

    return gitLog.stdOut.Trim().Substring(0, 4);
  }

  public static bool IsGitClean() {

    var gitStatus = RunGitCommand("status --porcelain");

    if (gitStatus.stdErr?.Length > 0) {
      Debug.LogWarning($"git command error: {gitStatus.stdErr}");
    }

    if (string.IsNullOrEmpty(gitStatus.stdOut)) {
      return true;
    }

    return false;
  }

  public static CommandOutput RunGitCommand(string gitCommand) {
    return RunCommand("C:\\Program Files\\Git\\bin\\git", gitCommand);
  }
  
  public static CommandOutput RunCommand(string command, string args) {
    System.Diagnostics.Process process = new System.Diagnostics.Process();
    // process.StartInfo.FileName = "/bin/bash"; // Use "cmd.exe" on Windows
    // process.StartInfo.Arguments = $"-c \"{command}\""; // Use "/C" on Windows
    process.StartInfo.FileName = command;
    process.StartInfo.Arguments = args;//"status --porcelain"; // Use "/C" on Windows
    // process.StartInfo.Arguments = "status"; // Use "/C" on Windows
    process.StartInfo.RedirectStandardOutput = true;
    process.StartInfo.RedirectStandardError = true;
    process.StartInfo.UseShellExecute = false;
    process.StartInfo.CreateNoWindow = true;

    // git log -1 --format="%H" --no-merges --invert-grep --grep='^\s*bundleVersion:\s[a-zA-Z0-9.]+$'

    process.Start();
    string output = process.StandardOutput.ReadToEnd();
    string error = process.StandardError.ReadToEnd();
    process.WaitForExit();

    return new CommandOutput(output, error);

    // Debug.Log("Output: " + output);
    // if (!string.IsNullOrEmpty(error)) {
    //   Debug.LogError("Error: " + error);
    // }
  }
}
