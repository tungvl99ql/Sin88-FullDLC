using UnityEngine;
using System.Collections;
using System.Collections.Generic;
//using WebSocketSharp;
using System;

namespace Core.Server.Api
{


    public class OutBounMessage
    {
        private List<int> req;
        private int commandId;
        private CommandTranslator cmd;
        public OutBounMessage(string command)
        {
            req = new List<int>();
            cmd = CommandTranslator.getInstance();
            this.commandId = Int32.Parse(cmd.getCommandId(command));
        }

        // them 2 byte dau tien - commandId
        public void addHead()
        {
            req.Add((commandId >> 8));
            req.Add((commandId & 0xff));
        }

        public void addArray(int[] array)
        {
            for (int i = 0; i < array.Length; i++)
            {
                req.Add(array[i]);
            }
        }

        public void writeAcii(string t)
        {
            byte[] bytes = System.Text.Encoding.ASCII.GetBytes(t);
            req.Add(bytes.Length); //do dai khi write assci
            for (int i = 0; i < bytes.Length; i++)
            {
                req.Add(bytes[i]);
            }

        }

        public void writeLongAcii(string t)
        {
            /*
            int count = t.Length;
            App.trace("DAI " + t.Length);
            int temp = 0;
            while (count > 125)
            {
                string t1 = t.Substring(0, 125);
                byte[] bytesss = System.Text.Encoding.ASCII.GetBytes(t1);
                req.Add(bytesss.Length);
                for (int i = 0; i < bytesss.Length; i++)
                {
                    req.Add(bytesss[i]);
                }
                count -= 125;
                temp++;
            }

            string t2 = t.Substring(temp * 125, count);
            byte[] bytes = System.Text.Encoding.BigEndianUnicode.GetBytes(t2);
            req.Add(bytes.Length); //do dai khi write assci
            for (int i = 0; i < bytes.Length; i++)
            {
                req.Add(bytes[i]);
            }
            */
            byte[] bytes = System.Text.Encoding.ASCII.GetBytes(t);
            writeInt(t.Length); //do dai khi write assci

            for (int i = 0; i < bytes.Length; i++)
            {
                req.Add(bytes[i]);
            }

        }

        public void writeByte(int t)
        {
            req.Add(t);
        }

        public void writeBytes(List<int> arr)
        {
            writeShort((short)arr.Count);
            foreach (int tmp in arr)
            {
                req.Add(tmp);
            }
        }

        public void writeString(string t)
        {
            /*
            req.Add(0);
            if (t.Length == 0)
            {
                req.Add(0);
            }
            else
            {
                byte[] bytes = System.Text.Encoding.ASCII.GetBytes(t);
                req.Add(bytes.Length); //do dai khi write string
                for (int i = 0; i < bytes.Length; i++)
                {
                    req.Add(0); //them 0 moi khi truyen string
                    req.Add(bytes[i]);
                }
            }
            */
            req.Add(0);
            if (t.Length == 0)
            {
                req.Add(0);
            }
            else
            {
                byte[] bytes = System.Text.Encoding.Unicode.GetBytes(t);
                change30(bytes);
                req.Add(bytes.Length / 2); //do dai khi write string
                                           //req.Add(0);

                for (int i = 0; i < bytes.Length; i++)
                {
                    //req.Add(i+1); //them 0 moi khi truyen string
                    req.Add(bytes[i]);
                    //i += 1;
                }

            }
        }
        public void writeStrings(string t)
        {
            req.Add(0);
            if (t.Length == 0)
            {
                req.Add(0);
            }
            else
            {
                byte[] bytes = System.Text.Encoding.Unicode.GetBytes(t);
                change30(bytes);
                req.Add(bytes.Length / 2); //do dai khi write string
                                           //req.Add(0);

                for (int i = 0; i < bytes.Length; i++)
                {
                    //req.Add(i+1); //them 0 moi khi truyen string
                    req.Add(bytes[i]);
                    //i += 1;
                }

            }
        }

        //Doi cho byte 30 de giai ma Unicode
        private void change30(byte[] bytes)
        {
            for (int i = 1; i < bytes.Length; i++)
            {
                int temp = (int)bytes[i - 1];
                bytes[i - 1] = bytes[i];
                bytes[i] = (byte)temp;
                i += 1;
            }
        }

        public void writeInt(Int32 num)
        {
            byte[] bytes = ConvertInt32ToByteArray(num);

            for (int i = bytes.Length - 1; i >= 0; i--)
            {
                req.Add(bytes[i]);

            }

        }

        public void writeLong(Int64 num)
        {
            byte[] bytes = ConvertInt64ToByteArray(num);

            for (int i = bytes.Length - 1; i >= 0; i--)
            {
                req.Add(bytes[i]);

            }

        }

        public void writeShort(Int16 num)
        {
            byte[] bytes = ConvertInt16ToByteArray(num);

            for (int i = bytes.Length - 1; i >= 0; i--)
            {
                req.Add(bytes[i]);

            }

        }

        public void print()
        {

            String t = "";

            for (int i = 0; i < req.Count; i++)
            {
                t += req[i] + ",";
            }

            Debug.Log(req.Count + "| " + t);
        }

        public void printBytesArray(byte[] req)
        {
            String t = "";
            for (int i = 0; i < req.Length; i++)
            {
                t += req[i] + ",";
            }
            Debug.Log(t);
        }

        public byte[] getReq()
        {
            return IntArrayToByteArray(req.ToArray());
        }

        public byte[] IntArrayToByteArray(int[] ints)
        {

            List<byte> bytes = new List<byte>(ints.GetUpperBound(0) * sizeof(byte));

            foreach (int integer in ints)
            {

                bytes.Add(BitConverter.GetBytes(integer)[0]);

            }

            return bytes.ToArray();

        }

        public static byte[] GetByteArrayFromIntArray(int[] intArray)
        {
            byte[] data = new byte[intArray.Length * 4];
            for (int i = 0; i < intArray.Length; i++)
                Array.Copy(BitConverter.GetBytes(intArray[i]), 0, data, i * 4, 4);
            return data;
        }

        public static byte[] ConvertInt64ToByteArray(Int64 I64)
        {
            return BitConverter.GetBytes(I64);
        }

        public static byte[] ConvertInt32ToByteArray(Int32 I32)
        {
            return BitConverter.GetBytes(I32);
        }

        public static byte[] ConvertInt16ToByteArray(Int16 I16)
        {
            return BitConverter.GetBytes(I16);
        }
    }


}