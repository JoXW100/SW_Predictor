using DynamicUIFramework.Events;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using System.Diagnostics;

namespace DynamicUIFramework.Elements
{
    public abstract class UIElement : IUIElement
    {
        public static bool DebugEnabled = false;

        public event EventHandler<CursorEventArgs>? OnCursorEnter;
        public event EventHandler<CursorEventArgs>? OnCursorLeave;
        public event EventHandler<CursorEventArgs>? OnCursorOver;
        public event EventHandler<CursorEventArgs>? OnClick;
        public event EventHandler<EventArgs>? OnChange;
        public event EventHandler<DrawUIEventArgs>? OnDrawStart;
        public event EventHandler<DrawUIEventArgs>? OnDrawEnd;

        public virtual IEnumerable<IUIElement?> Children
        {
            get => m_children;
            set
            {
                if (m_children != value)
                {
                    m_children = value.ToArray();
                    HasChanged = true;
                }
            }
        }

        public virtual bool IsVisible
        {
            get => m_isVisible;
            set
            {
                if (m_isVisible != value)
                {
                    m_isVisible = value;
                    Bounds = Rectangle.Empty;
                    HasChanged = true;
                }
            }
        }

        public virtual bool HasChanged
        {
            get => m_hasChanged;
            protected set
            {
                if (m_hasChanged != value)
                {
                    m_hasChanged = value;
                    if (m_hasChanged)
                    {
                        OnChange?.Invoke(this, EventArgs.Empty);
                    }
                }
            }
        }

        public virtual Rectangle Bounds { get; protected set; }

        protected IUIElement?[] m_children;
        protected bool m_isVisible;
        protected bool m_hasChanged;
        protected bool m_containsCursor;

        public UIElement(IEnumerable<IUIElement?>? children = null)
        {
            m_children = children?.ToArray() ?? Array.Empty<IUIElement?>();
            m_isVisible = true;
            m_hasChanged = true;
            Bounds = Rectangle.Empty;
        }

        public virtual IUIElement? GetChildAt(Point point)
        {
            foreach (var child in Children)
            {
                // Works with non-overlaping children
                if (child is not null && child.Bounds.Contains(point))
                {
                    return child.GetChildAt(point) ?? child;
                }
            }
            return null;
        }

        /// <summary>
        /// Invokes cursor events depending on the cursor position.
        /// </summary>
        protected void CheckCursorEvents()
        {
            if (OnCursorEnter is null && OnCursorLeave is null && OnCursorOver is null && OnClick is null)
            {
                return;
            }

            var cursor = Game1.getMousePosition();

            if (Bounds.Contains(cursor))
            {
                if (!m_containsCursor)
                {
                    m_containsCursor = true;
                    CursorEnter(cursor);
                }

                CursorOver(cursor);

                if (Game1.didPlayerJustClickAtAll())
                {
                    Click(cursor);
                }
            }
            else if (m_containsCursor)
            {
                m_containsCursor = false;
                CursorLeave(cursor);
            }
        }

        /// <summary>
        /// Draws debug info for the element, only called in DEBUG mode.
        /// </summary>
        /// <param name="sb">The <see cref="SpriteBatch"/> to draw the drawable to.</param>
        [Conditional("DEBUG")]
        protected virtual void DrawDebug(SpriteBatch sb)
        {
            if (DebugEnabled && IsVisible)
            {
                sb.DrawBorder(Bounds, 1f, color: Color.Red);
            }
        }

        /// <summary>
        /// Invokes the <see cref="OnCursorOver"/> event.
        /// </summary>
        /// <param name="location">The cursor location.</param>
        protected void CursorOver(Point location)
        {
            OnCursorOver?.Invoke(this, new CursorEventArgs() { Location = location });
        }

        /// <summary>
        /// Invokes the <see cref="OnCursorLeave"/> event.
        /// </summary>
        /// <param name="location">The cursor location.</param>
        protected void CursorEnter(Point location)
        {
            OnCursorEnter?.Invoke(this, new CursorEventArgs() { Location = location });
        }

        /// <summary>
        /// Invokes the <see cref="OnCursorLeave"/> event.
        /// </summary>
        /// <param name="location">The cursor location.</param>
        protected void CursorLeave(Point location)
        {
            OnCursorLeave?.Invoke(this, new CursorEventArgs() { Location = location });
        }

        /// <summary>
        /// Invokes the <see cref="OnClick"/> event.
        /// </summary>
        /// <param name="location">The cursor location.</param>
        protected void Click(Point location)
        {
            OnClick?.Invoke(this, new CursorEventArgs() { Location = location });
        }

        /// <summary>
        /// Invokes the <see cref="OnDrawStart"/> event.
        /// </summary>
        /// <param name="sb">The <see cref="SpriteBatch"/> to draw the drawable to.</param>
        /// <param name="offset">The positional offset when drawing.</param>
        /// <param name="size">The size of the drawable.</param>
        protected void DrawStart(SpriteBatch sb, Point? offset = null, Point? size = null)
        {
            OnDrawStart?.Invoke(this, new DrawUIEventArgs(sb) { Offset = offset, Size = size });
        }

        /// <summary>
        /// Invokes the <see cref="OnDrawEnd"/> event.
        /// </summary>
        /// <param name="sb">The <see cref="SpriteBatch"/> to draw the drawable to.</param>
        /// <param name="offset">The positional offset when drawing.</param>
        /// <param name="size">The size of the drawable.</param>
        protected void DrawEnd(SpriteBatch sb, Point? offset = null, Point? size = null)
        {
            OnDrawEnd?.Invoke(this, new DrawUIEventArgs(sb) { Offset = offset, Size = size });
        }

        public abstract void Draw(SpriteBatch sb, Point? offset = null, Point? size = null);
    }
}
