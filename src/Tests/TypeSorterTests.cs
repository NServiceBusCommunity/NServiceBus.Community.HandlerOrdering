public class TypeSorterTests(VerifySettings? output) :
    VerifyBase(output)
{
    public class SimpleSort
    {
        [Fact]
        public void Run()
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
            Assert.Equal(3, sorted.Count);
            Assert.Equal(typeof (Class3), sorted[0]);
            Assert.Equal(typeof (Class2), sorted[1]);
            Assert.Equal(typeof (Class1), sorted[2]);
        }

        class Class1;

        class Class2;

        class Class3;
    }

    public class Ensure_circular_dependencies_are_handled
    {
        [Fact]
        public void Run()
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
            Assert.Equal(expected, exception.Message.Replace("\r\n", "").Replace("\n", ""));
        }

        class Class1;
        class Class2;
        class Class3;
    }

    public class Ensure_self_dependencies_are_handled
    {
        [Fact]
        public void Run()
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
            Assert.Equal(expected, exception.Message.Replace("\r\n", "").Replace("\n", ""));
        }

        class Class1;
    }
}