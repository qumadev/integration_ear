using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace STR_SERVICE_INTEGRATION_EAR.BL
{
    public class Token
    {
        private static string _secretKey = "";
        private static int expirationDay;
        public Token()
        {
            _secretKey = ConfigurationManager.AppSettings["secret_key_token_email"].ToString(); //"this is my custom Secret key for authentication";
            expirationDay = Convert.ToInt32(ConfigurationManager.AppSettings["expiration_token_email"]);
            // var s = GenerarToken(6581, 785, "54");

            //var fsf = LeerToken("eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpZFNvbGljaXR1ZCI6IjY1ODEiLCJpZEFwcm9iYWRvciI6Ijc4NSIsImFyZWFBcHJvYmFkb3IiOiI1NCIsIm5iZiI6MTcxMTUyMTk2MywiZXhwIjoxNzExNjA4MzYzLCJpYXQiOjE3MTE1MjE5NjN9.yf7FcEsHSFUTJN4Lu7aHNAbt0HH0ZtAxbz1LiY5SPkk");

        }

        public string GenerarToken(string idSolicitud, string idAprobador, string areaAprobador, string accion, string estado, string rendicionId, string area)
        {
           

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_secretKey);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                new Claim("idS", idSolicitud), // idSolicitud
                new Claim("idA", idAprobador), // idAprobador
                new Claim("areaA", areaAprobador), // areaAprobador
                new Claim("ac", accion),    // accion
                new Claim("es", estado ),   // estado
                new Claim("renId",rendicionId), // rendicionId
                new Claim("ar", area) // area
            }),
                Expires = DateTime.UtcNow.AddDays(expirationDay), //DateTime.UtcNow.AddDays(60),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public (string idSolicitud, string idAprobador, string areaAprobador, string accion, string estado, string rendicionId, string area) LeerToken(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_secretKey);

                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;

                string idSolicitud = jwtToken.Claims.First(x => x.Type == "idS").Value;
                string idAprobador = jwtToken.Claims.First(x => x.Type == "idA").Value;
                string areaAprobador = jwtToken.Claims.First(x => x.Type == "areaA").Value;
                string accion = jwtToken.Claims.First(x => x.Type == "ac").Value;
                string estado = jwtToken.Claims.First(x => x.Type == "es").Value;
                string rendicionId = jwtToken.Claims.First(x => x.Type == "renId").Value;
                string area = jwtToken.Claims.First(x => x.Type == "ar").Value;
                return (idSolicitud, idAprobador, areaAprobador, accion, estado, rendicionId, area);
            }
            catch (Exception)
            {
                throw new Exception("No se puede leer token");
            }
        }
    }
}
