using System;
using System.Collections.Generic;
using System.Text;

using Newtonsoft.Json;

namespace AEServer.DB
{ 
    public class AEDBHelper
    {
        public static byte[] serializeObject(object obj)
        {
            string objJson = JsonConvert.SerializeObject(obj);

            byte[] objBytes = System.Text.Encoding.Default.GetBytes(objJson);

            return objBytes;
        }

        public static object unserializeObject(byte[] bytes)
        {
            string objJson = System.Text.Encoding.Default.GetString(bytes);

            object obj = JsonConvert.DeserializeObject(objJson);

            return obj;
        }

        public static bool isSameData(byte[] bytes1, byte[] bytes2)
        {
            // TO DO : compare byte array 8 byte per loop
            int length = bytes1.Length;
            if (length != bytes2.Length)
            {
                return false;
            }
            for (int i = 0; i < length; i++)
            {
                if (bytes1[i] != bytes2[i])
                    return false;
            }
            return true;
        }

        //private static unsafe bool UnSafeEquals(byte[] strA, byte[] strB)
        //{
        //    int length = strA.Length;
        //    if (length != strB.Length)
        //    {
        //        return false;
        //    }
        //    fixed (byte* str = strA)
        //    {
        //        byte* chPtr = str;
        //        fixed (byte* str2 = strB)
        //        {
        //            byte* chPtr2 = str2;
        //            byte* chPtr3 = chPtr;
        //            byte* chPtr4 = chPtr2;
        //            while (length >= 10)
        //            {
        //                if ((((*(((int*)chPtr3)) != *(((int*)chPtr4))) || (*(((int*)(chPtr3 + 2))) != *(((int*)(chPtr4 + 2))))) || ((*(((int*)(chPtr3 + 4))) != *(((int*)(chPtr4 + 4)))) || (*(((int*)(chPtr3 + 6))) != *(((int*)(chPtr4 + 6)))))) || (*(((int*)(chPtr3 + 8))) != *(((int*)(chPtr4 + 8)))))
        //                {
        //                    break;
        //                }
        //                chPtr3 += 10;
        //                chPtr4 += 10;
        //                length -= 10;
        //            }
        //            while (length > 0)
        //            {
        //                if (*(((int*)chPtr3)) != *(((int*)chPtr4)))
        //                {
        //                    break;
        //                }
        //                chPtr3 += 2;
        //                chPtr4 += 2;
        //                length -= 2;
        //            }
        //            return (length <= 0);
        //        }
        //    }
        //}
    }
}
