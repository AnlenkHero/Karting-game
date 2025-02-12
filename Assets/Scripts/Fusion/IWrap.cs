namespace Kart.Fusion
{
    /// <summary>
    /// Provides a standard interface for wrapping a reference type for network serialization.
    /// </summary>
    public interface IWrap<T>
    {
        /// <summary>
        /// Unwraps the stored value.
        /// </summary>
        T Unwrap();

        /// <summary>
        /// Wraps the provided value.
        /// </summary>
        void Wrap(T value);
    }
}