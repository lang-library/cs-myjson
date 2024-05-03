#region License and information
/*
------------------------------------------------------------------------------
This software is available under 2 licenses -- choose whichever you prefer.
------------------------------------------------------------------------------
ALTERNATIVE A - MIT License
Copyright (c) 2024 JavaCommons Technologies
Permission is hereby granted, free of charge, to any person obtaining a copy of
this software and associated documentation files (the "Software"), to deal in
the Software without restriction, including without limitation the rights to
use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies
of the Software, and to permit persons to whom the Software is furnished to do
so, subject to the following conditions:
The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
------------------------------------------------------------------------------
ALTERNATIVE B - Public Domain (www.unlicense.org)
This is free and unencumbered software released into the public domain.
Anyone is free to copy, modify, publish, use, compile, sell, or distribute this
software, either in source code form or as a compiled binary, for any purpose,
commercial or non-commercial, and by any means.
In jurisdictions that recognize copyright laws, the author or authors of this
software dedicate any and all copyright interest in the software to the public
domain. We make this dedication for the benefit of the public at large and to
the detriment of our heirs and successors. We intend this dedication to be an
overt act of relinquishment in perpetuity of all present and future rights to
this software under copyright law.
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN
ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
-----------------------------------------------------------------------------
*/
#endregion License and information

using Globals;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
//using Xunit.Abstractions;

namespace MyJson;

#region enums
public enum MyNodeType
{
    Array = 1,
    Object = 2,
    String = 3,
    Number = 4,
    NullValue = 5,
    Boolean = 6,
    None = 7,
    Custom = 0xFF,
}
public enum MyTextMode
{
    Compact,
    Indent
}
#endregion enums

#region MyData
public abstract partial class MyData
{
    #region flags
    public static bool DebugOutput = false;
    public static bool ShowDetail = false;
    public static bool ForceASCII = false; // Use Unicode by default
    public static bool NumberAsDecimal = false;
    public static bool DecimalAsString = false;
    //public static bool StrictParsing = false;
    //public static ITestOutputHelper XUnitOutput = null;
    public static MyDataFlagSetter ClearAllSettings()
    {
        return
            SetDebugOutput(false)
            .SetShowDetail(false)
            .SetForceASCII(false)
            .SetNumberAsDecimal(false)
            .SetDecimalAsString(false)
            //.SetXUnitOutput(null)
            ;
    }
    public static MyDataFlagSetter SetDebugOutput(bool flag)
    {
        return new MyDataFlagSetter().SetDebugOutput(flag);
    }
    public static MyDataFlagSetter SetShowDetail(bool flag)
    {
        return new MyDataFlagSetter().SetShowDetail(flag);
    }
    public static MyDataFlagSetter SetForceASCII(bool flag)
    {
        return new MyDataFlagSetter().SetForceASCII(flag);
    }
    public static MyDataFlagSetter SetNumberAsDecimal(bool flag)
    {
        return new MyDataFlagSetter().SetNumberAsDecimal(flag);
    }
    public static MyDataFlagSetter SetDecimalAsString(bool flag)
    {
        return new MyDataFlagSetter().SetDecimalAsString(flag);
    }
#if false
    public static MyDataFlagSetter SetStrictParsing(bool flag)
    {
        return new MyDataFlagSetter().SetStrictParsing(flag);
    }
    public static MyDataFlagSetter SetXUnitOutput(ITestOutputHelper helper)
    {
        return new MyDataFlagSetter().SetXUnitOutput(helper);
    }
#endif
    public class MyDataFlagSetter
    {
        public MyDataFlagSetter SetDebugOutput(bool flag)
        {
            DebugOutput = flag;
            return this;
        }
        public MyDataFlagSetter SetShowDetail(bool flag)
        {
            ShowDetail = flag;
            return this;
        }
        public MyDataFlagSetter SetForceASCII(bool flag)
        {
            ForceASCII = flag;
            return this;
        }
        public MyDataFlagSetter SetNumberAsDecimal(bool flag)
        {
            NumberAsDecimal = flag;
            return this;
        }
        public MyDataFlagSetter SetDecimalAsString(bool flag)
        {
            DecimalAsString = flag;
            return this;
        }
#if false
        public MyDataFlagSetter SetStrictParsing(bool flag)
        {
            StrictParsing = flag;
            return this;
        }
        public MyDataFlagSetter SetXUnitOutput(ITestOutputHelper helper)
        {
            XUnitOutput = helper;
            return this;
        }
#endif
    }
    #endregion flags

    #region Enumerators
    public struct Enumerator
    {
        private enum Type { None, Array, Object }
        private Type type;
        private Dictionary<string, MyData>.Enumerator m_Object;
        private List<MyData>.Enumerator m_Array;
        public bool IsValid { get { return type != Type.None; } }
        public Enumerator(List<MyData>.Enumerator aArrayEnum)
        {
            type = Type.Array;
            m_Object = default(Dictionary<string, MyData>.Enumerator);
            m_Array = aArrayEnum;
        }
        public Enumerator(Dictionary<string, MyData>.Enumerator aDictEnum)
        {
            type = Type.Object;
            m_Object = aDictEnum;
            m_Array = default(List<MyData>.Enumerator);
        }
        public KeyValuePair<string, MyData> Current
        {
            get
            {
                if (type == Type.Array)
                    return new KeyValuePair<string, MyData>(string.Empty, m_Array.Current);
                else if (type == Type.Object)
                    return m_Object.Current;
                return new KeyValuePair<string, MyData>(string.Empty, null);
            }
        }
        public bool MoveNext()
        {
            if (type == Type.Array)
                return m_Array.MoveNext();
            else if (type == Type.Object)
                return m_Object.MoveNext();
            return false;
        }
    }
    public struct ValueEnumerator
    {
        private Enumerator m_Enumerator;
        public ValueEnumerator(List<MyData>.Enumerator aArrayEnum) : this(new Enumerator(aArrayEnum)) { }
        public ValueEnumerator(Dictionary<string, MyData>.Enumerator aDictEnum) : this(new Enumerator(aDictEnum)) { }
        public ValueEnumerator(Enumerator aEnumerator) { m_Enumerator = aEnumerator; }
        public MyData Current { get { return m_Enumerator.Current.Value; } }
        public bool MoveNext() { return m_Enumerator.MoveNext(); }
        public ValueEnumerator GetEnumerator() { return this; }
    }
    public struct KeyEnumerator
    {
        private Enumerator m_Enumerator;
        public KeyEnumerator(List<MyData>.Enumerator aArrayEnum) : this(new Enumerator(aArrayEnum)) { }
        public KeyEnumerator(Dictionary<string, MyData>.Enumerator aDictEnum) : this(new Enumerator(aDictEnum)) { }
        public KeyEnumerator(Enumerator aEnumerator) { m_Enumerator = aEnumerator; }
        public string Current { get { return m_Enumerator.Current.Key; } }
        public bool MoveNext() { return m_Enumerator.MoveNext(); }
        public KeyEnumerator GetEnumerator() { return this; }
    }

