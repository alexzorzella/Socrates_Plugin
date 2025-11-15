using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class HcMixerVolume : HCommand {
    public List<string> AutocompleteOptions() {
        return new List<string>();
    }

    public string CommandFunction(params string[] parameters) {
        var mixerGroup = Resources.Load<AudioMixerGroup>(parameters[1]);

        if (mixerGroup != null) {
            float vol;
            mixerGroup.audioMixer.GetFloat("Volume", out vol);

            return $"{vol}";
        }

        return $"There is no Audio Mixer Group named {parameters[1]}";
    }

    public string CommandHelp() {
        return "(string mixerName)";
    }

    public string Keyword() {
        return "mixerInfo";
    }
}