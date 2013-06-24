using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using NUnit.Framework;
using OsmSharp.Osm.Data.SQLServer.SimpleSchema;
using OsmSharp.Osm.Data.SQLServer.SimpleSchema.Processor;
using OsmSharp.Osm.Data.XML.Processor;
using OsmSharp.Routing;
using OsmSharp.Routing.Graph.DynamicGraph.PreProcessed;
using OsmSharp.Routing.Graph.Router.Dykstra;
using OsmSharp.Routing.Osm.Data;
using OsmSharp.Routing.Osm.Interpreter;
using OsmSharp.Routing.Route;
using OsmSharp.Tools.Math.Geo;

namespace OsmSharp.Osm.Data.SQLServer.Unittests
{
    /// <summary>
    /// Contains some simple SQLServer routing tests.
    /// </summary>
    [TestFixture]
    public class SQLServerRoutingTests
    {
        /// <summary>
        /// A simple SQLServer routing regression test.
        /// </summary>
        [Test]
        public void SQLServerRoutingRegressionTests1()
        {
            // the connectionstring.
            string connectionString = 
                @"Server=windows8-test\sqlexpress;User Id=OsmSharp;Password=OsmSharp;Database=OsmData;";

            // drop whatever data is there.
            SqlConnection sqlConnection = new SqlConnection(connectionString);
            sqlConnection.Open();
            SQLServer.SimpleSchema.SchemaTools.SQLServerSimpleSchemaTools.Remove(sqlConnection);
            sqlConnection.Close();

            // create the source from the osm file.
            var xmlSource = new XmlDataProcessorSource(
                Assembly.GetExecutingAssembly().GetManifestResourceStream(
                    "OsmSharp.Osm.Data.SQLServer.Unittests.Data.ukraine1.osm"));

            // create the SQLServer processor target.
            var testTarget = new SQLServerSimpleSchemaDataProcessorTarget(connectionString, true);
            testTarget.RegisterSource(xmlSource); // register the source.
            testTarget.Pull(); // pull the data from source to target.

            var source = new SQLServerSimpleSchemaSource(
                connectionString);
            var tagsIndex = new OsmTagsIndex();
            var interpreter = new OsmRoutingInterpreter();
            var routingData = new OsmSourceRouterDataSource(
                interpreter, tagsIndex, source, VehicleEnum.Car);

            IRouter<RouterPoint> router = new Router<PreProcessedEdge>(routingData, interpreter, 
                new DykstraRoutingPreProcessed(routingData.TagsIndex));

            OsmSharpRoute route;

            RouterPoint point1 = router.Resolve(VehicleEnum.Car, 
                new GeoCoordinate(50.3150034243338, 34.8784106812928));
            RouterPoint point2 = router.Resolve(VehicleEnum.Car, 
                new GeoCoordinate(50.3092549484347, 34.8894929841615));
            route = router.Calculate(VehicleEnum.Car, point1, point2);

            Assert.IsNotNull(route);
        }
    }
}
