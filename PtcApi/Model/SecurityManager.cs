using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace PtcApi.Model
{
    public class SecurityManager
    {
        private JwtSettings _settings = null;
        public SecurityManager(JwtSettings settings)
        {
            _settings = settings;
        }

        public AppUserAuth ValidateUser(AppUser user)
        {
            var ret = new AppUserAuth();
            var authUser = new AppUser();

            using (var db = new PtcDbContext())
            {
                authUser = db.Users.Where(p => p.UserName.ToLower() == user.UserName.ToLower()
                && p.Password == user.Password).FirstOrDefault();
            }

            if (authUser != null)
            {
                ret = BuildUserAuthObject(authUser);
            }

            return ret;
        }
        protected List<AppUserClaim> GetUserClaims(AppUser authUser)
        {
            var list = new List<AppUserClaim>();

            try
            {
                using (var db = new PtcDbContext())
                {
                    list = db.Claims
                            .Where(p => p.UserId == authUser.UserId)
                            .ToList();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Exception trying to retrieve user claims", ex);
            }

            return list;
        }

        protected string BuildJwtToken(AppUserAuth authUser)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Key));

            var jwtClaims = new List<Claim>();
            jwtClaims.Add(new Claim(JwtRegisteredClaimNames.Sub, authUser.UserName));
            jwtClaims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));

            jwtClaims.Add(new Claim("isAuthenticated",
                authUser.IsAuthenticated.ToString().ToLower()));
            
            //add custom claims from the claim array
            foreach (var claim in authUser.Claims)
                jwtClaims.Add(new Claim(claim.ClaimType, claim.ClaimValue));

            var token = new JwtSecurityToken(
                issuer: _settings.Issuer,
                audience: _settings.Audience,
                claims: jwtClaims,
                notBefore: DateTime.UtcNow,
                expires: DateTime.UtcNow.AddMinutes(
                    _settings.MinutesToExpiration
                ),
                signingCredentials: new SigningCredentials(
                    key, SecurityAlgorithms.HmacSha256
                )
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        protected AppUserAuth BuildUserAuthObject(AppUser authUser)
        {
            var ret = new AppUserAuth
            {
                UserName = authUser.UserName,
                IsAuthenticated = true,
                BearerToken = new Guid().ToString(),
                Claims = GetUserClaims(authUser)
            };

            ret.BearerToken = BuildJwtToken(ret);

            return ret;
        }
    }
}