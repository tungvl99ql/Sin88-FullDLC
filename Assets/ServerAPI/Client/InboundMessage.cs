using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Core.Server.Api;

namespace Core.Server.Api
{


    public class InBoundMessage
    {
        public UnExpectMessage error;
        private byte[] reply;
        private string firstTwoBytes;
        public InBoundMessage(byte[] reply)
        {
            this.firstTwoBytes = reply[0] + reply[1] + "";
            this.reply = new byte[reply.Length - 2]; //older
            Array.ConstrainedCopy(reply, 2, this.reply, 0, reply.Length - 2);

        }

        public void CheckError()
        {

            int code = readByte(true);
            bool hasError = (code == -1);
            if (hasError)
            {
                App.printBytesArray(reply);
                readByte();
                var content = string.Empty;
                try
                {
                    content = readStrings();
                }
                catch (Exception)
                {
                    content = "SERVER MESSAGE: General Error... we don't know...";
                    throw;
                }
                error = new UnExpectMessage("ERROR CODE", content);
                //App.trace(" UNEXPECTED MESSSAGE: " + error.errorContent);
            }
        }

        public string getFirstTwoBytes()
        {
            return this.firstTwoBytes;
        }
        public long readLong()
        {
            byte[] bytes = new byte[8];
            //App.trace("=============");
            //print();
            Array.ConstrainedCopy(this.reply, 0, bytes, 0, 8); //COPPY 8 byte de giai ma so "long" gom 1 byte 0 bat dau va 8 byte chua du lieu

            byte[] temp = this.reply;
            this.reply = new byte[this.reply.Length - 8];
            Array.ConstrainedCopy(temp, 8, this.reply, 0, temp.Length - 8); //Tra lai chuoi byte con lai (- 8 byte da ma hoa) sau khi da ma hoa

            byte[] rs = new byte[bytes.Length];
            for (int i = 0; i < bytes.Length; i++)
            {
                rs[i] = bytes[bytes.Length - (i + 1)];
            }

            return BitConverter.ToInt64(rs, 0);
        }

        public int readShort()
        {
            byte[] bytes = new byte[2];
            Array.ConstrainedCopy(this.reply, 0, bytes, 0, 2); //COPPY 4 byte de giai ma so "long" gom 1 byte 0 bat dau va 8 byte chua du lieu

            byte[] temp = this.reply;
            this.reply = new byte[temp.Length - 2];
            Array.ConstrainedCopy(temp, 2, this.reply, 0, temp.Length - 2); //Tra lai chuoi byte con lai (- 4 byte da ma hoa) sau khi da ma hoa

            byte[] rs = new byte[bytes.Length];
            for (int i = 0; i < bytes.Length; i++)
            {
                rs[i] = (byte)bytes[bytes.Length - (i + 1)];
            }
            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes);

            return BitConverter.ToInt16(rs, 0);
        }

        public int readInt()
        {
            byte[] bytes = new byte[4];
            Array.ConstrainedCopy(this.reply, 0, bytes, 0, 4); //COPPY 4 byte de giai ma so "long" gom 1 byte 0 bat dau va 8 byte chua du lieu

            byte[] temp = this.reply;
            this.reply = new byte[this.reply.Length - 4];
            Array.ConstrainedCopy(temp, 4, this.reply, 0, temp.Length - 4); //Tra lai chuoi byte con lai (- 4 byte da ma hoa) sau khi da ma hoa

            byte[] rs = new byte[bytes.Length];
            for (int i = 0; i < bytes.Length; i++)
            {
                rs[i] = bytes[bytes.Length - (i + 1)];
            }

            return BitConverter.ToInt32(rs, 0);
        }


