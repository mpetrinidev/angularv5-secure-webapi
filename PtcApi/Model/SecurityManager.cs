using System;
using System.Collections.Generic;
using System.Linq;

namespace PtcApi.Model
{
    public class SecurityManager
    {
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
        protected AppUserAuth BuildUserAuthObject(AppUser authUser)
        {
            var ret = new AppUserAuth();
            var claims = new List<AppUserClaim>();

            ret.UserName = authUser.UserName;
            ret.IsAuthenticated = true;
            ret.BearerToken = new Guid().ToString();

            claims = GetUserClaims(authUser);

            foreach (var claim in claims)
            {
                try
                {
                    typeof(AppUserAuth).GetProperty(claim.ClaimType)
                    .SetValue(ret, Convert.ToBoolean(claim.ClaimValue), null);
                }
                catch
                {
                }
            }

            return ret;
        }
    }
}