using TransLib;
using TransLib.Vehicles;

namespace TranscoTests
{
    [TestClass]
    public class Tests
    {
        [TestMethod]
        public void TestToString()
        {
            var car = new Car("ABC123", "Toyota", "Corolla", 15000, 5);

            string result = car.ToString();

            Assert.AreEqual("ABC123 Toyota Corolla", result);
        }

        [TestMethod]
        public void TestGet()
        {
            List<int> new_list = new List<int>();
            new_list.Append(1);
            new_list.Append(2);
            new_list.Append(3);

            Assert.AreEqual(1, new_list.Get(0));
            Assert.AreEqual(2, new_list.Get(1));
            Assert.AreEqual(3, new_list.Get(2));
        }

        [TestMethod]
        public void TestExtend()
        {
            List<int> new_list = new List<int>();
            new_list.Append(1);
            new_list.Append(2);
            new_list.Append(3);

            Assert.AreEqual(1, new_list.Get(0));
            Assert.AreEqual(2, new_list.Get(1));
            Assert.AreEqual(3, new_list.Get(2));

            List<int> supp_list = new List<int>();
            supp_list.Append(10);
            supp_list.Append(11);
            supp_list.Append(12);

            new_list.Extend(supp_list);

            Assert.AreEqual(1, new_list.Get(0));
            Assert.AreEqual(2, new_list.Get(1));
            Assert.AreEqual(3, new_list.Get(2));
            Assert.AreEqual(10, new_list.Get(3));
            Assert.AreEqual(11, new_list.Get(4));
            Assert.AreEqual(12, new_list.Get(5));

        }

        [TestMethod]
        public void TestInsert()
        {
            List<int> new_list = new List<int>();
            new_list.Append(1);
            new_list.Append(2);
            new_list.Append(3);
            
            new_list.Insert(0, 4);

            Assert.AreEqual(4, new_list.Get(0));

            new_list.Insert(4, 10);

            Assert.AreEqual(10, new_list.Get(4));
        }
    }
}