using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using System.Diagnostics.CodeAnalysis;

namespace PredictorPatchFramework
{
    public sealed class MultiplayerManager<T> where T : class
    {
        private readonly Dictionary<long, T> MultiplayerContexts;
        private IModHelper? Helper;
        private IMonitor? Monitor;

        public MultiplayerManager(IModHelper? helper = null, IMonitor? monitor = null)
        {
            Helper = helper;
            Monitor = monitor;
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
        public bool Init(IModHelper helper, IMonitor? monitor = null)
        {
            if (Helper is null)
            {
                Helper = helper;
                Helper.Events.Multiplayer.PeerConnected += OnPeerConnected;
                Helper.Events.Multiplayer.PeerDisconnected += OnPeerDisconnected;
                return true;
            }
            if (Monitor is not null)
            {
                Monitor = monitor;
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
        /// Gets the context of the current active <see cref="Game1.player"/> if <see cref="Game1.player"/> is connected.
        /// </summary>
        /// <param name="context">The context of the current active <see cref="Game1.player"/></param>
        /// <returns>True if <see cref="Game1.player"/> is connected, False otherwise.</returns>
        public bool TryGetValue([MaybeNullWhen(false)] out T context)
        {
            return MultiplayerContexts.TryGetValue(Context.ScreenId, out context);
        }

        /// <summary>
        /// Gets the context of the current active <see cref="Game1.player"/> or creates a new context if no context exists.
        /// </summary>
        /// <param name="factory">The context factory function used to create a new context.</param>
        /// <returns>The context of the current active <see cref="Game1.player"/></returns>
        public T GetValue(Func<T>? factory= null)
        {
            if (MultiplayerContexts.TryGetValue(Context.ScreenId, out var context))
            {
                return context;
            }
            return MultiplayerContexts[Context.ScreenId] = (factory ?? Activator.CreateInstance<T>).Invoke();
        }

        /// <summary>
        /// Creates a new context for the current active <see cref="Game1.player"/> and returns it.
        /// </summary>
        /// <param name="factory">The context factory function used to create a new context.</param>
        /// <returns>The new context of the current active <see cref="Game1.player"/></returns>
        public T CreateValue(Func<T>? factory = null)
        {
            return MultiplayerContexts[Context.ScreenId] = (factory ?? Activator.CreateInstance<T>).Invoke();
        }

        private void OnPeerConnected(object? sender, PeerConnectedEventArgs e)
        {
            if (e.Peer.IsSplitScreen)
            {
                var id = e.Peer.ScreenID;
                if (id.HasValue)
                {
                    MultiplayerContexts[id.Value] = Activator.CreateInstance<T>();
                }
                else
                {
                    Monitor?.Log("Splitscreen peer had no screen id.", LogLevel.Warn);
                }
            }
        }

        private void OnPeerDisconnected(object? sender, PeerDisconnectedEventArgs e)
        {
            if (e.Peer.IsSplitScreen)
            {
                var id = e.Peer.ScreenID;
                if (id.HasValue)
                {
                    MultiplayerContexts.Remove(id.Value);
                }
                else
                {
                    Monitor?.Log("Splitscreen peer had no screen id.", LogLevel.Warn);
                }
            }
        }
    }
}
