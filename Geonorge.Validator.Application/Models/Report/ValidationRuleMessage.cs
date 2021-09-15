using System.Collections.Generic;

namespace Geonorge.Validator.Application.Models.Report
{
    public class ValidationRuleMessage
    {
        public string Message { get; set; }
        public string FileName { get; set; }
        public IEnumerable<string> XPaths { get; set; }
        public IEnumerable<string> GmlIds { get; set; }

        public ValidationRuleMessage(string message, string fileName, IEnumerable<string> xPaths, IEnumerable<string> gmlIds)
        {
            Message = message;
            FileName = fileName;
            XPaths = xPaths;
            GmlIds = gmlIds;
        }
    }
}
