using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using AutoGen.Core;
using Octokit;

namespace ChatRoom.Github;

public partial class IssueHelper : IAgent
{
    /// <summary>
    /// Get an issue from a GitHub repository
    /// </summary>
    /// <param name="owner">The owner of the repository</param>
    /// <param name="issueNumber">The issue number</param>
    /// <param name="name">The name of the repository</param>
    /// <returns></returns>
    [Function]
    public async Task<string> GetIssueAsync(string owner, string name, int issueNumber)
    {
        Console.WriteLine($"Getting issue {issueNumber} from {owner}/{name}");
        var issue = await _gitHubClient.Issue.Get(owner, name, issueNumber);
        var dto = new IssueDTO(issue);
        var json = JsonSerializer.Serialize(dto, _jsonSerializerOptions);

        return json;
    }

    /// <summary>
    /// Get the comments for an issue from a GitHub repository
    /// </summary>
    /// <param name="owner">The owner of the repository</param>
    /// <param name="name">The name of the repository</param>
    /// <param name="issueNumber">The issue number</param>
    [Function]
    public async Task<string> GetIssueCommentsAsync(string owner, string name, int issueNumber)
    {
        Console.WriteLine($"Getting comments for issue {issueNumber} from {owner}/{name}");
        var comments = await _gitHubClient.Issue.Comment.GetAllForIssue(owner, name, issueNumber);
        var json = JsonSerializer.Serialize(comments, _jsonSerializerOptions);

        return json;
    }

    /// <summary>
    /// Search issues in a GitHub repository
    /// </summary>
    /// <param name="owner">the repo owner</param>
    /// <param name="repoName">the repo name</param>
    /// <param name="query">the search query</param>
    /// <param name="limit">the number of issues to return</param>
    /// <param name="author">the author of the issue</param>
    /// <param name="assignee">the assignee of the issue</param>
    /// <param name="mentioned">the mentioned user</param>
    /// <param name="labels">the labels of the issue</param>
    /// <param name="state">the state of the issue. legal values are 'open' and 'closed'</param>
    [Function]
    public async Task<string> SearchIssuesAsync(
        string owner,
        string repoName,
        string query,
        string author,
        string assignee,
        string mentioned,
        string[] labels,
        int limit = 5,
        string state = "open")
    {
        Console.WriteLine($"Searching issues in {owner}/{repoName} with query: {query}");
        var request = new SearchIssuesRequest(query);
        request.In = [
            IssueInQualifier.Title,
            IssueInQualifier.Body,
            IssueInQualifier.Comment,
            ];
        request.Repos.Add(owner, repoName);
        if (assignee is not null)
        {
            Console.WriteLine($"Assignee: {assignee}");
            request.Assignee = assignee;
        }

        //if (from is not null)
        //{
        //    to ??= DateTime.Now;
        //    Console.WriteLine($"Since: {from}");
        //    request.Created = new DateRange(new DateTimeOffset(from.Value), new DateTimeOffset(to.Value));
        //}

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

        request.PerPage = 5;


        var issues = await _gitHubClient.Search.SearchIssues(request);
        var issueDTOs = issues.Items.Select(x => new IssueDTO(x));
        var json = JsonSerializer.Serialize(issueDTOs, _jsonSerializerOptions);

        return json;
    }

    private readonly GitHubClient _gitHubClient;
    private readonly IAgent _agent;
    private readonly JsonSerializerOptions _jsonSerializerOptions;

    public IssueHelper(IAgent agent, GitHubClient gitHubClient)
    {
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
            .RegisterMiddleware(functionCallMiddleware);
            
    }

    public string Name => _agent.Name;

    public Task<IMessage> GenerateReplyAsync(IEnumerable<IMessage> messages, GenerateReplyOptions? options = null, CancellationToken cancellationToken = default)
    {
        return _agent.GenerateReplyAsync(messages, options, cancellationToken);
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
