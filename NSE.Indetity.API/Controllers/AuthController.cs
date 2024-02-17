using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NSE.Indetity.API.Extensions;
using NSE.Indetity.API.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace NSE.Indetity.API.Controllers
{
    [ApiController]
    [Route("api/indetity")]
    public class AuthController : MainController
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly AppSettings _appSettings;

        public AuthController(SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager, IOptions<AppSettings> appSettings)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _appSettings = appSettings.Value;
        }

        [HttpPost("new-account")]
        public async Task<ActionResult> Register(UserRegister userRegister)
        {
            if (!ModelState.IsValid)
            {
                return CustomResponse(ModelState);
            }

            var user = new IdentityUser
            {
                UserName = userRegister.Email,
                Email = userRegister.Email,
                EmailConfirmed = true
            }; 

            var result = await _userManager.CreateAsync(user, userRegister.Password);

            if (result.Succeeded)
            {
                return CustomResponse(await GenerateJwt(userRegister.Email));
            }

            foreach (var error in result.Errors)
            {
                AddErrorsProcess(error.Description);
            } // caso haja erro na hora de criar um user que ele salve esse erro

            return CustomResponse();

        }

        [HttpPost("authenticate")]
        public async Task<ActionResult> Login(UserLogin userLogin)
        {
            if (!ModelState.IsValid)
            {
                return CustomResponse(ModelState);
            }

            var result = await _signInManager.PasswordSignInAsync(userLogin.Email, userLogin.Password, isPersistent: false, lockoutOnFailure: true); // valindando conforme a senha

            if (result.Succeeded)
            {
                return CustomResponse(await GenerateJwt(userLogin.Email));
            }

            if (result.IsLockedOut)
            {
                AddErrorsProcess("User blocked due to valid attempts");
                return CustomResponse();
            }

            AddErrorsProcess("Incorrect username or passwords");
            return CustomResponse();
        }

        [HttpPost]
        public async Task<UserResponseLogin> GenerateJwt(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            var claims = await _userManager.GetClaimsAsync(user);

            var identityClaims = await GetUserClaims(claims, user); 
            var encodedToken = EncodedToken(identityClaims);

            return GetResponseToken(encodedToken, user, claims);
        }

        private async Task<ClaimsIdentity> GetUserClaims(ICollection<Claim> claims, IdentityUser user)
        {
            var userRoles = await _userManager.GetRolesAsync(user);
                
            claims.Add(new Claim(type: JwtRegisteredClaimNames.Sub, value: user.Id)); // identifica o principal assunto do jwt (id do user)
            claims.Add(new Claim(type: JwtRegisteredClaimNames.Email, value: user.Email)); // Contém o endereço de e-mail do usuário.
            claims.Add(new Claim(type: JwtRegisteredClaimNames.Jti, value: Guid.NewGuid().ToString())); //  Um identificador exclusivo para rastrear tokens.
            claims.Add(new Claim(type: JwtRegisteredClaimNames.Nbf, value: ToUnixEpochDate(DateTime.UtcNow).ToString())); // Define o momento em que o token se torna válido (em formato de data Unix).
            claims.Add(new Claim(type: JwtRegisteredClaimNames.Iat, value: ToUnixEpochDate(DateTime.UtcNow).ToString(), ClaimValueTypes.Integer64)); // Indica o momento em que o token foi emitido (também em formato de data Unix).

            foreach (var userRole in userRoles)
            {
                claims.Add(new Claim(type: "role", value: userRole));
            } // é como um título ou uma permissão que o usuário tem.

            var identityClaims = new ClaimsIdentity();
            foreach(var claim in claims)
    {
                identityClaims.AddClaim(claim);
            }

            return identityClaims;
        }

        private string EncodedToken(ClaimsIdentity identityClaims)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
            var token = tokenHandler.CreateToken(new SecurityTokenDescriptor
            {
                Issuer = _appSettings.Issuer,
                Audience = _appSettings.ValidIn,
                Subject = identityClaims,
                Expires = DateTime.UtcNow.AddHours(_appSettings.ExpirationHours),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), algorithm: SecurityAlgorithms.HmacSha256Signature)
            });

           return tokenHandler.WriteToken(token);

        }

        private UserResponseLogin GetResponseToken(string encodedToken, IdentityUser user, IEnumerable<Claim> claims)
        {
            return new UserResponseLogin
            {
                AcessToken = encodedToken,
                ExpiresIn = TimeSpan.FromHours(_appSettings.ExpirationHours).TotalSeconds,
                UserToken = new UserToken
                {
                    Id = user.Id,
                    Email = user.Email,
                    Claims = claims.Select(c => new UserClaim { Type = c.Type, Value = c.Value })
                }
            };
        }

        private static long ToUnixEpochDate(DateTime date) => (long)Math.Round((date.ToUniversalTime() - new DateTimeOffset(year: 1970, month: 1,
        day: 1, hour: 0, minute: 0, second: 0, offset: TimeSpan.Zero)).TotalSeconds); // O método privado ToUnixEpochDate converte uma data e hora para o formato de data Unix (segundos desde a época Unix).
    }
}
