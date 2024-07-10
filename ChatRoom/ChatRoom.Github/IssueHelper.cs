using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using AutoGen.Core;
using ChatRoom.SDK;
using Octokit;

namespace ChatRoom.Github;

public partial class IssueHelper : INotifyAgent
{
    private readonly string repoOwner;
    private readonly string repoName;

    /// <summary>
    /// Get an issue from a GitHub repository
    /// </summary>
    /// <param name="issueNumber">The issue number</param>
    /// <returns></returns>
    [Function]
    public async Task<string> GetIssueAsync(int issueNumber)
    {
        Console.WriteLine($"Getting issue {issueNumber} from {this.repoName}/{this.repoName}");
        var issue = await _gitHubClient.Issue.Get(this.repoOwner, this.repoName, issueNumber);
        var dto = new IssueDTO(issue);
        var json = JsonSerializer.Serialize(dto, _jsonSerializerOptions);

        return json;
    }

    /// <summary>
    /// Get the comments for an issue from a GitHub repository
    /// </summary>
    /// <param name="issueNumber">The issue number</param>
    [Function]
    public async Task<string> GetIssueCommentsAsync(int issueNumber)
    {
        WriteLine($"Getting comments for issue {issueNumber} from {this.repoOwner}/{this.repoName}");
        var comments = await _gitHubClient.Issue.Comment.GetAllForIssue(this.repoOwner, this.repoName, issueNumber);
        var json = JsonSerializer.Serialize(comments, _jsonSerializerOptions);

        return json;
    }

    /// <summary>
    /// Search issues in a GitHub repository
    /// </summary>
    /// <param name="query">the search query</param>
    /// <param name="limit">the number of issues to return</param>
    /// <param name="author">the author of the issue</param>
    /// <param name="assignee">the assignee of the issue</param>
    /// <param name="mentioned">the mentioned user</param>
    /// <param name="labels">the labels of the issue</param>
    /// <param name="milestone">the milestone of the issue</param>
    /// <param name="state">the state of the issue. legal values are 'open' and 'closed'</param>
    [Function]
    public async Task<string> SearchIssuesAsync(
        string query,
        string author,
        string assignee,
        string mentioned,
        string[] labels,
        string milestone,
        int? limit,
        string state)
    {
        WriteLine($"Searching issues in {this.repoOwner}/{this.repoName} with query: {query}");
        var request = query switch
        {
            _ when string.IsNullOrEmpty(query) => new SearchIssuesRequest(),
            _ => new SearchIssuesRequest(query),
        };
        request.In = [
            IssueInQualifier.Title,
            IssueInQualifier.Body,
            IssueInQualifier.Comment,
            ];
        request.Repos.Add(this.repoOwner, this.repoName);
        if (assignee is not null)
        {
            Console.WriteLine($"Assignee: {assignee}");
            request.Assignee = assignee;
        }

        if (author is not null)
        {
            Console.WriteLine($"Author: {author}");
            request.Author = author;
        }

        if (mentioned is not null)
        {
            Console.WriteLine($"Mentioned: {mentioned}");
            request.Mentions = mentioned;
        }

        if (labels is not null)
        {
            Console.WriteLine($"Labels: {labels}");
            request.Labels = labels;
        }

        request.State = state switch
        {
            "open" => ItemState.Open,
            "closed" => ItemState.Closed,
            _ => null,
        };

        if (limit is not null)
        {
            Console.WriteLine($"Limit: {limit}");
            request.PerPage = limit.Value;
        }

        if (milestone is not null)
        {
            Console.WriteLine($"Milestone: {milestone}");
            request.Milestone = milestone;
        }

        request.Is = [IssueIsQualifier.Issue];


        var issues = await _gitHubClient.Search.SearchIssues(request);
        var issueDTOs = issues.Items.Select(x => new IssueDTO(x));
        var json = JsonSerializer.Serialize(issueDTOs, _jsonSerializerOptions);

        return json;
    }

    private readonly GitHubClient _gitHubClient;
    private readonly IAgent _agent;
    private readonly JsonSerializerOptions _jsonSerializerOptions;

    public event EventHandler<ChatMsg>? Notify;

    public IssueHelper(IAgent agent, GitHubClient gitHubClient, string repoOwner, string repoName)
    {
        this.repoOwner = repoOwner;
        this.repoName = repoName;
        _agent = agent;
        _gitHubClient = gitHubClient;
        _jsonSerializerOptions = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        FunctionContract[] tools = [
            this.GetIssueAsyncFunctionContract,
            this.GetIssueCommentsAsyncFunctionContract,
            this.SearchIssuesAsyncFunctionContract,
            ];

        var functionCallMiddleware = new FunctionCallMiddleware(
            functions: tools,
            functionMap: new Dictionary<string, Func<string, Task<string>>>
            {
                [this.GetIssueAsyncFunctionContract.Name!] = this.GetIssueAsyncWrapper,
                [this.GetIssueCommentsAsyncFunctionContract.Name!] = this.GetIssueCommentsAsyncWrapper,
                [this.SearchIssuesAsyncFunctionContract.Name!] = this.SearchIssuesAsyncWrapper,
            });

        _agent = agent
            .RegisterMiddleware(functionCallMiddleware)
            .RegisterMiddleware(async (msgs, option, innerAgent, ct) =>
            {
                try
                {
                    var reply = await innerAgent.GenerateReplyAsync(msgs, option, ct);
                    if (reply is ToolCallAggregateMessage)
                    {
                        return await innerAgent.GenerateReplyAsync(msgs.Append(reply), option, ct);
                    }

                    return reply;
                }
                catch (Exception ex)
                {
                    return new TextMessage(Role.Assistant, ex.Message, from: innerAgent.Name);
                }
            })
            .RegisterPrintMessage();

    }

    public string Name => _agent.Name;

    public Task<IMessage> GenerateReplyAsync(IEnumerable<IMessage> messages, GenerateReplyOptions? options = null, CancellationToken cancellationToken = default)
    {
        return _agent.GenerateReplyAsync(messages, options, cancellationToken);
    }

    private void WriteLine(string msg)
    {
        Notify?.Invoke(this, new ChatMsg(From: Name, Text: msg));
    }

    class IssueDTO
    {
        public IssueDTO(Issue issue)
        {
            Id = issue.Id;
            NodeId = issue.NodeId;
            Url = issue.Url;
            HtmlUrl = issue.HtmlUrl;
            CommentsUrl = issue.CommentsUrl;
            EventsUrl = issue.EventsUrl;
            Number = issue.Number;
            State = issue.State.StringValue;
            Title = issue.Title;
            Body = issue.Body;
            Repository = issue.Repository;
        }

        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("node_id")]
        public string NodeId { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; }

        [JsonPropertyName("html_url")]
        public string HtmlUrl { get; set; }

        [JsonPropertyName("comments_url")]
        public string CommentsUrl { get; set; }

        [JsonPropertyName("events_url")]
        public string EventsUrl { get; set; }

        [JsonPropertyName("number")]
        public int Number { get; set; }

        [JsonPropertyName("state")]
        public string State { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("body")]
        public string Body { get; set; }

        [JsonPropertyName("repository")]
        public Repository Repository { get; set; }
    }
}
