using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using OsmSharp.Osm;
using OsmSharp.Routing.Graph.DynamicGraph;
using OsmSharp.Routing.Graph.DynamicGraph.PreProcessed;
using OsmSharp.Routing.Graph.Router;
using OsmSharp.Tools.Collections;
using OsmSharp.Tools.Math;
using OsmSharp.Tools.Math.Geo;
using OsmSharp.Tools.Math.Geo.Simple;
using OsmSharp.Tools.Math.Structures;
using OsmSharp.Tools.Math.Structures.QTree;

namespace OsmSharp.Routing.Graph.Serialization.v1
{
    /// <summary>
    /// A router data source that dynamically loads data.
    /// </summary>
    internal class V1RouterDataSource : IBasicRouterDataSource<PreProcessedEdge>
    {
        /// <summary>
        /// Holds all graph data.
        /// </summary>
        private readonly SparseArray<Vertex> _vertices;

        /// <summary>
        /// Holds the coordinates of the vertices.
        /// </summary>
        private readonly SparseArray<Location> _coordinates;

        /// <summary>
        /// Holds the tags index.
        /// </summary>
        private readonly ITagsIndex _tagsIndex;

        /// <summary>
        /// Holds the vertex index.
        /// </summary>
        private readonly ILocatedObjectIndex<GeoCoordinate, uint> _vertexIndex;

        /// <summary>
        /// Creates a new router data source.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="tileMetas"></param>
        /// <param name="zoom"></param>
        /// <param name="v1RoutingSerializer"></param>
        /// <param name="initialCapacity"></param>
        /// <param name="tagsIndex"></param>
        internal V1RouterDataSource(ITagsIndex tagsIndex, 
            Stream stream,
            V1RoutingSerializer.SerializableGraphTileMetas tileMetas, 
            int zoom, V1RoutingSerializer v1RoutingSerializer,
            int initialCapacity = 1000)
        {
            _tagsIndex = tagsIndex;
            _vertices = new SparseArray<Vertex>(initialCapacity);
            _coordinates = new SparseArray<Location>(initialCapacity);

            _vertexIndex = new QuadTree<GeoCoordinate, uint>();

            _graphTileMetas = new Dictionary<Tile, V1RoutingSerializer.SerializableGraphTileMeta>();
            foreach (var tileMeta in tileMetas.Metas)
            {
                _graphTileMetas.Add(
                    new Tile(tileMeta.TileX, tileMeta.TileY, zoom), tileMeta);
            }

            _loadedTiles = new HashSet<Tile>();
            _tilesPerVertex = new Dictionary<uint, Tile>();
            _zoom = zoom;
            _routingSerializer = v1RoutingSerializer;
            _stream = stream;
        }

        /// <summary>
        /// Returns true if the given profile is supported.
        /// </summary>
        /// <param name="vehicle"></param>
        /// <returns></returns>
        public bool SupportsProfile(VehicleEnum vehicle)
        {
            // TODO: also save the profiles.
            return true;
        }
		
		
		/// <summary>
		/// Adds a supported vehicle profile.
		/// </summary>
		/// <param name="vehicle"></param>
		public void AddSupportedProfile(VehicleEnum vehicle)
		{

		}


        /// <summary>
        /// Returns all edges inside the given boundingbox.
        /// </summary>
        /// <param name="box"></param>
        /// <returns></returns>
        public KeyValuePair<uint, KeyValuePair<uint, PreProcessedEdge>>[] GetArcs(
            GeoCoordinateBox box)
        {
            // load the missing tiles.
            this.LoadMissingTile(box);

            // get all the vertices in the given box.
            IEnumerable<uint> vertices = _vertexIndex.GetInside(
                box);

            // loop over all vertices and get the arcs.
            var arcs = new List<KeyValuePair<uint, KeyValuePair<uint, PreProcessedEdge>>>();
            foreach (uint vertexId in vertices)
            {
                var location = _coordinates[(int)vertexId];
                if (location != null)
                {
                    // load tile if needed.
                    this.LoadMissingTile(new GeoCoordinate(
                        location.Latitude, location.Longitude));

                    // get the arcs and return.
                    if (_vertices.Length > vertexId)
                    {
                        var vertex = _vertices[(int)vertexId];
                        if (vertex != null &&
                            vertex.Arcs != null)
                        {
                            KeyValuePair<uint, PreProcessedEdge>[] localArcs = vertex.Arcs;
                            foreach (KeyValuePair<uint, PreProcessedEdge> localArc in localArcs)
                            {
                                arcs.Add(new KeyValuePair<uint, KeyValuePair<uint, PreProcessedEdge>>(
                                    vertexId, localArc));
                            }
                        }
                    }
                }
            }
            return arcs.ToArray();
        }

        /// <summary>
        /// Returns the tags index.
        /// </summary>
        public ITagsIndex TagsIndex
        {
            get { return _tagsIndex; }
        }

        /// <summary>
        /// Returns the location of the vertex with the given id.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="latitude"></param>
        /// <param name="longitude"></param>
        /// <returns></returns>
        public bool GetVertex(uint id, out float latitude, out float longitude)
        {
            Tile tile;
            if (_tilesPerVertex.TryGetValue(id, out tile))
            {
                // load missing tile if needed.
                this.LoadMissingTile(tile);
                _tilesPerVertex.Remove(id);
            }

            if (id > 0 && _vertices.Length > id)
            {
                Location location = _coordinates[(int)id];
                if (location != null)
                {
                    latitude = location.Latitude;
                    longitude = location.Longitude;
                    return true;
                }
            }
            latitude = float.MaxValue;
            longitude = float.MaxValue;
            return false;
        }

