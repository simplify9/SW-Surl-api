using System.Threading.Tasks;
using SW.PrimitiveTypes;
using SW.Surl.Domain;

namespace SW.Surl.Resources.Urls
{
    [Unprotect]
    public class Redirect : IGetHandler<string,object>
    {
        private readonly SurlDbContext db;

        public Redirect(SurlDbContext db)
        {
            this.db = db;
        }
        public async Task<object> Handle(string key)
        {
            
            ShortUrl url = await db.Set<ShortUrl>().FindAsync(key);

            if (url == null) return null;

            return new CqApiResult<string>(url.FullUrl)
            {
                Status = CqApiResultStatus.ChangedLocation
            };
        }
    }
}