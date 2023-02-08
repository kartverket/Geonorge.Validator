using Geonorge.Validator.Map.Models.Config.Map;
using Geonorge.Validator.Map.Models.Map;
using Microsoft.Extensions.Options;

namespace Geonorge.Validator.Map.Services
{
    public class MapSettingsService : IMapSettingsService
    {
        private readonly MapSettings _settings;

        public MapSettingsService
            (IOptions<MapSettings> options)
        {
            _settings = options.Value;
        }

        public MapSettings GetMapSettings()
        {
            _settings.Projections = GetProjections();

            return _settings;
        }

        private List<Projection> GetProjections()
        {
            var projections = new List<Projection>();

            foreach (var code in _settings.SupportedEpsgCodes)
            {
                var projection = Projection.Create(code);

                if (projection != null)
                    projections.Add(projection);
            }

            return projections;
        }
    }
}
