using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public static class SaveSystem
{
    private static string Path(string specificPath)
    {
        string path = Application.persistentDataPath + $"/{specificPath}.save";
        return path;
    }

    public static void SavePlayer(GameStats stats)
    {
        BinaryFormatter formatter = new BinaryFormatter();

        FileStream stream;

        stream = new FileStream(Path("savedata"), FileMode.Create);

        PlayerData data = new PlayerData(stats);

        formatter.Serialize(stream, data);

        stream.Close();
    }

    public static void DeleteSave(string name)
    {
        if (File.Exists(Path(name)))
        {
            File.Delete(Path(name));
        }
        else
        {
            Debug.LogError($"Tried to delete file in {Path(name)}.");
        }
    }

    public static bool FileExists(string specificFile)
    {
        return File.Exists(Path(specificFile));
    }

    public static PlayerData LoadPlayer(string path)
    {
        if (File.Exists(path))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(Path(path), FileMode.Open);

            PlayerData data = formatter.Deserialize(stream) as PlayerData;
            stream.Close();

            return data;
        }
        else
        {
            Debug.LogError($"Save file not found in {Path(path)}.");
            return null;
        }
    }
}