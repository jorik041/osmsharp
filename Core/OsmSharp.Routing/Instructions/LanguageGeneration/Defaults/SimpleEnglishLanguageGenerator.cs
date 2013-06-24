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
using OsmSharp.Routing.ArcAggregation.Output;

namespace OsmSharp.Routing.Instructions.LanguageGeneration.Defaults
{
    /// <summary>
    /// A simple instruction generator, translating instructions into the english language.
    /// </summary>
    public class SimpleEnglishLanguageGenerator : ILanguageGenerator
    {
        private string TurnDirection(RelativeDirectionEnum direction)
        {
            switch (direction)
            {
                case RelativeDirectionEnum.Right:
                case RelativeDirectionEnum.SharpRight:
                case RelativeDirectionEnum.SlightlyRight:
                    return "right";
                case RelativeDirectionEnum.Left:
                case RelativeDirectionEnum.SharpLeft:
                case RelativeDirectionEnum.SlightlyLeft:
                    return "left";
                case RelativeDirectionEnum.TurnBack:
                    return "back";
            }
            return string.Empty;
        }

        #region ILanguageGenerator Members

        /// <summary>
        /// Generates an instruction for a direct turn.
        /// </summary>
        /// <param name="instruction"></param>
        /// <param name="streetCountBeforeTurn"></param>
        /// <param name="streetTo"></param>
        /// <param name="direction"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        public Instruction GenerateDirectTurn(Instruction instruction, int streetCountBeforeTurn,
            List<KeyValuePair<string, string>> streetTo, RelativeDirectionEnum direction, List<PointPoi> list)
        {
            if (streetCountBeforeTurn == 1)
            {
                instruction.Text = string.Format("Take the first turn {0}, on {1}.",
                    TurnDirection(direction),
                    this.GetName("en",streetTo));
            }
            else
            {
                instruction.Text = string.Format("Take the {0}th turn {1}, on {2}.",
                    streetCountBeforeTurn,
                    TurnDirection(direction),
                    this.GetName("en",streetTo));
            }

            // returns the instruction with text.
            return instruction;
        }

