using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using OsmSharp.Tools.Math.Geo;
using OsmSharp.Osm.Renderer;
using System.Drawing;
using OsmSharp.Osm.Renderer.Gdi.Targets;

namespace OsmSharp.UI.Unittests.Map.Renderer
{
    /// <summary>
    /// Contains some tests on the view class.
    /// </summary>
    [TestFixture]
    public class ViewTests
    {
        /// <summary>
        /// A regression test for a bug found in the view class.
        /// </summary>
        [Test]
        public void ViewTestsSimple()
        {
            var center = new GeoCoordinate(51.0886211395264,3.45352852344513);

            Image img = new Bitmap(2500, 2500);
            var target = new ImageTarget(img);

            View view = View.CreateFrom(target, 17, center);

            GeoCoordinate centerAfter = view.ConvertFromTargetCoordinates(target,
                target.XRes / 2f, target.YRes / 2f);

            Assert.AreEqual(center.Latitude, centerAfter.Latitude, 0.000001);
            Assert.AreEqual(center.Longitude, centerAfter.Longitude, 0.000001);
        }
    }
}