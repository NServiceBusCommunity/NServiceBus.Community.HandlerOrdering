using System.Threading.Tasks;

public class Ensure_cascading_dependencies_are_resolved
{
    [Test]
    public async Task Run()
    {
        var types = new List<Type>
        {
            typeof(Class1),
            typeof(Class2),
            typeof(Class3)
        };
        var handlerDependencies = OrderHandlers.GetHandlerDependencies(types);

        await Assert.That(handlerDependencies[typeof(Class1)]).Contains(typeof(Class2));
        await Assert.That(handlerDependencies[typeof(Class1)]).Contains(typeof(Class3));
        await Assert.That(handlerDependencies[typeof(Class2)]).Contains(typeof(Class3));
    }

    class Class1 :
        IWantToRunAfter<Class2>,
        IWantToRunAfter<Class3>;

    class Class2 :
        IWantToRunAfter<Class3>;

    class Class3;
}