using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using OsmSharp.Routing.Graph;
using OsmSharp.Routing.Graph.DynamicGraph.SimpleWeighed;
using OsmSharp.Routing.Instructions;
using OsmSharp.Routing.Osm.Data.Processing;
using OsmSharp.Routing;
using OsmSharp.Osm.Data.XML.Processor;
using OsmSharp.Osm.Data.Core.Processor.Filter.Sort;
using OsmSharp.Tools.Math.Geo;
using OsmSharp.Routing.Route;
using System.Reflection;
using OsmSharp.Routing.Osm.Interpreter;
using OsmSharp.Osm;
using OsmSharp.Routing.Graph.Router.Dykstra;

namespace OsmSharp.UnitTests.Routing.Instructions
{
    /// <summary>
    /// Contains tests for instruction generation.
    /// </summary>
    [TestFixture]
    public class InstructionsGenerationTests
    {
        /// <summary>
        /// Tests a short route with a simple turn.
        /// </summary>
        [Test]
        public void InstructionGenerationSimpleTurnSameStreet()
        {
            // create new interpreter.
            var interpreter = new OsmRoutingInterpreter();

            // create the language generator.
            var languageGenerator = new LanguageTestGenerator();

            // calculate the route.
            OsmSharpRoute route = this.Calculate(
                new GeoCoordinate(51.09030, 3.44391), 
                new GeoCoordinate(51.09002, 3.44380));

            // generate the instructions.
            var instructionGenerator = new InstructionGenerator();
            List<Instruction> instructions = 
                instructionGenerator.Generate(route, interpreter, languageGenerator);

            // test the results in the language generator.
            Assert.AreEqual(3, instructions.Count);
            Assert.AreEqual("GeneratePoi:1", instructions[0].Text);
            Assert.AreEqual("GenerateDirectTurn:0_Left_0", instructions[1].Text);
            Assert.AreEqual("GeneratePoi:1", instructions[2].Text);
        }

        /// <summary>
        /// Tests a short route with a simple turn.
        /// </summary>
        [Test]
        public void InstructionGenerationSimpleTurnDifferentStreet()
        {
            // create new interpreter.
            var interpreter = new OsmRoutingInterpreter();

            // create the language generator.
            var languageGenerator = new LanguageTestGenerator();

            // calculate the route.
            OsmSharpRoute route = this.Calculate(
                new GeoCoordinate(51.088261, 3.443348),
                new GeoCoordinate(51.087785, 3.442715));

            // generate the instructions.
            var instructionGenerator = new InstructionGenerator();
            List<Instruction> instructions =
                instructionGenerator.Generate(route, interpreter, languageGenerator);

            // test the results in the language generator.
            Assert.AreEqual(3, instructions.Count);
            Assert.AreEqual("GeneratePoi:1", instructions[0].Text);
            Assert.AreEqual("GenerateDirectTurn:0_Left_0", instructions[1].Text);
            Assert.AreEqual("GeneratePoi:1", instructions[2].Text);
        }

        /// <summary>
        /// Tests a short route without any turns.
        /// </summary>
        [Test]
        public void InstructionGenerationNoTurns()
        {
            // create new interpreter.
            var interpreter = new OsmRoutingInterpreter();

            // create the language generator.
            var languageGenerator = new LanguageTestGenerator();

            // calculate the route.
            OsmSharpRoute route = this.Calculate(
                new GeoCoordinate(51.09002, 3.44380),
                new GeoCoordinate(51.089900970459, 3.44386267662048));

            // generate the instructions.
            var instructionGenerator = new InstructionGenerator();
            List<Instruction> instructions = 
                instructionGenerator.Generate(route, interpreter, languageGenerator);

            // test the results in the language generator.
            Assert.AreEqual(2, instructions.Count);
            Assert.AreEqual("GeneratePoi:1", instructions[0].Text);
            Assert.AreEqual("GeneratePoi:1", instructions[1].Text);
        }

        /// <summary>
        /// Tests a short route with just a few turns.
        /// </summary>
        [Test]
        public void InstructionGenerationAFewTurns()
        {
            // create new interpreter.
            var interpreter = new OsmRoutingInterpreter();

            // create the language generator.
            var languageGenerator = new LanguageTestGenerator();

            // calculate the route.
            OsmSharpRoute route = this.Calculate(
                new GeoCoordinate(51.089900970459, 3.44386267662048),
                new GeoCoordinate(51.0862655639648, 3.44465517997742));

            // generate the instructions.
            var instructionGenerator = new InstructionGenerator();
            List<Instruction> instructions = 
                instructionGenerator.Generate(route, interpreter, languageGenerator);

            // test the results in the language generator.
            Assert.AreEqual(6, instructions.Count);
            Assert.AreEqual("GeneratePoi:1", instructions[0].Text);
            Assert.AreEqual("GenerateDirectTurn:0_Right_0", instructions[1].Text);
            Assert.AreEqual("GenerateDirectTurn:0_Left_0", instructions[2].Text);
            Assert.AreEqual("GenerateDirectTurn:0_Left_0", instructions[3].Text);
            Assert.AreEqual("GenerateDirectTurn:0_Right_0", instructions[4].Text);
            Assert.AreEqual("GeneratePoi:1", instructions[5].Text);
        }

        /// <summary>
        /// Holds the router.
        /// </summary>
        private Router<SimpleWeighedEdge> _router;

        /// <summary>
        /// Calculates a route to test on.
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        private OsmSharpRoute Calculate(GeoCoordinate from, GeoCoordinate to)
        { 
            if (_router == null)
            {
                var interpreter = new OsmRoutingInterpreter();
                var tagsIndex = new OsmTagsIndex();

                // do the data processing.
                var memoryData =
                    new DynamicGraphRouterDataSource<SimpleWeighedEdge>(tagsIndex);
                var targetData = new SimpleWeighedDataGraphProcessingTarget(
                    memoryData, interpreter, memoryData.TagsIndex, VehicleEnum.Car);
                var dataProcessorSource = new XmlDataProcessorSource(
                    Assembly.GetExecutingAssembly().GetManifestResourceStream(
                        "OsmSharp.UnitTests.test_instructions.osm"));
                var sorter = new DataProcessorFilterSort();
                sorter.RegisterSource(dataProcessorSource);
                targetData.RegisterSource(sorter);
                targetData.Pull();

                _router = new Router<SimpleWeighedEdge>(
                    memoryData, interpreter, new DykstraRoutingLive(memoryData.TagsIndex));
            }

            RouterPoint fromPoint = _router.Resolve(VehicleEnum.Car, from);
            RouterPoint toPoint = _router.Resolve(VehicleEnum.Car, to);
            return _router.Calculate(VehicleEnum.Car, fromPoint, toPoint);
        }
    }
}