    public class LinqEnumerator : IEnumerator<KeyValuePair<string, MyData>>, IEnumerable<KeyValuePair<string, MyData>>
    {
        private MyData m_Node;
        private Enumerator m_Enumerator;
        internal LinqEnumerator(MyData aNode)
        {
            m_Node = aNode;
            if (m_Node != null)
                m_Enumerator = m_Node.GetEnumerator();
        }
        public KeyValuePair<string, MyData> Current { get { return m_Enumerator.Current; } }
        object IEnumerator.Current { get { return m_Enumerator.Current; } }
        public bool MoveNext() { return m_Enumerator.MoveNext(); }

        public void Dispose()
        {
            m_Node = null;
            m_Enumerator = new Enumerator();
        }

        public IEnumerator<KeyValuePair<string, MyData>> GetEnumerator()
        {
            return new LinqEnumerator(m_Node);
        }

        public void Reset()
        {
            if (m_Node != null)
                m_Enumerator = m_Node.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new LinqEnumerator(m_Node);
        }
    }

    #endregion Enumerators

#region common interface

    public abstract MyNodeType Tag { get; }

    public virtual MyData this[int aIndex] { get { return null; } set { } }

    public virtual MyData this[string aKey] { get { return null; } set { } }

    public virtual string Value { get { return ""; } set { } }

    public virtual int Count { get { return 0; } }

    public virtual bool IsNumber { get { return false; } }
    public virtual bool IsString { get { return false; } }
    public virtual bool IsBoolean { get { return false; } }
    public virtual bool IsNull { get { return false; } }
    public virtual bool IsArray { get { return false; } }
    public virtual bool IsObject { get { return false; } }

    public virtual bool Inline { get { return false; } set { } }

    public virtual void Add(string aKey, MyData aItem)
    {
    }
    public virtual void Add(MyData aItem)
    {
        Add("", aItem);
    }

    public virtual MyData Remove(string aKey)
    {
        return null;
    }

    public virtual MyData Remove(int aIndex)
    {
        return null;
    }

    public virtual MyData Remove(MyData aNode)
    {
        return aNode;
    }
    public virtual void Clear() { }

    public virtual MyData Clone()
    {
        return null;
    }

    public virtual IEnumerable<MyData> Children
    {
        get
        {
            yield break;
        }
    }

    public IEnumerable<MyData> DeepChildren
    {
        get
        {
            foreach (var C in Children)
                foreach (var D in C.DeepChildren)
                    yield return D;
        }
    }

    public virtual bool HasKey(string aKey)
    {
        return false;
    }

    public virtual MyData GetValueOrDefault(string aKey, MyData aDefault)
    {
        return aDefault;
    }

    #region ToString()
    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();
        WriteToStringBuilder(sb, 0, 0, MyTextMode.Compact, false);
        string json = sb.ToString();
        return json.Replace("\r\n", "\n");
    }
    internal abstract void WriteToStringBuilder(StringBuilder aSB, int aIndent, int aIndentInc, MyTextMode aMode, bool forceAscii);
    #endregion ToString()

    #region enumerator
    public abstract Enumerator GetEnumerator();
    public IEnumerable<KeyValuePair<string, MyData>> Linq { get { return new LinqEnumerator(this); } }
    public KeyEnumerator Keys { get { return new KeyEnumerator(GetEnumerator()); } }
    public ValueEnumerator Values { get { return new ValueEnumerator(GetEnumerator()); } }
    #endregion enumerator

