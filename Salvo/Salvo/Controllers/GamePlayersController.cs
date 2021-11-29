using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Salvo.Models;
using Salvo.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Salvo.Controllers
{
    [Route("api/gameplayers")]
    [ApiController]
    [Authorize("PlayerOnly")]
    public class GamePlayersController : ControllerBase
    {
        private IGamePlayerRepository _repository;
        private IPlayerRepository _playerRepository;
        public GamePlayersController(IGamePlayerRepository repository, IPlayerRepository playerRepository)
        {
            _repository = repository;
            _playerRepository = playerRepository;
        }

        // GET api/<GamePlayersController>/5
        [HttpGet("{id}", Name ="GetGameView")]
        public IActionResult GetGameView(long id)
        {
            try
            {
                string email = User.FindFirst("Player") != null ? User.FindFirst("Player").Value : "Guest";

                var gp = _repository.GetGamePlayerView(id);

                if (gp.Player.Email != email)
                    return Forbid();

                var gameView = new GameViewDTO
                {
                    Id = gp.Id,
                    CreationDate = gp.Game.CreationDate,
                    Ships = gp.Ships.Select(ship => new ShipDTO
                    {
                        Id = ship.Id,
                        Type = ship.Type,
                        Locations = ship.Locations.Select(shipLocation => new ShipLocationDTO
                        {
                            Id = shipLocation.Id,
                            Location = shipLocation.Location
                        }).ToList()
                    }).ToList(),
                    GamePlayers = gp.Game.GamePlayers.Select(gps => new GamePlayerDTO
                    {
                        Id = gps.Id,
                        JoinDate = gps.JoinDate,
                        Player = new PlayerDTO
                        {
                            Id = gps.Player.Id,
                            Email = gps.Player.Email
                        }
                    }).ToList(),
                    Salvos = gp.Game.GamePlayers.SelectMany(gps => gps.Salvos.Select(salvo => new SalvoDTO
                    {
                        Id = salvo.Id,
                        Turn = salvo.Turn,
                        Player = new PlayerDTO
                        {
                            Id = gps.Player.Id,
                            Email = gps.Player.Email
                        },
                        Locations = salvo.Locations.Select(salvoLocation => new SalvoLocationDTO
                        {
                            Id = salvoLocation.Id,
                            Location = salvoLocation.Location
                        }).ToList()
                    })).ToList()
            };

                return Ok(gameView);
            }
            catch(Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("{id}/ships")]
        public IActionResult Post(long id, [FromBody] List<ShipDTO> ships)
        {
            try
            {
                string email = User.FindFirst("Player") != null ? User.FindFirst("Player").Value : "Guest";
                Player player = _playerRepository.FindByEmail(email);

                GamePlayer gamePlayer = _repository.FindById(id);

                if (gamePlayer == null)
                    return StatusCode(403, "No existe el juego");

                if (gamePlayer.Player.Id != player.Id)
                    return StatusCode(403, "El usuario no se encuentra en el juego");

                if (gamePlayer.Ships.Count == 5)
                    return StatusCode(403, "Ya se posicionaron los barcos");

                gamePlayer.Ships = ships.Select(ship => new Ship
                {
                    GamePlayerId = gamePlayer.Id,
                    Type = ship.Type,
                    Locations = ship.Locations.Select(location => new ShipLocation
                    {
                        ShipId = ship.Id,
                        Location = location.Location
                    }).ToList()
                }).ToList();

                _repository.Save(gamePlayer);

                return StatusCode(201);
            }
            catch(Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("{id}/salvos")]
        public IActionResult Post(long id, [FromBody] SalvoDTO salvo)
        {
            try
            {
                //Buscamos el email logueado
                string email = User.FindFirst("Player") != null ? User.FindFirst("Player").Value : "Guest";
                Player player = _playerRepository.FindByEmail(email);

                //Buscamos el gameplayer
                GamePlayer gamePlayer = _repository.FindById(id);

                //Validamos el gameplayer (null)
                if (gamePlayer == null)
                    return StatusCode(403, "No existe el juego");

                //Validamos si el usuario se encuentra en el juego
                if (gamePlayer.Player.Id != player.Id)
                    return StatusCode(403, "El usuario no se encuentra en el juego");

                //Buscamos al oponente
                GamePlayer opponentGamePlayer = gamePlayer.getOpponent();

                if (gamePlayer.Game.GamePlayers.Count() != 2)
                    return StatusCode(403, "NO HAY A QUIEN DISPARAR!!!");

                opponentGamePlayer = _repository.FindById(opponentGamePlayer.Id);

                int playerTurn = 0;
                int opponentTurn = 0;

                playerTurn = gamePlayer.Salvos != null ? gamePlayer.Salvos.Count() + 1 : 1;

                if (opponentGamePlayer != null)
                    opponentTurn = opponentGamePlayer.Salvos != null ? opponentGamePlayer.Salvos.Count() : 0;

                if ((playerTurn - opponentTurn) < -1 || (playerTurn - opponentTurn) > 1)
                    return StatusCode(403, "No se puede adelantar el turno");

                gamePlayer.Salvos.Add(new Salvo.Models.Salvo
                {
                    GamePlayerId = gamePlayer.Id,
                    Turn = playerTurn,
                    Locations = salvo.Locations.Select(location => new SalvoLocation
                    {
                        SalvoId = salvo.Id,
                        Location = location.Location
                    }).ToList()
                });

                _repository.Save(gamePlayer);
                return StatusCode(201);
            }
            catch(Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
