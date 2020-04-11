using System;
using System.Collections.Generic;

namespace uberrider.DataModels
{
    public class MatchedSubstring
    {
        public int length { get; set; }
        public int offset { get; set; }
    }

    public class MainTextMatchedSubstring
    {
        public int length { get; set; }
        public int offset { get; set; }
    }

    public class StructuredFormatting
    {
        public string main_text { get; set; }
        public IList<MainTextMatchedSubstring> main_text_matched_substrings { get; set; }
        public string secondary_text { get; set; }
    }

    public class Term
    {
        public int offset { get; set; }
        public string value { get; set; }
    }

    public class Prediction
    {
        public string description { get; set; }
        public string id { get; set; }
        public IList<MatchedSubstring> matched_substrings { get; set; }
        public string place_id { get; set; }
        public string reference { get; set; }
        public StructuredFormatting structured_formatting { get; set; }
        public IList<Term> terms { get; set; }
        public IList<string> types { get; set; }
    }

}