#endregion common interface

    #region typecasting properties

    #region Double
    public virtual double AsDouble
    {
        get
        {
            double v = 0.0;
            if (double.TryParse(Value, NumberStyles.Float, CultureInfo.InvariantCulture, out v))
                return v;
            return 0.0;
        }
    }
    #endregion Double

    #region Int
    public virtual int AsInt
    {
        get { return (int)AsDouble; }
    }
    #endregion Int
    #region UInt
    public virtual uint AsUInt
    {
        get
        {
            return (uint)AsDouble;
        }
    }
    #endregion UInt

    #region Float
    public virtual float AsFloat
    {
        get { return (float)AsDouble; }
    }
    #endregion Float

    #region Bool
    public virtual bool AsBool
    {
        get
        {
            bool v = false;
            if (bool.TryParse(Value, out v))
                return v;
            return !string.IsNullOrEmpty(Value);
        }
    }
    #endregion Bool

    #region Long
    public virtual long AsLong
    {
        get
        {
            long val = 0;
            if (long.TryParse(Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out val))
                return val;
            return 0L;
        }
    }
    #endregion Long
    #region ULong
    public virtual ulong AsULong
    {
        get
        {
            ulong val = 0;
            if (ulong.TryParse(Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out val))
                return val;
            return 0;
        }
    }
    #endregion ULong

    #region Array
    public virtual MyArray AsArray
    {
        get
        {
            return this as MyArray;
        }
    }
    #endregion Array

    #region Object
    public virtual MyObject AsObject
    {
        get
        {
            return this as MyObject;
        }
    }
    #endregion Object

    #region Decimal
    public virtual decimal AsDecimal
    {
        get
        {
            decimal result;
            if (!decimal.TryParse(Value, out result))
                result = 0;
            return result;
        }
    }
    #endregion Decimal

    #region Byte
    public virtual byte AsByte
    {
        get
        {
            return (byte)AsInt;
        }
    }
    #endregion Byte
    #region SByte
    public virtual sbyte AsSByte
    {
        get
        {
            return (sbyte)AsInt;
        }
    }
    #endregion SByte

    #region Short
    public virtual short AsShort
    {
        get
        {
            return (short)AsInt;
        }
    }
    #endregion Short
    #region UShort
    public virtual ushort AsUShort
    {
        get
        {
            return (ushort)AsInt;
        }
    }
    #endregion UShort

    #region DateTime
    public virtual System.DateTime AsDateTime
    {
        get
        {
            System.DateTime result;
            if (!System.DateTime.TryParse(Value, CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
                result = new System.DateTime(0);
            return result;
        }
    }
    #endregion DateTime
    #region TimeSpan
    public virtual System.TimeSpan AsTimeSpan
    {
        get
        {
            System.TimeSpan result;
            if (!System.TimeSpan.TryParse(Value, CultureInfo.InvariantCulture, out result))
                result = new System.TimeSpan(0);
            return result;
        }
    }
    #endregion TimeSpan

    #region Guid
    public virtual System.Guid AsGuid
    {
        get
        {
            System.Guid result;
            System.Guid.TryParse(Value, out result);
            return result;
        }
    }
    #endregion Guid

    #region ByteArray
    public virtual byte[] AsByteArray
    {
        get
        {
            if (this.IsNull || !this.IsArray)
                return null;
            int count = Count;
            byte[] result = new byte[count];
            for (int i = 0; i < count; i++)
                result[i] = this[i].AsByte;
            return result;
        }
    }
    #endregion ByteArray
    #region ByteList
    public virtual List<byte> AsByteList
    {
        get
        {
            byte[] array = this.AsByteArray;
            if (array == null) return null;
            return new List<byte>(array);
        }
    }
    #endregion ByteList

    #region StringArray
    public virtual string[] AsStringArray
    {
        get
        {
            if (this.IsNull || !this.IsArray)
                return null;
            int count = Count;
            string[] result = new string[count];
            for (int i = 0; i < count; i++)
                result[i] = this[i].Value;
            return result;
        }
    }
    #endregion StringArray
    #region StringList
    public virtual List<string> AsStringList
    {
        get
        {
            string[] array = this.AsStringArray;
            if (array == null) return null;
            return new List<string>(array);
        }
    }
    #endregion StringList

    #region ObjectArray
    public virtual object[] AsObjectArray
    {
        get
        {
            if (this.IsNull || !this.IsArray)
                return null;
            int count = Count;
            object[] result = new object[count];
            for (int i = 0; i < count; i++)
                result[i] = ToObject(this[i]);
            return result;
        }
    }
    #endregion ObjectArray
    #region ObjectList
    public virtual List<object> AsObjectList
    {
        get
        {
            var array = this.AsObjectArray;
            if (array == null) return null;
            return new List<object>(array);
        }
    }
    #endregion ObjectList

    #region DecimalArray
    public virtual decimal[] AsDecimalArray
    {
        get
        {
            if (this.IsNull || !this.IsArray)
                return null;
            int count = Count;
            decimal[] result = new decimal[count];
            for (int i = 0; i < count; i++)
                result[i] = this[i].AsDecimal;
            return result;
        }
    }
    #endregion DecimalArray
    #region DecimalList
    public virtual List<decimal> AsDecimalList
    {
        get
        {
            var array = this.AsDecimalArray;
            if (array == null) return null;
            return new List<decimal>(array);
        }
    }
    #endregion DecimalList

    #region DoubleArray
    public virtual double[] AsDoubleArray
    {
        get
        {
            if (this.IsNull || !this.IsArray)
                return null;
            int count = Count;
            double[] result = new double[count];
            for (int i = 0; i < count; i++)
                result[i] = this[i].AsDouble;
            return result;
        }
    }
    #endregion DoubleArray
    #region DoubleList
    public virtual List<double> AsDoubleList
    {
        get
        {
            var array = this.AsDoubleArray;
            if (array == null) return null;
            return new List<double>(array);
        }
    }
    #endregion DoubleList


    #endregion typecasting properties

    #region operators

    public static implicit operator double(MyData d)
    {
        return (d == null) ? 0 : d.AsDouble;
    }
    public static implicit operator float(MyData d)
    {
        return (d == null) ? 0 : d.AsFloat;
    }
    public static implicit operator decimal(MyData d)
    {
        return d == null ? 0m : d.AsDecimal;
    }
    public static implicit operator int(MyData d)
    {
        return (d == null) ? 0 : d.AsInt;
    }
    public static implicit operator uint(MyData d)
    {
        return (d == null) ? 0 : d.AsUInt;
    }
    public static implicit operator long(MyData d)
    {
        return (d == null) ? 0L : d.AsLong;
    }
    public static implicit operator ulong(MyData d)
    {
        return (d == null) ? 0 : d.AsULong;
    }
    public static implicit operator bool(MyData d)
    {
        return (d == null) ? false : d.AsBool;
    }
    public static implicit operator byte(MyData d)
    {
        return d == null ? (byte)0 : d.AsByte;
    }
    public static implicit operator sbyte(MyData d)
    {
        return d == null ? (sbyte)0 : d.AsSByte;
    }
    public static implicit operator short(MyData d)
    {
        return d == null ? (short)0 : d.AsShort;
    }
    public static implicit operator ushort(MyData d)
    {
        return d == null ? (ushort)0 : d.AsUShort;
    }
    public static implicit operator DateTime(MyData d)
    {
        return d == null ? new DateTime(0) : d.AsDateTime;
    }
    public static implicit operator TimeSpan(MyData d)
    {
        return d == null ? new TimeSpan(0) : d.AsTimeSpan;
    }
    public static implicit operator Guid(MyData d)
    {
        return d == null ? Guid.Empty : d.AsGuid;
    }
    public static implicit operator byte[](MyData d)
    {
        return d == null ? null : d.AsByteArray;
    }
    public static implicit operator List<byte>(MyData d)
    {
        return d == null ? null : d.AsByteList;
    }
    public static implicit operator string[](MyData d)
    {
        return d == null ? null : d.AsStringArray;
    }
    public static implicit operator List<string>(MyData d)
    {
        return d == null ? null : d.AsStringList;
    }
    public static implicit operator object[](MyData d)
    {
        return d == null ? null : d.AsObjectArray;
    }
    public static implicit operator List<object>(MyData d)
    {
        return d == null ? null : d.AsObjectList;
    }
    public static implicit operator double[](MyData d)
    {
        return d == null ? null : d.AsDoubleArray;
    }
    public static implicit operator List<double>(MyData d)
    {
        return d == null ? null : d.AsDoubleList;
    }
    public static implicit operator decimal[](MyData d)
    {
        return d == null ? null : d.AsDecimalArray;
    }
    public static implicit operator List<decimal>(MyData d)
    {
        return d == null ? null : d.AsDecimalList;
    }

    #region ==/!=
    public static bool operator ==(MyData a, object b)
    {
        if (ReferenceEquals(a, b))
            return true;
        bool aIsNull = a is MyNull || ReferenceEquals(a, null);
        bool bIsNull = b is MyNull || ReferenceEquals(b, null);
        if (aIsNull && bIsNull)
            return true;
        return !aIsNull && a.Equals(b);
    }
    public static bool operator !=(MyData a, object b)
    {
        return !(a == b);
    }
    #endregion ==/!=

    #region Equals()/GetHashCode()
    public override bool Equals(object obj)
    {
        return ReferenceEquals(this, obj);
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }
    #endregion Equals()/GetHashCode()

#endregion operators

    #region FromJson()
    public static MyData FromJson(string aJSON)
    {
        return new MyTool()
            .InheritMyData()
            .FromJson(aJSON);
    }
    #endregion FromJson()

    #region ToJson()
    public static string ToJson(object o, int aIndent)
    {
        return new MyTool()
            .InheritMyData()
            .ToJson(o, aIndent);
    }
    public static string ToJson(object o, bool indent = false)
    {
        return new MyTool()
            .InheritMyData()
            .ToJson(o, indent);
    }
    #endregion ToJson()

    #region FromObject()
    public static MyData FromObject(object item)
    {
        return new MyTool()
            .InheritMyData()
            .FromObject(item);
    }
    #endregion FromObject()

    #region ToObject()
    public static object ToObject(MyData x)
    {
        return new MyTool()
            .InheritMyData()
            .ToObject(x);
    }
    #endregion ToObject()

    #region ToPrintalbe
    public static string ToPrintable(object x, string title = null)
    {
        return new MyTool()
            .InheritMyData()
            .ToPrintable(x, title);
    }
    #endregion ToPrintalbe

    #region FormatJson()
    public static string FormatJson(string json, int indent)
    {
        return new MyTool()
            .InheritMyData()
            .FormatJson(json, indent);
    }
    public static string FormatJson(string json, bool indent = false)
    {
        return new MyTool()
            .InheritMyData()
            .FormatJson(json, indent);
    }
    #endregion FormatJson()

    #region Escape()
    [ThreadStatic]
    private StringBuilder m_EscapeBuilder;
    internal StringBuilder EscapeBuilder
    {
        get
        {
            if (m_EscapeBuilder == null)
                m_EscapeBuilder = new StringBuilder();
            return m_EscapeBuilder;
        }
    }
    internal string Escape(string aText, bool forceAscii)
    {
        //Log(aText, "aText");
        //Log(forceAscii, "forceAscii");
        var sb = EscapeBuilder;
        sb.Length = 0;
        if (sb.Capacity < aText.Length + aText.Length / 10)
            sb.Capacity = aText.Length + aText.Length / 10;
        foreach (char c in aText)
        {
            switch (c)
            {
                case '\\':
                    sb.Append("\\\\");
                    break;
                case '\"':
                    sb.Append("\\\"");
                    break;
                case '\n':
                    sb.Append("\\n");
                    break;
                case '\r':
                    sb.Append("\\r");
                    break;
                case '\t':
                    sb.Append("\\t");
                    break;
                case '\b':
                    sb.Append("\\b");
                    break;
                case '\f':
                    sb.Append("\\f");
                    break;
                default:
                    if (c < ' ' || (forceAscii && c > 127))
                    {
                        ushort val = c;
                        sb.Append("\\u").Append(val.ToString("X4"));
                    }
                    else
                        sb.Append(c);
                    break;
            }
        }
        string result = sb.ToString();
        //Log(result, "result");
        sb.Length = 0;
        return result;
    }
    #endregion Escape()

    public static void Echo(object x, string title = null)
    {
        new MyTool()
            .InheritMyData()
            .Echo(x, title);
    }
    public static void Log(object x, string? title = null)
    {
        new MyTool()
            .InheritMyData()
            .Log(x, title);
    }
    public static void Debug(object x, string? title = null)
    {
        new MyTool()
            .InheritMyData()
            .Debug(x, title);
    }
    public static void Message(object x, string? title = null)
    {
        new MyTool()
            .InheritMyData()
            .Message(x, title);
    }
    public static string FullName(object x)
    {
        return new MyTool()
            .InheritMyData()
            .FullName(x);
    }
}
// End of MyData
#endregion MyData

#region MyArray
public partial class MyArray : MyData
{
    private List<MyData> m_List = new List<MyData>();
    private bool inline = false;
    public override bool Inline
    {
        get { return inline; }
        set { inline = value; }
    }

    public override MyNodeType Tag { get { return MyNodeType.Array; } }
    public override bool IsArray { get { return true; } }
    public override Enumerator GetEnumerator() { return new Enumerator(m_List.GetEnumerator()); }

    public override MyData this[int aIndex]
    {
        get
        {
            if (aIndex < 0 || aIndex >= m_List.Count)
                return MyNull.CreateOrGet(); // new MyLazyCreator(this);
            return m_List[aIndex];
        }
        set
        {
            if (value == null)
                value = MyNull.CreateOrGet();
            if (aIndex < 0 || aIndex >= m_List.Count)
                m_List.Add(value);
            else
                m_List[aIndex] = value;
        }
    }

    public override MyData this[string aKey]
    {
        get { return MyNull.CreateOrGet()/*new MyLazyCreator(this)*/; }
        set
        {
            if (value == null)
                value = MyNull.CreateOrGet();
            m_List.Add(value);
        }
    }

    public override int Count
    {
        get { return m_List.Count; }
    }

    public override void Add(string aKey, MyData aItem)
    {
        if (aItem == null)
            aItem = MyNull.CreateOrGet();
        m_List.Add(aItem);
    }

    public override MyData Remove(int aIndex)
    {
        if (aIndex < 0 || aIndex >= m_List.Count)
            return null;
        MyData tmp = m_List[aIndex];
        m_List.RemoveAt(aIndex);
        return tmp;
    }

    public override MyData Remove(MyData aNode)
    {
        m_List.Remove(aNode);
        return aNode;
    }

    public override void Clear()
    {
        m_List.Clear();
    }

    public override MyData Clone()
    {
        var node = new MyArray();
        node.m_List.Capacity = m_List.Capacity;
        foreach (var n in m_List)
        {
            if (n != null)
                node.Add(n.Clone());
            else
                node.Add(null);
        }
        return node;
    }

    public override IEnumerable<MyData> Children
    {
        get
        {
            foreach (MyData N in m_List)
                yield return N;
        }
    }

    internal override void WriteToStringBuilder(StringBuilder aSB, int aIndent, int aIndentInc, MyTextMode aMode, bool forceAscii)
    {
        aSB.Append('[');
        int count = m_List.Count;
        for (int i = 0; i < count; i++)
        {
            if (i > 0)
                aSB.Append(',');
            if (aMode == MyTextMode.Indent)
                aSB.AppendLine();

            if (aMode == MyTextMode.Indent)
                aSB.Append(' ', aIndent + aIndentInc);
            m_List[i].WriteToStringBuilder(aSB, aIndent + aIndentInc, aIndentInc, aMode, forceAscii);
        }
        if (aMode == MyTextMode.Indent)
            aSB.AppendLine().Append(' ', aIndent);
        aSB.Append(']');
    }
}
// End of MyArray
#endregion MyArray

#region MyObject
public partial class MyObject : MyData
{
    private Dictionary<string, MyData> m_Dict = new Dictionary<string, MyData>();

    private bool inline = false;
    public override bool Inline
    {
        get { return inline; }
        set { inline = value; }
    }

    public override MyNodeType Tag { get { return MyNodeType.Object; } }
    public override bool IsObject { get { return true; } }

    public override Enumerator GetEnumerator() { return new Enumerator(m_Dict.GetEnumerator()); }


    public override MyData this[string aKey]
    {
        get
        {
            if (aKey == null)
                return MyNull.CreateOrGet()/*new MyLazyCreator(this, aKey)*/;
            if (m_Dict.ContainsKey(aKey))
                return m_Dict[aKey];
            else
                return MyNull.CreateOrGet()/*new MyLazyCreator(this, aKey)*/;
        }
        set
        {
            if (aKey == null)
                return;
            if (value == null)
                value = MyNull.CreateOrGet();
            if (m_Dict.ContainsKey(aKey))
                m_Dict[aKey] = value;
            else
                m_Dict.Add(aKey, value);
        }
    }

    public override MyData this[int aIndex]
    {
        get
        {
            if (aIndex < 0 || aIndex >= m_Dict.Count)
                return MyNull.CreateOrGet();
            return m_Dict.ElementAt(aIndex).Value;
        }
        set
        {
            if (value == null)
                value = MyNull.CreateOrGet();
            if (aIndex < 0 || aIndex >= m_Dict.Count)
                return;
            string key = m_Dict.ElementAt(aIndex).Key;
            m_Dict[key] = value;
        }
    }

    public override int Count
    {
        get { return m_Dict.Count; }
    }

    public override void Add(string aKey, MyData aItem)
    {
        if (aItem == null)
            aItem = MyNull.CreateOrGet();

        if (aKey != null)
        {
            if (m_Dict.ContainsKey(aKey))
                m_Dict[aKey] = aItem;
            else
                m_Dict.Add(aKey, aItem);
        }
        else
            m_Dict.Add(Guid.NewGuid().ToString(), aItem);
    }

    public override MyData Remove(string aKey)
    {
        if (!m_Dict.ContainsKey(aKey))
            return null;
        MyData tmp = m_Dict[aKey];
        m_Dict.Remove(aKey);
        return tmp;
    }

    public override MyData Remove(int aIndex)
    {
        if (aIndex < 0 || aIndex >= m_Dict.Count)
            return null;
        var item = m_Dict.ElementAt(aIndex);
        m_Dict.Remove(item.Key);
        return item.Value;
    }

    public override MyData Remove(MyData aNode)
    {
        try
        {
            var item = m_Dict.Where(k => k.Value == aNode).First();
            m_Dict.Remove(item.Key);
            return aNode;
        }
        catch
        {
            return null;
        }
    }

    public override void Clear()
    {
        m_Dict.Clear();
    }

    public override MyData Clone()
    {
        var node = new MyObject();
        foreach (var n in m_Dict)
        {
            node.Add(n.Key, n.Value.Clone());
        }
        return node;
    }

    public override bool HasKey(string aKey)
    {
        return m_Dict.ContainsKey(aKey);
    }

    public override MyData GetValueOrDefault(string aKey, MyData aDefault)
    {
        MyData res;
        if (m_Dict.TryGetValue(aKey, out res))
            return res;
        return aDefault;
    }

    public override IEnumerable<MyData> Children
    {
        get
        {
            foreach (KeyValuePair<string, MyData> N in m_Dict)
                yield return N.Value;
        }
    }

    internal override void WriteToStringBuilder(StringBuilder aSB, int aIndent, int aIndentInc, MyTextMode aMode, bool forceAscii)
    {
        aSB.Append('{');
        bool first = true;
        foreach (var k in m_Dict)
        {
            if (!first)
                aSB.Append(',');
            first = false;
            if (aMode == MyTextMode.Indent)
                aSB.AppendLine();
            if (aMode == MyTextMode.Indent)
                aSB.Append(' ', aIndent + aIndentInc);
            aSB.Append('\"').Append(Escape(k.Key, forceAscii)).Append('\"');
            if (aMode == MyTextMode.Compact)
                aSB.Append(':');
            else
                aSB.Append(" : ");
            k.Value.WriteToStringBuilder(aSB, aIndent + aIndentInc, aIndentInc, aMode, forceAscii);
        }
        if (aMode == MyTextMode.Indent)
            aSB.AppendLine().Append(' ', aIndent);
        aSB.Append('}');
    }

}
// End of MyObject
#endregion MyObject

#region MyString
public partial class MyString : MyData
{
    private string m_Data;

    public override MyNodeType Tag { get { return MyNodeType.String; } }
    public override bool IsString { get { return true; } }

    public override Enumerator GetEnumerator() { return new Enumerator(); }

    public override string Value
    {
        get { return m_Data; }
        set
        {
            m_Data = value;
        }
    }

    public MyString(string aData)
    {
        m_Data = aData;
    }
    public override MyData Clone()
    {
        return new MyString(m_Data);
    }

    internal override void WriteToStringBuilder(StringBuilder aSB, int aIndent, int aIndentInc, MyTextMode aMode, bool forceAscii)
    {
        aSB.Append('\"').Append(Escape(m_Data, forceAscii)).Append('\"');
    }
    public override bool Equals(object obj)
    {
        if (base.Equals(obj))
            return true;
        string s = obj as string;
        if (s != null)
            return m_Data == s;
        MyString s2 = obj as MyString;
        if (s2 != null)
            return m_Data == s2.m_Data;
        return false;
    }
    public override int GetHashCode()
    {
        return m_Data.GetHashCode();
    }
    public override void Clear()
    {
        m_Data = "";
    }
}
// End of MyString
#endregion MyString

#region MyNumber
public partial class MyNumber : MyData
{
    internal object m_Data;

    public override MyNodeType Tag { get { return MyNodeType.Number; } }
    public override bool IsNumber { get { return true; } }
    public override Enumerator GetEnumerator() { return new Enumerator(); }

    public override string Value
    {
        get
        {
            return m_Data.ToString();
        }
        set
        {
            decimal v;
            if (decimal.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out v))
                m_Data = v;
        }
    }

    public override double AsDouble
    {
        get
        {
            return Convert.ToDouble(m_Data);
        }
    }
    public override long AsLong
    {
        get { return (long)Convert.ToDecimal(m_Data); }
    }
    public override ulong AsULong
    {
        get { return (ulong)Convert.ToDecimal(m_Data); }
    }

    public MyNumber(object aData)
    {
        if (aData == null)
        {
            throw new ArgumentException("Argument is null");
        }
        if (!IsNumeric(aData))
        {
            string error = $"Argument is not numeric: {aData.GetType().FullName}";
            throw new ArgumentException(error);
        }
        m_Data = aData;
    }

    public override MyData Clone()
    {
        return new MyNumber(m_Data);
    }

    internal override void WriteToStringBuilder(StringBuilder aSB, int aIndent, int aIndentInc, MyTextMode aMode, bool forceAscii)
    {
        aSB.Append(Value.ToString(CultureInfo.InvariantCulture));
    }
    private static bool IsNumeric(object value)
    {
        return value is int || value is uint
            || value is float || value is double
            || value is decimal
            || value is long || value is ulong
            || value is short || value is ushort
            || value is sbyte || value is byte;
    }
    public override bool Equals(object obj)
    {
        if (obj == null)
            return false;
        if (base.Equals(obj))
            return true;
        MyNumber s2 = obj as MyNumber;
        if (s2 != null)
            return m_Data == s2.m_Data;
        if (IsNumeric(obj))
            return Convert.ToDecimal(obj) == Convert.ToDecimal(m_Data);
        return false;
    }
    public override int GetHashCode()
    {
        return m_Data.GetHashCode();
    }
    public override void Clear()
    {
        m_Data = 0;
    }
}
// End of MyNumber
#endregion MyNumber

