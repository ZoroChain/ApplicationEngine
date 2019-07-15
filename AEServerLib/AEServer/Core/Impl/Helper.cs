
using System;
using System.Dynamic;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

using Newtonsoft.Json;

namespace AEServer
{

    class AEHelper
    {
        static DateTime _DateTime1970=new DateTime(1970,1,1);
        public static uint ToUnixTimeStamp(DateTime time)
        {
            TimeSpan t=time -_DateTime1970;
            return (uint)t.TotalSeconds;
        }
        public static ulong ToUnixTimeStampMS(DateTime time)
        {
            TimeSpan t=time -_DateTime1970;
            return (ulong)t.TotalMilliseconds;
        }
        
        public static string dumpObject(object obj)
        {
            // TO DO : dump object as json or other info
            if(obj == null)
            {
                return "null";
            }

            return JsonConvert.SerializeObject(obj);
        }

        public static object ConvertKVPairArrayToDynamicObject<VT>(KeyValuePair<string, VT>[] ary)
        {
            IDictionary<string, object> ret = new ExpandoObject();

            foreach(var item in ary)
            {
                ret[item.Key] = item.Value;
            }

            return ret;
        }
        
        public static bool HasProperty(dynamic obj, string name)
        {
            Type objType = obj.GetType();

            if (objType == typeof(ExpandoObject))
            {
                return ((IDictionary<string, object>)obj).ContainsKey(name);
            }

            return objType.GetProperty(name) != null;
        }

        public static dynamic CloneDynamicArray(dynamic src)
        {
            var srcAry = (ArrayList)src;
            var dstAry = new ArrayList(srcAry.Count);


            for(int i=0; i< srcAry.Count; ++i)
            {
                if(srcAry[i] is ExpandoObject)
                {
                    dstAry.Add(CloneDynamicObject(srcAry[i]));
                }
                else if(srcAry[i] is ArrayList)
                {
                    dstAry.Add(CloneDynamicArray(srcAry[i]));
                }
                else if(srcAry[i] is ICloneable)
                {
                    dstAry.Add(((ICloneable)srcAry[i]).Clone());
                }
                else
                {
                    dstAry.Add(srcAry[i]);
                }
            }

            return dstAry;
        }

        public static dynamic CloneDynamicObject(dynamic src)
        {
            var dst = new ExpandoObject();

            var original = (IDictionary<string, object>)src;
            var clone = (IDictionary<string, object>)dst;

            foreach (var kvp in original)
            {
                if(kvp.Value is ExpandoObject)
                {
                    clone.Add(kvp.Key, CloneDynamicObject(kvp.Value));
                }
                else if(kvp.Value is ArrayList)
                {
                    clone.Add(kvp.Key, CloneDynamicArray(kvp.Value));
                }
                else if(kvp.Value is ICloneable)
                {
                    clone.Add(kvp.Key, ((ICloneable)kvp.Value).Clone());
                }
                else
                {
                    clone.Add(kvp.Key, kvp.Value);
                }

            }

            return dst;
        }

        public static dynamic CopyExistDynamicObject(dynamic src, dynamic dst)
        {
            var original = (IDictionary<string, object>)src;
            var copy = (IDictionary<string, object>)dst;

            foreach (var kvp in original)
            {
                if(!copy.ContainsKey(kvp.Key))
                {
                    // only copy exist part
                    continue;
                }

                if(kvp.Value is ExpandoObject)
                {
                    CopyExistDynamicObject(kvp.Value, copy[kvp.Key]);
                }
                else if(kvp.Value is ArrayList)
                {
                    copy[kvp.Key] = CloneDynamicArray(kvp.Value);
                }
                else if(kvp.Value is ICloneable)
                {
                    copy[kvp.Key] = ((ICloneable)kvp.Value).Clone();
                }
                else
                {
                    copy[kvp.Key] = kvp.Value;
                }
            }

            return dst;
        }

        public static bool CompareDynamicArray(dynamic obj1, dynamic obj2)
        {
            var ary1 = (IList)obj1;
            var ary2 = (IList)obj2;

            if(ary1.Count != ary2.Count)
            {
                return false;
            }

            for(int i=0; i< ary1.Count; ++i)
            {
                object value1 = ary1[i];
                object value2 = ary2[i];

                if(value1 is ExpandoObject)
                {
                    if(!(value2 is ExpandoObject))
                    {
                        return false;
                    }

                    if(!CompareDynamicObject(value1, value2))
                    {
                        return false;
                    }
                }
                else if(value1 is IList)
                {
                    if(!(value2 is IList))
                    {
                        return false;
                    }

                    if(!CompareDynamicArray(value1, value2))
                    {
                        return false;
                    }
                }
                else
                {
                    if(!value1.Equals(value2))
                    {
                        return false;
                    }
                }

            }

            return true;
        }

        public static bool CompareDynamicObject(dynamic obj1, dynamic obj2)
        {
            var objDic1 = (IDictionary<string, object>)obj1;
            var objDic2 = (IDictionary<string, object>)obj2;

            foreach (var kvp1 in objDic1)
            {
                object value2 = null;
                if(!objDic2.TryGetValue(kvp1.Key, out value2))
                {
                    return false;
                }

                if(kvp1.Value is ExpandoObject)
                {
                    if(!(value2 is ExpandoObject))
                    {
                        return false;
                    }

                    if(!CompareDynamicObject(kvp1.Value, value2))
                    {
                        return false;
                    }
                }
                else if(kvp1.Value is IList)
                {
                    if(!(value2 is IList))
                    {
                        return false;
                    }

                    if(!CompareDynamicArray(kvp1.Value, value2))
                    {
                        return false;
                    }
                }
                else
                {
                    if(!kvp1.Value.Equals(value2))
                    {
                        return false;
                    }
                }

            }

            return true;
        }
        
        // // shallow copy
        // public static void StructToDynamic(object s, dynamic d)
        // {
        //     Type st = s.GetType();
        //     var objDic = (IDictionary<string, object>)d;
        //     PropertyInfo[] pis = st.GetProperties();

        //     foreach (PropertyInfo pi in pis)
        //     {
        //         d.TryAdd(pi.Name, pi.GetValue(s));
        //     }
        // }

        // // shallow copy
        // public static void DynamicToStruct(dynamic d, object s)
        // {
        //     Type st = s.GetType();
        //     var objDic = (IDictionary<string, object>)d;

        //     foreach (var kvp in objDic)
        //     {
        //         PropertyInfo pi = st.GetProperty(kvp.Key);
        //         if(pi == null)
        //         {
        //             // not found
        //             continue;
        //         }

        //         pi.SetValue(st, kvp.Value);
        //     }
        // }
    }

}