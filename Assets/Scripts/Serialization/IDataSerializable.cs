namespace Serialization
{
    /// <summary>
    /// Can be updated from IDataSerializer
    /// </summary>
    public interface IReadDataSerializable
    {
        /// <summary>
        /// Updated data from IDataSerializer
        /// </summary>
        bool Load(IReadDataSerializer serializer);
    }

    /// <summary>
    /// Can be updated from and saved to IDataSerializer
    /// </summary>
    public interface IDataSerializable : IReadDataSerializable
    {
        /// <summary>
        /// Save data to IDataSerializer
        /// </summary>
        bool Save(IDataSerializer serializer);

        /// <summary>
        /// Determine whether data is list
        /// </summary>
        bool IsArray { get; }
    }
}