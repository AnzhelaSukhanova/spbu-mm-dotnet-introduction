using System;
using System.Collections.ObjectModel;
using Homework2;
using NUnit.Framework;

namespace Homework2Tests
{
    public class NullConditionalOperatorTests
    {
        [Test]
        public void AccessElement_ShouldThrowException_WhenIndicesEmpty()
        {
            var coll = new Collection<int>();
            var ind = Array.Empty<int>();

            Assert.Throws<ArgumentException>(() =>
                NullConditionalOperator.AccessElement<int, int>(coll, ind));
        }

        [Test]
        public void AccessElement_ShouldThrowException_WhenIndexOutOfBounds()
        {
            var coll = new Collection<int> { 1 };
            var ind = new int[] { -1 };

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                NullConditionalOperator.AccessElement<int, int>(coll, ind));
        }

        [Test]
        public void AccessElement_ShouldThrowException_WhenItemInPathIsNotCollection()
        {
            var coll = new Collection<int> { 1 };
            var ind = new int[] { 0, 0 };

            Assert.Throws<ArgumentException>(() =>
                NullConditionalOperator.AccessElement<int, int>(coll, ind));
        }

        [Test]
        public void AccessElement_ShouldReturnElement_WhenItemsInPathAreNotNull()
        {
            var coll = new Collection<Collection<int>> { new Collection<int> { 1 } };
            var ind = new int[] { 0, 0 };

            int expected = 1;

            Assert.AreEqual(expected,
                NullConditionalOperator.AccessElement<Collection<int>, int>(coll, ind));
        }

        [Test]
        public void AccessElement_ShouldReturnNull_WhenItemInPathIsNullAndReturnTypeIsReference()
        {
            var coll = new Collection<Collection<string>> { null };
            var ind = new int[] { 0, 0 };

            string expected = null;

            Assert.AreEqual(expected,
                NullConditionalOperator.AccessElement<Collection<string>, string>(coll, ind));
        }

        [Test]
        public void AccessElement_ShouldReturnNull_WhenRootCollectionIsNull()
        {
            Collection<Collection<string>> coll = null;
            var ind = new int[] { 0, 0 };

            string expected = null;

            Assert.AreEqual(expected,
                NullConditionalOperator.AccessElement<Collection<string>, string>(coll, ind));
        }

        [Test]
        public void AccessElement_ShouldReturnDefault_WhenItemInPathIsNullAndReturnTypeIsValue()
        {
            var coll = new Collection<Collection<int>> { null };
            var ind = new int[] { 0, 0 };

            int expected = 0;

            Assert.AreEqual(expected,
                NullConditionalOperator.AccessElement<Collection<int>, int>(coll, ind));
        }
    }
}
