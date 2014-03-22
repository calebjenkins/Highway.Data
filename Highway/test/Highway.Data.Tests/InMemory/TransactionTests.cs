using System.Linq;
using Highway.Data.Contexts;
using Highway.Data.Tests.TestDomain;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Highway.Data.Tests.InMemory
{
    [TestClass]
    public class TransactionTests
    {
        [TestMethod]
        public void ShouldNotReturnModifiedObject()
        {
            //Arrange
            var context = new InMemoryDataContext();
            context.Add(new Foo { Name = "Testing"});
            context.Commit();
            
            //Act
            var foo = context.AsQueryable<Foo>().SingleOrDefault(x => x.Name == "Testing");
            foo.Name = "Should Matter";

            //Assert
            var fooTwo = context.AsQueryable<Foo>().SingleOrDefault(x => x.Name == "Testing");
            Assert.IsNotNull(fooTwo);
        }
    }
}