namespace LimDB.lib.Objects.Base
{
    /// <summary>
    /// Base class for all database objects, providing core tracking properties.
    /// </summary>
    public class BaseObject
    {
        /// <summary>
        /// Gets or sets the unique identifier for this object.
        /// This is automatically assigned during insertion.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this object is active.
        /// This is automatically set to true during insertion.
        /// </summary>
        public bool Active { get; set; }

        /// <summary>
        /// Gets or sets the date and time when this object was last modified (UTC).
        /// This is automatically updated during insertion and update operations.
        /// </summary>
        public DateTime Modified { get; set; }

        /// <summary>
        /// Gets or sets the date and time when this object was created (UTC).
        /// This is automatically set during insertion.
        /// </summary>
        public DateTime Created { get; set; }
    }
}