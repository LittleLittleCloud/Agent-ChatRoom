using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace ChatRoom.Client;

[ApiController]
[Route("api")]
public class ChatRoomClientController
{
    [HttpGet("get1")]
    public string Get()
    {
        return "Hello from ChatRoomClientService!";
    }

    [HttpGet("get2")]
    public string Get2()
    {
        return "Hello from ChatRoomClientService!";
    }
}
