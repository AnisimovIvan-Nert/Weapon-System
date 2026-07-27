using System;
using NUnit.Framework;

namespace ShiftableData.Tests
{
    public class ShiftableDataCollectionTests
    {
        [Test]
        public void Constructor_ZeroSize_LengthZero()
        {
            var collection = new ShiftableDataCollection<int>(0);
            Assert.AreEqual(0, collection.Length);
        }

        [Test]
        public void Constructor_WithSize_LengthZero()
        {
            var collection = new ShiftableDataCollection<int>(16);
            Assert.AreEqual(0, collection.Length);
        }

        [Test]
        public void Add_SingleElement()
        {
            var collection = new ShiftableDataCollection<int>(4);
            collection.Add(42);

            Assert.AreEqual(1, collection.Length);
            Assert.AreEqual(42, collection.Data[0]);
        }

        [Test]
        public void Add_MultipleElements()
        {
            var collection = new ShiftableDataCollection<int>(4);
            collection.Add(1, 2, 3);

            Assert.AreEqual(3, collection.Length);
            Assert.AreEqual(1, collection.Data[0]);
            Assert.AreEqual(2, collection.Data[1]);
            Assert.AreEqual(3, collection.Data[2]);
        }

        [Test]
        public void Add_GrowsBufferWhenFull()
        {
            var collection = new ShiftableDataCollection<int>(4);
            for (var i = 0; i < 10; i++)
                collection.Add(i);

            Assert.AreEqual(10, collection.Length);
            for (var i = 0; i < 10; i++)
                Assert.AreEqual(i, collection.Data[i]);
        }

        [Test]
        public void Add_PreservesExistingData()
        {
            var collection = new ShiftableDataCollection<int>(4);
            collection.Add(10, 20);
            collection.Add(30);

            Assert.AreEqual(3, collection.Length);
            Assert.AreEqual(10, collection.Data[0]);
            Assert.AreEqual(20, collection.Data[1]);
            Assert.AreEqual(30, collection.Data[2]);
        }

        [Test]
        public void Insert_AtBeginning()
        {
            var collection = new ShiftableDataCollection<int>(8);
            collection.Add(2, 3, 4);

            collection.Insert(0, 1);

            Assert.AreEqual(4, collection.Length);
            Assert.AreEqual(1, collection.Data[0]);
            Assert.AreEqual(2, collection.Data[1]);
            Assert.AreEqual(3, collection.Data[2]);
            Assert.AreEqual(4, collection.Data[3]);
        }

        [Test]
        public void Insert_InMiddle()
        {
            var collection = new ShiftableDataCollection<int>(8);
            collection.Add(1, 4);

            collection.Insert(1, 2, 3);

            Assert.AreEqual(4, collection.Length);
            Assert.AreEqual(1, collection.Data[0]);
            Assert.AreEqual(2, collection.Data[1]);
            Assert.AreEqual(3, collection.Data[2]);
            Assert.AreEqual(4, collection.Data[3]);
        }

        [Test]
        public void Insert_AtEnd_SameAsAdd()
        {
            var collection = new ShiftableDataCollection<int>(8);
            collection.Add(1, 2);

            collection.Insert(2, 3);

            Assert.AreEqual(3, collection.Length);
            Assert.AreEqual(1, collection.Data[0]);
            Assert.AreEqual(2, collection.Data[1]);
            Assert.AreEqual(3, collection.Data[2]);
        }

        [Test]
        public void Insert_MultipleElements()
        {
            var collection = new ShiftableDataCollection<int>(8);
            collection.Add(1, 5);

            collection.Insert(1, 2, 3, 4);

            Assert.AreEqual(5, collection.Length);
            Assert.AreEqual(1, collection.Data[0]);
            Assert.AreEqual(2, collection.Data[1]);
            Assert.AreEqual(3, collection.Data[2]);
            Assert.AreEqual(4, collection.Data[3]);
            Assert.AreEqual(5, collection.Data[4]);
        }

        [Test]
        public void Insert_EmptyData_DoesNothing()
        {
            var collection = new ShiftableDataCollection<int>(4);
            collection.Add(1, 2);

            collection.Insert(1);

            Assert.AreEqual(2, collection.Length);
        }

        [Test]
        public void Insert_NegativeIndex_Throws()
        {
            var collection = new ShiftableDataCollection<int>(4);
            Assert.Throws<ArgumentOutOfRangeException>(() => collection.Insert(-1, 1));
        }

        [Test]
        public void Insert_IndexBeyondLength_Throws()
        {
            var collection = new ShiftableDataCollection<int>(4);
            collection.Add(1);
            Assert.Throws<ArgumentOutOfRangeException>(() => collection.Insert(2, 1));
        }