#region MyBool
public partial class MyBool : MyData
{
    private bool m_Data;

    public override MyNodeType Tag { get { return MyNodeType.Boolean; } }
    public override bool IsBoolean { get { return true; } }
    public override Enumerator GetEnumerator() { return new Enumerator(); }

    public override string Value
    {
        get { return m_Data.ToString(); }
        set
        {
            bool v;
            if (bool.TryParse(value, out v))
                m_Data = v;
        }
    }
    public override bool AsBool
    {
        get { return m_Data; }
    }

    public MyBool(bool aData)
    {
        m_Data = aData;
    }

    public MyBool(string aData)
    {
        Value = aData;
    }

    public override MyData Clone()
    {
        return new MyBool(m_Data);
    }

    internal override void WriteToStringBuilder(StringBuilder aSB, int aIndent, int aIndentInc, MyTextMode aMode, bool forceAscii)
    {
        aSB.Append((m_Data) ? "true" : "false");
    }
    public override bool Equals(object obj)
    {
        if (obj == null)
            return false;
        if (obj is bool)
            return m_Data == (bool)obj;
        return false;
    }
    public override int GetHashCode()
    {
        return m_Data.GetHashCode();
    }
    public override void Clear()
    {
        m_Data = false;
    }
}
// End of MyBool
#endregion MyBool

