using System.Collections.Generic;
using System.Linq;

public interface ZAttribute {
       
}

public class ZAEnablesComboDetectors : ZAttribute {
       readonly List<string> enablesDetectors;

       public ZAEnablesComboDetectors(params string[] enablesDetectors) {
              this.enablesDetectors = enablesDetectors.ToList();
       }

       public bool Contains(string checkFor) {
              return enablesDetectors.Contains(checkFor);
       }
}

public class ZADisablesInput : ZAttribute {
    
}