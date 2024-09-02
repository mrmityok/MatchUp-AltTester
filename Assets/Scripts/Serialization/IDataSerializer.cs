using System;
using System.Collections.Generic;

namespace Serialization
{
    public interface IDataSerializeTransaction
    {
	    bool Commit();
        bool Rollback();
    }

	/// <summary>
	/// </summary>
	public interface IDataSerializeTransactionHolder
	{
		/// <summary>
		/// Add and return new transaction
		/// </summary>
		IDataSerializeTransaction BeginTransaction();

		/// <summary>
		/// Emit specified delegate when current (last) transaction will be successfully finished 
		/// but before commit will by raised
		/// </summary>
		bool AddTrSuccessHandler(Action onSuccess);

		/// <summary>
		/// Emit specified delegate when current (last) transaction will be failed 
		/// but before rollback will by raised
		/// </summary>
		bool AddTrFailHandler(Action onFail);

		/// <summary>
		/// Emit specified delegate when current (last) transaction will be commited
		/// </summary>
		bool AddTrCommittedHandler(Action onCommitted);

		/// <summary>
		/// Emit specified delegate when current (last) transaction will be rollbacked
		/// </summary>
		bool AddTrRollbackedHandler(Action onRollbacked);
	}

	/// <summary>
    /// Allows to read from it values with specified names 
    /// and update IReadDataSerializable from it
    /// </summary>
    public interface IReadDataSerializer : IDataSerializeTransactionHolder
    {
        /// <summary>
        /// Read from serializer value with specified name
        /// </summary>
        T Read<T>(string name, bool allowNull = false) where T : class;

        /// <summary>
        /// Read from serializer IReadDataSerializable with specified name
        /// </summary>
        bool Read(string name, IReadDataSerializable data);
        
        /// <summary>
        /// Read from serializer array of IReadDataSerializable with specified name
        /// </summary>
        bool Read(string name, IReadDataSerializable[] data);
        bool Read(string name, IEnumerable<IReadDataSerializable> data);

        /// <summary>
        /// Read from serializer IReadDataSerializable with specified index 
        /// (current object mast be list of IReadDataSerializable)
        /// </summary>
        bool Read(int index, IReadDataSerializable data);
        
        /// <summary>
        /// Return length of current object if it is list of objects or values
        /// </summary>
        int ArrayLength { get; }

        /// <summary>
        /// Determine whether current object contains value or object with specified name and its value is null
        /// </summary>
        bool IsNull(string name);

        /// <summary>
        /// Due to iOS AOT compilation only
        /// </summary>
        bool ReadBool(string name, ref bool ok, bool allowNull = false, bool defaultValue = false);
        int ReadInt(string name, ref bool ok, bool allowNull = false, int defaultValue = 0);
        uint ReadUInt(string name, ref bool ok, bool allowNull = false);
        long ReadLong(string name, ref bool ok, bool allowNull = false, long defaultValue = 0);
        ulong ReadULong(string name, ref bool ok, bool allowNull = false, ulong defaultValue = 0);
        float ReadFloat(string name, ref bool ok, bool allowNull = false, float defaultValue = 0);
        double ReadDouble(string name, ref bool ok, bool allowNull = false, double defaultValue = 0);
        DateTime? ReadDateTime(string name, string format, ref bool ok, bool allowNull = false, DateTime? defaultValue = null);
        string ReadString(string name, ref bool ok, bool allowNull = true);

        /// <summary>
        /// Return names of all current object's values
        /// </summary>
        IEnumerable<string> Keys { get; }
    }

    /// <summary>
    /// Allows to read and write to it values with specified names 
    /// and save IReadDataSerializable inside it
    /// </summary>
    public interface IDataSerializer : IReadDataSerializer
    {
        /// <summary>
        /// Write into serializer new value with specified name
        /// </summary>
        bool Write(string name, object data);

        /// <summary>
        /// Write into serializer new IDataSerializable object with specified name
        /// </summary>
        bool Write(string name, IDataSerializable data);

        /// <summary>
        /// Write into serializer new object as array of IDataSerializable with specified name
        /// </summary>
        bool Write(string name, IDataSerializable[] dataList);
        bool Write(string name, IEnumerable<IDataSerializable> dataList);
    }
}