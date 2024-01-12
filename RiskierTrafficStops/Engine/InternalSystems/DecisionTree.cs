// TODO: Implement
// using System;
// using System.Collections.Generic;
//
// namespace RiskierTrafficStops.Engine.InternalSystems
// {
//     // https://github.com/Rohit685/MysteriousCallouts/blob/master/HelperSystems/DecisionTree.cs
//     
//     internal abstract class DecisionNode
//     {
//         internal abstract string Evaluate(Dictionary<string, object> inputs);
//     }
//
//     internal class AttributeNode : DecisionNode
//     {
//         internal string attributeName;
//         internal object attributeValue;
//         internal DecisionNode trueBranch;
//         internal DecisionNode falseBranch;
//
//         internal AttributeNode(string name, object value, DecisionNode trueBranch, DecisionNode falseBranch)
//         {
//             attributeName = name;
//             attributeValue = value;
//             this.trueBranch = trueBranch;
//             this.falseBranch = falseBranch;
//         }
//
//         internal override string Evaluate(Dictionary<string, object> inputs)
//         {
//             object value;
//             if (inputs.TryGetValue(attributeName, out value))
//             {
//                 Logger.Normal($"{value.ToString()}");
//                 return value.Equals(attributeValue) ? trueBranch.Evaluate(inputs) : falseBranch.Evaluate(inputs);
//             }
//             else
//             {
//                 Logger.Normal($"Missing input value for attribute {attributeName}");
//                 throw new ArgumentException();
//             }
//         }
//     }
//
//     internal class DecisionLeaf : DecisionNode
//     {
//         private string decision;
//
//         internal DecisionLeaf(string value)
//         {
//             decision = value;
//         }
//
//         internal override string Evaluate(Dictionary<string, object> inputs)
//         {
//             return decision;
//         }
//     }
// }