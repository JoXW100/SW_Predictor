using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using System.Diagnostics.CodeAnalysis;

namespace Predictor.Framework
{
    public sealed class MultiplayerManager<T> where T : class
    {
        private readonly Dictionary<long, T> MultiplayerContexts;
        private IModHelper? Helper;

        public MultiplayerManager(IModHelper? helper = null)
        {
            Helper = helper;
            MultiplayerContexts = new();
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
        /// Gets the context of the corrent active <see cref="Game1.player"/> if <see cref="Game1.player"/> is connected.
        /// </summary>
        /// <param name="context">The context of the corrent active <see cref="Game1.player"/></param>
        /// <returns>True if <see cref="Game1.player"/> is connected, False otherwise.</returns>
        public bool TryGetValue([MaybeNullWhen(false)] out T context)
        {
            return MultiplayerContexts.TryGetValue(Context.ScreenId, out context);
        }

        public T GetValue()
        {
            if (MultiplayerContexts.TryGetValue(Context.ScreenId, out var context))
            {
                return context;
            }
            return MultiplayerContexts[Context.ScreenId] = Activator.CreateInstance<T>();
        }

        private void OnPeerConnected(object? sender, PeerConnectedEventArgs e)
        {
            if (e.Peer.IsSplitScreen)
            {
                MultiplayerContexts[e.Peer.ScreenID ?? 0] = Activator.CreateInstance<T>();
            }
        }

        private void OnPeerDisconnected(object? sender, PeerDisconnectedEventArgs e)
        {
            if (e.Peer.IsSplitScreen)
            {
                MultiplayerContexts.Remove(e.Peer.ScreenID ?? 0);
            }
        }
    }
}
