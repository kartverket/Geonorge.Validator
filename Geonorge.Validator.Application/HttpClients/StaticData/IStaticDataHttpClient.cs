using Reguleringsplanforslag.Rules.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Geonorge.Validator.Application.HttpClients.StaticData
{
    public interface IStaticDataHttpClient
    {
        Task<List<GmlDictionaryEntry>> GetArealformål();
        Task<List<GmlDictionaryEntry>> GetFeltnavnArealformål();
        Task<List<GeonorgeCodelistValue>> GetHensynskategori();
    }
}
