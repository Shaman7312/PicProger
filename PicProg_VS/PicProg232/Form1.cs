using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace PicProg232
{
    public partial class Form1 : Form
    {
        public dataTable Firmware = new dataTable();
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                using (FileStream fs = File.Open(openFileDialog1.FileName, FileMode.Open))
                {
                    //ch = ;
                        while ((byte)fs.ReadByte() == ':' && fs.Length>0)
                    {
                        //label2.Text = String.Format("{0:X}", fs.ReadByte()) + String.Format("{0:X}", fs.ReadByte());
                        int HWperstr = Convert.ToInt32("" + Char.ConvertFromUtf32(fs.ReadByte()) + Char.ConvertFromUtf32(fs.ReadByte()),16);  //Кол-во инфы
                        //label1.Text = HWperstr.ToString();
                        //int beginaddress = 0;

                       // int beginaddress = (byte)fs.ReadByte() * 16 + (byte)fs.ReadByte();
                        int address = Convert.ToInt32("" + Char.ConvertFromUtf32(fs.ReadByte()) + Char.ConvertFromUtf32(fs.ReadByte()) + Char.ConvertFromUtf32(fs.ReadByte()) + Char.ConvertFromUtf32(fs.ReadByte()), 16);  //Адресс инфы
                        address /= 2;

                        fs.ReadByte();
                        fs.ReadByte();

                        if (address == 0x2007)
                        {
                            string LowerData = "" + Char.ConvertFromUtf32(fs.ReadByte()) + Char.ConvertFromUtf32(fs.ReadByte());
                            string HigherData = "" + Char.ConvertFromUtf32(fs.ReadByte()) + Char.ConvertFromUtf32(fs.ReadByte());
                            Firmware.config1 = Convert.ToInt32(HigherData + LowerData, 16);
                        } else
                            if (address == 0x2008)
                        {
                            string LowerData = "" + Char.ConvertFromUtf32(fs.ReadByte()) + Char.ConvertFromUtf32(fs.ReadByte());
                            string HigherData = "" + Char.ConvertFromUtf32(fs.ReadByte()) + Char.ConvertFromUtf32(fs.ReadByte());
                            Firmware.config2 = Convert.ToInt32(HigherData + LowerData, 16);
                        }
                        else
                            if (address < 0x2000)
                            while (HWperstr > 0)
                        {
                            string LowerData = "" + Char.ConvertFromUtf32(fs.ReadByte()) + Char.ConvertFromUtf32(fs.ReadByte());
                            string HigherData = "" + Char.ConvertFromUtf32(fs.ReadByte()) + Char.ConvertFromUtf32(fs.ReadByte());

                            Firmware.data[address] = Convert.ToInt32(HigherData + LowerData,16);
                            Firmware.active[(address - address % 8) / 8] = true;
                            address++;
                            //читаем инфу
                            HWperstr-=2;
                        }
                        while (fs.ReadByte() != 10) {  }


                    }

                            
                }
                RefreshList();
            }

        }

        private void listBox2_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            listBox2.Items.Add("Addr + " + (char)9 +  "0000" + (char)9+ "0001" + (char)9 + "0002"
                                  + (char)9 + "0003" + (char)9 + "0004" + (char)9 + "0005"
                                  + (char)9 + "0006" + (char)9 + "0007");
            openFileDialog1.Filter = "Hex files (*.hex)|*.hex|All files (*.*)|*.*";
            openFileDialog1.FilterIndex = 1;
            openFileDialog1.RestoreDirectory = true;
            openFileDialog1.InitialDirectory = "c:\\";

            //int prob = 0x2568acff;
            //int stri = 0;
            //string st = String.Format("{0:X}", 465413);
            //stri = Convert.ToInt32(st, 16);
            //stri += 1;
            //label1.Text = stri.ToString();
            //label2.Text = st;
            //this.Text = ((byte)prob).ToString();
            serialPort1.Open();


            for (int i = 0; i < 1024; i++)
            {
                listBox1.Items.Add(String.Format("{0:X4} |", i*8));
            }
            RefreshList();
            serialPort1.DiscardInBuffer();
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void button5_Click(object sender, EventArgs e)
        {
            bool isRowClear = false;
            serialPort1.DiscardInBuffer();
            byte[] command = { 85 , 18, 4, 93, 68, 6, 170, 18, 0, 13, 53 };
            serialPort1.Write(command, 0, 1);
            System.Threading.Thread.Sleep(1000);
            int rowCounter = 0;
            //while (isRowClear == false)
            while (rowCounter < 260)
            {
                isRowClear = true;
                for (int i = 0; i < 8; i++)
                {

                    serialPort1.Write(command, 1, 3);
                    int readed = 0;
                    int info = 0;
                    int sec = DateTime.Now.Second;
                    
                    while (readed != 2 && DateTime.Now.Second - sec < 3)
                    {
                        if (serialPort1.BytesToRead > 0)
                        {
                            byte DataByte = (byte)(serialPort1.ReadByte());
                            if (readed == 0)
                                info = DataByte;
                            else if (readed == 1)
                                info += DataByte * 256;
                            readed++;
                        }
                        else
                            System.Threading.Thread.Sleep(10);

                    }
                    serialPort1.Write(command, 4, 2);
                    Firmware.data[rowCounter*8 + i] = info;
                    if (info != 0x3FFF) isRowClear = false;
                }
                if (!isRowClear) Firmware.active[rowCounter] = true;
                else Firmware.active[rowCounter] = false;
                rowCounter++;

                /*while (Firmware.active[rowCounter])
                {
                    for (int i = 0; i < 8; i++)
                    {
                        Firmware.data[rowCounter * 8 + i] = 0x3FFF;
                    }
                    Firmware.active[rowCounter] = false;
                    rowCounter++;
                }*/
            }
            //info /= 2;

            serialPort1.Write(command, 7, 3);          // Читаем конфиг
            byte[] dummyData = { 10, 10 };
            serialPort1.Write(dummyData, 0, 2);
            serialPort1.Write(command, 10, 1);
            System.Threading.Thread.Sleep(6);

            for (int i = 0; i < 7; i++)
            {
                serialPort1.Write(command, 4, 2);
                System.Threading.Thread.Sleep(6);
            }

            serialPort1.Write(command, 1, 3);
            System.Threading.Thread.Sleep(150);
            Firmware.config1 = serialPort1.ReadByte();
            Firmware.config1 += serialPort1.ReadByte() * 256;

            serialPort1.Write(command, 4, 2);
            serialPort1.Write(command, 1, 3);
            System.Threading.Thread.Sleep(150);
            Firmware.config2 = serialPort1.ReadByte();
            Firmware.config2 += serialPort1.ReadByte() * 256;

            serialPort1.Write(command, 6, 1);

            
            RefreshList();
        }



        public void RefreshList()
        {
            //Обновление таблицы
            //int i = 0;
            for (int i = 0; i < 1024; i++)      //   Вернуть i < 1024
            {
                //listBox1.Items.Add("");
                listBox1.Items[i] = String.Format("{0:X4} |", i * 8);
                for (int j = 0; j < 8; j++)
                {

                    listBox1.Items[i] += (char)9 + String.Format("{0:X4}", Firmware.data[i * 8 + j]);
                }
                //i++;
            }
            textBox1.Text = String.Format("{0:X4}", Firmware.config1);
            textBox2.Text = String.Format("{0:X4}", Firmware.config2);
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            serialPort1.Close();
        }

        private void button8_Click(object sender, EventArgs e)
        {
            byte[] command = { 10 };
            serialPort1.Write(command, 0, 1);
            System.Threading.Thread.Sleep(3);
            serialPort1.Write(command, 0, 1);
            System.Threading.Thread.Sleep(3);
            serialPort1.Write(command, 0, 1);
            System.Threading.Thread.Sleep(3);
            serialPort1.DiscardInBuffer();
            System.Threading.Thread.Sleep(3);
            serialPort1.Write(command, 0, 1);

            System.Threading.Thread.Sleep(3);
            if (serialPort1.BytesToRead > 0)
            {
                byte DataByte = (byte)(serialPort1.ReadByte());
                if (DataByte == 51) MessageBox.Show("Программатор обнаружен!", "Проверка соединения", MessageBoxButtons.OK);
                else MessageBox.Show("Нет соединения с программатором!", "Проверка соединения", MessageBoxButtons.OK);
            } else
                MessageBox.Show("Нет соединения с программатором!", "Проверка соединения", MessageBoxButtons.OK);


        }

        private void button9_Click(object sender, EventArgs e)
        {
            //serialPort1.StopBits = System.IO.Ports.StopBits.Two;
            byte[] command = { 85, 18, 2, 13, 53, 68 , 8, 170 };
            serialPort1.Write(command, 0, 1);
            System.Threading.Thread.Sleep(1500);
            serialPort1.Write(command, 1, 3);
            
            //serialPort1.Write(command, 2, 1);
            
            //serialPort1.Write(command, 3, 1);
            
            //data
            byte[] data = { 20, 40 };       // 0020
            serialPort1.Write(data, 0, 2);
            
            //serialPort1.Write(data, 1, 1);
            

            serialPort1.Write(command, 4, 1);
            System.Threading.Thread.Sleep(5);
            serialPort1.Write(command, 5, 2);
            
            //serialPort1.Write(command, 6, 1);
            
            


            System.Threading.Thread.Sleep(100);
            serialPort1.Write(command, 7, 1);
            //serialPort1.StopBits = System.IO.Ports.StopBits.One;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            byte[] command = { 85, 68, 9, 170, 68, 0 };
            serialPort1.Write(command, 0, 1);
            System.Threading.Thread.Sleep(1000);
            serialPort1.Write(command, 1, 2);
            System.Threading.Thread.Sleep(50);

            serialPort1.Write(command, 4, 2);
            System.Threading.Thread.Sleep(50);
            serialPort1.Write(command, 1, 2);
            System.Threading.Thread.Sleep(50);


            serialPort1.Write(command, 3, 1);


        }

        private void button4_Click(object sender, EventArgs e)
        {
            int lastActive = 0;

            for (int i = 0; i < 1024; i++)
                if (Firmware.active[i])
                    lastActive = i;

            byte[] command = { 85, 18, 2, 13, 53, 68, 8, 170, 68, 6 , 18, 0};
            serialPort1.Write(command, 0, 1);
            System.Threading.Thread.Sleep(1500);
            int rowCounter = 0;
            while (rowCounter<=lastActive)
            {

                for (int i = 0; i < 8; i++)
                {
                    //if (Firmware.data[rowCounter * 8 + i] != 0x3FFF)
                    serialPort1.Write(command, 1, 3);
                    byte[] data = { (byte)(Firmware.data[rowCounter*8+i]%256), (byte)(Firmware.data[rowCounter * 8 + i] / 256) };       // 0020
                    serialPort1.Write(data, 0, 2);
                    serialPort1.Write(command, 4, 1);
                    System.Threading.Thread.Sleep(5);
                    if (i < 7)
                    serialPort1.Write(command, 8, 2);
                }
                System.Threading.Thread.Sleep(5);
                serialPort1.Write(command, 5, 2);
                System.Threading.Thread.Sleep(10);
                serialPort1.Write(command, 8, 2);
                rowCounter++;
            }

            //command = new byte[] {  };
            serialPort1.Write(command, 10, 2);
            serialPort1.Write(command, 3, 1);
            byte[] dataconf = { (byte)(Firmware.config1 % 256), (byte)(Firmware.config1 / 256) };
            serialPort1.Write(dataconf, 0, 2);

            serialPort1.Write(command, 4, 1);
            System.Threading.Thread.Sleep(5);
            for (int i = 0; i < 7; i++)
            {
                serialPort1.Write(command, 8, 2);
                System.Threading.Thread.Sleep(5);
            }


            serialPort1.Write(command, 1, 3);
            dataconf = new byte[] { (byte)(Firmware.config1 % 256), (byte)(Firmware.config1 / 256) };
            serialPort1.Write(dataconf, 0, 2);
            serialPort1.Write(command, 4, 1);
            System.Threading.Thread.Sleep(5);
            serialPort1.Write(command, 5, 2);
            System.Threading.Thread.Sleep(100);

            serialPort1.Write(command, 8, 2);

            serialPort1.Write(command, 1, 3);
            dataconf = new byte[] { (byte)(Firmware.config2 % 256), (byte)(Firmware.config2 / 256) };
            serialPort1.Write(dataconf, 0, 2);
            serialPort1.Write(command, 4, 1);
            System.Threading.Thread.Sleep(5);
            serialPort1.Write(command, 5, 2);
            System.Threading.Thread.Sleep(100);


            serialPort1.Write(command, 7, 1);
        }

        private void button6_Click(object sender, EventArgs e)
        {
            bool isRowClear = false;
            serialPort1.DiscardInBuffer();
            byte[] command = { 85, 18, 4, 93, 68, 6, 170, 18, 0, 13, 53 };
            serialPort1.Write(command, 0, 1);
            System.Threading.Thread.Sleep(1000);
            int rowCounter = 0;
            int counter = 0;
            while (isRowClear == true && counter < 8191)
            {
                isRowClear = true;
                for (int i = 0; i < 8; i++)
                {

                    serialPort1.Write(command, 1, 3);
                    int readed = 0;
                    int info = 0;
                    int sec = DateTime.Now.Second;

                    while (readed != 2 && DateTime.Now.Second - sec < 3)
                    {
                        if (serialPort1.BytesToRead > 0)
                        {
                            byte DataByte = (byte)(serialPort1.ReadByte());
                            if (readed == 0)
                                info = DataByte;
                            else if (readed == 1)
                                info += DataByte * 256;
                            readed++;
                        }
                        else
                            System.Threading.Thread.Sleep(10);

                    }
                    counter++;
                    serialPort1.Write(command, 4, 2);
                    if (info != 0x3FFF) isRowClear = false;
                }
                rowCounter++;
            }
            if (!isRowClear) MessageBox.Show("Память микросхемы НЕ чиста" + (char)13 + (char)10 + "Адрес: " + String.Format("{0:X4}", counter/8), "Проверка чистоты.", MessageBoxButtons.OK);
            else MessageBox.Show("Память микросхемы чиста", "Проверка чистоты.", MessageBoxButtons.OK);
        }




        private void button7_Click(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {

        }
    }



    public class dataTable
    {
        public int[] data;
        public bool[] active;
        public int config1;
        public int config2;

        public dataTable()
        {
            data = new int[8192];
            active = new bool[1024];
            for (int i = 0; i < 1024; i++)
                active[i] = false;

            for (int i = 0; i < 8192; i++)
                data[i] = 0x3FFF;
            config1 = 0x3fff;
            config2 = 0x3fff;
        }

        public void Clear()
        {
            for (int i = 0; i < 1024; i++)
                active[i] = false;

            for (int i = 0; i < 8; i++)
                for (int j = 0; j < 1024; j++)
                    data[i*j] = 0x3FFF;
            config1 = 0x3fff;
            config2 = 0x3fff;
        }


    }
}
