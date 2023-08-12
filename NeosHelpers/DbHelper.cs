using System;
using System.Text;

namespace NeosPreCacher.NeosHelpers
{
    public class DbHelper
    {
        private static ushort[] INIT_TABLE = new ushort[10]
        {
             0,
             1,
             39,
             52,
             48,
             55,
             50,
             49,
             30,
             44
        };
        private static ushort[] SEED_TABLE = new ushort[31]
        {
             25185,
             25699,
             26213,
             26727,
             27241,
             27755,
             28269,
             28783,
             29297,
             29811,
             30325,
             30839,
             31353,
             16961,
             17475,
             17989,
             18503,
             19017,
             19531,
             20045,
             20559,
             21073,
             21587,
             22101,
             22615,
             23129,
             12592,
             13106,
             13620,
             14134,
             14648
        };

        public static unsafe string ProcessConnection(string connection, string seed)
        {
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.Append("Filename=");
            stringBuilder.Append(connection);
            stringBuilder.Append(";");

            byte* pointer = stackalloc byte[64];
            SEED_TABLE.AsSpan().CopyTo(new Span<ushort>(pointer, 32));
            for (int index1 = 0; index1 < 128; ++index1)
            {
                for (int index2 = 0; index2 < seed.Length; ++index2)
                {
                    for (int index3 = 0; index3 < seed.Length; ++index3)
                    {
                        if (index3 != index2)
                        {
                            int index4 = (seed[index3] * 43690 + index1) % 62;
                            int index5 = seed[index2] * 43690 % 62;
                            if (((index1 ^ index2 ^ index3) & 1) == 0)
                                index5 = 61 - index5;
                            byte num = pointer[index4];
                            pointer[index4] = pointer[index5];
                            pointer[index5] = num;
                        }
                    }
                }
            }
            for (int index = 0; index < seed.Length; ++index)
            {
                ushort num = seed[index];
                byte* numPtr = pointer + num % 62;
                stringBuilder.Insert(stringBuilder.Length - num % (index + 1), (char)*numPtr);
            }
            for (int index = 0; index < INIT_TABLE.Length; ++index)
            {
                int num = INIT_TABLE[index] + index + 59;
                if (index == 0)
                    stringBuilder.Append((char)num);
                else
                    stringBuilder.Insert(stringBuilder.Length - (seed.Length + index), (char)num);
            }
            return stringBuilder.ToString();
        }
    }
}
