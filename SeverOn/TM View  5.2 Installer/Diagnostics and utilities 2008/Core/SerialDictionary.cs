using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

using log4net;

namespace Serveron.Utility.Core
{
	/// <summary>
	/// Wraps the functionality needed to use easy XML Serialization with a
	/// generic dictionary
	/// </summary>
	/// <typeparam name="TKey">
	/// Type use to key the dictionary: can be any value type.
	/// </typeparam>
	/// <typeparam name="TValue">
	/// Type of values stored in the dictionary: can be any value or reference type.
	/// </typeparam>
	[XmlRoot(ElementName="SerializableDictionary")]
	[Serializable]
	public class SerialDictionary<TKey, TValue> : Dictionary<TKey, TValue>, IXmlSerializable
	{
		static ILog _log = LogManager.GetLogger(typeof(SerialDictionary<TKey, TValue>));
		[XmlIgnore]
		public string FileName { get; set; }
		[XmlIgnore]
		public string DictionaryName { get; set; }
		string _defaultDictionaryName = "SerializableDictionary";

		[XmlIgnore]
		static XmlSerializer _keySerializer, _valueSerializer;

		static SerialDictionary()
		{
			_keySerializer = new XmlSerializer(typeof(TKey));
			_valueSerializer = new XmlSerializer(typeof(TValue));
		}

		public SerialDictionary()
		{
			DictionaryName = _defaultDictionaryName;
		}

		protected SerialDictionary(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}

		#region IXmlSerializable Members

		public XmlSchema GetSchema()
		{
			return null;
		}

		public void ReadXml(XmlReader reader)
		{
			bool wasEmpty = reader.IsEmptyElement;
			reader.Read();

			if (wasEmpty)
				return;

			while (reader.NodeType != System.Xml.XmlNodeType.EndElement)
			{
				reader.ReadStartElement("item");

				reader.ReadStartElement("key");
				TKey key = (TKey)_keySerializer.Deserialize(reader);
				reader.ReadEndElement();

				reader.ReadStartElement("value");
				TValue value = (TValue)_valueSerializer.Deserialize(reader);
				reader.ReadEndElement();

				this.Add(key, value);

				reader.ReadEndElement();
				reader.MoveToContent();
			}
			reader.ReadEndElement();
		}

		public void WriteXml(XmlWriter writer)
		{
			foreach (TKey key in this.Keys)
			{
				writer.WriteStartElement("item");

				writer.WriteStartElement("key");
				_keySerializer.Serialize(writer, key);
				writer.WriteEndElement();

				writer.WriteStartElement("value");
				TValue value = this[key];
				_valueSerializer.Serialize(writer, value);
				writer.WriteEndElement();

				writer.WriteEndElement();
			}
		}

		#endregion

		public static SerialDictionary<TKey, TValue> Load(string pathName, string dictionaryName)
		{
			if (File.Exists(pathName))
			{
				using (FileStream stream = File.Open(pathName, FileMode.Open, FileAccess.Read, FileShare.Read))
				{
					try
					{
						SerialDictionary<TKey, TValue> result = Load(stream, dictionaryName);
						result.FileName = pathName;
						return result;
					}
					catch (Exception ex)
					{
						string msg = string.Format("Cannot load from {0} - using empty dictionary", pathName);
						_log.Error(msg, ex);
					}
					finally
					{
						if (stream != null)
							stream.Close();
					}
				}
			}
			
			// return an empty dictionary by default (exception of file not present)
			return new SerialDictionary<TKey, TValue>
				{
					DictionaryName = dictionaryName,
					FileName = pathName
				};
		}

		static SerialDictionary<TKey, TValue> Load(Stream stream, string dictionaryName)
		{
			XmlSerializer xs = SerializerLookup.GetSerializer<TKey, TValue>(dictionaryName);
			
			SerialDictionary<TKey, TValue> result =
				(SerialDictionary<TKey, TValue>)xs.Deserialize(stream);
			result.DictionaryName = dictionaryName;
			return result;
		}

		public void Save()
		{
			Save(FileName);
		}

		public XmlNode Serialize(string rootName)
		{
			MemoryStream stream = new MemoryStream();
			// Prepare to override element name attribute

			XmlSerializer xs = SerializerLookup.GetSerializer<TKey, TValue>(rootName);
			xs.Serialize(stream, this);
			XmlDocument doc = new XmlDocument();
			doc.LoadXml(Encoding.ASCII.GetString(stream.GetBuffer(), 0, (int)stream.Length));
			return doc.DocumentElement;
		}

		public void Save(string fileName)
		{
			if (!string.IsNullOrEmpty(fileName))
			{
				FileStream stream = null;
				try
				{
					// Prepare to override element name attribute

					// Save the dictionary
					if (File.Exists(fileName))
						stream = File.Open(fileName, FileMode.Truncate, FileAccess.Write);
					else
						stream = File.Open(fileName, FileMode.Create, FileAccess.Write);
					XmlSerializer xs = SerializerLookup.GetSerializer<TKey, TValue>(DictionaryName);
					xs.Serialize(stream, this);
				}
				catch (Exception ex)
				{
					string msg = string.Format("Cannot save SerializedDictionary {0}", fileName);
					_log.Error(msg, ex);
				}
				finally
				{
					if (stream != null)
						stream.Close();
				}
			}
		}
	}

	public static class SerializerLookup
	{
		static Dictionary<string, XmlSerializer> _serializers = new Dictionary<string,XmlSerializer>();

		static string CreateKey<TKey, TValue>(string dictionaryName)
		{
			string fullName = string.Format("{0}.{1}.{2}",
				dictionaryName, typeof(TKey).Name, typeof(TValue).Name);
			return fullName;
		}

		public static XmlSerializer GetSerializer<TKey, TValue>(string dictionaryName)
		{
			string key = CreateKey<TKey, TValue>(dictionaryName);
			if (_serializers.ContainsKey(key))
				return _serializers[key];
			else
			{
				var xs = new XmlSerializer(typeof(SerialDictionary<TKey, TValue>),
					new XmlRootAttribute(dictionaryName));
				_serializers.Add(key, xs);
				return xs;
			}
		}
		
		public static XmlSerializer GetSerializer(Type t, string rootName)
		{
			string key = string.Format("{0}.{1}", t.FullName, rootName);
			
			if (_serializers.ContainsKey(key))
				return _serializers[key];
			else
			{
				var xs = new XmlSerializer(t, new XmlRootAttribute(rootName));
				_serializers.Add(key, xs);
				return xs;
			}
		}
		
		public static XmlSerializer GetListSerializer<T>(string rootName)
		{
			string key = string.Format("{0}.List.{1}", typeof(T).FullName, rootName);
			
			if (_serializers.ContainsKey(key))
				return _serializers[key];
			else
			{
				var xs = new XmlSerializer(typeof(List<T>), new XmlRootAttribute(rootName));
				_serializers.Add(key, xs);
				return xs;
			}
		}
	}
}