        [Test]
        public void Insert_GrowsBufferAsNeeded()
        {
            var collection = new ShiftableDataCollection<int>(2);
            collection.Add(1);

            collection.Insert(1, 2, 3, 4, 5);

            Assert.AreEqual(5, collection.Length);
            for (var i = 0; i < 5; i++)
                Assert.AreEqual(i + 1, collection.Data[i]);
        }

        [Test]
        public void Remove_FromBeginning()
        {
            var collection = new ShiftableDataCollection<int>(8);
            collection.Add(1, 2, 3, 4, 5);

            collection.Remove(0, 2);

            Assert.AreEqual(3, collection.Length);
            Assert.AreEqual(3, collection.Data[0]);
            Assert.AreEqual(4, collection.Data[1]);
            Assert.AreEqual(5, collection.Data[2]);
        }

        [Test]
        public void Remove_FromMiddle()
        {
            var collection = new ShiftableDataCollection<int>(8);
            collection.Add(1, 2, 3, 4, 5);

            collection.Remove(1, 3);

            Assert.AreEqual(3, collection.Length);
            Assert.AreEqual(1, collection.Data[0]);
            Assert.AreEqual(4, collection.Data[1]);
            Assert.AreEqual(5, collection.Data[2]);
        }

        [Test]
        public void Remove_SingleElement()
        {
            var collection = new ShiftableDataCollection<int>(8);
            collection.Add(1, 2, 3);

            collection.Remove(1, 2);

            Assert.AreEqual(2, collection.Length);
            Assert.AreEqual(1, collection.Data[0]);
            Assert.AreEqual(3, collection.Data[1]);
        }

        [Test]
        public void Remove_AllElements()
        {
            var collection = new ShiftableDataCollection<int>(8);
            collection.Add(1, 2, 3);

            collection.Remove(0, 3);

            Assert.AreEqual(0, collection.Length);
        }

        [Test]
        public void Remove_DefaultEndIndex_RemovesToEnd()
        {
            var collection = new ShiftableDataCollection<int>(8);
            collection.Add(1, 2, 3, 4, 5);

            collection.Remove(2);

            Assert.AreEqual(2, collection.Length);
            Assert.AreEqual(1, collection.Data[0]);
            Assert.AreEqual(2, collection.Data[1]);
        }

        [Test]
        public void Remove_SameIndices_DoesNothing()
        {
            var collection = new ShiftableDataCollection<int>(8);
            collection.Add(1, 2, 3);

            collection.Remove(1, 1);

            Assert.AreEqual(3, collection.Length);
            Assert.AreEqual(1, collection.Data[0]);
            Assert.AreEqual(2, collection.Data[1]);
            Assert.AreEqual(3, collection.Data[2]);
        }

        [Test]
        public void Remove_NegativeStartIndex_Throws()
        {
            var collection = new ShiftableDataCollection<int>(4);
            collection.Add(1, 2, 3);
            Assert.Throws<ArgumentOutOfRangeException>(() => collection.Remove(-1, 2));
        }

        [Test]
        public void Remove_StartIndexBeyondLength_Throws()
        {
            var collection = new ShiftableDataCollection<int>(4);
            collection.Add(1, 2, 3);
            Assert.Throws<ArgumentOutOfRangeException>(() => collection.Remove(4, 5));
        }

        [Test]
        public void Remove_EndIndexBeyondLength_Throws()
        {
            var collection = new ShiftableDataCollection<int>(4);
            collection.Add(1, 2, 3);
            Assert.Throws<ArgumentOutOfRangeException>(() => collection.Remove(0, 5));
        }

        [Test]
        public void Remove_StartGreaterThanEnd_Throws()
        {
            var collection = new ShiftableDataCollection<int>(4);
            collection.Add(1, 2, 3);
            Assert.Throws<ArgumentException>(() => collection.Remove(2, 1));
        }

        [Test]
        public void Replace_SameSize()
        {
            var collection = new ShiftableDataCollection<int>(8);
            collection.Add(1, 2, 3, 4);

            collection.Replace(1, 3, 20, 30);

            Assert.AreEqual(4, collection.Length);
            Assert.AreEqual(1, collection.Data[0]);
            Assert.AreEqual(20, collection.Data[1]);
            Assert.AreEqual(30, collection.Data[2]);
            Assert.AreEqual(4, collection.Data[3]);
        }

        [Test]
        public void Replace_SmallerData()
        {
            var collection = new ShiftableDataCollection<int>(8);
            collection.Add(1, 2, 3, 4, 5);

            collection.Replace(1, 4, 20);

            Assert.AreEqual(3, collection.Length);
            Assert.AreEqual(1, collection.Data[0]);
            Assert.AreEqual(20, collection.Data[1]);
            Assert.AreEqual(5, collection.Data[2]);
        }

