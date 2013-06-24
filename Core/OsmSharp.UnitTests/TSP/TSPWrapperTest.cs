using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using NUnit.Framework;
using OsmSharp.Osm;
using OsmSharp.Osm.Data.Core.Processor.Filter.Sort;
using OsmSharp.Osm.Data.XML.Processor;
using OsmSharp.Routing;
using OsmSharp.Routing.Graph;
using OsmSharp.Routing.Graph.DynamicGraph.PreProcessed;
using OsmSharp.Routing.Graph.Router.Dykstra;
using OsmSharp.Routing.Osm.Data.Processing;
using OsmSharp.Routing.Osm.Interpreter;
using OsmSharp.Routing.Route;
using OsmSharp.Routing.TSP;
using OsmSharp.Routing.TSP.Genetic;
using OsmSharp.Tools.DelimitedFiles;
using OsmSharp.Tools.Math.Geo;

namespace OsmSharp.UnitTests.TSP
{
    /// <summary>
    /// Some tests on the TSP wrapper.
    /// </summary>
    [TestFixture]
    public class TSPWrapperTest
    {
        /// <summary>
        /// Tests the vehicle type of the resulting route.
        /// </summary>
        [Test]
        public void TestTSPWrapperVehicle()
        {
            // calculate TSP.
            OsmSharpRoute route = this.CalculateTSP(Assembly.GetExecutingAssembly()
                                                            .GetManifestResourceStream(
                                                                @"OsmSharp.UnitTests.tsp_real.osm"),
                                                    Assembly.GetExecutingAssembly()
                                                            .GetManifestResourceStream(
                                                                @"OsmSharp.UnitTests.tsp_real.csv"),
                                                    false,
                                                    VehicleEnum.Car);

            Assert.IsNotNull(route);
            Assert.AreEqual(VehicleEnum.Car, route.Vehicle);
        }

        /// <summary>
        /// Tests the vehicle type of the resulting route.
        /// </summary>
        [Test]
        public void TestTSPWrapperMetric()
        {
            // calculate TSP.
            OsmSharpRoute route = this.CalculateTSP(Assembly.GetExecutingAssembly()
                                                            .GetManifestResourceStream(
                                                                @"OsmSharp.UnitTests.tsp_real.osm"),
                                                    Assembly.GetExecutingAssembly()
                                                            .GetManifestResourceStream(
                                                                @"OsmSharp.UnitTests.tsp_real.csv"),
                                                    false,
                                                    VehicleEnum.Car);

            Assert.IsNotNull(route);
            Assert.AreNotEqual(0, route.TotalDistance);
            Assert.AreNotEqual(0, route.TotalTime);
        }

        /// <summary>
        /// Calculates the TSP.
        /// </summary>
        /// <param name="dataStream"></param>
        /// <param name="csvStream"></param>
        /// <param name="pbf"></param>
        /// <param name="vehicleEnum"></param>
        /// <returns></returns>
        private OsmSharpRoute CalculateTSP(Stream dataStream, Stream csvStream, bool pbf, VehicleEnum vehicleEnum)
        {
            // create the router.
            var interpreter = new OsmRoutingInterpreter();
            var tagsIndex = new OsmTagsIndex();

            // do the data processing.
            var osmData =
                new DynamicGraphRouterDataSource<PreProcessedEdge>(tagsIndex);
            var targetData = new PreProcessedDataGraphProcessingTarget(
                osmData, interpreter, osmData.TagsIndex, vehicleEnum);
            var dataProcessorSource = new XmlDataProcessorSource(dataStream);
            var sorter = new DataProcessorFilterSort();
            sorter.RegisterSource(dataProcessorSource);
            targetData.RegisterSource(sorter);
            targetData.Pull();

            IRouter<RouterPoint> router = new Router<PreProcessedEdge>(osmData, interpreter,
                new DykstraRoutingPreProcessed(osmData.TagsIndex));

            // read the source files.
            const int latitudeIdx = 2;
            const int longitudeIdx = 3;
            string[][] pointStrings = OsmSharp.Tools.DelimitedFiles.DelimitedFileHandler.ReadDelimitedFileFromStream(
                csvStream,
                DelimiterType.DotCommaSeperated);
            var points = new List<RouterPoint>();
            int cnt = 10;
            foreach (string[] line in pointStrings)
            {
                if (points.Count >= cnt)
                {
                    break;
                }
                var latitudeString = (string)line[latitudeIdx];
                var longitudeString = (string)line[longitudeIdx];

                //string route_ud = (string)line[1];

                double longitude = 0;
                double latitude = 0;
                if (double.TryParse(longitudeString, System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out longitude) &&
                   double.TryParse(latitudeString, System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out latitude))
                {
                    var point = new GeoCoordinate(latitude, longitude);

                    RouterPoint resolved = router.Resolve(VehicleEnum.Car, point);
                    if (resolved != null && router.CheckConnectivity(VehicleEnum.Car, resolved, 100))
                    {
                        points.Add(resolved);
                    }
                }
            }

            var tspSolver = new RouterTSPWrapper<RouterPoint, RouterTSP>(
                new RouterTSPAEXGenetic(), router, interpreter);
            return tspSolver.CalculateTSP(vehicleEnum, points.ToArray());
        }
    }
}