#region MyNull
public partial class MyNull : MyData
{
    static MyNull m_StaticInstance = new MyNull();
    public static bool reuseSameInstance = true;
    public static MyNull CreateOrGet()
    {
        if (reuseSameInstance)
            return m_StaticInstance;
        return new MyNull();
    }
    private MyNull() { }

    public override MyNodeType Tag { get { return MyNodeType.NullValue; } }
    public override bool IsNull { get { return true; } }
    public override Enumerator GetEnumerator() { return new Enumerator(); }

    public override string Value
    {
        get { return "null"; }
        set { }
    }
    public override bool AsBool
    {
        get { return false; }
    }

    public override MyData Clone()
    {
        return CreateOrGet();
    }

    public override bool Equals(object obj)
    {
        if (object.ReferenceEquals(this, obj))
            return true;
        return (obj is MyNull);
    }
    public override int GetHashCode()
    {
        return 0;
    }

    internal override void WriteToStringBuilder(StringBuilder aSB, int aIndent, int aIndentInc, MyTextMode aMode, bool forceAscii)
    {
        aSB.Append("null");
    }
}
// End of MyNull
#endregion MyNull

#region MyTool
public class MyTool
{
    bool DebugOutput = false;
    bool ShowDetail = false;
    bool ForceASCII = false; // Use Unicode by default
    bool NumberAsDecimal = false;
    bool DecimalAsString = false;
    //bool StrictParsing = false;
    //ITestOutputHelper XUnitOutput = null;

