using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Salvo.Models;
using Salvo.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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
                if (String.IsNullOrEmpty(player.Email))
                {
                    return StatusCode(403, "Debe ingresar un email");
                }
                if (String.IsNullOrEmpty(player.Password))
                {
                    return StatusCode(403, "Debe ingresar un password");
                }

                Player dbPlayer = _repository.FindByEmail(player.Email);
                if (dbPlayer != null)
                {
                    return StatusCode(403, "Email está en uso");
                }
                else
                {
                    if (String.IsNullOrEmpty(player.Name))
                    {
                        return StatusCode(403, "Debe ingresar un nombre");
                    }

                    //Verificamos la longitud del password
                    string len = @"\w{8,}";

                    if (!Regex.IsMatch(player.Password, len))
                    {
                        return StatusCode(403, "El password debe tener al menos 8 caracteres");
                    }

                    //Verificamos que el password contenga un caracter numerico
                    Regex rg = new(@"\d");
                    if (!rg.IsMatch(player.Password))
                    {
                        return StatusCode(403, "El password debe contener al menos un numero");
                    }

                    //Verificamos que el password contenga un caracter especial
                    string mustCharacter = "[@$!%*#?&]+";
                    rg = new(mustCharacter);
                    if (!rg.IsMatch(player.Password))
                    {
                        return StatusCode(403, "El password debe contener al menos un caracter especial");
                    }

                    //Verificamos que el password no contenga el nombre del usuario
                    string nameInPass = @"(" + player.Name + ")";
                    if(Regex.IsMatch(player.Password, nameInPass))
                    {
                        return StatusCode(403, "El password no debe contener tu nombre");
                    }
                }    

                Player newPlayer = new Player
                {
                    Email = player.Email,
                    Password = player.Password,
                    Name = player.Name
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
