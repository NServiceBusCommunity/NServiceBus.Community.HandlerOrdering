class TypeSorter
{
    Dictionary<Type, List<Type>> dependencies;
    List<Type> sorted = new();
    List<Type> stack = null!;
    public ReadOnlyCollection<Type> Sorted;
    HashSet<Type> visited = new();

    public TypeSorter(Dictionary<Type, List<Type>> dependencies)
    {
        this.dependencies = dependencies;
        foreach (var item in dependencies.Keys)
        {
            stack = new();
            Visit(item);
        }

        Sorted = new(sorted);
    }

    void Visit(Type item)
    {
        stack.Add(item);
        if (visited.Contains(item))
        {
            if (!sorted.Contains(item))
            {
                stack.Reverse();
                var stringBuilder = new StringBuilder();
                stringBuilder.AppendLine("Cyclic dependency detected.");
                for (var index = 0; index < stack.Count - 1; index++)
                {
                    var type = stack[index];
                    var dependentType = stack[index + 1];
                    stringBuilder.AppendLine($"'{type.Name}' wants to run after '{dependentType.Name}'.");
                }

                throw new(stringBuilder.ToString());
            }

            return;
        }

        visited.Add(item);
        if (dependencies.TryGetValue(item, out var values))
        {
            foreach (var dependency in values)
            {
                Visit(dependency);
            }
        }

        sorted.Add(item);
    }
}
