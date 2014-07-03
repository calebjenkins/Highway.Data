using System;
using System.Linq;
using Common.Logging.Simple;
using Highway.Data.EntityFramework.Tests.Properties;
using Highway.Data.EntityFramework.Tests.UnitTests;
using Highway.Data.Tests.TestDomain;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Highway.Data.EntityFramework.Tests.Mapping
{
    [TestClass]
    public class MergeCommandTests
    {
        private IDataContext _context = new TestDataContext(Settings.Default.Connection, new FooMappingConfiguration(),
            new NoOpLogger());


        [TestMethod]
        public void MyMethod()
        {
            var command = new Merge<Foo>(new Foo(){Address = "Temp Data", FullName = "Test"}, x => new {x.FullName, x.Id});

            string outputQuery = command.OutputQuery(_context);
            Console.WriteLine(outputQuery);
            Assert.IsTrue(!string.IsNullOrWhiteSpace(outputQuery));
        }

        [TestMethod]
        public void ShouldInsertWhenNoMatchForUnique()
        {
            //Arrange
            _context.AsQueryable<Foo>().Where(x => x.FullName == "Testing Merge").ToList().ForEach(x => _context.Remove(x));
            _context.Commit();

            var target = new Merge<Foo>(new Foo() { Address = "Temp Data", FullName = "Testing Merge" }, x => new { x.FullName, x.Id });

            //Act
            target.Execute(_context);

            //Assert
            var insertedFoo = _context.AsQueryable<Foo>().Single(x => x.FullName == "Testing Merge" && x.Address == "Temp Data");
            Assert.IsTrue(insertedFoo.Id > 0);
        }

        [TestMethod]
        public void ShouldUpdateWhenMatchForUnique()
        {
            //Arrange
            _context.AsQueryable<Foo>().Where(x => x.FullName == "Testing Merge").ToList().ForEach(x => _context.Remove(x));
            _context.Commit();

            var existingFoo = new Foo() {Address = "Temp Data", FullName = "Before Merge"};
            _context.Add(existingFoo);
            _context.Commit();

            var target = new Merge<Foo>(new Foo() { Address = "Temp Data", FullName = "Testing Merge" }, x => new { x.Address });

            //Act
            target.Execute(_context);

            //Assert
            var insertedFoo = _context.AsQueryable<Foo>().Single(x => x.FullName == "Testing Merge" && x.Address == "Temp Data");
            Assert.IsTrue(insertedFoo.Id == existingFoo.Id);
        }

    }
}