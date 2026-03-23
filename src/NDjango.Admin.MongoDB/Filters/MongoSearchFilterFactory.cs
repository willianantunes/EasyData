using System.Threading;
using System.Threading.Tasks;

using Newtonsoft.Json.Linq;

using NDjango.Admin.Services;
using NDjango.Admin.AspNetCore.AdminDashboard.Services;

namespace NDjango.Admin.MongoDB
{
    internal class MongoSearchFilterFactory : ISearchFilterFactory
    {
        public async Task<EasyFilter> CreateSearchFilterAsync(MetaData model, string searchQuery, CancellationToken ct = default)
        {
            var filter = new MongoSubstringFilter(model);
            var jobj = new JObject { ["class"] = MongoSubstringFilter.Class, ["value"] = searchQuery };
            var json = jobj.ToString(Newtonsoft.Json.Formatting.None);
            using (var sr = new System.IO.StringReader(json))
            using (var jr = new Newtonsoft.Json.JsonTextReader(sr)) {
                await filter.ReadFromJsonAsync(jr, ct);
            }
            return filter;
        }
    }
}
