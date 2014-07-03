using System.Net.Sockets;
using Common.Logging;
using Common.Logging.Simple;

using Highway.Data.EntityFramework.Tests.Properties;
using Highway.Data.EntityFramework.Tests.UnitTests;
using Highway.Data.Tests.TestDomain;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace Highway.Data.EntityFramework.Tests.Mapping
{
    [TestClass]
    public class ObjectMappingDetailTests
    {
        private IDataContext _context = new TestDataContext(Settings.Default.Connection, new FooMappingConfiguration(),
            new ConsoleOutLogger("Test",LogLevel.All, false,false,false,""));

        [TestMethod]
        public void ShouldGetEntityMappingInformationForKeyProperties()
        {
            //Arrange
            //Act
            MappingDetail results = _context.GetMappingFor<Foo>();

            //Assert
            Assert.AreEqual("Foos", results.Table);
            Assert.AreEqual("dbo", results.Schema);
            Assert.AreEqual(3, results.Properties.Count());
            Assert.IsTrue(results.Properties.Any(x => x.Property.Name == "Id" && x.ColumnName == "Id"));
            Assert.IsTrue(results.Properties.Any(x => x.Property.Name == "FullName" && x.ColumnName == "Name"));
            Assert.IsTrue(results.Properties.Any(x => x.Property.Name == "Address" && x.ColumnName == "Address"));
        }
    }
}