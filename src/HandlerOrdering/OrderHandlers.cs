class OrderHandlers :
    INeedInitialization
{
    static readonly Lazy<MethodInfo> addHandlerMethod = new(() =>
    {
        var extensionsType = typeof(EndpointConfiguration).Assembly.GetType("NServiceBus.MessageHandlerRegistrationExtensions");
        return extensionsType?.GetMethod("AddHandler", BindingFlags.Static | BindingFlags.Public)
            ?? throw new("Could not find 'AddHandler' method on MessageHandlerRegistrationExtensions. Raise an issue here https://github.com/NServiceBusCommunity/NServiceBus.Community.HandlerOrdering/issues/new");
    });

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

        foreach (var handlerType in sorted)
        {
            var genericMethod = addHandlerMethod.Value.MakeGenericMethod(handlerType);
            genericMethod.Invoke(null, [configuration]);
        }
    }

    static Dictionary<Type, List<Type>> GetHandlerDependencies(EndpointConfiguration configuration)
    {
        var settings = configuration.GetSettings();
        if (!settings.TryGet("TypesToScan", out List<Type> types))
        {
            throw new("Could not extract 'TypesToScan' from settings. Raise an issue here https://github.com/NServiceBusCommunity/NServiceBus.Community.HandlerOrdering/issues/new");
        }
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
