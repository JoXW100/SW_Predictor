using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using System.Diagnostics.CodeAnalysis;

namespace Predictor.Framework
{
    public sealed class MultiplayerManager<T> where T : class
    {
        private readonly Dictionary<long, T> MultiplayerContexts = new();
        private IModHelper? Helper;

        public MultiplayerManager(IModHelper? helper = null)
        {
            Helper = helper;
            if (Helper is not null)
            {
                Helper.Events.Multiplayer.PeerConnected += OnPeerConnected;
                Helper.Events.Multiplayer.PeerDisconnected += OnPeerDisconnected;
            }
        }

        ~MultiplayerManager()
        {
            if (Helper is not null)
            {
                Helper.Events.Multiplayer.PeerConnected -= OnPeerConnected;
                Helper.Events.Multiplayer.PeerDisconnected -= OnPeerDisconnected;
            }
        }

        /// <summary>
        /// Initializes the multiplayer manager if helper was not provided in constructor.
        /// </summary>
        /// <param name="helper">Helper for binding event handlers</param>
        /// <returns>True if the manager was initialized, False if it was previously initialized.</returns>
        internal bool Init(IModHelper helper)
        {
            if (Helper is null)
            {
                Helper = helper;
                Helper.Events.Multiplayer.PeerConnected += OnPeerConnected;
                Helper.Events.Multiplayer.PeerDisconnected += OnPeerDisconnected;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Clears existing multiplayer contexts, removing connected player data.
        /// </summary>
        public void ClearConnections()
        {
            MultiplayerContexts.Clear();
        }

        /// <summary>
        /// Checks if the current active <see cref="Game1.player"/> exists in the multiplayer contexts.
        /// </summary>
        /// <returns>True if <see cref="Game1.player"/> exists in the multiplayer contexts</returns>
        public bool IsConnected()
        {
            return Context.IsMainPlayer || MultiplayerContexts.ContainsKey(Game1.player.UniqueMultiplayerID);
        }

        /// <summary>
        /// Gets the context of the corrent active <see cref="Game1.player"/> if <see cref="Game1.player"/> is connected.
        /// </summary>
        /// <param name="context">The context of the corrent active <see cref="Game1.player"/></param>
        /// <returns>True if <see cref="Game1.player"/> is connected, False otherwise.</returns>
        public bool TryGetValue([MaybeNullWhen(false)] out T context)
        {
            if (Context.IsMainPlayer)
            {
                if (!MultiplayerContexts.TryGetValue(0, out context))
                {
                    context = MultiplayerContexts[0] = Activator.CreateInstance<T>();
                }
                return true;
            }

            return MultiplayerContexts.TryGetValue(Game1.player.UniqueMultiplayerID, out context);
        }

        private void OnPeerConnected(object? sender, PeerConnectedEventArgs e)
        {
            MultiplayerContexts[e.Peer.PlayerID] = Activator.CreateInstance<T>();
        }

        private void OnPeerDisconnected(object? sender, PeerDisconnectedEventArgs e)
        {
            MultiplayerContexts.Remove(e.Peer.PlayerID);
        }
    }
}
