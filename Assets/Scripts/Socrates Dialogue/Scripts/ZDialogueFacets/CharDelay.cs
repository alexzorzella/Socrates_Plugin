namespace SocratesDialogue {
    public class CharDelay : ZDialogueFacet {
        readonly float delay;

        public CharDelay(float delay) {
            this.delay = delay;
        }

        public CharDelay(string delayUncast) {
            float.TryParse(delayUncast, out delay);
        }

        public float GetDelay() {
            return delay;
        }
        
        public ZDialogueFacet Clone() {
            return new CharDelay(delay);
        }
    }
}