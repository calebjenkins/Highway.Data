using System;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure;
using System.Security.Cryptography.X509Certificates;
using Common.Logging.Simple;

using Highway.Data.EntityFramework.Tests.Properties;
using Highway.Data.EntityFramework.Tests.UnitTests;
using Highway.Data.Tests;
using Highway.Data.Tests.TestDomain;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace Highway.Data.EntityFramework.Tests.Mapping
{
    [TestClass]
    public class ObjectMappingDetailTests
    {
        private IDataContext _context = new TestDataContext(Settings.Default.Connection, new FooMappingConfiguration(),
            new NoOpLogger());

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

    [TestClass]
    public class MergeCommandTests
    {
        [TestMethod]
        public void MyMethod()
        {
            var context = null;
            var command = new MergeCommand(new Foo(), x => x.);

            var sql = command.OutputSqlStatement(context)
        }
    }
}