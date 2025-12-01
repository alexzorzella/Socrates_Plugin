using System;

public class Condition {
    Predicate<object> condition;

    public Condition(Predicate<object> condition) {
        this.condition = condition;
    }
    
    public bool Met() {
        return condition.Invoke(null);
    }
}