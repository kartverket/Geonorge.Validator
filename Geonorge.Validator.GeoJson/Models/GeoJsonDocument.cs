using DiBK.RuleValidator.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using static Geonorge.Validator.GeoJson.Helpers.GeoJsonHelper;

namespace Geonorge.Validator.GeoJson.Models
{
    public class GeoJsonDocument : IDisposable
    {
        private bool _disposed = false;
        private readonly ConcurrentDictionary<JToken, IndexedGeoJsonGeometry> _geometries = new();
        private ILookup<string, IndexedGeoJsonGeometry> _geometriesByType;

        public string Id { get; } = Guid.NewGuid().ToString();
        public JToken Document { get; private set; }
        public string FileName { get; private set; }
        public ReadOnlyCollection<JToken> Features { get; private set; }

        private GeoJsonDocument(JToken document, string fileName)
        {
            Document = document;
            FileName = fileName;
            Initialize();
        }

        public List<IndexedGeoJsonGeometry> GetGeometriesByType(params string[] geometryNames)
        {
            var geometries = new List<IndexedGeoJsonGeometry>();

            foreach (var name in geometryNames)
                geometries.AddRange(_geometriesByType[name]);

            return geometries;
        }

        public IndexedGeoJsonGeometry GetOrCreateGeometry(JToken feature)
        {
            if (_geometries.TryGetValue(feature, out var indexed))
                return indexed;

            var newIndexed = IndexedGeoJsonGeometry.Create(feature);

            if (newIndexed == null)
                return null;

            _geometries.TryAdd(feature, newIndexed);

            return newIndexed;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    foreach (var (_, geometry) in _geometries)
                        geometry.Dispose();
                }

                _disposed = true;
            }
        }

        private void Initialize()
        {
            var tokens = GetAllTokens(Document);

            var features = tokens
                .Where(token => token.Type == JTokenType.Property &&
                    ((JProperty)token).Name == "type" && ((JProperty)token).Value.Value<string>() == "Feature")
                .Select(token => (JToken)token.Parent)
                .ToList();

            Features = new ReadOnlyCollection<JToken>(features);

            Parallel.ForEach(
                Features,
                new ParallelOptions { MaxDegreeOfParallelism = GetMaxDegreeOfParallelism() },
                token => _geometries.TryAdd(token, IndexedGeoJsonGeometry.Create(token))
            );

            _geometriesByType = _geometries.ToLookup(kvp => kvp.Value.Type, kvp => kvp.Value);
        }

        public static async Task<GeoJsonDocument> CreateAsync(InputData data)
        {
            using var streamReader = new StreamReader(data.Stream);
            using var jsonReader = new JsonTextReader(streamReader);
            var document = await JToken.LoadAsync(jsonReader);

            return new GeoJsonDocument(document, data.FileName);
        }

        private static int GetMaxDegreeOfParallelism()
        {
            var max = Environment.ProcessorCount / 4;
            return max > 0 ? max : 1;
        }
    }
}
