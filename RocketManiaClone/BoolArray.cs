using System;

namespace RocketManiaClone
{
    public class BoolArray
    {
        private readonly byte[] byteArray;

        public BoolArray(int length)
        {
            Length = length;
            if (length % 8 != 0) length += 8 - length % 8;
            byteArray = new byte[length / 8];
        }

        public int Length { get; }

        private void ByteBitPosition(int index, out int byteIndex, out byte bitMask)
        {
            if (index < 0 || index >= Length)
                throw new IndexOutOfRangeException("Index out of range.");
            byteIndex = byteArray.Length - 1 - index / 8;
            byte bitPosition = (byte)(index % 8);
            bitMask = (byte)(1 << bitPosition);
        }

        public bool this[int index]
        {
            get
            {
                ByteBitPosition(index, out int byteIndex, out byte bitMask);
                return (byteArray[byteIndex] & bitMask) != 0;
            }
            set
            {
                ByteBitPosition(index, out int byteIndex, out byte bitMask);
                if (value)
                    byteArray[byteIndex] |= bitMask;
                else
                    byteArray[byteIndex] &= (byte)(255 - bitMask);
            }
        }

        public void Clear()
        {
            for (int i = 0; i < byteArray.Length; ++i)
                byteArray[i] = 0;
        }
    }
}
