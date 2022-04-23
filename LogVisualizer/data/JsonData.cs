using System.Collections.Generic;

namespace LogVisualizer {
    public class JsonData
    {
        public class AnalysisData
        {
            public List<Filters> filters { get; set; }
        }

        public class Filters
        {
            public string name { get; set; }
            public string start { get; set; }
            public string end { get; set; }
        }
    }
}
