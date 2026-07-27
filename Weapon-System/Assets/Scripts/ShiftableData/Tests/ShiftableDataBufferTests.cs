using System;
using NUnit.Framework;

namespace ShiftableData.Tests
{
    public class ShiftableDataBufferTests
    {
        [Test]
        public void Constructor_ZeroSize_EmptyArray()
        {
            var buffer = new ShiftableDataBuffer<int>();
            Assert.AreEqual(0, buffer.Size);
        }

        [Test]
        public void Constructor_WithSize_CreatesArray()
        {
            var buffer = new ShiftableDataBuffer<int>(8);
            Assert.AreEqual(8, buffer.Size);
        }

        [Test]
        public void Resize_Shrink_Truncates()
        {
            var buffer = new ShiftableDataBuffer<int>(8)
            {
                Data =
                {
                    [0] = 1,
                    [3] = 4
                }
            };

            buffer.Resize(4);

            Assert.AreEqual(4, buffer.Size);
            Assert.AreEqual(1, buffer.Data[0]);
            Assert.AreEqual(4, buffer.Data[3]);
        }

        [Test]
        public void Resize_Grow_PreservesData()
        {
            var buffer = new ShiftableDataBuffer<int>(4)
            {
                Data =
                {
                    [0] = 10,
                    [1] = 20
                }
            };

            buffer.Resize(8);

            Assert.AreEqual(8, buffer.Size);
            Assert.AreEqual(10, buffer.Data[0]);
            Assert.AreEqual(20, buffer.Data[1]);
            Assert.AreEqual(0, buffer.Data[4]);
        }

        [Test]
        public void Resize_NegativeSize_Throws()
        {
            var buffer = new ShiftableDataBuffer<int>(4);
            Assert.Throws<ArgumentOutOfRangeException>(() => buffer.Resize(-1));
        }

        [Test]
        public void IncreaseSize_FromZero_GrowsToFour()
        {
            var buffer = new ShiftableDataBuffer<int>();
            buffer.IncreaseSize(1);
            Assert.AreEqual(4, buffer.Size);
        }

        [Test]
        public void IncreaseSize_DoublesUntilSufficient()
        {
            var buffer = new ShiftableDataBuffer<int>(4);
            buffer.IncreaseSize(10);
            Assert.AreEqual(16, buffer.Size);
        }

        [Test]
        public void IncreaseSize_AlreadyLargeEnough_NoChange()
        {
            var buffer = new ShiftableDataBuffer<int>(16);
            buffer.IncreaseSize(8);
            Assert.AreEqual(16, buffer.Size);
        }

        [Test]
        public void IncreaseSize_PreservesExistingData()
        {
            var buffer = new ShiftableDataBuffer<int>(4)
            {
                Data =
                {
                    [0] = 1,
                    [1] = 2,
                    [2] = 3,
                    [3] = 4
                }
            };

            buffer.IncreaseSize(5);

            Assert.AreEqual(1, buffer.Data[0]);
            Assert.AreEqual(2, buffer.Data[1]);
            Assert.AreEqual(3, buffer.Data[2]);
            Assert.AreEqual(4, buffer.Data[3]);
        }

        [Test]
        public void ShiftRight_EndIndexDefault_ShiftsToEnd()
        {
            var buffer = new ShiftableDataBuffer<int>(8)
            {
                Data =
                {
                    [0] = 1,
                    [1] = 2,
                    [2] = 3
                }
            };

            buffer.ShiftRight(0, -1, 2);

            Assert.AreEqual(0, buffer.Data[0]);
            Assert.AreEqual(0, buffer.Data[1]);
            Assert.AreEqual(1, buffer.Data[2]);
            Assert.AreEqual(2, buffer.Data[3]);
            Assert.AreEqual(3, buffer.Data[4]);
        }

        [Test]
        public void ShiftRight_WithEndIndex_ShiftsRange()
        {
            var buffer = new ShiftableDataBuffer<int>(8)
            {
                Data =
                {
                    [0] = 1,
                    [1] = 2,
                    [2] = 3,
                    [3] = 4,
                }
            };

            buffer.ShiftRight(1, 3, 2);

            Assert.AreEqual(1, buffer.Data[0]);
            Assert.AreEqual(0, buffer.Data[1]);
            Assert.AreEqual(0, buffer.Data[2]);
            Assert.AreEqual(2, buffer.Data[3]);
            Assert.AreEqual(3, buffer.Data[4]);
        }
        
