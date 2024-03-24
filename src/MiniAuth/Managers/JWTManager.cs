using JWT;
using JWT.Algorithms;
using JWT.Serializers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
namespace MiniAuth.Managers
{
    public interface IJWTManager
    {
        string DecodeToken(string token);
        string GetToken(string sub, string name, int expMins, IEnumerable<string> roles);
    }

    public class JWTManager : IJWTManager
    {
        private readonly X509Certificate2 _certificate;
        private readonly IJwtDecoder _decoder;

        public JWTManager(string subjectName, string password, string cerPath)
        {
            if (!File.Exists(cerPath))
            {
                using (RSA rsa = RSA.Create(2048))
                {
                    CertificateRequest req = new CertificateRequest($"CN={subjectName}", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
                    DateTimeOffset startDate = DateTimeOffset.UtcNow.AddDays(-1);
                    DateTimeOffset endDate = startDate.AddYears(99);
                    req.CertificateExtensions.Add(new X509BasicConstraintsExtension(false, false, 0, false));
                    using (X509Certificate2 cert = req.CreateSelfSigned(startDate, endDate))
                    {
                        byte[] pfxBytes = cert.Export(X509ContentType.Pfx, password);
                        File.WriteAllBytes(cerPath, pfxBytes);
                    }
                }
            }
            _certificate = new X509Certificate2(cerPath, password);
            {
                IJsonSerializer serializer = new JsonNetSerializer();
                IDateTimeProvider provider = new UtcDateTimeProvider();
                IJwtValidator validator = new JwtValidator(serializer, provider);
                IBase64UrlEncoder urlEncoder = new JwtBase64UrlEncoder();
                IJwtAlgorithm algorithm = new RS256Algorithm(_certificate);
                IJwtDecoder decoder = new JwtDecoder(serializer, validator, urlEncoder, algorithm);
                _decoder = decoder;
            }
        }

        public string GetToken(string sub, string name, int expMins, IEnumerable<string> roles)
        {
            //TODO:token id
            var id = Guid.NewGuid().ToString();
            var payload = new Dictionary<string, object>
            {
                { "sub", sub },
                { "name", name },
                { "iss", "miniauth"},
                { "roles", roles},
                { "jti", id},
                { "exp", DateTimeOffset.UtcNow.AddMinutes(expMins).ToUnixTimeSeconds() },
                { "iat", DateTimeOffset.UtcNow.ToUnixTimeSeconds() },
            };
            IJwtAlgorithm algorithm = new RS256Algorithm(_certificate);
            IJsonSerializer serializer = new JsonNetSerializer();
            IBase64UrlEncoder urlEncoder = new JwtBase64UrlEncoder();
            IJwtEncoder encoder = new JwtEncoder(algorithm, serializer, urlEncoder);
            const string key = null;
            var token = encoder.Encode(payload, key);
            return token;
        }

        public string DecodeToken(string token)
        {
            var json = _decoder.Decode(token);
            return json;
        }
    }
}
