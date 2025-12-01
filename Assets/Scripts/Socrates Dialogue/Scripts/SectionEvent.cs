using System;

namespace NewSocratesDialogue {
    public class SectionEvent : ZFacet {
        readonly Action action;
        readonly bool onStart;
        bool hasTriggered;

        public SectionEvent(Action action, bool onStart, bool hasTriggered = false) {
            this.action = action;
            this.onStart = onStart;
            this.hasTriggered = hasTriggered;
        }

        public void Trigger() {
            action.Invoke();
            hasTriggered = true;
        }
        
        public bool TriggersOnStart() {
            return onStart;
        }
        
        public bool HasTriggered() {
            return hasTriggered;
        }

        public ZFacet Clone() {
            return new SectionEvent((Action)action.Clone(), onStart, hasTriggered);
        }
    }
}