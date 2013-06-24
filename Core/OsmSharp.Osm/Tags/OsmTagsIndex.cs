using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OsmSharp.Tools.Collections;
using OsmSharp.Tools.Math;

namespace OsmSharp.Osm
{
    /// <summary>
    /// An osm tags index.
    /// </summary>
    public class OsmTagsIndex : ITagsIndex
    {
        /// <summary>
        /// Holds all the tags objects.
        /// </summary>
        private ObjectTable<OsmTags> _tags;

        /// <summary>
        /// Creates a new tags index with a given strings table.
        /// </summary>
        public OsmTagsIndex()
        {
            //_string_table = string_table;
            _tags = new ObjectTable<OsmTags>(true);
		}
		
		/// <summary>
		/// Creates a new tags index with a given strings table.
		/// </summary>
		public OsmTagsIndex(ObjectTable<OsmTags> tags)
		{
			//_string_table = string_table;
			_tags = tags;
		}

        /// <summary>
        /// Returns the tags with the given id.
        /// </summary>
        /// <param name="tags_int"></param>
        /// <returns></returns>
        public IDictionary<string, string> Get(uint tags_int)
        {
            OsmTags osm_tags = _tags.Get(tags_int);
            if (osm_tags != null)
            {
                return osm_tags.GetTags();
            }
            return null;
        }

        /// <summary>
        /// Adds tags to this index.
        /// </summary>
        /// <param name="tags"></param>
        /// <returns></returns>
        public uint Add(IDictionary<string, string> tags)
        {
            OsmTags osm_tags = OsmTags.CreateFrom( tags);
            if (osm_tags != null)
            {
                return _tags.Add(osm_tags);
            }
            throw new ArgumentNullException("tags", "Tags dictionary cannot be null or empty!");
        }

		/// <summary>
		/// Holds all the tags objects.
		/// </summary>
		public ObjectTable<OsmTags> Tags
		{
			get { return _tags; }
		}

        /// <summary>
        /// Holds tags in a very memory efficient way.
        /// </summary>
        public class OsmTags
        {
			/// <summary>
			/// Holds all the tags.
			/// </summary>
            private readonly string[] _keys;

            /// <summary>
            /// Holds all the values.
            /// </summary>
			private readonly string[] _values;
			
			/// <summary>
			/// Creates a new tags object.
			/// </summary>
			/// <param name="keys"></param>
			/// <param name="values"></param>
			public OsmTags(string[] keys, string[] values)
			{
				_keys = keys;
				_values = values;
			}
			
			/// <summary>
			/// Holds all the tags.
			/// </summary>
			public string[] Keys
			{
				get { return _keys; }
			}

			/// <summary>
			/// Holds all the tags.
			/// </summary>
			public string[] Values
			{
				get { return _values; }
			}

			/// <summary>
			/// Creates a new tags object.
			/// </summary>
			/// <param name="tags"></param>
			/// <returns></returns>
			internal static OsmTags CreateFrom(IDictionary<string, string> tags)
			{
				if (tags != null)
				{
					string[] keys_int = new string[tags.Count];
					string[] values_int = new string[tags.Count];
					int idx = 0;
					foreach (KeyValuePair<string, string> tag in tags)
					{
						keys_int[idx] = tag.Key; // string_table.Add(tag.Key);
						values_int[idx] = tag.Value; // string_table.Add(tag.Value);
						idx++;
					}
					return new OsmTags(keys_int, values_int);
				}
				return null;  // don't waste space on tags that contain no information.
			}
			
			/// <summary>
			/// Returns the actual tags.
			/// </summary>
			public IDictionary<string, string> GetTags()
			{
				Dictionary<string, string> tags = new Dictionary<string, string>();
				for (int idx = 0; idx < this._keys.GetLength(0); idx++)
				{
					tags.Add(this._keys[idx],
					         this._values[idx]);
				}
				return tags;
			}
			
			/// <summary>
			/// Returns true if the objects represent the same information.
			/// </summary>
			/// <param name="obj"></param>
			/// <returns></returns>
			public override bool Equals(object obj)
			{
				if (!object.ReferenceEquals(this, obj))
				{
					if (obj is OsmTags)
					{
						OsmTags other = (obj as OsmTags);
						if (other._keys.Length == this._keys.Length)
						{
							// make sure all object in the first are in the second and vice-versa.
							for (int idx1 = 0; idx1 < this._keys.Length; idx1++)
							{
								bool found = false;
								for (int idx2 = 0; idx2 < other._values.Length; idx2++)
								{
									if (this._keys[idx1] == other._keys[idx2] &&
									    this._values[idx1] == other._values[idx2])
									{
										found = true;
										break;
									}
								}
								if (!found)
								{
									return false;
								}
							}
							return true; // no loop was done without finding the same key-value pair.
						}
					}
					return false;
				}
				return true;
			}
			
			/// <summary>
			/// Serves as a hash function.
			/// </summary>
			/// <returns></returns>
			public override int GetHashCode()
			{
			    if (_keys != null)
			    {
			        int hash = _keys.Length;
			        foreach (string value in Keys)
			        {
			            hash = hash ^ value.GetHashCode();
			        }
			        return hash;
			    }
			    return 0;
			}
        }
    }
}
