using System;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure;

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
        private IDataContext _context = new TestDataContext(Settings.Default.Connection, new FooMappingConfiguration(), new NoOpLogger());

        [TestMethod]
        public void ShouldGetEntityMappingInformationForKeyProperties()
        {
            //Arrange
            //Act
            MappingDetail results = _context.GetMappingFor<Foo>();

            //Assert
            Assert.AreEqual(3, results.Properties.Count());
            Assert.IsTrue(results.Properties.Any(x => x.Property.Name == "Id" && x.ColumnName == "Id"));
            Assert.IsTrue(results.Properties.Any(x => x.Property.Name == "Name" && x.ColumnName == "Name"));
            Assert.IsTrue(results.Properties.Any(x => x.Property.Name == "Address" && x.ColumnName == "Address"));
        }

        [TestMethod]
        public void MyMethod()
        {
            var objectContext = ((IObjectContextAdapter)_context).ObjectContext;
            var storageMetadata = objectContext.MetadataWorkspace.GetItems(DataSpace.SSpace);
            var entityProps = (from s in storageMetadata where s.BuiltInTypeKind == BuiltInTypeKind.EntityType select s as EntityType);
            var personRightStorageMetadata = (from m in entityProps where m.Name == "Foo" select m).Single();
            foreach (var item in personRightStorageMetadata.Properties)
            {
                Console.WriteLine(item.Name);
            }
        }
    }
}