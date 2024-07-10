using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChatRoom.SDK;

namespace ChatRoom.Planner;

internal class ChatroomOrchestratorFactory
{
    public static ReactPlanningOrchestrator CreateReactPlanningOrchestrator(ReActPlannerConfiguration plannerConfiguration)
    {
        var dynamicGroupChat = new DynamicGroupChat(plannerConfiguration.OpenAIConfiguration);
        var reactPlanningOrchestrator = new ReactPlanningOrchestrator(dynamicGroupChat, plannerConfiguration);

        return reactPlanningOrchestrator;
    }
}
