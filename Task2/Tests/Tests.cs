using System.Collections.Generic;
using NUnit.Framework;
using Main;

namespace Tests
{
    public class Tests
    {
        private static List<int> intList = new List<int> { 13, 4, 20, 34, 1, 92 };

        [Test]
        public static void IntList_Index_1()
        {
            Assert.IsNull(NullAccessor.Get<List<int>, int?>(new Index(-1))(intList));
        }

        [Test]
        public static void IntList_Index0()
        {
            Assert.AreEqual(NullAccessor.Get<List<int>, int?>(new Index(0))(intList), 13);
        }

        [Test]
        public static void IntList_Index2()
        {
            Assert.AreEqual(NullAccessor.Get<List<int>, int?>(new Index(2))(intList), 20);
        }

        [Test]
        public static void IntList_Index5()
        {
            Assert.AreEqual(NullAccessor.Get<List<int>, int?>(new Index(5))(intList), 92);
        }

        [Test]
        public static void IntList_Index6()
        {
            Assert.IsNull(NullAccessor.Get<List<int>, int?>(new Index(6))(intList));
        }

        [Test]
        public static void Index_Index0()
        {
            Assert.IsNull(NullAccessor.Get<Index, int?>(new Index(0))(new Index(64)));
        }

        [Test]
        public static void Index_FieldValue()
        {
            Assert.AreEqual(NullAccessor.Get<Index, int?>(new Field("Value"))(new Index(64)), 64);
        }

        [Test]
        public static void Index_FieldSomeField()
        {
            Assert.IsNull(NullAccessor.Get<Index, int?>(new Field("SomeField"))(new Index(64)));
        }

        private class IndexPair
        {
            public Index FstIndex;
            public Index SndIndex;

            public IndexPair(Index fst, Index snd) { FstIndex = fst; SndIndex = snd; }
        }

        private static IndexPair indexPair = new IndexPair(new Index(17), new Index(5));

        [Test]
        public static void IndexPair_Index1()
        {
            Assert.IsNull(NullAccessor.Get<IndexPair, Index>(new Index(1))(indexPair));
        }

        [Test]
        public static void IndexPair_FieldFstIndex()
        {
            Assert.AreEqual(NullAccessor.Get<IndexPair, Index>(new Field("FstIndex"))(indexPair).Value, 17);
        }

        [Test]
        public static void IndexPair_FieldSndIndex()
        {
            Assert.AreEqual(NullAccessor.Get<IndexPair, Index>(new Field("SndIndex"))(indexPair).Value, 5);
        }

        [Test]
        public static void IndexPair_FieldAny()
        {
            Assert.IsNull(NullAccessor.Get<IndexPair, Index>(new Field("Any"))(indexPair));
        }

        [Test]
        public static void IndexPair_FieldFstIndex_FieldValue()
        {
            Assert.AreEqual(NullAccessor.Get<IndexPair, int?>(new Field("FstIndex"), new Field("Value"))(indexPair), 17);
        }

        private class SomeClass
        {
            public List<Field> Fields;
            public Index Index;

            public SomeClass(params Field[] fields) { Fields = new List<Field>(fields); Index = new Index(Fields.Count); }
        }

        private static SomeClass someClass = new SomeClass(new Field("a"), new Field("BC"), new Field("def"));

        [Test]
        public static void SomeClass_FieldFields_Index1_FieldName()
        {
            Assert.AreEqual(NullAccessor.Get<SomeClass, string>(new Field("Fields"), new Index(1), new Field("Name"))(someClass), "BC");
        }
    }
}
