using DynamicUIFramework.Events;
using Microsoft.Xna.Framework;

namespace DynamicUIFramework
{
    public interface IUIElement : IUIDrawable
    {
        /// <summary>
        /// Event triggered when cursor enters the bounds of the element.
        /// </summary>
        public event EventHandler<CursorEventArgs>? OnCursorEnter;
        /// <summary>
        /// Event triggered when cursor leaves the bounds of the element.
        /// </summary>
        public event EventHandler<CursorEventArgs>? OnCursorLeave;
        /// <summary>
        /// Event triggered each frame while cursor is inside the bounds of the element.
        /// </summary>
        public event EventHandler<CursorEventArgs>? OnCursorOver;
        /// <summary>
        /// Event triggered when the element changes.
        /// </summary>
        public event EventHandler<EventArgs>? OnChange;
        /// <summary>
        /// Event triggered when clicking (left mouse, right mouse, gamepad X etc.) inside the UI element.
        /// </summary>
        public event EventHandler<CursorEventArgs>? OnClick;
        /// <summary>
        /// Event triggered before drawing the element.
        /// </summary>
        public event EventHandler<DrawUIEventArgs>? OnDrawStart;
        /// <summary>
        /// Event triggered after drawing the element.
        /// </summary>
        public event EventHandler<DrawUIEventArgs>? OnDrawEnd;

        /// <summary>
        /// The contained child elements.
        /// </summary>
        public IEnumerable<IUIElement?> Children { get; set; }

        /// <summary>
        /// <see langword="true"/> if the element is visible, <see langword="false"/> otherwise.
        /// </summary>
        public bool IsVisible { get; set; }

        /// <summary>
        /// <see langword="true"/> if element has changed since the last draw call, <see langword="false"/> otherwise.
        /// </summary>
        public bool HasChanged { get; }

        /// <summary>
        /// The bounds containing the element, <see cref="Rectangle.Empty"/> if not drawn before.
        /// </summary>
        public Rectangle Bounds { get; }

        /// <summary>
        /// Gets the child element at the given position, or <see langword="null"/> if no child exists at the given position.
        /// </summary>
        /// <param name="position">The window postion to search</param>
        /// <returns>The child element at the given position, or <see langword="null"/></returns>
        public IUIElement? GetChildAt(Point point);
    }
}
