using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Salvo.Models;
using Salvo.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Salvo.Controllers
{
    [Route("api/players")]
    [ApiController]
    public class PlayersController : ControllerBase
    {
        private IPlayerRepository _repository;
        public PlayersController(IPlayerRepository repository)
        {
            _repository = repository;
        }
        [HttpPost]
        public IActionResult Post([FromBody] PlayerDTO player)
        {
            try
            {
                //Verificamos que el mail y el password no esten vacios
                if(String.IsNullOrEmpty(player.Email) || String.IsNullOrEmpty(player.Password))
                {
                    return StatusCode(403, "Datos invalidos");
                }

                Player dbPlayer = _repository.FindByEmail(player.Email);
                if (dbPlayer != null)
                {
                    return StatusCode(403, "Email está en uso");
                }
                    

                Player newPlayer = new Player
                {
                    Email = player.Email,
                    Password = player.Password
                };

                _repository.Save(newPlayer);
                return StatusCode(201, newPlayer);
            }
            catch(Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