        [Test]
        public void Replace_LargerData()
        {
            var collection = new ShiftableDataCollection<int>(8);
            collection.Add(1, 2, 3);

            collection.Replace(1, 2, 20, 30);

            Assert.AreEqual(4, collection.Length);
            Assert.AreEqual(1, collection.Data[0]);
            Assert.AreEqual(20, collection.Data[1]);
            Assert.AreEqual(30, collection.Data[2]);
            Assert.AreEqual(3, collection.Data[3]);
        }

        [Test]
        public void Replace_DefaultEndIndex()
        {
            var collection = new ShiftableDataCollection<int>(8);
            collection.Add(1, 2, 3, 4);

            collection.Replace(2, -1, 30, 40);

            Assert.AreEqual(4, collection.Length);
            Assert.AreEqual(1, collection.Data[0]);
            Assert.AreEqual(2, collection.Data[1]);
            Assert.AreEqual(30, collection.Data[2]);
            Assert.AreEqual(40, collection.Data[3]);
        }

        [Test]
        public void Replace_EntireCollection()
        {
            var collection = new ShiftableDataCollection<int>(8);
            collection.Add(1, 2, 3);

            collection.Replace(0, -1, 10, 20, 30);

            Assert.AreEqual(3, collection.Length);
            Assert.AreEqual(10, collection.Data[0]);
            Assert.AreEqual(20, collection.Data[1]);
            Assert.AreEqual(30, collection.Data[2]);
        }

        [Test]
        public void Resize_Shrink_TruncatesLength()
        {
            var collection = new ShiftableDataCollection<int>(16);
            collection.Add(1, 2, 3, 4, 5);

            collection.Resize(3);

            Assert.AreEqual(3, collection.Length);
            Assert.AreEqual(1, collection.Data[0]);
            Assert.AreEqual(2, collection.Data[1]);
            Assert.AreEqual(3, collection.Data[2]);
        }

        [Test]
        public void Resize_Grow_PreservesData()
        {
            var collection = new ShiftableDataCollection<int>(4);
            collection.Add(1, 2);

            collection.Resize(8);

            Assert.AreEqual(2, collection.Length);
            Assert.AreEqual(1, collection.Data[0]);
            Assert.AreEqual(2, collection.Data[1]);
        }

        [Test]
        public void AddRemoveCycle_MaintainsCorrectness()
        {
            var collection = new ShiftableDataCollection<int>(4);

            for (var i = 0; i < 20; i++)
                collection.Add(i);

            Assert.AreEqual(20, collection.Length);

            for (var i = 0; i < 10; i++)
                collection.Remove(0, 1);

            Assert.AreEqual(10, collection.Length);
            for (var i = 0; i < 10; i++)
                Assert.AreEqual(i + 10, collection.Data[i]);
        }

        [Test]
        public void InsertRemoveInterleaved_MaintainsCorrectness()
        {
            var collection = new ShiftableDataCollection<int>(4);
            collection.Add(1, 2, 3);

            collection.Insert(1, 10);
            Assert.AreEqual(4, collection.Length);
            Assert.AreEqual(1, collection.Data[0]);
            Assert.AreEqual(10, collection.Data[1]);
            Assert.AreEqual(2, collection.Data[2]);
            Assert.AreEqual(3, collection.Data[3]);

            collection.Remove(0, 1);
            Assert.AreEqual(3, collection.Length);
            Assert.AreEqual(10, collection.Data[0]);
            Assert.AreEqual(2, collection.Data[1]);
            Assert.AreEqual(3, collection.Data[2]);
        }

        [Test]
        public void Add_WithByteArray()
        {
            var collection = new ShiftableDataCollection<byte>(16);
            collection.Add(0xFF, 0x00, 0xAB);

            Assert.AreEqual(3, collection.Length);
            Assert.AreEqual(0xFF, collection.Data[0]);
            Assert.AreEqual(0x00, collection.Data[1]);
            Assert.AreEqual(0xAB, collection.Data[2]);
        }

        [Test]
        public void Insert_AtPositionZero_EmptyCollection()
        {
            var collection = new ShiftableDataCollection<int>(4);
            collection.Insert(0, 42);

            Assert.AreEqual(1, collection.Length);
            Assert.AreEqual(42, collection.Data[0]);
        }

        [Test]
        public void Remove_EntireRange()
        {
            var collection = new ShiftableDataCollection<int>(8);
            collection.Add(1, 2, 3, 4, 5);

            collection.Remove(0, 5);

            Assert.AreEqual(0, collection.Length);
        }
    }
}
