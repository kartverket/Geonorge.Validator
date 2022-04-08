using System.Collections.Generic;

namespace Geonorge.Validator.Application.Models.Data.Codelist
{
    public class GmlCodeSpace
    {
        public string FeatureMemberName { get; private set; }
        public List<CodeSpace> CodeSpaces { get; } = new();

        public GmlCodeSpace(string featureMemberName)
        {
            FeatureMemberName = featureMemberName;
        }
    }
}