        public String readString(bool print = false)
        {
            /*
             int n = this.reply[1];
             byte[] temp = this.reply;
             if (n == 0)
             {
                 //App.trace("khong co ky tu");
                 this.reply = new byte[this.reply.Length - 1];
                 Array.ConstrainedCopy(temp, 2, this.reply, 0, temp.Length - 2);
                 return "";
             }
             byte[] bytes = new byte[n * 2];
             Array.ConstrainedCopy(this.reply, 3, bytes, 0, bytes.Length - 1); // COPPY chuoi byte de giai ma "string" gom 1 byte "do dai = N" + chuoi "N" bytes chua du lieu
                                                                               //Debug.Log("GIAI MA STRING");
                                                                               //printBytesArray(bytes);

             //Tra lai chuoi byte con lai (- N byte da ma hoa) sau khi da ma hoa

             //0 1 2 3 4 5 6 7 8 9 10
             //0 4 1 1 1 1 1 1 1 1
             this.reply = new byte[this.reply.Length - n * 2 - 2];
             Array.ConstrainedCopy(temp, n * 2 + 2, this.reply, 0, temp.Length - n * 2 - 2);

             bytes[bytes.Length - 1] = (byte)0;
             change30(bytes, 30);
             change30(bytes, 1);
             //Debug.Log("SAU KHI DOI 30");
             //printBytesArray(bytes);
             string rs = System.Text.Encoding.Unicode.GetString(bytes);
             return rs;
             */

            /*
            int n = this.reply[1];
            byte[] temp = this.reply;
            if (n == 0)
            {
                //App.trace("khong co ky tu");
                this.reply = new byte[this.reply.Length - 1];
                Array.ConstrainedCopy(temp, 2, this.reply, 0, temp.Length - 2);
                return "";
            }

            byte[] bytes = new byte[n * 2];
            //printBytesArray(bytes);
            //App.trace(n.ToString() + "|" + temp.Length);
            //printBytesArray(this.reply);
            Array.ConstrainedCopy(this.reply, 2, bytes, 0, n * 2); // COPPY chuoi byte de giai ma "string" gom 1 byte "do dai = N" + chuoi "N" bytes chua du lieu
            change30(bytes, 0);
            //change30(bytes, 1);
            //change30(bytes, 30);
            Array.ConstrainedCopy(bytes, 1, bytes, 0, n * 2 - 1);
            bytes[bytes.Length - 1] = 0;
            //printBytesArray(bytes);

            //Tra lai chuoi byte con lai (- N byte da ma hoa) sau khi da ma hoa

            this.reply = new byte[temp.Length - n * 2 - 2];
            Array.ConstrainedCopy(temp, n * 2 + 2, this.reply, 0, temp.Length - n * 2 - 2);
            //App.trace("DO DAI SAU KHI COP = " + reply.Length);
            string rs = System.Text.Encoding.Unicode.GetString(bytes);
            return rs;
            */

            /*
            int n = readShort() * 2;
            //int n = this.reply[1];

            byte[] temp = this.reply;
            if (n == 0)
            {
                //App.trace("khong co ky tu");
                this.reply = new byte[this.reply.Length - 1];
                Array.ConstrainedCopy(temp, 0, this.reply, 0, temp.Length);
                return "";
            }

            byte[] bytes = new byte[n];
            //printBytesArray(bytes);
            //App.trace(n.ToString() + "|" + temp.Length);
            //printBytesArray(this.reply);
            Array.ConstrainedCopy(this.reply, 0, bytes, 0, n); // COPPY chuoi byte de giai ma "string" gom 1 byte "do dai = N" + chuoi "N" bytes chua du lieu
            change30(bytes, 0);
            //change30(bytes, 1);
            //change30(bytes, 30);
            Array.ConstrainedCopy(bytes, 1, bytes, 0, n - 1);
            bytes[bytes.Length - 1] = 0;
            //printBytesArray(bytes);

            //Tra lai chuoi byte con lai (- N byte da ma hoa) sau khi da ma hoa

            this.reply = new byte[temp.Length - n];
            Array.ConstrainedCopy(temp, n, this.reply, 0, temp.Length - n);
            //App.trace("DO DAI SAU KHI COP = " + reply.Length);
            string rs = System.Text.Encoding.Unicode.GetString(bytes, 0, bytes.Length);

            return rs;
            */
            
            return readStrings();
        }
        /// <summary>
        /// Bản nâng cấp của readString (Nên dùng)
        /// </summary>
        public String readStrings(bool printArr = false)
        {
            int n = readShort() * 2;
            //int n = this.reply[1];

            byte[] temp = this.reply;
            if (n == 0)
            {
                //App.trace("khong co ky tu");
                this.reply = new byte[this.reply.Length + 1];
                Array.ConstrainedCopy(temp, 0, this.reply, 0, temp.Length);
                return "";
            }

            byte[] bytes = new byte[n + 1];

            //App.trace(n.ToString() + "|" + temp.Length);
            //printBytesArray(this.reply);
            Array.ConstrainedCopy(this.reply, 0, bytes, 0, n); // COPPY chuoi byte de giai ma "string" gom 1 byte "do dai = N" + chuoi "N" bytes chua du lieu
            if (printArr)
                printBytesArray(bytes);
            //bytes[n - 1] = 0;
            change30(bytes, 0);
            Array.ConstrainedCopy(bytes, 1, bytes, 0, n);


            if (printArr)
                printBytesArray(bytes);

            //Tra lai chuoi byte con lai (- N byte da ma hoa) sau khi da ma hoa

            this.reply = new byte[temp.Length - n];
            Array.ConstrainedCopy(temp, n, this.reply, 0, temp.Length - n);
            //App.trace("DO DAI SAU KHI COP = " + reply.Length);
            string rs = System.Text.Encoding.Unicode.GetString(bytes, 0, bytes.Length);
           // rs = rs.Replace("\ufffd", ""); // fix .Net 3.5 up to 4.5
            if (printArr)
                App.trace(bytes.Length.ToString());

            if (rs != null && rs.Length > 1)
            {
                string newString = rs.Remove(rs.Length - 1);
                return newString;
            }
            return rs;
        }


