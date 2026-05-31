using System;
using System.Collections;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

using Microsoft.Win32;
using System.Collections.Generic;

namespace Serveron.Utility.Core
{
	/// <summary>
	/// A RegistryPersistable knows how to save itself
	/// to the Registry and restore itself at a later time.
	/// </summary>
	abstract public class RegistryPersistable
	{
		#region Private state, construction and disposal

		/// <summary>
		/// Log4Net support
		/// </summary>
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(RegistryPersistable));
        protected string _topLevelKeyName = null;
        protected string _subkeyName = null;
        protected string _valueName = null;

		/// <summary>
		/// Create a RegistryPersistable that is not bound
		/// to a particular Registry location and that has
		/// not attempt to load itself from the Registry.
		/// The object may be persisted (and a Registry
		/// binding established for the duration of the
		/// instance lifetime) by calling the three-argument
		/// form of Save().
		/// </summary>
		public RegistryPersistable()
		{
		}

		/// <summary>
		/// Create a RegistryPersistable by loading from
		/// the Windows Registry from the given top-level
		/// key, subkey and named value.  If the subkey or
		/// named value does not exist, no exception is
		/// thrown and the PersistableDictionary starts empty.
		/// </summary>
		/// <param name="topLevelKeyName">"HKLM", "HKEY_LOCAL_MACHINE",
		/// "HKCU", or "HKEY_CURRENT_USER", case-insensitive.</param>
		/// <param name="subkeyName">a Registry subkey path</param>
		/// <param name="valueName">a Registry value name</param>
		public RegistryPersistable(string topLevelKeyName, string subkeyName, string valueName)
		{
			_topLevelKeyName = topLevelKeyName;
			_subkeyName = subkeyName;
			_valueName = valueName;
		}

		#endregion

		#region Public interface

		/// <summary>
		/// Save our contents to the Registry at the
		/// location established when this instances
		/// was loaded (or created).  If Registry path
		/// information was not previously set, an
		/// exception is thrown.
		/// </summary>
		public void Save()
		{
			if (this._topLevelKeyName == null || this._subkeyName == null || this._valueName == null)
			{
				throw new InvalidOperationException("Registry path information not set.");
			}

			SaveImpl();
		}

		/// <summary>
		/// Save our contents to the Registry at the
		/// given top-level key, subkey and value name.
		/// The arguments are saved internally as the
		/// new Registry location for the object.
		/// </summary>
		/// <param name="topLevelKeyName">"HKLM", "HKEY_LOCAL_MACHINE",
		/// "HKCU", or "HKEY_CURRENT_USER", case-insensitive.</param>
		/// <param name="subkeyName"></param>
		/// <param name="valueName"></param>
		public void Save(string topLevelKeyName, string subkeyName, string valueName)
		{
			this._topLevelKeyName = topLevelKeyName;
			this._subkeyName = subkeyName;
			this._valueName = valueName;

			SaveImpl();
		}

		/// <summary>
		/// Return the contents serialized as a blob.  The
		/// return value is idential to the bytes that would
		/// be saved to the Registry by the Save() methods.
		/// </summary>
		/// <returns></returns>
		public virtual byte[] Serialize()
		{
			return Static.Serialize(GetSerializableObject());
		}

		/// <summary>
		/// Load the contents from the blob found in HKCU at the
		/// given top-level key, subkey and value name.  The top-level
		/// key, subkey and name are saved internally as the Registry
		/// location of the object.  They may be changed (effectively
		/// copying the object to a new Registry location) by calling
		/// the three-argument form of Save().
		/// </summary>
		/// <param name="topLevelKeyName"></param>
		/// <param name="subkeyName"></param>
		/// <param name="valueName"></param>
		public void Load(string topLevelKeyName, string subkeyName, string valueName)
		{
			this._topLevelKeyName = topLevelKeyName;
			this._subkeyName = subkeyName;
			this._valueName = valueName;

			LoadImpl();
		}

		/// <summary>
		/// Load the contents from the blob found in HKCU at the
		/// top-level key, subkey and value name previously bound
		/// to the instance.  If Registry information not previously
		/// set, an exception is thrown.
		/// </summary>
		public void Load()
		{
			if (this._topLevelKeyName == null || this._subkeyName == null || this._valueName == null)
			{
				throw new InvalidOperationException("Registry path information not set.");
			}

			LoadImpl();
		}

		/// <summary>
		/// Deserialize the contents from the blob passed as an argument.
		/// </summary>
		/// <param name="blob"></param>
		public object Deserialize(byte[] blob)
		{
			return Static.Deserialize(blob);
		}

