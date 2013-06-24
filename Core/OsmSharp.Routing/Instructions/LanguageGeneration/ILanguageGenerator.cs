// OsmSharp - OpenStreetMap tools & library.
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OsmSharp.Tools.Math.Geo.Meta;

namespace OsmSharp.Routing.Instructions.LanguageGeneration
{
    /// <summary>
    /// Language generator interface.
    /// </summary>
    public interface ILanguageGenerator
    {
        /// <summary>
        /// Generates a direct turn instruction.
        /// </summary>
        /// <param name="instruction"></param>
        /// <param name="streetCountBeforeTurn"></param>
        /// <param name="streetTo"></param>
        /// <param name="direction"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        Instruction GenerateDirectTurn(Instruction instruction, int streetCountBeforeTurn, List<KeyValuePair<string, string>> streetTo, RelativeDirectionEnum direction, List<Routing.ArcAggregation.Output.PointPoi> list);

        /// <summary>
        /// Generates an indirect turn instruction.
        /// </summary>
        /// <param name="instruction"></param>
        /// <param name="streetCountTurn"></param>
        /// <param name="streetCountBeforeTurn"></param>
        /// <param name="streetTo"></param>
        /// <param name="direction"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        Instruction GenerateIndirectTurn(Instruction instruction, int streetCountTurn, int streetCountBeforeTurn, List<KeyValuePair<string, string>> streetTo, RelativeDirectionEnum direction, List<Routing.ArcAggregation.Output.PointPoi> list);

        /// <summary>
        /// Generates a POI instruction.
        /// </summary>
        /// <param name="instruction"></param>
        /// <param name="list"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        Instruction GeneratePoi(Instruction instruction, List<Routing.ArcAggregation.Output.PointPoi> list, RelativeDirectionEnum? direction);

        /// <summary>
        /// Generates an indirect turn instruction.
        /// </summary>
        /// <param name="instruction"></param>
        /// <param name="streetCountBeforeTurn"></param>
        /// <param name="streetTo"></param>
        /// <param name="relativeDirectionEnum"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        Instruction GenerateDirectFollowTurn(Instruction instruction, int streetCountBeforeTurn, List<KeyValuePair<string, string>> streetTo, RelativeDirectionEnum relativeDirectionEnum, List<Routing.ArcAggregation.Output.PointPoi> list);

        /// <summary>
        /// Generates and indirect turn instruction.
        /// </summary>
        /// <param name="instruction"></param>
        /// <param name="streetCountTurn"></param>
        /// <param name="streetCountBeforeTurn"></param>
        /// <param name="streetTo"></param>
        /// <param name="relativeDirectionEnum"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        Instruction GenerateIndirectFollowTurn(Instruction instruction, int streetCountTurn, int streetCountBeforeTurn, List<KeyValuePair<string, string>> streetTo, RelativeDirectionEnum relativeDirectionEnum, List<Routing.ArcAggregation.Output.PointPoi> list);

        /// <summary>
        /// Generates an immidiate turn instruction.
        /// </summary>
        /// <param name="instruction"></param>
        /// <param name="firstStreetCountTo"></param>
        /// <param name="firstStreetTo"></param>
        /// <param name="firstDirection"></param>
        /// <param name="secondStreetTo"></param>
        /// <param name="secondDirection"></param>
        /// <returns></returns>
        Instruction GenerateImmidiateTurn(Instruction instruction, int firstStreetCountTo, List<KeyValuePair<string, string>> firstStreetTo, RelativeDirection firstDirection, List<KeyValuePair<string, string>> secondStreetTo, RelativeDirection secondDirection);

        /// <summary>
        /// Generates a roundabout instruction.
        /// </summary>
        /// <param name="instruction"></param>
        /// <param name="count"></param>
        /// <param name="nextStreet"></param>
        /// <returns></returns>
        Instruction GenerateRoundabout(Instruction instruction, int count, List<KeyValuePair<string, string>> nextStreet);

        /// <summary>
        /// Generates a simple turn instructions.
        /// </summary>
        /// <param name="direction_instruction"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        Instruction GenerateSimpleTurn(Instruction direction_instruction, RelativeDirectionEnum direction);
    }
}
