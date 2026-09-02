using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// A self-instantiating singleton object that stores the gradients used by the dialogue system.
/// To add more gradients to the collection of available gradients, make a publicly facing gradient
/// and add it in Initialize (or make one in the Initialize function itself, but in this instance
/// the Unity editor provides a window that makes it easy to edit the gradient.)
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
    /// Returns the gradient listed in the dictionary with the passed name.
    /// If the passed name has no gradient listed, the first gradient in the
    /// dictionary is returned. If there are no gradients in the dictionary,
    /// the function returns null.
    /// </summary>
    /// <param name="gradientName"></param>
    /// <returns></returns>
    public Gradient GetGradient(string gradientName) {
        if (gradients.Count <= 0) { return null; }
        if (gradients.ContainsKey(gradientName)) { return gradients[gradientName]; }
        
        return gradients.First().Value;
    }
}