    public MyTool()
    {
    }
    public MyTool InheritMyData()
    {
        return
            SetDebugOutput(MyData.DebugOutput)
            .SetShowDetail(MyData.ShowDetail)
            .SetForceASCII(MyData.ForceASCII)
            .SetNumberAsDecimal(MyData.NumberAsDecimal)
            .SetDecimalAsString(MyData.DecimalAsString)
            //.SetStrictParsing(MyData.StrictParsing)
            //.SetXUnitOutput(MyData.XUnitOutput)
            ;
    }
    public MyTool SetDebugOutput(bool flag)
    {
        DebugOutput = flag;
        return this;
    }
    public MyTool SetShowDetail(bool flag)
    {
        ShowDetail = flag;
        return this;
    }
    public MyTool SetForceASCII(bool flag)
    {
        ForceASCII = flag;
        return this;
    }
    public MyTool SetNumberAsDecimal(bool flag)
    {
        NumberAsDecimal = flag;
        return this;
    }
    public MyTool SetDecimalAsString(bool flag)
    {
        DecimalAsString = flag;
        return this;
    }
#if false
    public MyTool SetStrictParsing(bool flag)
    {
        StrictParsing = flag;
        return this;
    }
    public MyTool SetXUnitOutput(ITestOutputHelper helper)
    {
        XUnitOutput = helper;
        return this;
    }
#endif
    public void Echo(object x, string title = null)
    {
        string s = ToPrintable(x, title);
        Console.WriteLine(s);
        System.Diagnostics.Debug.WriteLine(s);
    }
    public void Log(object x, string? title = null)
    {
        string s = ToPrintable(x, title);
        Console.Error.WriteLine("[Log] " + s);
        System.Diagnostics.Debug.WriteLine("[Log] " + s);
        //if (XUnitOutput != null)
        //    try { XUnitOutput.WriteLine("[Log] " + s); } catch { }
    }
    public void Debug(object x, string? title = null)
    {
        if (!DebugOutput) return;
        string s = ToPrintable(x, title);
        Console.Error.WriteLine("[Debug] " + s);
        System.Diagnostics.Debug.WriteLine("[Debug] " + s);
        //if (XUnitOutput != null)
        //    try { XUnitOutput.WriteLine("[Debug] " + s); } catch { }
    }
    public void Message(object x, string? title = null)
    {
        if (title == null) title = "Message";
        String s = ToPrintable(x, null);
        NativeMethods.MessageBoxW(IntPtr.Zero, s, title, 0);
    }
    public string FullName(object x)
    {
        if (x is null) return "null";
        string fullName = ((object)x).GetType().FullName;
        return fullName.Split('`')[0];
    }
    public string ToPrintable(object x, string title = null)
    {
        string s = "";
        if (title != null) s = title + ": ";
        if (x is null) return s + "null";
        if (x is string)
        {
            if (!ShowDetail) return s + (string)x;
            return s + "`" + (string)x + "`";
        }
        string output = null;
        if (x is MyString)
        {
            var value = (MyString)x;
            output = ToJson(value, true);
        }
        else if (x is MyData)
        {
            var value = (MyData)x;
            output = ToJson(value, true);
        }
        else
        {
            try
            {
                output = ToJson(x, true);
            }
            catch (Exception)
            {
                output = x.ToString();
            }
        }
        if (!ShowDetail) return s+ output;
        return s+ $"<{FullName(x)}> {output}";
    }

