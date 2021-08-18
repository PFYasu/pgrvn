﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PgRvn.Server.Types
{
    public class PgBit : PgType
    {
        public static readonly PgBit Default = new();
        public override int Oid => PgTypeOIDs.Bit;
        public override short Size => sizeof(byte);
        public override int TypeModifier => 1;

        public override byte[] ToBytes(object value, PgFormat formatCode)
        {
            if (TypeModifier != 1)
            {
                throw new PgErrorException(PgErrorCodes.FdwInvalidDataType,
                    $"Expected type modifier of '1' for bit type, but got '{TypeModifier}' which is unsupported.");
            }

            if (formatCode == PgFormat.Text)
            {
                return (bool)value ? Utf8GetBytes("1") : Utf8GetBytes("0");
            }

            return (bool)value ? PgBool.TrueBuffer : PgBool.FalseBuffer;
        }

        public override object FromBytes(byte[] buffer, PgFormat formatCode)
        {
            if (formatCode == PgFormat.Text)
            {
                return Utf8GetString(buffer).Equals("1");
            }

            return buffer.Equals(PgBool.TrueBuffer);
        }
    }
}