		/// <summary>
		/// This class serves strictly as a namespace for the
		/// static methods it contains.
		/// </summary>
		public class Static
		{
			/// <summary>
			/// Serialize the object and return the blob
			/// </summary>
			/// <param name="obj"></param>
			/// <returns></returns>
			public static byte[] Serialize(object obj)
			{
				using (MemoryStream ms = new MemoryStream())
				{
					new BinaryFormatter().Serialize(ms, obj);
					ms.Flush();
					return ms.ToArray();
				}
			}

			/// <summary>
			/// Deserialize the object from the blob passed as an argument.
			/// </summary>
			/// <param name="blob"></param>
			public static object Deserialize(byte[] blob)
			{
				MemoryStream ms = null;
				try
				{
					ms = new MemoryStream(blob);
					return new BinaryFormatter().Deserialize(ms);
				}
				catch (Exception ex)
				{
					log.Info(String.Format("RegistryPersistable.Deserialize(): {0}: {1}", ex.GetType().Name, ex.Message));
					throw;
				}
				finally
				{
					if (ms != null) ms.Close();
				}
			}

			/// <summary>
			/// Delete a value from a key
			/// </summary>
			/// <param name="topLevelKeyName"></param>
			/// <param name="subkeyName"></param>
			/// <param name="valueName"></param>
			public static void Delete(string topLevelKeyName, string subkeyName, string valueName)
			{
				using (RegistryKey topLevel = GetTopLevelKey(topLevelKeyName))
				{
					RegistryKey subkey = null;
					try
					{
						subkey = topLevel.OpenSubKey(subkeyName, true);
						if (subkey != null)
						{
							System.Collections.Generic.List<string> v = new System.Collections.Generic.List<string>(subkey.GetValueNames());
							if (v.Contains(valueName))
								subkey.DeleteValue(valueName);
						}
					}
					catch (Exception ex)	// log and eat any error
					{
						log.Error("Deleting a registry entry.", ex);
					}
					finally
					{
						if (subkey != null) ((IDisposable)subkey).Dispose();
					}
				}
			}


		}

		#endregion

		#region Subclass interface

		/// <summary>
		/// Return the object to serialize.  The object may
		/// be a collection, so long as all the objects in
		/// the collection are Serializable.
		/// </summary>
		/// <returns>object to serialize</returns>
		protected abstract object GetSerializableObject();

		/// <summary>
		/// The argument is the object just deserialized
		/// from the Registry store.  The subclass can do
		/// as it sees fit, i.e. save a reference.
		/// </summary>
		/// <param name="obj"></param>
		protected abstract void SetSerializableObject(object obj);

		/// <summary>
		/// A Load() request failed to find or correctly load
		/// a copy of the content.  The subclass may set an
		/// empty collection, etc.
		/// </summary>
		protected abstract void SetDefaultSerializableObject();

		#endregion

		#region Private implementation

		/// <summary>
		/// Save the contents at the bound Registry path.
		/// </summary>
		protected virtual void SaveImpl()
		{
			using (RegistryKey topLevel = GetTopLevelKey(_topLevelKeyName))
			{
				RegistryKey subkey = null;
				try
				{
					subkey = topLevel.CreateSubKey(_subkeyName);
					if (subkey != null)
					{
						subkey.SetValue(_valueName, Serialize());
					}
				}
				finally
				{
					if (subkey != null) ((IDisposable)subkey).Dispose();
				}
			}
		}

		/// <summary>
		/// Load the object from the bound Registry path
		/// </summary>
		private void LoadImpl()
		{
			// top level open should not fail...if it does, something is
			// seriously wrong or there is a serious bug in the code, so
			// let the exception propogate.

			using (RegistryKey topLevel = GetTopLevelKey(_topLevelKeyName))
			{
				RegistryKey subkey = null;

				try
				{
					subkey = topLevel.OpenSubKey(_subkeyName);
					if (subkey != null)
					{
                        byte[] blob = subkey.GetValue(_valueName) as byte[];
                        object theValue = (blob != null) ? Deserialize(blob) : null;
						if (theValue != null)
							SetSerializableObject(theValue);
					}
				}
				catch (ArgumentException e1)
				{
					log.Info(String.Format("PersistableDictionary.Load(): {0}: {1}", e1.GetType().Name, e1.Message));
				}
				catch (System.Security.SecurityException e2)
				{
					log.Info(String.Format("PersistableDictionary.Load(): {0}: {1}", e2.GetType().Name, e2.Message));
				}
				finally
				{
					if (GetSerializableObject() == null) SetDefaultSerializableObject();
					if (subkey != null) ((IDisposable)subkey).Dispose();
				}
			}
		}

