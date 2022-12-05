﻿using BanchoMultiplayerBot.Behaviour;
using BanchoMultiplayerBot.Config;
using BanchoSharp;
using BanchoSharp.Interfaces;
using BanchoSharp.Multiplayer;

namespace BanchoMultiplayerBot;

public class Lobby
{
    
    public Bot Bot { get; }
    public MultiplayerLobby MultiplayerLobby { get; }
    public LobbyConfiguration Configuration { get; }

    public List<IBotBehaviour> Behaviours { get; } = new();

    public int GamesPlayed { get; private set; }

    public bool IsRecovering { get; private set; }

    public event Action? OnLobbyChannelJoined;
    public event Action<IPrivateIrcMessage>? OnUserMessage;
    public event Action<IPrivateIrcMessage>? OnAdminMessage;
    public event Action<IPrivateIrcMessage>? OnBanchoMessage;
    
    private readonly string _channelName;
    
    public Lobby(Bot bot, LobbyConfiguration configuration, string channel)
    {
        Bot = bot;
        Configuration = configuration;
        MultiplayerLobby = new MultiplayerLobby(Bot.Client, long.Parse(channel[4..]), channel);
        _channelName = channel;
    }
    
    public Lobby(Bot bot, LobbyConfiguration configuration, MultiplayerLobby lobby)
    {
        Bot = bot;
        Configuration = configuration;
        MultiplayerLobby = lobby;
        _channelName = lobby.ChannelName;
    }

    public async Task SetupAsync(bool joined = false)
    {
        // Add default behaviours
        AddBehaviour(new LobbyManagerBehaviour());
        AddBehaviour(new MapManagerBehaviour());
        AddBehaviour(new BanBehaviour());
        
        // Add "custom" behaviours
        if (Configuration.Behaviours != null)
        {
            foreach (var behaviourName in Configuration.Behaviours)
            {
                if (behaviourName == "AutoHostRotate")
                    AddBehaviour(new AutoHostRotateBehaviour());
                if (behaviourName == "AntiAfk")
                    AddBehaviour(new AntiAfkBehaviour());
                if (behaviourName == "AutoStart")
                    AddBehaviour(new AutoStartBehaviour());
                if (behaviourName == "AbortVote")
                    AddBehaviour(new AbortVoteBehaviour());
                if (behaviourName == "Help")
                    AddBehaviour(new HelpBehaviour());
            }   
        }

        foreach (var behaviour in Behaviours)
        {
            behaviour.Setup(this);
        }

        MultiplayerLobby.OnMatchFinished += () =>
        {
            GamesPlayed++;
        };

        MultiplayerLobby.OnSettingsUpdated += () =>
        {
            IsRecovering = false;
        };

        Bot.Client.OnPrivateMessageReceived += ClientOnPrivateMessageReceived;
        Bot.Client.OnPrivateMessageSent += ClientOnPrivateMessageSent;

        if (!joined)
        {
            IsRecovering = true;

            Bot.Client.OnChannelJoined += channel =>
            {
                if (channel.ChannelName != _channelName) return;
                
                OnLobbyChannelJoined?.Invoke();
            };
            
            // "Temporary" fix for the fact that JoinChannelAsync calls 
            // OnChannelJoined
            await Bot.Client.SendAsync($"JOIN {_channelName}");
        }
        else
        {
            OnLobbyChannelJoined?.Invoke();
        }
    }

    private void ClientOnPrivateMessageSent(IPrivateIrcMessage e)
    {
        OnAdminMessage?.Invoke(e);
    }

    public void SendMessage(string message)
    {
        Bot.SendMessage(_channelName, message);
    }

    /// <summary>
    /// Get what string to use when passing a player as a parameter in tournament commands.
    /// This will make sure to prioritize player ID, or use player names if not available.
    /// </summary>
    internal string GetPlayerIdentifier(string playerName)
    {
        int? playerId = MultiplayerLobby.Players.FirstOrDefault(x => x.Name == playerName)?.Id;

        return playerId == null ? playerName.Replace(' ', '_') : $"#{playerId}";
    }

    private void AddBehaviour(IBotBehaviour behaviour)
    {
        Behaviours.Add(behaviour);
    }
    
    private void ClientOnPrivateMessageReceived(IPrivateIrcMessage message)
    {
        if (message.IsBanchoBotMessage)
        {
            OnBanchoMessage?.Invoke(message);
            return;
        }
        
        if (message.Recipient != _channelName)
            return;

        if (message.Sender == Bot.Configuration.Username)
        {
            OnAdminMessage?.Invoke(message);
        }
        
        OnUserMessage?.Invoke(message);
    }

}