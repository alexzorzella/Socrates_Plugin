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

        /// <summary>
        /// Shifts the start and end character indices back by the passed value.
        /// </summary>
        /// <param name="by"></param>
        public void ShiftCharIndices(int by) {
            startCharIndex -= by;
            endCharIndex -= by;
        }

        /// <summary>
        /// Links this token to the passed token.
        /// </summary>
        /// <param name="linkedToken"></param>
        public void LinkToken(AnnotationToken linkedToken) {
            this.linkedToken = linkedToken;
        }
        
        /// <summary>
        /// Returns whether the annotation has a dynamic value cached.
        /// </summary>
        /// <returns></returns>
        public bool ContainsDynamicValue() {
            return !string.IsNullOrEmpty(passedValue);
        }

        /// <summary>
        /// Returns the dynamic value as a float.
        /// </summary>
        /// <returns></returns>
        public float GetDynamicValueAsFloat() {
            float result = 0;
            result = float.Parse(passedValue);
            return result;
        }
        
        public string GetPassedValue() { return passedValue; }
        public int GetStartCharIndex() { return startCharIndex; }
        public int GetEndCharIndex() { return endCharIndex; }
        
        public AnnotationToken GetLinkedToken() { return linkedToken; }
        public bool IsOpener() { return isOpener; }
        public SocraticAnnotation.RichTextType GetRichTextType() { return richTextType; }

        public bool HasExecutedAction() { return hasExecutedAction; }
        public void ExecuteAction() { hasExecutedAction = true; }

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