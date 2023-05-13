using UnityEngine;

namespace ModCompanion.Data
{
    /// <summary>
    /// Represents a model for instructions.
    /// </summary>
    public class Instruction : MonoBehaviour
    {
        /// <summary>
        /// System instructions
        /// </summary>
        public string FromSystem { get; set; } = string.Empty;
        /// <summary>
        /// User instructions
        /// </summary>
        public string FromUser { get; set; } = string.Empty;

        public Instruction() { }

    }
}
