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
            // Arrange
            var car = new Car("ABC123", "Toyota", "Corolla", 15000, 5);

            // Act
            string result = car.ToString();

            // Assert
            Assert.AreEqual("ABC123 Toyota Corolla", result);
        }


    }
}