﻿// License: MIT
// Source, Docs, Issues: https://github.com/cdanek/kaimira-weighted-list/

using System.Collections;
using System.Text;
using static RiskierTrafficStops.Engine.Helpers.WeightErrorHandlingType;

namespace RiskierTrafficStops.Engine.Helpers;

/// <summary>
/// This implements an algorithm for sampling from a discrete probability distribution via a generic list
/// with extremely fast O(1) get operations and small (close to minimally small) O(n) space complexity and
/// O(n) CRUD complexity. In other words, you can add any item of type T to a List with an integer weight,
/// and get a random item from the list with probability ( weight / sum-weights ).
/// </summary>
internal class WeightedList<T> : IEnumerable<T>
{
    /// <summary>
    /// Create a new WeightedList with an optional System.Random.
    /// </summary>
    /// <param name="rand"></param>
    public WeightedList(Random rand = null)
    {
        _rand = rand ?? new Random();
    }

    /// <summary>
    /// Create a WeightedList with the provided items and an optional System.Random.
    /// </summary>
    public WeightedList(IEnumerable<WeightedListItem<T>> listItems, Random rand = null)
    {
        _rand = rand ?? new Random();
        foreach (var item in listItems)
        {
            _list.Add(item.Item);
            _weights.Add(item.Weight);
        }

        Recalculate();
    }

    public WeightErrorHandlingType BadWeightErrorHandling { get; set; } = SetWeightToOne;

    public T Next()
    {
        if (Count == 0) return default;
        var nextInt = _rand.Next(Count);
        if (_areAllProbabilitiesIdentical) return _list[nextInt];
        var nextProbability = _rand.Next(_totalWeight);
        return (nextProbability < _probabilities[nextInt]) ? _list[nextInt] : _list[_alias[nextInt]];
    }

    public void AddWeightToAll(int weight)
    {
        if (weight + _minWeight <= 0 && BadWeightErrorHandling == ThrowExceptionOnAdd)
            throw new ArgumentException(
                $"Subtracting {-1 * weight} from all items would set weight to non-positive for at least one element.");
        for (var i = 0; i < Count; i++)
        {
            _weights[i] = FixWeight(_weights[i] + weight);
        }

        Recalculate();
    }

    public void SubtractWeightFromAll(int weight) => AddWeightToAll(weight * -1);

    public void SetWeightOfAll(int weight)
    {
        if (weight <= 0 && BadWeightErrorHandling == ThrowExceptionOnAdd)
            throw new ArgumentException("Weight cannot be non-positive.");
        for (var i = 0; i < Count; i++) _weights[i] = FixWeight(weight);
        Recalculate();
    }

    public int TotalWeight => _totalWeight;

    /// <summary>
    /// Minimum weight in the structure. 0 if Count == 0.
    /// </summary>
    public int MinWeight => _minWeight;

    /// <summary>
    /// Maximum weight in the structure. 0 if Count == 0.
    /// </summary>
    public int MaxWeight => _maxWeight;

    public IReadOnlyList<T> Items => _list.AsReadOnly();

    public IEnumerator<T> GetEnumerator() => _list.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => _list.GetEnumerator();

    public void Add(T item, int weight)
    {
        _list.Add(item);
        _weights.Add(FixWeight(weight));
        Recalculate();
    }

    public void Add(ICollection<WeightedListItem<T>> listItems)
    {
        foreach (var listItem in listItems)
        {
            _list.Add(listItem.Item);
            _weights.Add(FixWeight(listItem.Weight));
        }

        Recalculate();
    }

    public void Clear()
    {
        _list.Clear();
        _weights.Clear();
        Recalculate();
    }

    // 06.01.2024 by MarcelWRLD: Method to clone the whole weighted list and return the new list
    public WeightedList<T> Clone(Random rand = null)
    {
        WeightedList<T> clone = new(rand)
        {
            _list = _list.ToList(), // pass a new list, not a reference
            _weights = _weights.ToList() // pass a new list, not a reference
        };
        clone.Recalculate();
        return clone;
    }

    // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
    public void Contains(T item) => _list.Contains(item);

    public int IndexOf(T item) => _list.IndexOf(item);

    public void Insert(int index, T item, int weight)
    {
        _list.Insert(index, item);
        _weights.Insert(index, FixWeight(weight));
        Recalculate();
    }

    public void Remove(T item)
    {
        var index = IndexOf(item);
        RemoveAt(index);
        Recalculate();
    }

    public void RemoveAt(int index)
    {
        _list.RemoveAt(index);
        _weights.RemoveAt(index);
        Recalculate();
    }

    public T this[int index] => _list[index];

    public int Count => _list.Count;

    public void SetWeight(T item, int newWeight) => SetWeightAtIndex(IndexOf(item), FixWeight(newWeight));

    public int GetWeightOf(T item) => GetWeightAtIndex(IndexOf(item));

    public void SetWeightAtIndex(int index, int newWeight)
    {
        _weights[index] = FixWeight(newWeight);
        Recalculate();
    }

    public int GetWeightAtIndex(int index) => _weights[index];

