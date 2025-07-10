using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Net;

namespace site.diogocosta.dev.Converters
{
    public class IpAddressConverter : ValueConverter<string?, IPAddress?>
    {
        public IpAddressConverter() : base(
            // De string para IPAddress (para o banco)
            str => string.IsNullOrEmpty(str) ? null : IPAddress.Parse(str),
            // De IPAddress para string (do banco)
            ip => ip == null ? null : ip.ToString()
        )
        {
        }
    }
}
