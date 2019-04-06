﻿using System.Collections.Generic;
using System.Linq;

namespace DiscordChatExporter.Core.Models
{
    public class Mentionables
    {
        public IReadOnlyList<User> Users { get; }

        public IReadOnlyList<Channel> Channels { get; }

        public IReadOnlyList<Role> Roles { get; }

        public Mentionables(IReadOnlyList<User> users, IReadOnlyList<Channel> channels, IReadOnlyList<Role> roles)
        {
            Users = users;
            Channels = channels;
            Roles = roles;
        }

        public User GetUser(string id) =>
            Users.FirstOrDefault(u => u.Id == id) ?? User.CreateUnknownUser(id);

        public Channel GetChannel(string id) =>
            Channels.FirstOrDefault(c => c.Id == id) ?? Channel.CreateDeletedChannel(id);

        public Role GetRole(string id) =>
            Roles.FirstOrDefault(r => r.Id == id) ?? Role.CreateDeletedRole(id);
    }
}