using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using OsmSharp.Osm;
using OsmSharp.Routing.Graph.DynamicGraph;
using OsmSharp.Routing.Graph.DynamicGraph.Memory;
using OsmSharp.Routing.Graph.DynamicGraph.PreProcessed;
using OsmSharp.Routing.Graph.Router;
using OsmSharp.Tools.Collections;
using OsmSharp.Tools.IO;
using OsmSharp.Tools.Math;
using OsmSharp.Tools.Math.Geo;
using OsmSharp.Tools.Math.Geo.Simple;
using ProtoBuf;
using ProtoBuf.Meta;

namespace OsmSharp.Routing.Graph.Serialization.v1
{
    /// <summary>
    /// A v1 routing serializer.
    /// </summary>
    /// <remarks>Versioning is implemented in the file format to guarantee backward compatibility.</remarks>
    public class V1RoutingSerializer : RoutingSerializer
    {
        /// <summary>
        /// Holds the size of the tile meta.
        /// </summary>
        private const int TileMetaSize = 2 * 4 + 2 * 8;

        /// <summary>
        /// Holds the zoom.
        /// </summary>
        private const int Zoom = 15;

        /// <summary>
        /// Holds the runtime type model.
        /// </summary>
        private readonly RuntimeTypeModel _runtimeTypeModel;

        /// <summary>
        /// Creates a new v1 serializer.
        /// </summary>
        public V1RoutingSerializer()
        {
            RuntimeTypeModel typeModel = TypeModel.Create();
            typeModel.Add(typeof(SerializableGraphTileMeta), true);
            typeModel.Add(typeof(SerializableGraphTileMetas), true);
            typeModel.Add(typeof(SerializableGraphTile), true);
            typeModel.Add(typeof(SerializableGraphVertex), true);
            typeModel.Add(typeof(SerializableGraphArc), true);
            //typeModel.Add(typeof(SerializableOsmTags), true);
            //typeModel.Add(typeof(SerializableOsmTagsIndex), true);

            //OsmTagsIndex
            typeModel.Add(typeof(ITagsIndex), false).AddSubType(1, typeof(OsmTagsIndex));
            typeModel.Add(typeof(OsmTagsIndex), false).SetSurrogate(typeof(SerializableOsmTagsIndex));
            typeModel.Add(typeof(ObjectTable<OsmTagsIndex.OsmTags>), false).SetSurrogate(typeof(SerializableObjectTable<OsmTagsIndex.OsmTags>));
            typeModel.Add(typeof(OsmTagsIndex.OsmTags), false).SetSurrogate(typeof(SerializableOsmTags));

            typeModel.Add(typeof(PreProcessedEdge), false).SetSurrogate(typeof(SerializablePreProcessedEdge));

            _runtimeTypeModel = typeModel;
        }

        /// <summary>
        /// Returns the version number.
        /// </summary>
        public override uint Version
        {
            get { return 1; }
        }

