using System;

[Serializable]
public class SaveData {
    readonly float sfxVolume;
    readonly float musicVolume;

    /// <summary>
    /// Returns the data in SaveData as a GameStats object. GameStats may
    /// contain more data than SaveData, but it's usually safe to store the object
    /// on initialization as the other fields will be populated later.
    /// </summary>
    /// <returns></returns>
    public GameStats AsStats() {
        GameStats result = new();

        result.sfxVolume = sfxVolume;
        result.musicVolume = musicVolume;
        
        return result;
    }
    
    /// <summary>
    /// While saving, the GameStats are passed so that its data can be copied
    /// into something serializable. SaveData is the object that the save
    /// system serializes into a binary file.
    /// </summary>
    /// <param name="stats"></param>
    public SaveData(GameStats stats) {
        sfxVolume = stats.sfxVolume;
        musicVolume = stats.musicVolume;
    }
}