		/// <summary>
		/// Get a top-level key by name.  Only local machine
		/// and current user are supported.
		/// </summary>
		/// <param name="topLevelKeyName"></param>
		/// <returns></returns>
		static protected RegistryKey GetTopLevelKey(string topLevelKeyName)
		{
			switch (topLevelKeyName.ToLower())
			{
				case "hklm":
				case "hkey_local_machine":
					return Registry.LocalMachine;

				case "hkcu":
				case "hkey_current_user":
					return Registry.CurrentUser;

				default:
					throw new ArgumentOutOfRangeException("topLevelKeyName", "unknown top level key");
			}
		}

		#endregion
	}

	/// <summary>
	/// An IDictionary, based on System.Collections.Hashtable,
	/// which knows how to persist itself to the local Registry.
	/// </summary>
	public class PersistableDictionary: RegistryPersistable, IDictionary
	{
		#region Private state, construction and disposal

		/// <summary>
		/// Log4Net support
		/// </summary>
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(PersistableDictionary));
		private IDictionary _hashtable = null;

		/// <summary>
		/// Create a PersistableDictionary by loading from
		/// the Windows Registry from the given top-level
		/// key, subkey and named value.  If the subkey or
		/// named value does not exist, no exception is
		/// thrown and the PersistableDictionary starts empty.
		/// </summary>
		/// <param name="topLevelKeyName">"HKLM", "HKEY_LOCAL_MACHINE",
		/// "HKCU", or "HKEY_CURRENT_USER", case-insensitive.</param>
		/// <param name="subkey">a Registry subkey path</param>
		/// <param name="valueName">a Registry value name</param>
		public PersistableDictionary(string topLevelKeyName, string subkeyName, string valueName):
			base(topLevelKeyName, subkeyName, valueName)
		{
			this.Load();
		}

		/// <summary>
		/// Create a default PersistableDictionary.
		/// </summary>
		public PersistableDictionary()
		{
			_hashtable = new Hashtable();
		}

		/// <summary>
		/// Create a new PersistableDictionary as a shallow
		/// copy of the argument IDictionary.  This constructor
		/// should be used when you wish to create a new
		/// PersistableDictionary with non-default load factor,
		/// hash code provider, etc.  First, create a
		/// System.Collections.Hashtable with the desired
		/// properties.  Then pass it to this constructor.
		/// The dictionary held internally by this class will
		/// retain both the non-default properties and the
		/// content references, if any, of the prototype.
		/// </summary>
		/// <param name="prototype">prototype dictionary</param>
		public PersistableDictionary(Hashtable prototype)
		{
			_hashtable = (IDictionary)prototype.Clone();
		}

		/// <summary>
		/// Create a PersistableDictionary by loading from
		/// the Windows Registry from the given top-level
		/// key, subkey and named value.  If the subkey or
		/// named value does not exist, no exception is
		/// thrown and the PersistableDictionary starts empty.
		/// This constructor
		/// should be used when you wish to create a new
		/// PersistableDictionary with non-default load factor,
		/// hash code provider, etc.  First, create a
		/// System.Collections.Hashtable with the desired
		/// properties.  Then pass it to this constructor.
		/// The dictionary held internally by this class will
		/// retain the non-default properties of the prototype.
		/// It will not retain the contents, if any, of the
		/// prototype; instead, it will be loaded initially
		/// from the Registry location defined by the arguments.
		/// </summary>
		/// <param name="prototype">a prototype Dictionary</param>
		/// <param name="topLevelKeyName">"HKLM", "HKEY_LOCAL_MACHINE",
		/// "HKCU", or "HKEY_CURRENT_USER", case-insensitive.</param>
		/// <param name="subkey">a Registry subkey path</param>
		/// <param name="valueName">a Registry value name</param>
		public PersistableDictionary(Hashtable prototype,
			string topLevelKeyName, string subkeyName, string valueName):
			base(topLevelKeyName, subkeyName, valueName)
		{
			_hashtable = (IDictionary)prototype.Clone();
			_hashtable.Clear();
			this.Load();
		}

		#endregion

		#region Overrides

		/// <summary>
		/// Return the object to serialize.  The object may
		/// be a collection, so long as all the objects in
		/// the collection are Serializable.
		/// </summary>
		/// <returns>object to serialize</returns>
		protected override object GetSerializableObject()
		{
			return this._hashtable;
		}

		/// <summary>
		/// The argument is the object just deserialized
		/// from the Registry store.  The subclass can do
		/// as it sees fit, i.e. save a reference.
		/// </summary>
		/// <param name="obj"></param>
		protected override void SetSerializableObject(object obj)
		{
			this._hashtable = (Hashtable)obj;
		}

		/// <summary>
		/// A Load() request failed to find or correctly load
		/// a copy of the content.  The subclass may set an
		/// empty collection, etc.
		/// </summary>
		protected override void SetDefaultSerializableObject()
		{
			this._hashtable = new Hashtable();
		}

		#endregion

		#region IDictionary Members

		/// <summary>
		/// PersistableDictionaries are all read/write
		/// </summary>
		public bool IsReadOnly
		{
			get
			{
				return false;
			}
		}

		/// <summary>
		/// Return the IDictionaryEnumerator for this instance.
		/// </summary>
		/// <returns></returns>
		public IDictionaryEnumerator GetEnumerator()
		{
			return _hashtable.GetEnumerator();
		}

		/// <summary>
		/// 
		/// </summary>
		public object this[object key]
		{
			get
			{
				return _hashtable[key];
			}
			set
			{
				_hashtable[key] = value;
			}
		}

		/// <summary>
		/// Remove the element with the specified key
		/// </summary>
		/// <param name="key"></param>
		public void Remove(object key)
		{
			_hashtable.Remove(key);
		}

		/// <summary>
		/// Return true if this instance contains the
		/// specified key.
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public bool Contains(object key)
		{
			return _hashtable.Contains(key);
		}

		/// <summary>
		/// Clear this instance.
		/// </summary>
		public void Clear()
		{
			_hashtable.Clear();
		}

		/// <summary>
		/// Get the ICollection containing this instance's values.
		/// </summary>
		public ICollection Values
		{
			get
			{
				return _hashtable.Values;
			}
		}

		/// <summary>
		/// Add an element with the specified value to this
		/// instance at the given key.
		/// </summary>
		/// <param name="key"></param>
		/// <param name="value"></param>
		public void Add(object key, object value)
		{
			_hashtable.Add(key, value);
		}

		/// <summary>
		/// Get the ICollection containing this instance's keys.
		/// </summary>
		public ICollection Keys
		{
			get
			{
				return _hashtable.Keys;
			}
		}

		/// <summary>
		/// PersistableDictionaries are not fixed-size.
		/// </summary>
		public bool IsFixedSize
		{
			get
			{
				return false;
			}
		}

		#endregion

		#region ICollection Members

		/// <summary>
		/// PersistableDictionaries do not support synchronization.
		/// http://blogs.msdn.com/brada/archive/2003/09/28/50391.aspx
		/// Caller must handle locking.
		/// </summary>
		public bool IsSynchronized
		{
			get
			{
				return false;
			}
		}

		/// <summary>
		/// Return the count of items contained in this instance.
		/// </summary>
		public int Count
		{
			get
			{
				return _hashtable.Count;
			}
		}

		/// <summary>
		/// Copy the elements of this instance to the given array
		/// starting at the given index.
		/// </summary>
		/// <param name="array"></param>
		/// <param name="index"></param>
		public void CopyTo(Array array, int index)
		{
			_hashtable.CopyTo(array, index);
		}

		/// <summary>
		/// Synchronization is not supported by PersistableDictionaries.
		/// http://blogs.msdn.com/brada/archive/2003/09/28/50391.aspx
		/// Caller must handle locking.
		/// </summary>
		public object SyncRoot
		{
			get
			{
				throw new NotSupportedException("SyncRoot");
			}
		}

		#endregion

		#region IEnumerable Members

		/// <summary>
		/// Return an IDictionaryEnumerator for this instance.
		/// </summary>
		/// <returns></returns>
		IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return _hashtable.GetEnumerator();
		}

		#endregion

	}

	/// <summary>
	/// An indexed List, based on System.Collections.ArrayList,
	/// which knows how to persist itself in the Registry.
	/// </summary>
	public class PersistableList: RegistryPersistable, IList
	{
		#region Private state, construction and disposal

		public IList _list;

		/// <summary>
		/// Create a PersistableList by loading from
		/// the Windows Registry from the given top-level
		/// key, subkey and named value.  If the subkey or
		/// named value does not exist, no exception is
		/// thrown and the PersistableList starts empty.
		/// </summary>
		/// <param name="topLevelKeyName">"HKLM", "HKEY_LOCAL_MACHINE",
		/// "HKCU", or "HKEY_CURRENT_USER", case-insensitive.</param>
		/// <param name="subkey">a Registry subkey path</param>
		/// <param name="valueName">a Registry value name</param>
		public PersistableList(string topLevelKeyName, string subkeyName, string valueName):
			base(topLevelKeyName, subkeyName, valueName)
		{
			this.Load();
		}

		/// <summary>
		/// Create a default PersistableList.
		/// </summary>
		public PersistableList()
		{
			_list = new ArrayList();
		}

		/// <summary>
		/// Create a new PersistableList as a shallow
		/// copy of the argument ArrayList.  This constructor
		/// should be used when you wish to create a new
		/// PersistableList with non-default capactin.
		/// First, create a
		/// System.Collections.ArrayList with the desired
		/// properties.  Then pass it to this constructor.
		/// The list held internally by this class will
		/// retain both the non-default properties and the
		/// content references, if any, of the prototype.
		/// </summary>
		/// <param name="prototype">prototype list</param>
		public PersistableList(ArrayList prototype)
		{
			_list = (IList)prototype.Clone();
		}

        /// <summary>
        /// Create a persistable list of object that will be saved to registry
        /// </summary>
        /// <param name="list"></param>
        public PersistableList(IList list)
        {
            _list = list;
        }

		/// <summary>
		/// Create a PersistableList by loading from
		/// the Windows Registry from the given top-level
		/// key, subkey and named value.  If the subkey or
		/// named value does not exist, no exception is
		/// thrown and the PersistableList starts empty.
		/// This constructor
		/// should be used when you wish to create a new
		/// PersistableList with non-default capacity.
		/// First, create a
		/// System.Collections.ArrrayList with the desired
		/// properties.  Then pass it to this constructor.
		/// The list held internally by this class will
		/// retain the non-default properties of the prototype.
		/// It will not retain the contents, if any, of the
		/// prototype; instead, it will be loaded initially
		/// from the Registry location defined by the arguments.
		/// </summary>
		/// <param name="prototype">a prototype list</param>
		/// <param name="topLevelKeyName">"HKLM", "HKEY_LOCAL_MACHINE",
		/// "HKCU", or "HKEY_CURRENT_USER", case-insensitive.</param>
		/// <param name="subkey">a Registry subkey path</param>
		/// <param name="valueName">a Registry value name</param>
		public PersistableList(ArrayList prototype,
			string topLevelKeyName, string subkeyName, string valueName):
			base(topLevelKeyName, subkeyName, valueName)
		{
			_list = (IList)prototype.Clone();
			_list.Clear();
			this.Load();
		}

		#endregion

		#region Overrides

		/// <summary>
		/// Return the object to serialize.  The object may
		/// be a collection, so long as all the objects in
		/// the collection are Serializable.
		/// </summary>
		/// <returns>object to serialize</returns>
		protected override object GetSerializableObject()
		{
			return this._list;
		}

		/// <summary>
		/// The argument is the object just deserialized
		/// from the Registry store.  The subclass can do
		/// as it sees fit, i.e. save a reference.
		/// </summary>
		/// <param name="obj"></param>
		protected override void SetSerializableObject(object obj)
		{
			this._list = (ArrayList)obj;
		}

		/// <summary>
		/// A Load() request failed to find or correctly load
		/// a copy of the content.  The subclass may set an
		/// empty collection, etc.
		/// </summary>
		protected override void SetDefaultSerializableObject()
		{
			this._list = new ArrayList();
		}

		#endregion

		#region IList Members

		/// <summary>
		/// A PersistableList is not read-only.
		/// </summary>
		public bool IsReadOnly
		{
			get
			{
				return false;
			}
		}

		/// <summary>
		/// Get or set the object at the index.
		/// </summary>
		public object this[int index]
		{
			get
			{
				return _list[index];
			}
			set
			{
				_list[index] = value;
			}
		}

		/// <summary>
		/// Removes the object at the specified index of the list.
		/// </summary>
		/// <param name="index"></param>
		public void RemoveAt(int index)
		{
			_list.RemoveAt(index);
		}

		/// <summary>
		/// Insert an object into the list at the specified index.
		/// </summary>
		/// <param name="index"></param>
		/// <param name="value"></param>
		public void Insert(int index, object value)
		{
			_list.Insert(index, value);
		}

		/// <summary>
		/// Removes the first occurrence of the specific
		/// object from the list.
		/// </summary>
		/// <param name="value"></param>
		public void Remove(object value)
		{
			_list.Remove(value);
		}

		/// <summary>
		/// Returns true if the specific object
		/// is found in the list.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public bool Contains(object value)
		{
			return _list.Contains(value);
		}

		/// <summary>
		/// Clear the list.
		/// </summary>
		public void Clear()
		{
			_list.Clear();
		}

		/// <summary>
		/// Returns the 0-based index of the first
		/// occurrence of the specified value within
		/// the list.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public int IndexOf(object value)
		{
			return _list.IndexOf(value);
		}

		/// <summary>
		/// Adds the object to the end of the list.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public int Add(object value)
		{
			return _list.Add(value);
		}

		/// <summary>
		/// A PersistableList is not fixed-size.
		/// </summary>
		public bool IsFixedSize
		{
			get
			{
				return false;
			}
		}

		#endregion

		#region ICollection Members

		/// <summary>
		/// PersistableList does not support synchronized wrappers.
		/// </summary>
		public bool IsSynchronized
		{
			get
			{
				return false;
			}
		}

		/// <summary>
		/// Gets the number of items actually contained in the list.
		/// </summary>
		public int Count
		{
			get
			{
				return _list.Count;
			}
		}

		/// <summary>
		/// Copies all the elements from the list to
		/// the argument array starting at the index.
		/// </summary>
		/// <param name="array"></param>
		/// <param name="index"></param>
		public void CopyTo(Array array, int index)
		{
			_list.CopyTo(array, index);
		}

		/// <summary>
		/// PersistableList does not support synchronized wrappers.
		/// </summary>
		public object SyncRoot
		{
			get
			{
				throw new NotSupportedException("SyncRoot");
			}
		}

		#endregion

		#region IEnumerable Members

		/// <summary>
		/// Get an enumerator for the list
		/// </summary>
		/// <returns></returns>
		public IEnumerator GetEnumerator()
		{
			return _list.GetEnumerator();
		}

		#endregion
	}


    /// <summary>
    /// An indexed List, based on System.Collections.ArrayList,
    /// which knows how to persist itself in the Registry.
    /// </summary>
    public class PersistableListofStrings : RegistryPersistable
    {
        #region Private state, construction and disposal

        public List<string> _list;

        /// <summary>
        /// Create a default PersistableList.
        /// </summary>
        public PersistableListofStrings()
        {
            _list = new List<string>();
        }

        /// <summary>
        /// Create a new PersistableList as a shallow
        /// copy of the argument ArrayList.  This constructor
        /// should be used when you wish to create a new
        /// PersistableList with non-default capactin.
        /// First, create a
        /// System.Collections.ArrayList with the desired
        /// properties.  Then pass it to this constructor.
        /// The list held internally by this class will
        /// retain both the non-default properties and the
        /// content references, if any, of the prototype.
        /// </summary>
        /// <param name="prototype">prototype list</param>
        public PersistableListofStrings(List<string> prototype)
        {
            _list = prototype;
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Return the object to serialize.  The object may
        /// be a collection, so long as all the objects in
        /// the collection are Serializable.
        /// </summary>
        /// <returns>object to serialize</returns>
        protected override object GetSerializableObject()
        {
            return this._list;
        }

        /// <summary>
        /// The argument is the object just deserialized
        /// from the Registry store.  The subclass can do
        /// as it sees fit, i.e. save a reference.
        /// </summary>
        /// <param name="obj"></param>
        protected override void SetSerializableObject(object obj)
        {
            this._list = (List<string>)obj;
        }

        /// <summary>
        /// A Load() request failed to find or correctly load
        /// a copy of the content.  The subclass may set an
        /// empty collection, etc.
        /// </summary>
        protected override void SetDefaultSerializableObject()
        {
            this._list = new List<string>();
        }

        #endregion
        protected override void SaveImpl()
        {
            using (RegistryKey topLevel = GetTopLevelKey(_topLevelKeyName))
            {
                RegistryKey subkey = null;
                try
                {
                    subkey = topLevel.CreateSubKey(_subkeyName);
                    if (subkey != null)
                    {
                        for (int i=0; i<_list.Count; i++)
                        {
                            subkey.SetValue(
                                string.Format("{0}{1}", _valueName, i),
                                _list[i]);
                        }
                    }
                }
                finally
                {
                    if (subkey != null) ((IDisposable)subkey).Dispose();
                }
            }
        }
    }

}
