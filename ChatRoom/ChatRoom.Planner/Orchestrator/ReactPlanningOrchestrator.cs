using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChatRoom.SDK;

namespace ChatRoom.Planner;

internal class ReactPlanningOrchestrator : IOrchestrator
{
    private readonly DynamicGroupChat _dynamicGroupChat;
    private readonly ReActPlannerConfiguration _plannerConfiguration;

    public ReactPlanningOrchestrator(DynamicGroupChat dynamicGroupChat, ReActPlannerConfiguration plannerConfiguration)
    {
        _dynamicGroupChat = dynamicGroupChat;
        _plannerConfiguration = plannerConfiguration;
    }

    public async Task<string?> GetNextSpeaker(AgentInfo[] members, ChatMsg[] messages)
    {
        var lastMessage = messages.LastOrDefault();
        if (lastMessage is null)
        {
            return null;
        }

        var lastMessageAgent = members.FirstOrDefault(x => x.Name == lastMessage.From);
        if (lastMessageAgent is null)
        {
            return null;
        }

        if (lastMessageAgent.Name == _plannerConfiguration.Name)
        {
            try
            {
                return await _dynamicGroupChat.GetNextSpeaker(members, messages);
            }
            catch (Exception)
            {
                return null;
            }
        }
        else
        {
            return _plannerConfiguration.Name;
        }
    }
}