        [Test]
        public void ShiftRight_ExceedBuffer_WithDropEnabled_DropExtra()
        {
            var buffer = new ShiftableDataBuffer<int>(4)
            {
                Data =
                {
                    [0] = 1,
                    [1] = 2,
                    [2] = 3,
                    [3] = 4,
                }
            };

            buffer.ShiftRight(1, 3, 2);

            Assert.AreEqual(1, buffer.Data[0]);
            Assert.AreEqual(0, buffer.Data[1]);
            Assert.AreEqual(0, buffer.Data[2]);
            Assert.AreEqual(2, buffer.Data[3]);
        }

        [Test]
        public void ShiftRight_ClearsSourcePositions()
        {
            var buffer = new ShiftableDataBuffer<int>(8)
            {
                Data =
                {
                    [0] = 10,
                    [1] = 20
                }
            };

            buffer.ShiftRight(0, 2);

            Assert.AreEqual(0, buffer.Data[0]);
            Assert.AreEqual(10, buffer.Data[1]);
            Assert.AreEqual(20, buffer.Data[2]);
        }

        [Test]
        public void ShiftRight_DistanceOne_ShiftsByOne()
        {
            var buffer = new ShiftableDataBuffer<int>(6)
            {
                Data =
                {
                    [2] = 50,
                    [3] = 60
                }
            };

            buffer.ShiftRight(2, 4);

            Assert.AreEqual(0, buffer.Data[2]);
            Assert.AreEqual(50, buffer.Data[3]);
            Assert.AreEqual(60, buffer.Data[4]);
        }

        [Test]
        public void ShiftRight_InvalidDistance_Zero_Throws()
        {
            var buffer = new ShiftableDataBuffer<int>(4);
            Assert.Throws<ArgumentOutOfRangeException>(() => buffer.ShiftRight(0, 2, 0));
        }

        [Test]
        public void ShiftRight_InvalidDistance_Negative_Throws()
        {
            var buffer = new ShiftableDataBuffer<int>(4);
            Assert.Throws<ArgumentOutOfRangeException>(() => buffer.ShiftRight(0, 2, -1));
        }

        [Test]
        public void ShiftRight_NegativeStartIndex_Throws()
        {
            var buffer = new ShiftableDataBuffer<int>(4);
            Assert.Throws<ArgumentOutOfRangeException>(() => buffer.ShiftRight(-1, 2));
        }

        [Test]
        public void ShiftRight_ExceedsBounds_Throws()
        {
            var buffer = new ShiftableDataBuffer<int>(4);
            Assert.Throws<ArgumentException>(() => buffer.ShiftRight(0, -1, 4));
        }

        [Test]
        public void ShiftRight_StartIndexEqualsEndIndex_Throws()
        {
            var buffer = new ShiftableDataBuffer<int>(4);
            Assert.Throws<ArgumentException>(() => buffer.ShiftRight(2, 2));
        }

        [Test]
        public void ShiftRight_StartIndexGreaterThanEndIndex_Throws()
        {
            var buffer = new ShiftableDataBuffer<int>(4);
            Assert.Throws<ArgumentException>(() => buffer.ShiftRight(3, 1));
        }
        
        [Test]
        public void ShiftRight_ExceedBufferWithDropDisabled_Throws()
        {
            var buffer = new ShiftableDataBuffer<int>(4)
            {
                Data =
                {
                    [0] = 1,
                    [1] = 2,
                    [2] = 3,
                    [3] = 4,
                }
            };

            Assert.Throws<InvalidOperationException>(() => buffer.ShiftRight(1, 3, 2, false));
        }

        [Test]
        public void ShiftLeft_ShiftsRangeLeft()
        {
            var buffer = new ShiftableDataBuffer<int>(8)
            {
                Data =
                {
                    [2] = 10,
                    [3] = 20,
                    [4] = 30,
                    [5] = 40
                }
            };

            buffer.ShiftLeft(4, 6, 2);

            Assert.AreEqual(30, buffer.Data[2]);
            Assert.AreEqual(40, buffer.Data[3]);
            Assert.AreEqual(0, buffer.Data[4]);
            Assert.AreEqual(0, buffer.Data[5]);
        }