        /// <summary>
        /// Generates an instruction for an indirect turn.
        /// </summary>
        /// <param name="instruction"></param>
        /// <param name="streetCountTurn"></param>
        /// <param name="streetCountBeforeTurn"></param>
        /// <param name="streetTo"></param>
        /// <param name="direction"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        public Instruction GenerateIndirectTurn(Instruction instruction, int streetCountTurn, int streetCountBeforeTurn,
            List<KeyValuePair<string, string>> streetTo, RelativeDirectionEnum direction, List<PointPoi> list)
        {
            instruction.Text = string.Format("Take the {0}d turn {1}, on {2}.",
                streetCountBeforeTurn,
                TurnDirection(direction),
                this.GetName("en",streetTo));

            // returns the instruction with text.
            return instruction;
        }

        /// <summary>
        /// Generates an instruction for a POI.
        /// </summary>
        /// <param name="instruction"></param>
        /// <param name="list"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        public Instruction GeneratePoi(Instruction instruction, List<PointPoi> list, RelativeDirectionEnum? direction)
        {
            if (direction == null)
            {
                instruction.Text = string.Format("Poi");
            }
            else
            {
                instruction.Text = string.Format("Poi:{0}", direction);
            }

            // returns the instruction with text.
            return instruction;
        }

        /// <summary>
        /// Generates an instruction for a turn followed by another turn.
        /// </summary>
        /// <param name="instruction"></param>
        /// <param name="streetCountBeforeTurn"></param>
        /// <param name="streetTo"></param>
        /// <param name="direction"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        public Instruction GenerateDirectFollowTurn(Instruction instruction, int streetCountBeforeTurn, List<KeyValuePair<string, string>> streetTo, 
            RelativeDirectionEnum direction, List<PointPoi> list)
        {
            if (streetCountBeforeTurn == 1)
            {
                instruction.Text = string.Format("Turn {1} to stay on {0}.",
                    this.GetName("en",streetTo),
                    TurnDirection(direction));
            }
            else
            {
                instruction.Text = string.Format("Turn {1}d street {2} to stay on {0}.",
                    this.GetName("en",streetTo),
                    streetCountBeforeTurn,
                    TurnDirection(direction));
            }

            // returns the instruction with text.
            return instruction;
        }

        /// <summary>
        /// Generates an instruction for an indirect turn.
        /// </summary>
        /// <param name="instruction"></param>
        /// <param name="streetCountTurn"></param>
        /// <param name="streetCountBeforeTurn"></param>
        /// <param name="streetTo"></param>
        /// <param name="direction"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        public Instruction GenerateIndirectFollowTurn(Instruction instruction, int streetCountTurn, int streetCountBeforeTurn, List<KeyValuePair<string, string>> streetTo, 
            RelativeDirectionEnum direction, List<PointPoi> list)
        {
            if (streetCountBeforeTurn == 1)
            {
                instruction.Text = string.Format("Turn {1} to stay on {0}.",
                    this.GetName("en",streetTo),
                    TurnDirection(direction));
            }
            else
            {
                instruction.Text = string.Format("Take the {1}d street {2} to stay on {0}.",
                    this.GetName("en",streetTo),
                    streetCountBeforeTurn,
                    TurnDirection(direction));
            }

            // returns the instruction with text.
            return instruction;
        }

        /// <summary>
        /// Generates an instruction for an immidiate turn.
        /// </summary>
        /// <param name="instruction"></param>
        /// <param name="firstStreetCountTo"></param>
        /// <param name="firstStreetTo"></param>
        /// <param name="firstDirection"></param>
        /// <param name="secondStreetTo"></param>
        /// <param name="secondDirection"></param>
        /// <returns></returns>
        public Instruction GenerateImmidiateTurn(Instruction instruction, int firstStreetCountTo, List<KeyValuePair<string, string>> firstStreetTo,
            RelativeDirection firstDirection, List<KeyValuePair<string, string>> secondStreetTo, RelativeDirection secondDirection)
        {
            if (firstStreetCountTo == 1)
            {
                instruction.Text = string.Format("Take the first turn {0}, on the {1}, and turn immidiately {2} on the {3}.",
                    TurnDirection(firstDirection.Direction),
                    this.GetName("en",firstStreetTo),
                    TurnDirection(secondDirection.Direction),
                    this.GetName("en",secondStreetTo));
            }
            else
            {
                instruction.Text = string.Format("Take the {4}d turn {0}, on the {1}, and turn immidiately {2} on the {3}.",
                    TurnDirection(firstDirection.Direction),
                    this.GetName("en",firstStreetTo),
                    TurnDirection(secondDirection.Direction),
                    this.GetName("en",secondStreetTo),
                    firstStreetCountTo);
            }

            // returns the instruction with text.
            return instruction;
        }

        /// <summary>
        /// Generates an instruction for a roundabout.
        /// </summary>
        /// <param name="instruction"></param>
        /// <param name="count"></param>
        /// <param name="nextStreet"></param>
        /// <returns></returns>
        public Instruction GenerateRoundabout(Instruction instruction, int count, List<KeyValuePair<string, string>> nextStreet)
        {
            instruction.Text = string.Format("Take the {0}d at the next roundabout on the {1}.",
                count,
                this.GetName("en",nextStreet));

            // returns the instruction with text.
            return instruction;
        }

        /// <summary>
        /// Generates an instruction for a simple turn.
        /// </summary>
        /// <param name="instruction"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        public Instruction GenerateSimpleTurn(Instruction instruction, RelativeDirectionEnum direction)
        {
            instruction.Text = string.Format("Turn {0}", this.TurnDirection(direction));

            return instruction;
        }

        #endregion
        
        private string GetName(string language_key, List<KeyValuePair<string, string>> tags)
        {
            language_key = language_key.ToLower();

            string name = string.Empty;
            foreach (KeyValuePair<string, string> tag in tags)
            {
                if (tag.Key != null && tag.Key.ToLower() == string.Format("name:{0}", language_key))
                {
                    return tag.Value;
                }
                if (tag.Key != null && tag.Key.ToLower() == "name")
                {
                    name = tag.Key;
                }
            }
            return name;
        }
    }
}
