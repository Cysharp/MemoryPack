namespace BinaryPack.Models.Interfaces
{
    /// <summary>
    /// An <see langword="interface"/> for models that support a default initialization
    /// </summary>
    public interface IInitializable
    {
        /// <summary>
        /// Loads the custom default values for a given model
        /// </summary>
        void Initialize();
    }
}