        /// <summary>
        /// Does the v1 serialization.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="graph"></param>
        /// <returns></returns>
        protected override void DoSerialize(RoutingSerializerStream stream, 
            DynamicGraphRouterDataSource<PreProcessedEdge> graph)
        {
            // create an index per tile.
            // TODO: improve this first naive memory-heavy proof-of-concept implementation.
            var dataPerTile = new Dictionary<Tile, List<SerializableGraphVertex>>();
            for (uint vertex = 0; vertex < graph.VertexCount + 1; vertex++)
            {
                float latitude, longitude;
                if (graph.GetVertex(vertex, out latitude, out longitude))
                { // the vertex was found.
                    // save this vertex.
                    var serializableGraphVertex = new SerializableGraphVertex();
                    serializableGraphVertex.Id = vertex;
                    serializableGraphVertex.Latitude = latitude;
                    serializableGraphVertex.Longitude = longitude;

                    // calculate the vertex tile.
                    Tile vertexTile = Tile.CreateAroundLocation(new GeoCoordinate(latitude, longitude), Zoom);

                    // add this vertex to the list.
                    List<SerializableGraphVertex> tileList = null;
                    if (!dataPerTile.TryGetValue(vertexTile, out tileList))
                    {
                        tileList = new List<SerializableGraphVertex>();
                        dataPerTile.Add(vertexTile, tileList);
                    }
                    tileList.Add(serializableGraphVertex);

                    // get the arcs.
                    KeyValuePair<uint, PreProcessedEdge>[] arcs = graph.GetArcs(vertex);

                    // serialize the arcs.
                    List<SerializableGraphArc> serializableGraphArcs = null;
                    if (arcs != null && arcs.Length > 0)
                    {
                        serializableGraphArcs = new List<SerializableGraphArc>();
                        for (int idx = 0; idx < arcs.Length; idx++)
                        {
                            KeyValuePair<uint, PreProcessedEdge> arc = arcs[idx];
                            // get destination tile.
                            if (graph.GetVertex(arc.Key, out latitude, out longitude))
                            { // the destionation was found.
                                Tile destinationTile = Tile.CreateAroundLocation(
                                    new GeoCoordinate(latitude, longitude), Zoom);
                                var serializableGraphArc = new SerializableGraphArc();
                                serializableGraphArc.DestinationId = arc.Key;
                                serializableGraphArc.TileX = destinationTile.X;
                                serializableGraphArc.TileY = destinationTile.Y;
                                serializableGraphArc.Edge = arc.Value;
                                serializableGraphArcs.Add(serializableGraphArc);
                            }
                        }
                    }

                    // add arcs to the vertex.
                    if (serializableGraphArcs != null && serializableGraphArcs.Count > 0)
                    {
                        serializableGraphVertex.Arcs = serializableGraphArcs.ToArray();
                    }
                }
            }

            // build the type model of supported types for the pbf serializer.
            RuntimeTypeModel typeModel = _runtimeTypeModel;

            // calculate the space needed for the tile offset.
            const long tileMetaOffset = 4 + 8 + 8;
            long tileOffset = TileMetaSize * dataPerTile.Count + 
                tileMetaOffset; // all tile metadata + a tile count + tags offset.

            // serialize all individual tiles.
            var tileMetas = new List<SerializableGraphTileMeta>();
            stream.Seek(tileOffset, SeekOrigin.Begin); // move to right after the tile meta data.
            foreach (KeyValuePair<Tile, List<SerializableGraphVertex>> tileData in dataPerTile)
            {
                // create the tile meta.
                var tileMeta = new SerializableGraphTileMeta();
                tileMeta.TileX = tileData.Key.X;
                tileMeta.TileY = tileData.Key.Y;
                tileMeta.Offset = stream.Position;

                // create the tile.
                var serializableGraphTile = new SerializableGraphTile();
                serializableGraphTile.Vertices = tileData.Value.ToArray();

                // serialize the tile.
                typeModel.Serialize(stream, serializableGraphTile);

                // calculate the length of the data that was just serialized.
                tileMeta.Length = (int)(stream.Position - tileMeta.Offset);
                tileMetas.Add(tileMeta);
            }

            // save the tags offset.
            long tagsOffset = stream.Position;

            // serialize all tile meta data.
            stream.Seek(tileMetaOffset, SeekOrigin.Begin);
            var serializableGraphTileMetas = new SerializableGraphTileMetas();
            serializableGraphTileMetas.Metas = tileMetas.ToArray();
            typeModel.Serialize(stream, serializableGraphTileMetas);

            // save the meta end.
            long tileMetaEnd = stream.Position;

            // serialize the tags.
            stream.Seek(tagsOffset, SeekOrigin.Begin);
            if (graph.TagsIndex is OsmTagsIndex)
            {
                typeModel.Serialize(stream, graph.TagsIndex as OsmTagsIndex);
            }
            else
            {
                throw new ArgumentOutOfRangeException("graph", "Graph can only be serialized with a tags index of type OsmTagsIndex.");
            }

            // save all the offsets.
            stream.Seek(0, SeekOrigin.Begin);
            byte[] tagOffsetBytes = BitConverter.GetBytes(tagsOffset);
            stream.Write(tagOffsetBytes, 0, tagOffsetBytes.Length); // 8 bytes
            byte[] tileCountBytes = BitConverter.GetBytes(tileMetas.Count);
            stream.Write(tileCountBytes, 0, tileCountBytes.Length); // 4 bytes
            byte[] tileMetaEndBytes = BitConverter.GetBytes(tileMetaEnd);
            stream.Write(tileMetaEndBytes, 0, tileMetaEndBytes.Length); // 8 bytes

            stream.Flush();
        }

