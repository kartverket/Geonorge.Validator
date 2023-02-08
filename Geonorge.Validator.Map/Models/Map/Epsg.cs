namespace Geonorge.Validator.Map.Models.Map
{
    public class Epsg
    {
        public Epsg(int code)
        {
            Code = code;
        }

        public int Code { get; private set; }
        public string CodeString => Code != 0 ? $"EPSG:{Code}" : null;
    }
}
