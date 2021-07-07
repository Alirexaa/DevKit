using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace DevKit.Web.Services.Jwt
{
    public class JwtService : IJwtService
    {
        private readonly JwtSetting _settings;

        public JwtService(IOptionsSnapshot<JwtSetting> settings)
        {
            _settings = settings.Value;
        }

        public async Task<string> GenerateToken<TUser>(TUser user)
        {
            var secretKey = Encoding.UTF8.GetBytes(_settings.SecretKey); // longer that 16 character
            var signingCredentials = new SigningCredentials(new SymmetricSecurityKey(secretKey),SecurityAlgorithms.HmacSha256Signature);

            var encryptionkey = Encoding.UTF8.GetBytes(_settings.EncryptionKey); //must be 16 character
            var encryptingCredentials = new EncryptingCredentials(new SymmetricSecurityKey(encryptionkey),SecurityAlgorithms.Aes128CbcHmacSha256);
            var claims = await _getClaimsAsync(user);

            var securityTokenDescriptor = new SecurityTokenDescriptor
            {
                IssuedAt = DateTime.Now,
                Expires = DateTime.Now.AddMinutes(_settings.ExpirationMinutes),
                NotBefore = DateTime.Now.AddMinutes(_settings.NotBeforeMinutes),
                Subject = new ClaimsIdentity(claims),
                Audience = _settings.Audience,
                Issuer = _settings.Issuer,
                EncryptingCredentials = encryptingCredentials,
                SigningCredentials = signingCredentials,
            };

            //JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
            //JwtSecurityTokenHandler.DefaultMapInboundClaims = false;
            //JwtSecurityTokenHandler.DefaultOutboundClaimTypeMap.Clear();

            var jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
            var securityToken = jwtSecurityTokenHandler.CreateToken(securityTokenDescriptor);

            return securityToken.ToString();
        }

        public  async Task<IEnumerable<Claim>> _getClaimsAsync<TUser>(TUser user)
        {
            //var result = await signInManager.ClaimsFactory.CreateAsync(user);
            //add custom claims
            //var list = new List<Claim>(result.Claims);
            //list.Add(new Claim(ClaimTypes.MobilePhone, "09123456987"));

            //JwtRegisteredClaimNames.Sub
            //var securityStampClaimType = new ClaimsIdentityOptions().SecurityStampClaimType;

            //var list = new List<Claim>
            //{
            //    new Claim(ClaimTypes.Name, user.UserName),
            //    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            //    //new Claim(ClaimTypes.MobilePhone, "09123456987"),
            //    //new Claim(securityStampClaimType, user.SecurityStamp.ToString())
            //};

            //var roles = new Role[] { new Role { Name = "Admin" } };
            //foreach (var role in roles)
            //    list.Add(new Claim(ClaimTypes.Role, role.Name));

            return new List<Claim>();
        }
    }
}