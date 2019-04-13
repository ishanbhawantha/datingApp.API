using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using DatingApp.API.Data;
using DatingApp.API.DTOs;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace DatingApp.API.Controllers
{
    [Route("api/[controller]")]
    public class AuthController :Controller
    {
        private readonly IAuthRepository _repo;

        public AuthController(IAuthRepository repo)
        {
            _repo = repo;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody]UserForRegisterDTO userForRegisterDTO){
                  
           userForRegisterDTO.Username = userForRegisterDTO.Username.ToLower();
         
           if(await _repo.UserExists(userForRegisterDTO.Username))
            ModelState.AddModelError("UserName", "User name already exist");


           if(!ModelState.IsValid)
                return BadRequest(ModelState);


            var userToCreate = new User{
                Username = userForRegisterDTO.Username
            };
            var createUser = await _repo.Register(userToCreate, userForRegisterDTO.Password);
            return StatusCode(201);

        }

        [HttpPost("login")]
        public async Task<IActionResult> login([FromBody]UserForLoginDTO userForLoginDTO)
        {   
            string username = userForLoginDTO.Username.ToLower();
            string password = userForLoginDTO.Password;

            var userFromRepo = _repo.Login(username, password);

            if(userFromRepo == null)
                return Unauthorized();

            //Generate a token.
            var tokenHandler = new JwtSecurityTokenHandler();

            var key = Encoding.ASCII.GetBytes("Super secrete key");
            var tokenDescripter = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.NameIdentifier, userFromRepo.Id.ToString()),
                    new Claim(ClaimTypes.Name , userFromRepo.Username)
                }),
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),SecurityAlgorithms.HmacSha256Signature)

            }

            var token = tokenHandler.CreateToken(tokenDescripter);
            var tokenString = tokenHandler.WriteToken(token);

            return Ok(new {tokenString});





        }


    }
}