        /// <summary>
        /// Does the v1 deserialization.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="lazy"></param>
        /// <returns></returns>
        protected override IBasicRouterDataSource<PreProcessedEdge> DoDeserialize(
            RoutingSerializerStream stream, bool lazy)
        {
            // serialize all tile meta data.
            stream.Seek(0, SeekOrigin.Begin);
            var tagOffsetBytes = new byte[8];
            stream.Read(tagOffsetBytes, 0, tagOffsetBytes.Length);
            var tagOffset = BitConverter.ToInt64(tagOffsetBytes, 0);

            var tileCountBytes = new byte[4];
            stream.Read(tileCountBytes, 0, tileCountBytes.Length);
            var tileCount = BitConverter.ToInt32(tileCountBytes, 0);

            var tileMetaEndBytes = new byte[8];
            stream.Read(tileMetaEndBytes, 0, tileMetaEndBytes.Length);
            var tileMetaEnd = BitConverter.ToInt64(tileMetaEndBytes, 0);

            // deserialize meta data.
            var meta = (SerializableGraphTileMetas)_runtimeTypeModel.Deserialize(
                new CappedStream(stream, stream.Position, tileMetaEnd - stream.Position), null, 
                    typeof(SerializableGraphTileMetas));

            // deserialize the tags index.
            var serializableTagIndex = (OsmTagsIndex)_runtimeTypeModel.Deserialize(
                new CappedStream(stream, tagOffset, stream.Length - tagOffset), null,
                    typeof(OsmTagsIndex));
            OsmTagsIndex tagsIndex = serializableTagIndex;// serializableTagIndex.To();

            // create the datasource.
            var routerDataSource = new V1RouterDataSource(tagsIndex, stream, meta, Zoom,
                    this, 1000);
            if (!lazy)
            { // pre-load everything.
                foreach (var tileMeta in meta.Metas)
                {
                    routerDataSource.LoadMissingTile(new Tile(tileMeta.TileX, tileMeta.TileY, Zoom));
                }
            }

            // return router datasource.
            return routerDataSource;
        }

        /// <summary>
        /// Deserialize the given tile data.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="meta"></param>
        /// <returns></returns>
        internal SerializableGraphTile DeserializeTile(Stream stream, SerializableGraphTileMeta meta)
        {
            return (SerializableGraphTile)_runtimeTypeModel.Deserialize(
                new CappedStream(stream, meta.Offset, meta.Length), null,
                    typeof(SerializableGraphTile));
        }

        #region Serializable Classes

        /// <summary>
        /// Serializable metadata.
        /// </summary>
        [ProtoContract]
        private class SerializableMeta
        {
            /// <summary>
            /// The tile count.
            /// </summary>
            [ProtoMember(1)]
            public int TileCount { get; set; }

            /// <summary>
            /// The starting position of the tags index.
            /// </summary>
            [ProtoMember(2)]
            public long TagsOffset { get; set; }
        }

        /// <summary>
        /// Serializable object containing all metadata tiles.
        /// </summary>
        [ProtoContract]
        internal class SerializableGraphTileMetas
        {
            /// <summary>
            /// Gets/sets the metas.
            /// </summary>
            [ProtoMember(1)]
            public SerializableGraphTileMeta[] Metas { get; set; }
        }

        /// <summary>
        /// Serializable object containing all metadata about a tile and it's location.
        /// </summary>
        [ProtoContract]
        internal class SerializableGraphTileMeta
        {
            /// <summary>
            /// The tile x-coordinate.
            /// </summary>
            [ProtoMember(1)]
            public int TileX { get; set; }

            /// <summary>
            /// The tile y-coordinate.
            /// </summary>
            [ProtoMember(2)]
            public int TileY { get; set; }

            /// <summary>
            /// The tile offset.
            /// </summary>
            [ProtoMember(3)]
            public long Offset { get; set; }

            /// <summary>
            /// The tile length.
            /// </summary>
            [ProtoMember(4)]
            public int Length { get; set; }
        }

        /// <summary>
        /// Serializable object containing all data in a dynamic graph in one tile.
        /// </summary>
        [ProtoContract]
        internal class SerializableGraphTile
        {
            /// <summary>
            /// Gets/sets the vertices.
            /// </summary>
            [ProtoMember(1)]
            public SerializableGraphVertex[] Vertices { get; set; }
        }

        /// <summary>
        /// Serializable object containing all data about one vertex and it's outgoing/incoming edges.
        /// </summary>
        [ProtoContract]
        internal class SerializableGraphVertex
        {
            /// <summary>
            /// The id of the vertex.
            /// </summary>
            [ProtoMember(1)]
            public uint Id { get; set; }

