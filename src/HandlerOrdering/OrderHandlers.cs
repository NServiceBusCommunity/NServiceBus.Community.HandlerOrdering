class OrderHandlers :
    INeedInitialization
{
    public void Customize(EndpointConfiguration configuration)
    {
        if (configuration.GetApplyInterfaceHandlerOrdering())
        {
            ApplyInterfaceHandlerOrdering(configuration);
        }
    }

    static void ApplyInterfaceHandlerOrdering(EndpointConfiguration configuration)
    {
        var handlerDependencies = GetHandlerDependencies(configuration);
        var sorted = new TypeSorter(handlerDependencies).Sorted;

        var addHandlerMethod = typeof(EndpointConfiguration).GetMethod("AddHandler", BindingFlags.Instance | BindingFlags.Public);
        if (addHandlerMethod == null)
        {
            throw new($"Could not find 'AddHandler' method on {nameof(EndpointConfiguration)}. Raise an issue here https://github.com/NServiceBusExtensions/NServiceBus.HandlerOrdering/issues/new");
        }

        foreach (var handlerType in sorted)
        {
            var genericMethod = addHandlerMethod.MakeGenericMethod(handlerType);
            genericMethod.Invoke(configuration, null);
        }
    }

    static Dictionary<Type, List<Type>> GetHandlerDependencies(EndpointConfiguration configuration)
    {
        var field = typeof(EndpointConfiguration)
            .GetField("scannedTypes", BindingFlags.Instance | BindingFlags.NonPublic);
        if (field == null)
        {
            throw new($"Could not extract 'scannedTypes' field from {nameof(EndpointConfiguration)}. Raise an issue here https://github.com/NServiceBusExtensions/NServiceBus.HandlerOrdering/issues/new");
        }

        var types = (List<Type>) field.GetValue(configuration)!;
        return GetHandlerDependencies(types);
    }

    internal static Dictionary<Type, List<Type>> GetHandlerDependencies(List<Type> types)
    {
        var dictionary = new Dictionary<Type, List<Type>>();
        foreach (var type in types)
        {
            var interfaces = type.GetInterfaces();
            foreach (var face in interfaces)
            {
                if (!face.IsGenericType)
                {
                    continue;
                }

                if (face.GetGenericTypeDefinition() != typeof(IWantToRunAfter<>))
                {
                    continue;
                }

                if (!dictionary.TryGetValue(type, out var dependencies))
                {
                    dictionary[type] = dependencies = new();
                }

                dependencies.Add(face.GenericTypeArguments.First());
            }
        }

        return dictionary;
    }
}