    #region ToJson()
    public string ToJson(object o, int aIndent)
    {
        if (o == null) return "null";
        MyData x = o as MyData;
        if (x == null)
        {
            x = FromObject(o);
        }
        StringBuilder sb = new StringBuilder();
        x.WriteToStringBuilder(sb, 0, aIndent, MyTextMode.Indent, this.ForceASCII);
        string json = sb.ToString();
        return json.Replace("\r\n", "\n");
    }
    public string ToJson(object o, bool indent = false)
    {
        if (o == null) return "null";
        MyData x = o as MyData;
        if (x == null)
        {
            x = FromObject(o);
        }
        StringBuilder sb = new StringBuilder();
        if (indent)
        {
            x.WriteToStringBuilder(sb, 0, 2, MyTextMode.Indent, this.ForceASCII);
            string json = sb.ToString();
            return json.Replace("\r\n", "\n");
        }
        else
        {
            x.WriteToStringBuilder(sb, 0, 0, MyTextMode.Compact, this.ForceASCII);
            string json = sb.ToString();
            return json.Replace("\r\n", "\n");
        }
    }
    #endregion ToJson()

    #region ToObject()
    public object ToObject(MyData x)
    {
        if (x == null) return null;
        if (x is MyNull) return null;
        if (x is MyBool) return x.AsBool;
        if (x is MyNumber)
        {
            return ((MyNumber)x).m_Data;
        }
        if (x is MyString) return x.Value;
        if (x is MyArray)
        {
            var result = new List<object>();
            var array = x as MyArray;
            for (int i = 0; i < array!.Count; i++)
            {
                result.Add(ToObject(array[i]));
            }
            return result;
        }
        if (x is MyObject)
        {
            var result = new Dictionary<string, object>();
            var obj = x as MyObject;
            var keys = obj!.Keys;
            foreach (var key in keys)
            {
                result[key] = ToObject(obj![key]);
            }
            return result;
        }
        throw new Exception($"{x.GetType().FullName} is not supported");
    }
    #endregion ToObject()

