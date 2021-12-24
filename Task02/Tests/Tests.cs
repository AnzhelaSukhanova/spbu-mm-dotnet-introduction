using NUnit.Framework;
using NullabilityAccessOperator;
using System;
using System.Collections.Generic;
using System.Collections;

namespace Tests {

    internal class TestClass {
        internal string Name => _name;
        string _name;

        internal TestClass(string name) {
            _name = name;
        }
    }
    public class Tests {
        [SetUp]
        public void Setup() {
        }

        [Test]
        public void TwoDimension() {
            string[][] array = new string[1][];
            array[0] = new string[2];
            array[0][0] = "00";

            var indices = new int[4][];
            indices[0] = new[] { 0, 0 };
            indices[1] = new[] { 0, 1 };
            indices[2] = new[] { 0, 0, 0};
            indices[3] = new[] { 1, 0};

            Assert.AreEqual("00", (string)NullabilityAccess.GetFromArray<Array>(array, indices[0]));
            Assert.AreEqual(default(string), (string)NullabilityAccess.GetFromArray<Array>(array, indices[1]));
            Assert.Null(NullabilityAccess.GetFromArray<Array>(array, indices[2]));
            Assert.Null(NullabilityAccess.GetFromArray<Array>(array, indices[3]));
            
        }

        [Test]
        public void ThreeDimension() {
            int[][][] array = new int[1][][];
            array[0] = new int[1][];
            array[0][0] = new int[2];
            array[0][0][0] = 42;

            var indices = new int[4][];
            indices[0] = new[] { 0, 0, 0 };
            indices[1] = new[] { 0, 0, 1 };
            indices[2] = new[] { 0, 1, 0 };
            indices[3] = new[] { 1, 0, 0 };

            Assert.AreEqual(42, (int)NullabilityAccess.GetFromArray<Array>(array, indices[0]));
            Assert.AreEqual(default(int), (int)NullabilityAccess.GetFromArray<Array>(array, indices[1]));
            Assert.Null(NullabilityAccess.GetFromArray<Array>(array, indices[2]));
            Assert.Null(NullabilityAccess.GetFromArray<Array>(array, indices[3]));

        }

        [Test]
        public void NullSource() {
            Assert.Null(NullabilityAccess.GetFromArray<Array>(null, new int[1]));
        }

        [Test]
        public void EmptyOrNullIndexesArray() {
            Assert.Throws<ArgumentException>(() => NullabilityAccess.GetFromArray<Array>(null, null));
            Assert.Throws<ArgumentException>(() => NullabilityAccess.GetFromArray<Array>(null, new List<int>()));
        }
    }
}