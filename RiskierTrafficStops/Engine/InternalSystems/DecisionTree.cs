namespace RiskierTrafficStops.Engine.InternalSystems;
// https://github.com/Rohit685/MysteriousCallouts/blob/master/HelperSystems/DecisionTree.cs

internal abstract class DecisionNode
{
    internal abstract string Evaluate(Dictionary<string, object> inputs);
}

internal class AttributeNode : DecisionNode
{
    internal string AttributeName;
    internal object AttributeValue;
    internal DecisionNode TrueBranch;
    internal DecisionNode FalseBranch;

    internal AttributeNode(string name, object value, DecisionNode trueBranch, DecisionNode falseBranch)
    {
        AttributeName = name;
        AttributeValue = value;
        this.TrueBranch = trueBranch;
        this.FalseBranch = falseBranch;
    }

    internal override string Evaluate(Dictionary<string, object> inputs)
    {
        object value;
        if (inputs.TryGetValue(AttributeName, out value))
        {
            Logger.Normal($"{value.ToString()}");
            return value.Equals(AttributeValue) ? TrueBranch.Evaluate(inputs) : FalseBranch.Evaluate(inputs);
        }
        else
        {
            Logger.Normal($"Missing input value for attribute {AttributeName}");
            throw new ArgumentException();
        }
    }
}

internal class DecisionLeaf : DecisionNode
{
    private string decision;

    internal DecisionLeaf(string value)
    {
        decision = value;
    }

    internal override string Evaluate(Dictionary<string, object> inputs)
    {
        return decision;
    }
}