    public override string ToString()
    {
        StringBuilder sb = new();
        sb.Append("WeightedList<");
        sb.Append(typeof(T).Name);
        sb.Append(">: TotalWeight:");
        sb.Append(TotalWeight);
        sb.Append(", Min:");
        sb.Append(_minWeight);
        sb.Append(", Max:");
        sb.Append(_maxWeight);
        sb.Append(", Count:");
        sb.Append(Count);
        sb.Append(", {");
        for (var i = 0; i < _list.Count; i++)
        {
            sb.Append(_list[i].ToString());
            sb.Append(":");
            sb.Append(_weights[i].ToString());
            if (i < _list.Count - 1) sb.Append(", ");
        }

        sb.Append("}");
        return sb.ToString();
    }

    private List<T>
        _list = []; // 06.01.2024 by MarcelWRLD: removed 'readonly' modifier to allow cloning a weighted list

    private List<int>
        _weights = []; // 06.01.2024 by MarcelWRLD: removed 'readonly' modifier to allow cloning a weighted list

    private readonly List<int> _probabilities = [];
    private readonly List<int> _alias = [];
    private readonly Random _rand;
    private int _totalWeight;
    private bool _areAllProbabilitiesIdentical;
    private int _minWeight;
    private int _maxWeight;

    /// <summary>
    /// https://www.keithschwarz.com/darts-dice-coins/
    /// </summary>
    private void Recalculate()
    {
        _totalWeight = 0;
        _areAllProbabilitiesIdentical = false;
        _minWeight = 0;
        _maxWeight = 0;
        var isFirst = true;

        _alias.Clear(); // STEP 1
        _probabilities.Clear(); // STEP 1

        List<int> scaledProbabilityNumerator = new(Count);
        List<int> small = new(Count); // STEP 2
        List<int> large = new(Count); // STEP 2
        foreach (var weight in _weights)
        {
            if (isFirst)
            {
                _minWeight = _maxWeight = weight;
                isFirst = false;
            }

            _minWeight = (weight < _minWeight) ? weight : _minWeight;
            _maxWeight = (_maxWeight < weight) ? weight : _maxWeight;
            _totalWeight += weight;
            scaledProbabilityNumerator.Add(weight * Count); // STEP 3 
            _alias.Add(0);
            _probabilities.Add(0);
        }

        // Degenerate case, all probabilities are equal.
        if (_minWeight == _maxWeight)
        {
            _areAllProbabilitiesIdentical = true;
            return;
        }

        // STEP 4
        for (var i = 0; i < Count; i++)
        {
            if (scaledProbabilityNumerator[i] < _totalWeight)
                small.Add(i);
            else
                large.Add(i);
        }

        // STEP 5
        while (small.Count > 0 && large.Count > 0)
        {
            var l = small[small.Count - 1]; // 5.1
            small.RemoveAt(small.Count - 1);
            var g = large[large.Count - 1]; // 5.2
            large.RemoveAt(large.Count - 1);
            _probabilities[l] = scaledProbabilityNumerator[l]; // 5.3
            _alias[l] = g; // 5.4
            var tmp = scaledProbabilityNumerator[g] + scaledProbabilityNumerator[l] -
                      _totalWeight; // 5.5, even though using ints for this algorithm is stable
            scaledProbabilityNumerator[g] = tmp;
            if (tmp < _totalWeight)
                small.Add(g); // 5.6 the large is now in the small pile
            else
                large.Add(g); // 5.7 add the large back to the large pile
        }

        // STEP 6
        while (large.Count > 0)
        {
            var g = large[large.Count - 1]; // 6.1
            large.RemoveAt(large.Count - 1);
            _probabilities[g] = _totalWeight; //6.1
        }

        // STEP 7 - Can't happen for this implementation but left in source to match Keith Schwarz's algorithm
#pragma warning disable S125 // Sections of code should not be commented out
        //while (small.Count > 0)
        //{
        //    int l = small[^1]; // 7.1
        //    small.RemoveAt(small.Count - 1);
        //    _probabilities[l] = _totalWeight;
        //}
#pragma warning restore S125 // Sections of code should not be commented out
    }

    // Adjust bad weights silently.
    internal static int FixWeightSetToOne(int weight) => (weight <= 0) ? 1 : weight;

    // Throw an exception when adding a bad weight.
    internal static int FixWeightExceptionOnAdd(int weight) =>
        (weight <= 0) ? throw new ArgumentException("Weight cannot be non-positive") : weight;

    private int FixWeight(int weight) => (BadWeightErrorHandling == ThrowExceptionOnAdd)
        ? FixWeightExceptionOnAdd(weight)
        : FixWeightSetToOne(weight);
}

/// <summary>
/// A single item for a list with matching T. Create one or more WeightedListItems, add to a Collection
/// and Add() to the WeightedList for a single calculation pass.
/// </summary>
/// <typeparam name="T"></typeparam>
internal readonly struct WeightedListItem<T>
{
    internal readonly T Item;
    internal readonly int Weight;

    public WeightedListItem(T item, int weight)
    {
        Item = item;
        Weight = weight;
    }
}

internal enum WeightErrorHandlingType
{
    SetWeightToOne, // Default
    ThrowExceptionOnAdd, // Throw exception for adding non-positive weight.
}