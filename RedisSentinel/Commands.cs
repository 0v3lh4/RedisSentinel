using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedisSentinel
{
    public class Commands
    {
        public static string ArrayPrefixSlice = "*";
        public static string BulkStringPrefixSlice = "$";
        public static string LineEndPrefixSlice = "\r\n";

        public static byte[] MASTER()
        {
            return GenerateCommand("SENTINEL", "MASTERS");
        }

        public static byte[] TESTE()
        {
            return GenerateCommand("GET", "teste");
        }

        private static byte[] GenerateCommand(params string[] commands)
        {
            StringBuilder command = new StringBuilder();
            command.Append(ArrayPrefixSlice);
            command.Append(commands.Length);
            command.Append(LineEndPrefixSlice);

            for (int i = 0; i < commands.Count(); i++)
            {
                command.Append(BulkStringPrefixSlice);
                command.Append(commands[i].Length);
                command.Append(LineEndPrefixSlice);
                command.Append(commands[i]);
                command.Append(LineEndPrefixSlice);
            }

            return StringToBytes(command.ToString()).Item1;
        }

        private static Tuple<byte[], int> StringToBytes(string value)
        {
            var bytes = Encoding.UTF8.GetBytes(value);
            return new Tuple<byte[], int>(bytes, bytes.Length);
        }
    }
}
