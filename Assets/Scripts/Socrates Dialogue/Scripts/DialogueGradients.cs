using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// A self-instantiating singleton object that stores the gradients used
/// by the dialogue system. In the future, multiple gradients will be
/// supported.
/// </summary>
public class DialogueGradients : MonoBehaviour {
    static DialogueGradients _i;

    public Gradient rainbow;

    static readonly Dictionary<string, Gradient> gradients = new();
    
    void Start() {
        DontDestroyOnLoad(gameObject);
    }
	
    public static DialogueGradients i {
        get {
            if (_i == null) {
                DialogueGradients x = Resources.Load<DialogueGradients>("DialogueGradients");

                _i = Instantiate(x);
                _i.Initialize();
            }
            return _i;
        }
    }

    void Initialize() {
        gradients.Add("rainbow", rainbow);
    }
    
    /// <summary>
    /// Returns the graident listed in the dictionary with the passed name.
    /// If the passed name has no gradient listed, the first gradient in the
    /// dicitonary is returned. If there are no gradients in the dictionary,
    /// the funciton returns null;
    /// </summary>
    /// <param name="gradientName"></param>
    /// <returns></returns>
    public Gradient GetGradient(string gradientName) {
        if (gradients.Count <= 0) { return null; }
        if (gradients.ContainsKey(gradientName)) { return gradients[gradientName]; }
        
        return gradients.First().Value;
    }
}