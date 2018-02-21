using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace TokenBasedAuth.Controllers
{
    [Route("api/[controller]")]
    public class ValuesController : Controller
    {
        private readonly JwtConfig _jwtConfig;

        public ValuesController(IOptions<JwtConfig> jwtOptions)
        {
            _jwtConfig = jwtOptions.Value;
        }

        // GET api/values
        [Authorize]
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new[] { "value1", "value2" };
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("token")]
        public IActionResult Post([FromBody]LoginViewModel loginViewModel)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    //This method returns user id from username and password.
                    var userId = GetUserIdFromCredentials(loginViewModel);
                    if (userId == Guid.Empty)
                    {
                        return Unauthorized();
                    }

                    var claims = new[]
                    {
                        new Claim(JwtRegisteredClaimNames.Sub, loginViewModel.Email),
                        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                    };

                    var token = new JwtSecurityToken
                    (
                        issuer: _jwtConfig.Issuer,
                        audience: _jwtConfig.Audience,
                        claims: claims,
                        expires: DateTime.UtcNow.AddDays(60),
                        notBefore: DateTime.UtcNow,
                        signingCredentials: new SigningCredentials(
                            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtConfig.SigningKey)),
                            SecurityAlgorithms.HmacSha256)
                    );

                    return Ok(new {token = new JwtSecurityTokenHandler().WriteToken(token)});
                }

                return BadRequest();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        private Guid GetUserIdFromCredentials(LoginViewModel loginViewModel)
        {
            return loginViewModel.Id;
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }


    }

    public class LoginViewModel
    {
        public Guid Id => new Guid("201fa30d-14c1-44da-811b-82eb62f5260e");
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