        [Test]
        public void ShiftLeft_EndIndexDefault_ShiftsToEnd()
        {
            var buffer = new ShiftableDataBuffer<int>(8)
            {
                Data =
                {
                    [3] = 10,
                    [4] = 20,
                    [5] = 30
                }
            };

            buffer.ShiftLeft(3, -1, 2);

            Assert.AreEqual(10, buffer.Data[1]);
            Assert.AreEqual(20, buffer.Data[2]);
            Assert.AreEqual(30, buffer.Data[3]);
            Assert.AreEqual(0, buffer.Data[6]);
            Assert.AreEqual(0, buffer.Data[7]);
        }

        [Test]
        public void ShiftLeft_ClearsTrailingPositions()
        {
            var buffer = new ShiftableDataBuffer<int>(6)
            {
                Data =
                {
                    [0] = 1,
                    [1] = 2,
                    [2] = 3,
                    [3] = 4,
                    [4] = 5,
                    [5] = 6
                }
            };

            buffer.ShiftLeft(2, 6, 2);

            Assert.AreEqual(3, buffer.Data[0]);
            Assert.AreEqual(4, buffer.Data[1]);
            Assert.AreEqual(5, buffer.Data[2]);
            Assert.AreEqual(6, buffer.Data[3]);
            Assert.AreEqual(0, buffer.Data[4]);
            Assert.AreEqual(0, buffer.Data[5]);
        }

        [Test]
        public void ShiftLeft_DistanceOne_ShiftsByOne()
        {
            var buffer = new ShiftableDataBuffer<int>(6)
            {
                Data =
                {
                    [2] = 10,
                    [3] = 20,
                    [4] = 30
                }
            };

            buffer.ShiftLeft(2, 5);

            Assert.AreEqual(10, buffer.Data[1]);
            Assert.AreEqual(20, buffer.Data[2]);
            Assert.AreEqual(30, buffer.Data[3]);
            Assert.AreEqual(0, buffer.Data[4]);
        }

        [Test]
        public void ShiftLeft_InvalidDistance_Zero_Throws()
        {
            var buffer = new ShiftableDataBuffer<int>(4);
            Assert.Throws<ArgumentOutOfRangeException>(() => buffer.ShiftLeft(2, 4, 0));
        }

        [Test]
        public void ShiftLeft_NegativeStartIndex_Throws()
        {
            var buffer = new ShiftableDataBuffer<int>(4);
            Assert.Throws<ArgumentOutOfRangeException>(() => buffer.ShiftLeft(-1, 4));
        }

        [Test]
        public void ShiftLeft_ExceedsBounds_Throws()
        {
            var buffer = new ShiftableDataBuffer<int>(4);
            Assert.Throws<ArgumentException>(() => buffer.ShiftLeft(1, -1, 2));
        }

        [Test]
        public void ShiftLeft_StartIndexEqualsEndIndex_Throws()
        {
            var buffer = new ShiftableDataBuffer<int>(4);
            Assert.Throws<ArgumentException>(() => buffer.ShiftLeft(2, 2));
        }

        [Test]
        public void ShiftLeft_StartIndexGreaterThanEndIndex_Throws()
        {
            var buffer = new ShiftableDataBuffer<int>(4);
            Assert.Throws<ArgumentException>(() => buffer.ShiftLeft(3, 1));
        }

        [Test]
        public void ShiftRight_Then_ShiftLeft_RoundTrips()
        {
            var buffer = new ShiftableDataBuffer<int>(8)
            {
                Data =
                {
                    [0] = 1,
                    [1] = 2,
                    [2] = 3,
                    [3] = 4
                }
            };

            buffer.ShiftRight(2, 4, 2);
            buffer.ShiftLeft(4, 6, 2);

            Assert.AreEqual(1, buffer.Data[0]);
            Assert.AreEqual(2, buffer.Data[1]);
            Assert.AreEqual(3, buffer.Data[2]);
            Assert.AreEqual(4, buffer.Data[3]);
            Assert.AreEqual(0, buffer.Data[4]);
        }
    }
}
