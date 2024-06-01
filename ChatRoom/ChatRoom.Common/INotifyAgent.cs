using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoGen.Core;

namespace ChatRoom.SDK;

public interface INotifyAgent : IAgent
{
    event EventHandler<ChatMsg>? Notify;
}
