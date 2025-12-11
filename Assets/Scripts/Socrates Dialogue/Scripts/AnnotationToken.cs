namespace SocratesDialogue {
    public class AnnotationToken {
        readonly string passedValue;
        readonly SocraticAnnotation.RichTextType richTextType;
        readonly bool isOpener;

        int startCharIndex;
        int endCharIndex;
        AnnotationToken linkedToken;
        bool hasExecutedAction;
        
        AnnotationToken(
            string passedValue,
            int startCharIndex,
            int endCharIndex,
            SocraticAnnotation.RichTextType richTextType,
            bool isOpener) {
            this.passedValue = passedValue;
            this.startCharIndex = startCharIndex;
            this.endCharIndex = endCharIndex;
            this.richTextType = richTextType;
            this.isOpener = isOpener;
        }

        public string GetPassedValue() { return passedValue; }
        public int GetStartCharIndex() { return startCharIndex; }

        public void ShiftCharIndices(int by) {
            startCharIndex -= by;
            endCharIndex -= by;
        }
        
        public int GetEndCharIndex() { return endCharIndex; }

        public void LinkToken(AnnotationToken linkedToken) { this.linkedToken = linkedToken; }
        
        public AnnotationToken GetLinkedToken() { return linkedToken; }
        public bool IsOpener() { return isOpener; }
        public SocraticAnnotation.RichTextType GetRichTextType() { return richTextType; }

        public bool HasExecutedAction() { return hasExecutedAction; }
        public void ExecuteAction() { hasExecutedAction = true; }
        
        public bool ContainsDynamicValue() {
            return !string.IsNullOrEmpty(passedValue);
        }

        public float GetDynamicValueAsFloat() {
            float result = 0;
            result = float.Parse(passedValue);
            return result;
        }

        public class Builder {
            string passedValue = "";
            int startCharIndex = -1;
            int endCharIndex = -1;
            SocraticAnnotation.RichTextType richTextType;
            bool isOpener = true;

            public Builder WithPassedValue(string passedValue) {
                this.passedValue = passedValue;
                return this;
            }

            public Builder WithStartCharIndex(int startCharIndex) {
                this.startCharIndex = startCharIndex;
                return this;
            }

            public Builder WithEndCharIndex(int endCharIndex) {
                this.endCharIndex = endCharIndex;
                return this;
            }
            
            public Builder IsCloser() {
                isOpener = false;
                return this;
            }

            public Builder WithRichTextType(SocraticAnnotation.RichTextType richTextType) {
                this.richTextType = richTextType;
                return this;
            }

            public AnnotationToken Build() {
                return new AnnotationToken(passedValue, startCharIndex, endCharIndex, richTextType, isOpener);
            }
        }
    }
}