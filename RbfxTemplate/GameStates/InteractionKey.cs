using System;
using Urho3DNet;

namespace RbfxTemplate.GameStates
{
    /// <summary>
    /// Touch or mouse interaction key.
    /// </summary>
    public readonly struct InteractionKey : IEquatable<InteractionKey>
    {
        /// <summary>
        /// Is source a mouse or a touch?
        /// </summary>
        public readonly bool IsMouse;

        /// <summary>
        /// Mouse button or Touch identifier.
        /// </summary>
        public readonly int SourceId;

        /// <summary>
        /// Qualifiers mask.
        /// </summary>
        public readonly Qualifier Qualifiers;

        /// <summary>
        /// Construct interaction key from touch event.
        /// </summary>
        /// <param name="touchId">Touch identifier.</param>
        public InteractionKey(int touchId)
        {
            IsMouse = false;
            SourceId = touchId;
            Qualifiers = 0;
        }

        /// <summary>
        /// Construct interaction key from mouse event.
        /// </summary>
        /// <param name="buttonId">Mouse button identifier.</param>
        public InteractionKey(MouseButton buttonId, Qualifier qualifiers)
        {
            IsMouse = true;
            SourceId = (int)buttonId;
            Qualifiers = qualifiers;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = IsMouse.GetHashCode();
                hashCode = (hashCode * 397) ^ SourceId;
                hashCode = (hashCode * 397) ^ (int)Qualifiers;
                return hashCode;
            }
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>Returns true if the current object is equal to the other parameter; otherwise returns false.</returns>
        public bool Equals(InteractionKey other)
        {
            return IsMouse == other.IsMouse && SourceId == other.SourceId && Qualifiers == other.Qualifiers;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            return obj is InteractionKey other && Equals(other);
        }

        public static bool operator ==(InteractionKey left, InteractionKey right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(InteractionKey left, InteractionKey right)
        {
            return !left.Equals(right);
        }
    }
}