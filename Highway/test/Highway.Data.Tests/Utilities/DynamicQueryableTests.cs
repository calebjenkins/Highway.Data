#region

using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Dynamic;
using Highway.Data.Tests.TestDomain;
using Microsoft.VisualStudio.TestTools.UnitTesting;

#endregion

namespace Highway.Data.Tests.Utilities
{
    [TestClass]
    public class DynamicQueryableTests : BaseCollectionTests
    {
        [TestMethod]
        public void ShouldFilterByString()
        {
            //Act
            var results = items.AsQueryable().Where("FullName = @0", "Devlin");

            //Assert
            Assert.IsTrue(results.Count() == 1);
            Assert.AreEqual("Devlin", results.Single().FullName);
        }

        [TestMethod]
        public void ShouldSelectByString()
        {
            //Act
            IEnumerable results = items.AsQueryable().Select("new(FullName as FirstName)");

            //Assert
            Assert.IsTrue(results.Count() == 3);
            Assert.AreEqual("Devlin", results.Cast<dynamic>().First().FirstName);
        }
    }

    public class BaseCollectionTests
    {
        public ICollection<Foo> items = new Collection<Foo>
        {
            new Foo {FullName = "Devlin"},
            new Foo {FullName = "Tim"},
            new Foo {FullName = "Allen"}
        };
    }

    [TestClass]
    public class DynamicEnumerableTests : BaseCollectionTests
    {
        [TestMethod]
        public void ShouldFilterByString()
        {
            //Act
            var results = items.Where("FullName = @0", "Devlin");

            //Assert
            Assert.IsTrue(results.Count() == 1);
            Assert.AreEqual("Devlin", results.Single().FullName);
        }

        [TestMethod]
        public void ShouldSelectByString()
        {
            //Act
            IEnumerable results = items.Select("new(FullName as FirstName)");

            //Assert
            Assert.IsTrue(results.Count() == 3);
            Assert.AreEqual("Devlin", results.Cast<dynamic>().First().FirstName);
        }
    }
}