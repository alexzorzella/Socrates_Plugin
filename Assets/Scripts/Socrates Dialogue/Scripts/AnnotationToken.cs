public class AnnotationToken {
    public string passedValue = "";
    public int startCharIndex = -1;
    public int endCharIndex = -1;
    public bool executedAction = false;
    public AnnotationToken linkedToken;
    public bool opener = true;
    public SocraticAnnotation.RichTextType richTextType;

    public bool ContainsDynamicValue() {
        return !string.IsNullOrEmpty(passedValue);
    }

    public float GetDynamicValueAsFloat() {
        float result = 0;
        result = float.Parse(passedValue);
        return result;
    }
}