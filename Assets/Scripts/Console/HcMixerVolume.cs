using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class HcMixerVolume : HCommand {
    public List<string> AutocompleteOptions() {
        return new List<string>();
    }

    public string CommandFunction(params string[] parameters) {
        if (parameters.Length < 2) {
            return "Please specify the name of an audio mixer group";
        }

        string mixerName = parameters[1];
        
        var mixerGroup = Resources.Load<AudioMixerGroup>(mixerName);

        if (mixerGroup != null) {
            float vol;
            mixerGroup.audioMixer.GetFloat("Volume", out vol);

            return $"{mixerName} vol: {vol}";
        }

        return $"There is no audio mixer group group named {mixerName}";
    }

    public string CommandHelp() {
        return "(string mixerName)";
    }

    public string Keyword() {
        return "mixerInfo";
    }
}