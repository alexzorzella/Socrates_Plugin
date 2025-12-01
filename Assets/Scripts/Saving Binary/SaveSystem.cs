using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public static class SaveSystem {
    static string Path(string specificPath) {
        var path = Application.persistentDataPath + $"/{specificPath}.save";
        return path;
    }

    public static void SavePlayer(GameStats stats) {
        var formatter = new BinaryFormatter();

        FileStream stream;

        stream = new FileStream(Path("savedata"), FileMode.Create);

        var data = new PlayerData(stats);

        formatter.Serialize(stream, data);

        stream.Close();
    }

    public static void DeleteSave(string name) {
        if (File.Exists(Path(name)))
            File.Delete(Path(name));
        else
            Debug.LogError($"Tried to delete file in {Path(name)}.");
    }

    public static bool FileExists(string specificFile) {
        return File.Exists(Path(specificFile));
    }

    public static PlayerData LoadPlayer(string path) {
        if (File.Exists(Path(path))) {
            var formatter = new BinaryFormatter();
            var stream = new FileStream(Path(path), FileMode.Open);

            var data = formatter.Deserialize(stream) as PlayerData;
            stream.Close();

            return data;
        }

        Debug.LogWarning($"Save file not found in {Path(path)}.");
        return null;
    }
}