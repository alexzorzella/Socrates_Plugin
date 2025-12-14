using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public static class SaveSystem {
    /// <summary>
    /// Returns a persistent data path given the filename and subfolder.
    /// </summary>
    /// <param name="filename"></param>
    /// <param name="subfolder"></param>
    /// <returns></returns>
    static string Path(string filename, string subfolder = "saves") {
        var path = Application.persistentDataPath + $"/{subfolder}/{filename}";
        return path;
    }
    
    /// <summary>
    /// Returns whether a file with the passed name exists.
    /// </summary>
    /// <param name="filename"></param>
    /// <returns></returns>
    public static bool FileExists(string filename) {
        return File.Exists(Path(filename));
    }
    
    /// <summary>
    /// Saves the passed SaveData to the passed filename, overwriting the file if specified.
    /// </summary>
    /// <param name="stats"></param>
    /// <param name="filename"></param>
    /// <param name="overwrite"></param>
    public static void Save(GameStats stats, string filename = "save.txt", bool overwrite = true) {
        var formatter = new BinaryFormatter();

        if (overwrite && FileExists(filename)) {
            return;
        }

        FileStream stream = new FileStream(Path(filename), FileMode.Create);

        var data = new SaveData(stats);

        formatter.Serialize(stream, data);

        stream.Close();
    }
    
    /// <summary>
    /// Returns the data saved in the file with the passed filename.
    /// </summary>
    /// <param name="filename"></param>
    /// <returns></returns>
    public static SaveData LoadSave(string filename) {
        string path = Path(filename);
        
        if (FileExists(filename)) {
            var formatter = new BinaryFormatter();
            var stream = new FileStream(path, FileMode.Open);

            var data = formatter.Deserialize(stream) as SaveData;
            stream.Close();

            return data;
        }

        Debug.LogWarning($"Load failed: no save file not found at {path}.");
        return null;
    }
    
    public static void DeleteSave(string filename) {
        string path = Path(filename);
        
        if (File.Exists(path)) {
            File.Delete(path);
        } else {
            Debug.LogError($"Delete failed: no save file found at {path}.");
        }
    }
}