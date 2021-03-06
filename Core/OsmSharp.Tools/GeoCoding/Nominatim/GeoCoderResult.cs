﻿// OsmSharp - OpenStreetMap tools & library.
// Copyright (C) 2012 Abelshausen Ben
// 
// This file is part of OsmSharp.
// 
// OsmSharp is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// OsmSharp is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with OsmSharp. If not, see <http://www.gnu.org/licenses/>.

namespace OsmSharp.Tools.GeoCoding.Nominatim
{
    /// <summary>
    /// Represents a geocoding result.
    /// </summary>
    public class GeoCoderResult : IGeoCoderResult
    {
        /// <summary>
        /// Latitude.
        /// </summary>
        public double Latitude
        {
            get;
            set;
        }

        /// <summary>
        /// Longitude.
        /// </summary>
        public double Longitude
        {
            get;
            set;
        }

        /// <summary>
        /// The query text.
        /// </summary>
    	public string Text { get; set;  }

        /// <summary>
        /// The accuracy.
        /// </summary>
        public AccuracyEnum Accuracy
        {
            get;
            set;
        }
    }
}