        /// <summary>
        /// Returns all vertices in this router data source.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<uint> GetVertices()
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Returns all arcs for the given vertex.
        /// </summary>
        /// <param name="vertexId"></param>
        /// <returns></returns>
        public KeyValuePair<uint, PreProcessedEdge>[] GetArcs(uint vertexId)
        {
            Tile tile;
            if (_tilesPerVertex.TryGetValue(vertexId, out tile))
            {
                // load missing tile if needed.
                this.LoadMissingTile(tile);
                _tilesPerVertex.Remove(vertexId);
            }

            // get the arcs and return.
            if (_vertices.Length > vertexId)
            {
                var vertex = _vertices[(int) vertexId];
                if (vertex != null &&
                    vertex.Arcs != null)
                {
                    return vertex.Arcs;
                }
            }
            return new KeyValuePair<uint, PreProcessedEdge>[0];
        }

        /// <summary>
        /// Returns true if the given vertex has the given neighbour.
        /// </summary>
        /// <param name="vertex"></param>
        /// <param name="neighbour"></param>
        /// <returns></returns>
        public bool HasNeighbour(uint vertex, uint neighbour)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns the vertex count.
        /// </summary>
        public uint VertexCount
        {
            get { throw new NotSupportedException(); }
        }

        /// <summary>
        /// Represents a simple vertex.
        /// </summary>
        internal class Vertex
        {
            /// <summary>
            /// Holds an array of edges starting at this vertex.
            /// </summary>
            public KeyValuePair<uint, PreProcessedEdge>[] Arcs;
        }

        /// <summary>
        /// Represents the location.
        /// </summary>
        internal class Location
        {
            /// <summary>
            /// Gets/sets the latitude.
            /// </summary>
            public float Latitude { get; set; }

            /// <summary>
            /// Gets/sets the longitude.
            /// </summary>
            public float Longitude { get; set; }
        }

        #region Dynamic Tile Loading

        /// <summary>
        /// Holds the stream containing the graph data.
        /// </summary>
        private readonly Stream _stream;

        /// <summary>
        /// Holds the routing serializer.
        /// </summary>
        private readonly V1RoutingSerializer _routingSerializer;

        /// <summary>
        /// Holds the tile metas.
        /// </summary>
        private readonly Dictionary<Tile, V1RoutingSerializer.SerializableGraphTileMeta> _graphTileMetas;

        /// <summary>
        /// Holds the loaded tiles.
        /// </summary>
        private readonly HashSet<Tile> _loadedTiles;

        /// <summary>
        /// Holds the tile to get the current vertex.
        /// </summary>
        private readonly Dictionary<uint, Tile> _tilesPerVertex; 

        /// <summary>
        /// The zoom level of the cached tiles.
        /// </summary>
        private readonly int _zoom;

        /// <summary>
        /// Resize if needed.
        /// </summary>
        /// <param name="size"></param>
        private void Resize(uint size)
        {
            if (_coordinates.Length < size)
            {
                _coordinates.Resize((int)size); // increasing a sparse array size is very cheap.
                _vertices.Resize((int)size); // increasing a sparse array size is very cheap.
            }
        }

        /// <summary>
        /// Loads all missing tiles.
        /// </summary>
        /// <param name="box"></param>
        private void LoadMissingTile(GeoCoordinateBox box)
        {
            // creates a tile range.
            TileRange tileRange = TileRange.CreateAroundBoundingBox(box, _zoom);
            foreach (var tile in tileRange)
            {
                this.LoadMissingTile(tile);
            }
        }

        /// <summary>
        /// Loads the missing tile at the given coordinate.
        /// </summary>
        /// <param name="coordinate"></param>
        private void LoadMissingTile(GeoCoordinate coordinate)
        {
            this.LoadMissingTile(Tile.CreateAroundLocation(coordinate, _zoom));
        }

        /// <summary>
        /// Loads the missing tiles.
        /// </summary>
        /// <param name="tile"></param>
        internal void LoadMissingTile(Tile tile)
        {
            if (!_loadedTiles.Contains(tile))
            { // the tile was not loaded yet.
                V1RoutingSerializer.SerializableGraphTileMeta meta;
                if (_graphTileMetas.TryGetValue(tile, out meta))
                { // the meta data is available.
                    V1RoutingSerializer.SerializableGraphTile tileData =
                        _routingSerializer.DeserializeTile(_stream, meta);
                    foreach (var vertex in tileData.Vertices)
                    {
                        // resize.
                        this.Resize(vertex.Id + 1);

                        // create the location.
                        var vertexLocation = new Location();
                        vertexLocation.Latitude = vertex.Latitude;
                        vertexLocation.Longitude = vertex.Longitude;
                        _coordinates[(int)vertex.Id] = vertexLocation;

                        // convert the arcs.
                        if (vertex.Arcs != null)
                        {
                            var arcs = new KeyValuePair<uint, PreProcessedEdge>[vertex.Arcs.Length];
                            for (int idx = 0; idx < vertex.Arcs.Length; idx++)
                            {
                                // convert the arc.
                                arcs[idx] = new KeyValuePair<uint, PreProcessedEdge>(
                                    vertex.Arcs[idx].DestinationId, vertex.Arcs[idx].Edge);

                                // store the target tile.
                                _tilesPerVertex[vertex.Arcs[idx].DestinationId] = 
                                    new Tile(vertex.Arcs[idx].TileX, vertex.Arcs[idx].TileY, _zoom);
                            }
                            _vertices[(int)vertex.Id] = new Vertex()
                            {
                                Arcs = arcs
                            };
                        }
                        _vertexIndex.Add(new GeoCoordinate(vertex.Latitude,
                            vertex.Longitude), vertex.Id);
                    }
                }

                _loadedTiles.Add(tile); // tile is loaded.
            }
        }

        #endregion
    }
}
