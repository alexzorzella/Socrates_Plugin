using System.Collections.Generic;
using System.Linq;

public interface ZFacet {
    public ZFacet Clone();
}

namespace NewSocratesDialogue {
    public class NextSection : ZFacet {
    }

    public class CharSound : ZFacet {
        readonly string soundName;
        readonly bool monotone;
    }
}

public class NewDialogueSection {
    readonly string speakerName;
    readonly string content;

    public NewDialogueSection(params ZFacet[] facets) : this(facets.ToList()) { }

    public NewDialogueSection(List<ZFacet> facets) {
        this.facets = facets;
    }

    List<ZFacet> facets;

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

    public NewDialogueSection Clone() {
        List<ZFacet> facetsCopy = new List<ZFacet>();

        foreach (var facet in facets) {
            facetsCopy.Add(facet);
        }

        NewDialogueSection clone = new NewDialogueSection(facetsCopy);

        return clone;
    }
}