            /// <summary>
            /// Gets/sets latitude of the vertex.
            /// </summary>
            [ProtoMember(2)]
            public float Latitude { get; set; }

            /// <summary>
            /// Gets/sets longitude of the vertex.
            /// </summary>
            [ProtoMember(3)]
            public float Longitude { get; set; }

            /// <summary>
            /// Gets/sets the arcs.
            /// </summary>
            [ProtoMember(4)]
            public SerializableGraphArc[] Arcs { get; set; }
        }

        /// <summary>
        /// Serializable object containt all data about one arc.
        /// </summary>
        [ProtoContract]
        internal class SerializableGraphArc
        {
            /// <summary>
            /// Gets/sets the destination id.
            /// </summary>
            [ProtoMember(1)]
            public uint DestinationId { get; set; }

            /// <summary>
            /// Gets/sets the tile x-coordinate.
            /// </summary>
            [ProtoMember(2)]
            public int TileX { get; set; }

            /// <summary>
            /// Gets/sets the tile y-coordinate.
            /// </summary>
            [ProtoMember(3)]
            public int TileY { get; set; }

            /// <summary>
            /// Gets/sets the edge data.
            /// </summary>
            [ProtoMember(4)]
            public PreProcessedEdge Edge { get; set; }
        }

        /// <summary>
        /// 
        /// </summary>
        [ProtoContract]
        public class SerializableOsmTags
        {
            /// <summary>
            /// 
            /// </summary>
            [ProtoMember(1)]
            public string[] Keys { get; set; }
            /// <summary>
            /// 
            /// </summary>
            [ProtoMember(2)]
            public string[] Values { get; set; }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="value"></param>
            /// <returns></returns>
            public static implicit operator SerializableOsmTags(OsmTagsIndex.OsmTags value)
            {
                return value == null ? null : new SerializableOsmTags { Keys = (string[])value.Keys, Values = (string[])value.Values };
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="value"></param>
            /// <returns></returns>
            public static implicit operator OsmTagsIndex.OsmTags(SerializableOsmTags value)
            {
                return value == null ? null : new OsmTagsIndex.OsmTags(value.Keys, value.Values);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        [ProtoContract]
        public class SerializableObjectTable<T>
        {
            /// <summary>
            /// 
            /// </summary>
            [ProtoMember(1)]
            public T[] Strings { get; set; }
            /// <summary>
            /// 
            /// </summary>
            [ProtoMember(2)]
            public int InitCapacity { get; set; }
            /// <summary>
            /// 
            /// </summary>
            [ProtoMember(3)]
            public bool BuildReverseIndex { get; set; }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="value"></param>
            /// <returns></returns>
            public static implicit operator SerializableObjectTable<T>(ObjectTable<T> value)
            {
                if (value == null)
                {
                    return null;
                }
                var strings = new T[value.Count];
                Array.Copy(value.Strings, strings, (int)value.Count);
                var serializableObjectTable = new SerializableObjectTable<T> { Strings = strings, InitCapacity = value.InitCapacity, BuildReverseIndex = value.IsReverseIndexed };
                return serializableObjectTable;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="value"></param>
            /// <returns></returns>
            public static implicit operator ObjectTable<T>(SerializableObjectTable<T> value)
            {
                return value == null ? null : new ObjectTable<T>(value.BuildReverseIndex, value.InitCapacity, value.Strings);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [ProtoContract]
        public class SerializableOsmTagsIndex
        {
            /// <summary>
            /// 
            /// </summary>
            [ProtoMember(1)]
            public ObjectTable<OsmTagsIndex.OsmTags> Tags { get; set; }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="value"></param>
            /// <returns></returns>
            public static implicit operator SerializableOsmTagsIndex(OsmTagsIndex value)
            {
                return value == null ? null : new SerializableOsmTagsIndex { Tags = value.Tags };
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="value"></param>
            /// <returns></returns>
            public static implicit operator OsmTagsIndex(SerializableOsmTagsIndex value)
            {
                return value == null ? null : new OsmTagsIndex(value.Tags);
            }
        }

        ///// <summary>
        ///// Serializable version of the osm tags index.
        ///// </summary>
        //[ProtoContract]
        //private class SerializableOsmTagsIndex
        //{
        //    /// <summary>
        //    /// Gets/sets the strings.
        //    /// </summary>
        //    [ProtoMember(1)]
        //    public string[] Strings { get; set; }
            
        //    /// <summary>
        //    /// Gets/sets the tags.
        //    /// </summary>
        //    [ProtoMember(2)]
        //    public SerializableOsmTags[] Tags { get; set; }

        //    /// <summary>
        //    /// Converts a non-serializable version of the OsmTagsIndex to the serializable version.
        //    /// </summary>
        //    /// <param name="value"></param>
        //    /// <returns></returns>
        //    public static SerializableOsmTagsIndex From(OsmTagsIndex value)
        //    {
        //        // create the serializable version.
        //        var index = new SerializableOsmTagsIndex();

        //        // create the string table.
        //        index.Tags = new SerializableOsmTags[value.Tags.Count];
        //        var stringTable = new ObjectTable<string>(false);
        //        for (int idx = 0; idx < value.Tags.Count; idx++)
        //        {
        //            OsmTagsIndex.OsmTags tag = value.Tags.Get((uint) idx);
        //            var serializableOsmTags = new SerializableOsmTags();
        //            serializableOsmTags.Keys = new uint[tag.Keys.Length];
        //            serializableOsmTags.Values = new uint[tag.Values.Length];

        //            for (int tagIdx = 0; tagIdx < tag.Keys.Length; tagIdx++)
        //            {
        //                serializableOsmTags.Keys[tagIdx] = stringTable.Add(tag.Keys[tagIdx]);
        //                serializableOsmTags.Values[tagIdx] = stringTable.Add(tag.Values[tagIdx]);
        //            }
        //            index.Tags[idx] = serializableOsmTags;
        //        }

        //        // convert the string table to an array.
        //        index.Strings = new string[stringTable.Count];
        //        for (int idx = 0; idx < index.Strings.Length; idx++)
        //        {
        //            index.Strings[idx] = stringTable.Get((uint)idx);
        //        }
        //        return index;
        //    }

        //    /// <summary>
        //    /// Converts the serializable version of the OsmTagsIndex to the non-serializable version.
        //    /// </summary>
        //    /// <returns></returns>
        //    public OsmTagsIndex To()
        //    {
        //        var tagsIndex = new OsmTagsIndex();
        //        foreach (var serializableOsmTag in this.Tags)
        //        {
        //            var tags = new Dictionary<string, string>();
        //            for (int idx = 0; idx < serializableOsmTag.Keys.Length; idx++)
        //            {
        //                tags[this.Strings[serializableOsmTag.Keys[idx]]] =
        //                    this.Strings[serializableOsmTag.Values[idx]];
        //            }

        //            tagsIndex.Add(tags);
        //        }
        //        return tagsIndex;
        //    }
        //}

        ///// <summary>
        ///// Serializable version of a collection of osm tags.
        ///// </summary>
        //[ProtoContract]
        //private class SerializableOsmTags
        //{
        //    /// <summary>
        //    /// Gets/sets the keys.
        //    /// </summary>
        //    [ProtoMember(1)]
        //    public uint[] Keys { get; set; }

        //    /// <summary>
        //    /// Gets/sets the values.
        //    /// </summary>
        //    [ProtoMember(2)]
        //    public uint[] Values { get; set; }
        //}

        /// <summary>
        /// 
        /// </summary>
        [ProtoContract]
        public class SerializablePreProcessedEdge
        {
            /// <summary>
            /// 
            /// </summary>
            [ProtoMember(1)]
            public float Weight { get; set; }
            /// <summary>
            /// 
            /// </summary>
            [ProtoMember(2)]
            public uint Tags { get; set; }
            /// <summary>
            /// 
            /// </summary>
            [ProtoMember(3)]
            public bool Forward { get; set; }
            /// <summary>
            /// 
            /// </summary>
            [ProtoMember(4)]
            public bool Backward { get; set; }
            /// <summary>
            /// 
            /// </summary>
            /// <param name="value"></param>
            /// <returns></returns>
            public static implicit operator SerializablePreProcessedEdge(PreProcessedEdge value)
            {
                return value == null ? null : new SerializablePreProcessedEdge { Weight = (float)value.Weight, Tags = value.Tags, Forward = value.Forward, Backward = value.Backward };
            }
            /// <summary>
            /// 
            /// </summary>
            /// <param name="value"></param>
            /// <returns></returns>
            public static implicit operator PreProcessedEdge(SerializablePreProcessedEdge value)
            {
                return value == null ? null : new PreProcessedEdge(value.Weight, value.Forward, value.Backward, value.Tags);
            }
        }

        #endregion
    }
}