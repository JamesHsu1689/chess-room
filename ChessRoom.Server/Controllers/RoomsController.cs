//using ChessRoom.Server.Models;
//using ChessRoom.Server.Services;
//using Microsoft.AspNetCore.Mvc;

//namespace ChessRoom.Server.Controllers
//{
//    [ApiController]
//    [Route("api/[controller]")]
//    public class RoomsController : ControllerBase
//    {
//        private readonly IRoomStore _store;
//        public RoomsController(IRoomStore store) => _store = store;

//        public record CreateRoomDto(bool AllowSpectators = true);

//        [HttpPost]
//        public ActionResult Create([FromBody] CreateRoomDto? dto)
//        {
//            var room = new Room { AllowSpectators = dto?.AllowSpectators ?? true };
//            _store.Add(room);
//            var joinUrl = $"{Request.Scheme}://{Request.Host}/r/{room.Id}";
//            return Ok(new { roomId = room.Id, joinUrl, hostToken = room.HostToken });
//        }

//        [HttpGet("{roomId}")]
//        public ActionResult Get(string roomId)
//        {
//            if(!_store.TryGet(roomId, out var room)) return NotFound();
//            return Ok(new
//            {
//                roomId = room.Id,
//                status = room.Status,
//                state = room.State,
//                presence = room.Presence()
//            });
//        }
//    }
//}


using Microsoft.AspNetCore.Mvc;
using ChessRoom.Server.Models;
using ChessRoom.Server.Services;

namespace ChessRoom.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RoomsController : ControllerBase
{
    private readonly IRoomStore _store;
    public RoomsController(IRoomStore store) => _store = store;

    public record CreateRoomDto(bool AllowSpectators = true);

    [HttpPost]
    public ActionResult Create([FromBody] CreateRoomDto? dto)
    {
        var room = new Room { AllowSpectators = dto?.AllowSpectators ?? true };
        _store.Add(room);
        var joinUrl = $"{Request.Scheme}://{Request.Host}/r/{room.Id}";
        return Ok(new { roomId = room.Id, joinUrl, hostToken = room.HostToken });
    }
}