using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public static class SaveSystem {
    public static void SavePlayer(GameStats stats) {
        var formatter = new BinaryFormatter();

        FileStream stream;

        stream = new FileStream(Path("savedata"), FileMode.Create);

        var data = new PlayerData(stats);

        formatter.Serialize(stream, data);

        stream.Close();
    }

    static string Path(string specificPath) {
        var path = Application.persistentDataPath + $"/{specificPath}.save";
        return path;
    }
    
    static bool FileExists(string filename) {
        return File.Exists(Path(filename));
    }

    public static PlayerData LoadSave(string filename) {
        if (FileExists(filename)) {
            var formatter = new BinaryFormatter();
            var stream = new FileStream(Path(filename), FileMode.Open);

            var data = formatter.Deserialize(stream) as PlayerData;
            stream.Close();

            return data;
        }

        Debug.LogWarning($"Save file not found in {Path(filename)}.");
        return null;
    }
    
    public static void DeleteSave(string filename) {
        if (File.Exists(Path(filename))) {
            File.Delete(Path(filename));
        } else {
            Debug.LogError($"Tried to delete file in {Path(filename)}.");
        }
    }
}