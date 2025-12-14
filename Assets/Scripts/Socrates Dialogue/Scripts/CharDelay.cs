namespace SocratesDialogue {
    public class CharDelay : ZFacet {
        readonly float delay;

        public CharDelay(float delay) {
            this.delay = delay;
        }

        public float GetDelay() {
            return delay;
        }
        
        public ZFacet Clone() {
            return new CharDelay(delay);
        }
    }
}