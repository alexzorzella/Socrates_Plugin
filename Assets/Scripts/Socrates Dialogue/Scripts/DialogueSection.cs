using System.Collections.Generic;
using System.Linq;

namespace NewSocratesDialogue {
    public interface ZFacet { }
}

namespace NewSocratesDialogue {
    public class DialogueSection {
        readonly string speakerName;
        readonly string content;
        readonly string sound;
    
        public DialogueSection(
            string speakerName, 
            string content, 
            string sound, 
            params ZFacet[] facets) : 
            this(
                speakerName, 
                content, 
                sound, 
                facets.ToList()) { }
    
        DialogueSection(
            string speakerName, 
            string content, 
            string sound, 
            List<ZFacet> facets) {
            this.speakerName = speakerName;
            this.content = content;
            this.sound = sound;
            this.facets = facets;
        }
    
        readonly List<ZFacet> facets;
    
        public void AddFacet(ZFacet facet) {
            facets.Add(facet);
        }
    
        public bool HasFacet<T>() where T : ZFacet {
            return GetFacet<T>() != null;
        }
    
        public T GetFacet<T>() where T : ZFacet {
            foreach (var facet in facets) {
                if (typeof(T).IsInstanceOfType(facet)) {
                    return (T)facet;
                }
            }
    
            return default;
        }
        
        public string GetSpeaker() {
            return speakerName;
        }
    
        public string GetContent() {
            return content;
        }
    
        public string GetSound() {
            return sound;
        }
    }
}