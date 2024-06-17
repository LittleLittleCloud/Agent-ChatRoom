using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoGen.Core;
using ChatRoom.SDK;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace ChatRoom.SDK;

public class ConsoleHostedAgentService : IHostedService
{
    private ChatPlatformClient client;
    private IAgent[] _agents;

    public ConsoleHostedAgentService(ChatPlatformClient client, params IAgent[] agents)
    {
        this.client = client;
        this._agents = agents;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        foreach (var agent in this._agents)
        {
            await this.client.RegisterAgentAsync(agent);
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        foreach (var agent in this._agents)
        {
            await this.client.UnregisterAgentAsync(agent);
        }
    }
}