    #region FromJson()
    public MyData FromJson(string aJSON)
    {
#if false
        var o = (StrictParsing) ?
            MyStrictParser.Parse(aJSON, NumberAsDecimal) :
            MyParser.Parse(aJSON, NumberAsDecimal);
#else
        var o = JsoncParser.Parse(aJSON, NumberAsDecimal);
        //var o = MyJsoncParser.Parse(aJSON, NumberAsDecimal);
#endif
        return FromObject(o);
    }
#endregion FromJson()

    #region FromObject()
    public MyData FromObject(object item)
    {
        if (item == null)
        {
            //Console.Error.WriteLine("FromObject(2)");
            return MyNull.CreateOrGet();
        }

        Type type = item.GetType();
        if (type == typeof(MyArray)
         || type == typeof(MyObject)
         || type == typeof(MyString)
         || type == typeof(MyNumber)
         || type == typeof(MyBool)
         || type == typeof(MyNull))
        {
            return (MyData)item;
        }
        if (type == typeof(string) || type == typeof(char))
        {
            string str = item.ToString();
            return new MyString(str);
        }
        else if (type == typeof(byte) || type == typeof(sbyte))
        {
            return new MyNumber(item);
        }
        else if (type == typeof(short) || type == typeof(ushort))
        {
            return new MyNumber(item);
        }
        else if (type == typeof(int) || type == typeof(uint))
        {
            return new MyNumber(item);
        }
        else if (type == typeof(long) || type == typeof(ulong))
        {
            return new MyNumber(item);
        }
        else if (type == typeof(float))
        {
            return new MyNumber(item);
        }
        else if (type == typeof(double))
        {
            return new MyNumber(item);
        }
        else if (type == typeof(decimal))
        {
            if (DecimalAsString)
                return new MyString(item.ToString());
            return new MyNumber(item);
        }
        else if (type == typeof(bool))
        {
            return new MyBool((bool)item);
        }
        else if (type == typeof(DateTime))
        {
            return new MyString(((DateTime)item).ToString("yyyy-MM-ddTHH\\:mm\\:ss.fffffffzzz"));
        }
        else if (type == typeof(TimeSpan))
        {
            return new MyString(item.ToString());
        }
        else if (type == typeof(Guid))
        {
            return new MyString(item.ToString());
        }
        else if (type.IsEnum)
        {
            return new MyString(item.ToString());
        }
        else if (item is ExpandoObject)
        {
            var dic = item as IDictionary<string, object>;
            var result = new MyObject();
            foreach (var key in dic.Keys)
            {
                result[key] = FromObject(dic[key]);
            }
            return result;
        }
        else if (item is IList)
        {
            IList list = item as IList;
            var result = new MyArray();
            for (int i = 0; i < list.Count; i++)
            {
                result.Add(FromObject(list[i]));
            }
            return result;
        }
        else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
        {
            Type keyType = type.GetGenericArguments()[0];
            var result = new MyObject();
            //Refuse to output dictionary keys that aren't of type string
            if (keyType != typeof(string))
            {
                return result;
            }
            IDictionary dict = item as IDictionary;
            foreach (object key in dict.Keys)
            {
                result[(string)key] = FromObject(dict[key]);
            }
            return result;
        }
        else
        {
            Type keyType = type.GetGenericArguments()[0];
            FieldInfo[] fieldInfos = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy);
            var result = new MyObject();
            for (int i = 0; i < fieldInfos.Length; i++)
            {
                if (fieldInfos[i].IsDefined(typeof(IgnoreDataMemberAttribute), true))
                    continue;
                object value = fieldInfos[i].GetValue(item);
                result[GetMemberName(fieldInfos[i])] = FromObject(value);
            }
            PropertyInfo[] propertyInfo = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy);
            for (int i = 0; i < propertyInfo.Length; i++)
            {
                if (!propertyInfo[i].CanRead || propertyInfo[i].IsDefined(typeof(IgnoreDataMemberAttribute), true))
                    continue;
                object value = propertyInfo[i].GetValue(item, null);
                result[GetMemberName(propertyInfo[i])] = FromObject(value);
            }
            return result;
        }
    }

    static string GetMemberName(MemberInfo member)
    {
        if (member.IsDefined(typeof(DataMemberAttribute), true))
        {
            DataMemberAttribute dataMemberAttribute = (DataMemberAttribute)Attribute.GetCustomAttribute(member, typeof(DataMemberAttribute), true);
            if (!string.IsNullOrEmpty(dataMemberAttribute.Name))
                return dataMemberAttribute.Name;
        }

        return member.Name;
    }
    #endregion FromObject()

    public string FormatJson(string json, int indent)
    {
        MyData md = this.FromJson(json);
        return this.ToJson(md, indent);
    }
    public string FormatJson(string json, bool indent = false)
    {
        MyData md = this.FromJson(json);
        return this.ToJson(md, indent);
    }
    internal static class NativeMethods
    {
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        internal static extern int MessageBoxW(
            IntPtr hWnd, string lpText, string lpCaption, uint uType);
    }
}
#endregion MyTool
