using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using System;
using System.Linq;
using ShaneSpace.GameSite.Models;
using Autofac;
using ShaneSpace.GameSite.Domain;
using MediatR;
using System.Collections.Generic;
using Microsoft.AspNet.SignalR.Infrastructure;

namespace ShaneSpace.GameSite.WebApi.Hubs
{
    public abstract class BaseHub<T> : Hub where T : Hub
    {
        protected User _user;
        protected readonly ILifetimeScope _hubLifetimeScope;
        protected readonly IUserMappingService _userMappingService;
        protected readonly IMediator _mediator;
        protected readonly IHubContext _hubContext;

        private readonly static IHubContext hubContext = GlobalHost.ConnectionManager.GetHubContext<T>();
        private readonly static HubUserManager<string> _users = new HubUserManager<string>();
        private readonly static HubGroupManager<string> _groups = new HubGroupManager<string>();

        public BaseHub(ILifetimeScope lifetimeScope)
        {
            _hubLifetimeScope = lifetimeScope.BeginLifetimeScope();
            _mediator = _hubLifetimeScope.Resolve<IMediator>();
            _userMappingService = _hubLifetimeScope.Resolve<IUserMappingService>();
            _hubContext = _hubLifetimeScope.Resolve<IConnectionManager>().GetHubContext<T>();
        }

        // connection management
        public override Task OnConnected()
        {
            GetUser();
            string name = _user.SingalRIdentity();

            _users.Add(name, Context.ConnectionId);

            return base.OnConnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            GetUser();
            string name = _user.SingalRIdentity();

            _users.Remove(name, Context.ConnectionId);

            _groups.LeaveAllGroups(Context.ConnectionId);

            return base.OnDisconnected(stopCalled);
        }

        public override Task OnReconnected()
        {
            GetUser();
            string name = _user.SingalRIdentity();

            if (!_users.GetConnections(name).Contains(Context.ConnectionId))
            {
                _users.Add(name, Context.ConnectionId);
            }

            return base.OnReconnected();
        }

        protected void GetUser()
        {
            _user = _userMappingService.GetUserFromIdentity(Context.User.Identity);
        }

        internal new Dictionary<string, HashSet<string>> Clients { get { return _users.GetConnections(); } }

        internal new Dictionary<string, HashSet<string>> Groups { get { return _groups.GetConnections(); } }

        internal string[] GetConnectionsForUser(string username)
        {
            return _users.GetConnections(username).ToArray();
        }

        // messaging
        public async virtual Task<HubClientMessage> SendHubMessageToAllAsync(string type, dynamic contents)
        {
            var clients = _users.GetConnections().SelectMany(x => x.Value).ToArray();
            return await SendHubMessageToClientsAsync(clients, type, contents);
        }

        protected async virtual Task<HubClientMessage> SendHubMessageToOthersAsync(string type, dynamic contents)
        {
            var clients = _users.GetConnections().SelectMany(x => x.Value).Where(x => x != Context.ConnectionId).ToArray();
            return await SendHubMessageToClientsAsync(clients, type, contents);
        }

        public async virtual Task<HubClientMessage> SendHubMessageToGroupAsync(string groupName, string type, dynamic contents)
        {
            var clients = ClientsInGroup(groupName);
            return await SendHubMessageToClientsAsync(clients, type, contents);
        }

        protected async virtual Task<HubClientMessage> SendHubMessageToOthersInGroupAsync(string groupName, string type, dynamic contents)
        {
            var clients = OthersInGroup(groupName);
            return await SendHubMessageToClientsAsync(clients, type, contents);
        }

        public async virtual Task<HubClientMessage> SendHubMessageToClientAsync(string client, string type, dynamic contents)
        {
            return await SendHubMessageToClientsAsync(new[] { client }, type, contents);
        }

        public async virtual Task<HubClientMessage> SendHubMessageToClientsAsync(string[] clients, string type, dynamic contents)
        {
            var message = new HubClientMessage { Type = type, Contents = contents };
            await _hubContext.Clients.Clients(clients).hubMessage(message);
            return message;
        }

        // group management
        protected void JoinGroup(string groupName)
        {
            _groups.JoinGroup(groupName, Context.ConnectionId);
        }

        protected void LeaveGroup(string groupName)
        {
            _groups.LeaveGroup(groupName, Context.ConnectionId);
        }

        internal string[] ClientsInGroup(string groupName)
        {
            return _groups.GetConnections(groupName).ToArray();
        }

        protected string[] OthersInGroup(string groupName)
        {
            return _groups.GetConnections(groupName).Where(x => x != Context.ConnectionId).ToArray();
        }
    }

    public class HubClientMessage
    {
        public Guid Id { get; } = Guid.NewGuid();
        public string Type { get; set; }
        public dynamic Contents { get; set; }
    }
}