        //Doi cho byte 30 de giai ma Unicode
        public void change30(byte[] bytes, byte a)
        {
            for (int i = bytes.Length - 1; i > 0; i--)
            {
                if (bytes[i] == a && i < bytes.Length)
                {
                    /*
                    bytes[i + 2] = a;
                    bytes[i] = 0;
                    */
                    if (i > 1)
                    {
                        byte temp = bytes[i - 2];
                        bytes[i - 2] = bytes[i];
                        bytes[i] = (byte)temp;
                        //i -= 2;
                    }
                }
            }
        }

        public String readAscii()
        {
            int n = this.reply[0];
            byte[] bytes = new byte[n];
            Array.ConstrainedCopy(this.reply, 1, bytes, 0, bytes.Length); // COPPY chuoi byte de giai ma "string" gom 1 byte "do dai = N" + chuoi "N" bytes chua du lieu

            //Tra lai chuoi byte con lai (- N byte da ma hoa) sau khi da ma hoa
            byte[] temp = this.reply;
            if (n == 0)
            {
                this.reply = new byte[this.reply.Length - 1];
                Array.ConstrainedCopy(temp, 1, this.reply, 0, temp.Length - 1);
            }
            else
            {
                this.reply = new byte[this.reply.Length - n - 1];
                Array.ConstrainedCopy(temp, n + 1, this.reply, 0, temp.Length - n - 1);
            }

            return System.Text.Encoding.ASCII.GetString((bytes));
        }

        public int readByte(bool cutByte = false)
        {
            int rs = this.reply[0];

            sbyte m_rs = (sbyte)rs;

            byte[] temp = this.reply; //Tra lai chuoi byte sau khi lay 1 byte da giai ma
            if (cutByte == false)
            {
                this.reply = new byte[this.reply.Length - 1];
                Array.ConstrainedCopy(temp, 1, this.reply, 0, temp.Length - 1);
            }
            return m_rs;
        }

        public List<int> readBytes()
        {
            //this.print();
            List<int> rsList = new List<int>();
            int lengthA = readShort();

            int rs = 0;
            sbyte m_res = 0;
            for (int i = 0; i < lengthA; i++)
            {
                rs = this.reply[i];
                m_res = (sbyte)rs;
                rsList.Add(m_res);
            }

            byte[] temp = this.reply; //Tra lai chuoi byte sau khi lay 1 byte da gia ma
            this.reply = new byte[temp.Length - lengthA];
            Array.ConstrainedCopy(temp, lengthA, this.reply, 0, temp.Length - lengthA);
            return rsList;
        }

        public List<string> readStringArray()
        {
            List<string> rsList = new List<string>();
            int length = readShort();
            for (int i = 0; i < length; i++)
            {
                rsList.Add(readString());
            }
            return rsList;
        }

        public byte[] ConvertInt64ToByteArray(Int64 I64)
        {
            return BitConverter.GetBytes(I64);
        }

        public void print()
        {
            String t = "";
            for (int i = 0; i < this.reply.Length; i++)
            {
                //Debug.Log(req[i] +",");
                t += this.reply[i] + ",";
            }
            Debug.Log("CHUOI BYTE = " + t);
        }
        void printBytesArray(byte[] req)
        {
            String t = "";
            if (req == null || req.Length == 0)
            {
                t = "KHONG CO DATA";
            }
            else
            {
                for (int i = 0; i < req.Length; i++)
                {
                    t += req[i] + ",";
                }
            }
            //Debug.Log("IN RA NE" + t);
        }

        public int getLength()
        {
            return this.reply.Length;
        }

        public byte[] getData()
        {
            byte[] sss = new byte[reply.Length + 2];
            sss[0] = sss[1] = 0;
            Array.ConstrainedCopy(this.reply, 0, sss, 2, reply.Length);
            return sss;
        }

    }

}