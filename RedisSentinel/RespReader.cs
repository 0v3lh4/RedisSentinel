using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedisSentinel
{
    public class RespReader : IDisposable
    {
        enum RespType : byte
        {
            SimpleStrings = (byte)'+',
            Errors = (byte)'-',
            Integers = (byte)':',
            BulkStrings = (byte)'$',
            Arrays = (byte)'*'
        }

        internal MemoryStream Response { get; set; }

        private RespReader(byte[] response)
        {
            Response = new MemoryStream(response);
        }

        string ReadFirstLine()
        {
            var sb = new StringBuilder();

            int current;
            var prev = default(char);
            while ((current = Response.ReadByte()) != -1)
            {
                var c = (char)current;
                if (prev == '\r' && c == '\n')
                {
                    break;
                }
                else if (prev == '\r' && c == '\r')
                {
                    sb.Append(prev);
                    continue;
                }
                else if (c == '\r')
                {
                    prev = c;
                    continue;
                }

                prev = c;
                sb.Append(c);
            }

            return sb.ToString();
        }

        object FetchResponse()
        {
            var type = (RespType)Response.ReadByte();
            switch (type)
            {
                case RespType.SimpleStrings:
                    {
                        return ReadFirstLine();
                    }
                case RespType.Errors:
                    {
                        return ReadFirstLine();
                    }
                case RespType.Integers:
                    {
                        var line = ReadFirstLine();
                        return long.Parse(line);
                    }
                case RespType.BulkStrings:
                    {
                        var line = ReadFirstLine();
                        var length = int.Parse(line);
                        if (length == -1)
                        {
                            return null;
                        }
                        var buffer = new byte[length];
                        Response.Read(buffer, 0, length);

                        ReadFirstLine();

                        return Encoding.UTF8.GetString(buffer);
                    }
                case RespType.Arrays:
                    {
                        var line = ReadFirstLine();
                        var length = int.Parse(line);

                        if (length == 0)
                        {
                            return new object[0];
                        }
                        if (length == -1)
                        {
                            return null;
                        }

                        var objects = new object[length];

                        for (int i = 0; i < length; i++)
                        {
                            objects[i] = FetchResponse();
                        }

                        return objects;
                    }
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void Dispose()
        {
            if (Response != null)
            {
                Response.Close();
                Response = null;
            }
        }

        public class Factory
        {
            public static object Return(byte[] response)
            {
                using (var respreader = new RespReader(response))
                {
                    return respreader.FetchResponse();
                }
            }
        }
    }
}
