using System;
using System.Security.Cryptography;
using System.Threading.Tasks;
using FluentValidation;
using SW.PrimitiveTypes;
using SW.Surl.Domain;
using SW.Surl.Model;

namespace SW.Surl.Resources.Urls
{
    public class Create : ICommandHandler<ShortUrlCreate,object>
    {
        private readonly SurlDbContext db;

        public Create(SurlDbContext db)
        {
            this.db = db;
        }
        public async Task<object> Handle(ShortUrlCreate request)
        {

            var csp = new RNGCryptoServiceProvider();
            ShortUrl entity;
            string unique;


            do
            {
                entity = null;
                byte[] buffer = new byte[5];
                csp.GetBytes(buffer);
                unique = Convert.ToBase64String(buffer);
                unique = unique.Remove(unique.IndexOf('=')).Replace("/", "");

                if (unique != Uri.EscapeUriString(unique))
                    continue;

                entity = await db.Set<ShortUrl>().FindAsync(unique);

            } while (entity != null);

            db.Add(new ShortUrl(unique, request.Url));

            await db.SaveChangesAsync();

            return unique; ;
        }

        private class Validate : AbstractValidator<ShortUrlCreate>
        {
            public Validate()
            {
                RuleFor(p => p.Url).NotEmpty();
                RuleFor(p => p.Url).
                    Custom((value, context) =>
                    {
                        if (value != null && !Uri.IsWellFormedUriString(value, UriKind.Absolute))
                        {
                            context.AddFailure("Invalid url");
                        }
                    });
            }
        }
    }
}