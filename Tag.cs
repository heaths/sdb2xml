// Copyright 2020 Heath Stewart.
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;

namespace sdb2xml
{
    internal class Tag : IConvertible
    {
        internal Tag(Pdb pdb, int id, Tag parent)
        {
            this.pdb = pdb;
            this.id = id;
            Parent = parent;
        }

        internal string Name
        {
            get
            {
                var ptr = NativeMethods.SdbTagToString(Type);
                return Marshal.PtrToStringUni(ptr);
            }
        }

        internal int Size
        {
            get
            {
                return NativeMethods.SdbGetTagDataSize(pdb, id);
            }
        }

        internal short Type
        {
            get
            {
                return NativeMethods.SdbGetTagFromTagID(pdb, id);
            }
        }

        internal short BaseType
        {
            get
            {
                return (short)((int)Type & 61440);
            }
        }

        internal bool IsDateTime
        {
            get
            {
                var type = Type;
                return type == 20481;
            }
        }

        internal bool IsFile
        {
            get
            {
                var type = Type;
                return type == -28670 || type == -28669 || type == -28667;
            }
        }

        internal bool IsGuid
        {
            get
            {
                var type = Type;
                return type == -28665 || type == -28668 || type == -28666;
            }
        }

        internal Tag Parent { get; }

        internal IEnumerable<Tag> Tags
        {
            get
            {
                for (var childId = NativeMethods.SdbGetFirstChild(pdb, id); childId != 0; childId = NativeMethods.SdbGetNextChild(pdb, id, childId))
                {
                    yield return new Tag(pdb, childId, this);
                }
                yield break;
            }
        }

        internal Tag Find(short type)
        {
            var num = NativeMethods.SdbFindFirstTag(pdb, id, type);
            if (num != 0)
            {
                return new Tag(pdb, num, this);
            }
            return null;
        }

        internal byte[] GetData()
        {
            var array = new byte[Size];
            if (NativeMethods.SdbReadBinaryTag(pdb, id, array, array.Length))
            {
                return array;
            }
            return null;
        }

        public override string ToString()
        {
            return ToString(CultureInfo.InvariantCulture);
        }

        public TypeCode GetTypeCode()
        {
            var baseType = BaseType;
            if (baseType <= 8192)
            {
                if (baseType <= -28672)
                {
                    if (baseType != -32768)
                    {
                        if (baseType != -28672)
                        {
                            return TypeCode.Empty;
                        }
                        return TypeCode.Object;
                    }
                }
                else
                {
                    if (baseType == 4096)
                    {
                        return TypeCode.DBNull;
                    }
                    if (baseType != 8192)
                    {
                        return TypeCode.Empty;
                    }
                    return TypeCode.Byte;
                }
            }
            else if (baseType <= 16384)
            {
                if (baseType == 12288)
                {
                    return TypeCode.Int16;
                }
                if (baseType != 16384)
                {
                    return TypeCode.Empty;
                }
                return TypeCode.Int32;
            }
            else if (baseType != 20480)
            {
                if (baseType != 24576)
                {
                    return TypeCode.Empty;
                }
            }
            else
            {
                if (IsDateTime)
                {
                    return TypeCode.DateTime;
                }
                return TypeCode.Int64;
            }
            return TypeCode.String;
        }

        public bool ToBoolean(IFormatProvider provider)
        {
            var num = ToInt64(provider);
            return num != 0L;
        }

        public byte ToByte(IFormatProvider provider)
        {
            var value = ToInt64(provider);
            return Convert.ToByte(value);
        }

        public char ToChar(IFormatProvider provider)
        {
            var text = ToString(provider);
            if (1 == text.Length)
            {
                return text[0];
            }
            throw new InvalidCastException();
        }

        public DateTime ToDateTime(IFormatProvider provider)
        {
            var fileTime = ToInt64(provider);
            return DateTime.FromFileTimeUtc(fileTime);
        }

        public decimal ToDecimal(IFormatProvider provider)
        {
            var value = ToInt64(provider);
            return Convert.ToDecimal(value);
        }

        public double ToDouble(IFormatProvider provider)
        {
            var value = ToInt64(provider);
            return Convert.ToDouble(value);
        }

        public short ToInt16(IFormatProvider provider)
        {
            var value = ToInt64(provider);
            return Convert.ToInt16(value);
        }

        public int ToInt32(IFormatProvider provider)
        {
            var value = ToInt64(provider);
            return Convert.ToInt32(value);
        }

        public long ToInt64(IFormatProvider provider)
        {
            var typeCode = GetTypeCode();
            switch (typeCode)
            {
                case TypeCode.Byte:
                    {
                        var b = NativeMethods.SdbReadBYTETag(pdb, id, 0);
                        return (long)((ulong)b);
                    }
                case TypeCode.Int16:
                    {
                        var num = NativeMethods.SdbReadWORDTag(pdb, id, 0);
                        return (long)num;
                    }
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                    goto IL_89;
                case TypeCode.Int32:
                    {
                        var num2 = NativeMethods.SdbReadDWORDTag(pdb, id, 0);
                        return (long)num2;
                    }
                case TypeCode.Int64:
                    break;
                default:
                    if (typeCode != TypeCode.DateTime)
                    {
                        goto IL_89;
                    }
                    break;
            }
            return NativeMethods.SdbReadQWORDTag(pdb, id, 0L);
        IL_89:
            throw new InvalidCastException();
        }

        public sbyte ToSByte(IFormatProvider provider)
        {
            var value = ToInt64(provider);
            return Convert.ToSByte(value);
        }

        public float ToSingle(IFormatProvider provider)
        {
            var value = ToInt64(provider);
            return Convert.ToSingle(value);
        }

        public string ToString(IFormatProvider provider)
        {
            var typeCode = GetTypeCode();
            if (typeCode != TypeCode.Object)
            {
                switch (typeCode)
                {
                    case TypeCode.Byte:
                    case TypeCode.Int16:
                    case TypeCode.Int32:
                    case TypeCode.Int64:
                        return ToInt64(provider).ToString(provider);
                    case TypeCode.UInt16:
                    case TypeCode.UInt32:
                        break;
                    default:
                        switch (typeCode)
                        {
                            case TypeCode.DateTime:
                                return ToDateTime(provider).ToString("o", CultureInfo.InvariantCulture);
                            case TypeCode.String:
                                return NativeMethods.SdbGetStringTagPtr(pdb, id);
                        }
                        break;
                }
            }
            else if (IsGuid)
            {
                var array = new byte[16];
                if (NativeMethods.SdbReadBinaryTag(pdb, id, array, array.Length))
                {
                    var guid = new Guid(array);
                    return guid.ToString("B", provider);
                }
            }
            return null;
        }

        public object ToType(Type conversionType, IFormatProvider provider)
        {
            throw new InvalidCastException();
        }

        public ushort ToUInt16(IFormatProvider provider)
        {
            var value = ToInt64(provider);
            return Convert.ToUInt16(value);
        }

        public uint ToUInt32(IFormatProvider provider)
        {
            var value = ToInt64(provider);
            return Convert.ToUInt32(value);
        }

        public ulong ToUInt64(IFormatProvider provider)
        {
            var value = ToInt64(provider);
            return Convert.ToUInt64(value);
        }

        private readonly Pdb pdb;

        private readonly int id;
    }
}
