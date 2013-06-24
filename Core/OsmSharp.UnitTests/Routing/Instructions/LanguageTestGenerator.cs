using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OsmSharp.Routing.ArcAggregation.Output;
using OsmSharp.Routing.Instructions;
using OsmSharp.Routing.Instructions.LanguageGeneration;
using OsmSharp.Tools.Math.Geo.Meta;

namespace OsmSharp.UnitTests.Routing.Instructions
{
    /// <summary>
    /// Language test generator.
    /// </summary>
    public class LanguageTestGenerator : ILanguageGenerator
    {
        /// <summary>
        /// Direct turn instruction.
        /// </summary>
        /// <param name="instruction"></param>
        /// <param name="streetCountBeforeTurn"></param>
        /// <param name="streetTo"></param>
        /// <param name="direction"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        public Instruction GenerateDirectTurn(Instruction instruction, 
            int streetCountBeforeTurn, 
            List<KeyValuePair<string, string>> streetTo, 
            RelativeDirectionEnum direction, 
            List<PointPoi> list)
        {
            instruction.Text = string.Format("GenerateDirectTurn:{0}_{1}_{2}",
                                             streetCountBeforeTurn, direction.ToString(), list.Count);
            return instruction;
        }

        /// <summary>
        /// Generates an indirect turn.
        /// </summary>
        /// <param name="instruction"></param>
        /// <param name="streetCountTurn"></param>
        /// <param name="streetCountBeforeTurn"></param>
        /// <param name="streetTo"></param>
        /// <param name="direction"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        public Instruction GenerateIndirectTurn(Instruction instruction, 
            int streetCountTurn, 
            int streetCountBeforeTurn, 
            List<KeyValuePair<string, string>> streetTo, 
            RelativeDirectionEnum direction, 
            List<PointPoi> list)
        {
            instruction.Text = string.Format("GenerateIndirectTurn:{0}_{1}_{2}_{3}",
                                             streetCountTurn, streetCountBeforeTurn,
                                             direction.ToString(), list.Count);
            return instruction;
        }

        /// <summary>
        /// Generates poi instruction.
        /// </summary>
        /// <param name="instruction"></param>
        /// <param name="list"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        public Instruction GeneratePoi(Instruction instruction, List<PointPoi> list, 
            RelativeDirectionEnum? direction)
        {
            if (direction.HasValue)
            {
                instruction.Text = string.Format("GeneratePoi:{0}_{1}",
                    list.Count, direction.Value.ToString());
            }
            else
            {
                instruction.Text = string.Format("GeneratePoi:{0}",
                                                 list.Count);
            }
            return instruction;
        }

        /// <summary>
        /// Generates a direct follow turn.
        /// </summary>
        /// <param name="instruction"></param>
        /// <param name="streetCountBeforeTurn"></param>
        /// <param name="streetTo"></param>
        /// <param name="relativeDirectionEnum"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        public Instruction GenerateDirectFollowTurn(Instruction instruction, 
            int streetCountBeforeTurn, 
            List<KeyValuePair<string, string>> streetTo, 
            RelativeDirectionEnum relativeDirectionEnum,
            List<PointPoi> list)
        {
            instruction.Text = string.Format("GenerateDirectFollowTurn:{0}_{1}_{2}",
                                             streetCountBeforeTurn, relativeDirectionEnum.ToString(), list.Count);
            return instruction;
        }

        /// <summary>
        /// Generates an indirect follow turn.
        /// </summary>
        /// <param name="instruction"></param>
        /// <param name="streetCountTurn"></param>
        /// <param name="streetCountBeforeTurn"></param>
        /// <param name="streetTo"></param>
        /// <param name="relativeDirectionEnum"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        public Instruction GenerateIndirectFollowTurn(Instruction instruction, 
            int streetCountTurn, 
            int streetCountBeforeTurn, 
            List<KeyValuePair<string, string>> streetTo, 
            RelativeDirectionEnum relativeDirectionEnum,
            List<PointPoi> list)
        {
            instruction.Text = string.Format("GenerateDirectFollowTurn:{0}_{1}_{2}",
                                             streetCountBeforeTurn, streetCountBeforeTurn,
                                             relativeDirectionEnum.ToString(), list.Count);
            return instruction;
        }

        /// <summary>
        /// Generates an immidiate turn.
        /// </summary>
        /// <param name="instruction"></param>
        /// <param name="firstStreetCountTo"></param>
        /// <param name="firstStreetTo"></param>
        /// <param name="firstDirection"></param>
        /// <param name="secondStreetTo"></param>
        /// <param name="secondDirection"></param>
        /// <returns></returns>
        public Instruction GenerateImmidiateTurn(Instruction instruction, 
            int firstStreetCountTo, 
            List<KeyValuePair<string, string>> firstStreetTo, 
            OsmSharp.Tools.Math.Geo.Meta.RelativeDirection firstDirection, 
            List<KeyValuePair<string, string>> secondStreetTo, 
            RelativeDirection secondDirection)
        {
            instruction.Text = string.Format("GenerateImmidiateTurn:{0}_{1}_{2}_{3}",
                                             firstStreetCountTo, firstDirection,
                                             firstDirection.ToString(), 
                                             secondDirection.ToString());
            return instruction;
        }

        /// <summary>
        /// Generates a roundabout instruction.
        /// </summary>
        /// <param name="instruction"></param>
        /// <param name="count"></param>
        /// <param name="nextStreet"></param>
        /// <returns></returns>
        public Instruction GenerateRoundabout(Instruction instruction, 
            int count, List<KeyValuePair<string, string>> nextStreet)
        {
            instruction.Text = string.Format("GenerateRoundabout:{0}",
                                             count);
            return instruction;
        }

        /// <summary>
        /// Generates a simple turn instruction.
        /// </summary>
        /// <param name="instruction"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        public Instruction GenerateSimpleTurn(Instruction instruction, 
            RelativeDirectionEnum direction)
        {
            instruction.Text = string.Format("GenerateSimpleTurn:{0}",
                                             direction.ToString());
            return instruction;
        }
    }
}
