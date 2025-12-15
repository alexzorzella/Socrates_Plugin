using System.Collections.Generic;
using System.Linq;

namespace SocratesDialogue {
    public interface ZDialogueFacet { }
}

namespace SocratesDialogue {
    public class DialogueSection {
        readonly List<ZDialogueFacet> facets;
        
        public DialogueSection(params ZDialogueFacet[] facets) : 
            this(facets.ToList()) { }
    
        public DialogueSection(List<ZDialogueFacet> facets) {
            this.facets = facets;
        }
    
    
        public void AddFacet(ZDialogueFacet dialogueFacet) {
            facets.Add(dialogueFacet);
        }
    
        /// <summary>
        /// Returns whether the dialogue contains a facet of type T.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public bool HasFacet<T>() where T : ZDialogueFacet {
            return GetFacet<T>() != null;
        }
        
        /// <summary>
        /// Returns the first instance of type T it finds in the list of facets and
        /// null if it doesn't find anything.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetFacet<T>() where T : ZDialogueFacet {
            foreach (var facet in facets) {
                if (typeof(T).IsInstanceOfType(facet)) {
                    return (T)facet;
                }
            }
    
            return default;
        }
    }
}