using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

public static class AlexLang {
	static int selectedLanguage;

	static readonly List<LangListener> listeners = new();

	public static void RegisterListener(LangListener listener) {
		listeners.Add(listener);
		listener.OnLangChanged();
	}

	static void OnLangChanged() {
		foreach (LangListener listener in listeners) {
			listener.OnLangChanged();
		}
	}

	public static string GetSelectedLanguage() {
		return languages[selectedLanguage];
	}

	public static void SetSelectedLanguage(int selectedLanguage) {
		AlexLang.selectedLanguage = selectedLanguage;
		OnLangChanged();
	}

	public static void PlusPlus() {
		selectedLanguage = IncrementWithOverflow.Run(selectedLanguage, languages.Count, 1);
	}

	public static void MinusMinus() {
		selectedLanguage = IncrementWithOverflow.Run(selectedLanguage, languages.Count, -1);
	}

	const char seperator = '§';

	static readonly List<string> languages = new();
	static readonly Dictionary<string, List<string>> dictionary = new();

	public static void ParseFile() {
		string contents = File.ReadAllText(Path.Combine(Application.streamingAssetsPath, "Localization", "lang.tsv"));

		string[] lines = contents.Split('\n');

		for (int i = 0; i < lines.Length; i++) {
			string line = lines[i];
			string[] entries = line.Split('\t');
			string tokenId = entries[0];

			List<string> translations = entries.ToList();
			translations.RemoveAt(0);

			if (languages.Count > 0) {
				if (dictionary.ContainsKey(tokenId)) {
					throw new ArgumentException($"Duplicate tokenId '{tokenId}'");
				}

				dictionary.Add(tokenId, translations);
			} else {
				languages.AddRange(translations);
			}
		}
	}

	public static string ForToken(string tokenId) {
		if (dictionary.TryGetValue(tokenId, out var value)) {
			return value[selectedLanguage];
		}

		Debug.LogError($"Token {tokenId} not found.");

		return "tokenId_not_found";
	}

	public static List<string> ListForToken(string tokenId) {
		if (dictionary.TryGetValue(tokenId, out var value)) {
			return value[selectedLanguage].Split(seperator).ToList();
		}

		return new List<string>();
	}
}