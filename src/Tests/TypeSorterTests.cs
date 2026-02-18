using System.Threading.Tasks;

public class TypeSorterTests(VerifySettings? output) :
    VerifyBase(output)
{
    public class SimpleSort
    {
        [Test]
        public async Task Run()
        {
            var dependencies = new Dictionary<Type, List<Type>>
            {
                [typeof(Class1)] =
                [
                    typeof(Class2),
                    typeof(Class3)
                ],
                [typeof(Class2)] = [typeof(Class3)]
            };
            var sorted = new TypeSorter(dependencies).Sorted;
            await Assert.That(sorted.Count).IsEqualTo(3);
            await Assert.That(sorted[0]).IsEqualTo(typeof (Class3));
            await Assert.That(sorted[1]).IsEqualTo(typeof (Class2));
            await Assert.That(sorted[2]).IsEqualTo(typeof (Class1));
        }

        class Class1;

        class Class2;

        class Class3;
    }

    public class Ensure_circular_dependencies_are_handled
    {
        [Test]
        public async Task Run()
        {
            var dependencies = new Dictionary<Type, List<Type>>
            {
                [typeof(Class1)] =
                [
                    typeof(Class2)
                ],
                [typeof(Class2)] = [typeof(Class3)],
                [typeof(Class3)] = [typeof(Class1)]
            };
            var exception = Assert.Throws<Exception>(() => new TypeSorter(dependencies));
            var expected =
                """
                    Cyclic dependency detected.
                    'Class1' wants to run after 'Class3'.
                    'Class3' wants to run after 'Class2'.
                    'Class2' wants to run after 'Class1'.

                    """.Replace("\r\n","").Replace("\n","");
            await Assert.That(exception.Message.Replace("\r\n", "").Replace("\n", "")).IsEqualTo(expected);
        }

        class Class1;
        class Class2;
        class Class3;
    }

    public class Ensure_self_dependencies_are_handled
    {
        [Test]
        public async Task Run()
        {
            var dependencies = new Dictionary<Type, List<Type>>
            {
                [typeof(Class1)] =
                [
                    typeof(Class1)
                ]
            };
            var exception = Assert.Throws<Exception>(() => new TypeSorter(dependencies));
            var expected = """
                           Cyclic dependency detected.
                           'Class1' wants to run after 'Class1'.

                           """.Replace("\r\n","").Replace("\n","");
            await Assert.That(exception.Message.Replace("\r\n", "").Replace("\n", "")).IsEqualTo(expected);
        }

        class Class1;
    }
}