﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

using Telerik.WinControls;
using Telerik.WinControls.UI;


namespace TelerikWinFormsApp4
{
    public partial class main_page : Telerik.WinControls.UI.RadForm
    {
        public main_page()
        {
            InitializeComponent();
          
        }


        private int get_key_length()
        {
            if (radRadioButton1.IsChecked == true)
            {
                return 4;
            }
            else if (radRadioButton2.IsChecked == true)
            {
                return 6;
            }
            else if (radRadioButton3.IsChecked == true)
            {
                return 8;
            }
            else

                return 0;
        }


        private void btn_encrypt_Click(object sender, EventArgs e)
        {
           

            tb_encoutput.Text = "";
            //get the key    
            string key = tb_enckey.Text;
            //get the encryption key length
            int key_length = get_key_length();

            byte[] key_array = key_parsing(key_length, key);
            string data = tb_encinput.Text;
   
                AES_Text(data, key_array, key_length);
           // radProgressBar1.Value2 =100;

        }



        private byte[] key_parsing(int key_length, string key)
        {
            
            byte[] key_array = new byte[key_length * 4];
            int key_it = 0;
            for (int j = 0; j < key_length * 8; j += 2)
            {
                try {
                    string temp = "" + key[j] + key[j + 1];
                    int val = int.Parse(temp, System.Globalization.NumberStyles.HexNumber);
                    key_array[key_it] = (byte)val;
                    key_it++;
                }

                catch
                {
          
                    RadMessageBox.ThemeName = btn_enc_file.ThemeName;
                    RadMessageBox.Instance.MinimumSize = new System.Drawing.Size(100, 100);
                    DialogResult ds = RadMessageBox.Show(this, " Incomplete or Empty Key Entries! Misleading Output Expected ", " Key Error ", MessageBoxButtons.RetryCancel, RadMessageIcon.Error);
                    
                    
                    break;
                }



            }

            return key_array;
        }

        private void AES_Text(string data, byte[] key_array, int key_length)
        {
            //space padding
            while (data.Length % 16 != 0)
                data = data + " ";

            for (int i = 0; i < data.Length / 16; ++i)
            {
                //input parsing
                int it = 0;
                byte[] data_array = new byte[16];
                for (int j = 16 * i; j < 16 * (i + 1); ++j)
                {
                    data_array[it] = Convert.ToByte(data[j]);
                    it++;
                }

                int newit = 0;
                byte[,] state = new byte[4, 4];
                for (int x = 0; x < 4; x++)
                {
                    for (int y = 0; y < 4; y++)
                    {
                        state[x, y] = data_array[newit];
                        newit++;
                    }
                }
                // ------ end input parsing

                state = AES_Encryptor(state, key_array, key_length);

                for (int x = 0; x < 4; x++)
                {
                    for (int y = 0; y < 4; y++)
                    {
                        tb_encoutput.Text = tb_encoutput.Text + state[x, y].ToString("X2");
                    }
                }
            }

        }

        private byte[,] AES_Encryptor(byte[,] state, byte[] key_array, int key_length)
        {
            //AES_core
            int rounds = key_length + 6;
            byte[] exp_key = ExpandKey(key_array, key_length);

            state = AddRoundKey(state, exp_key, 0);

            for (int i = 1; i < rounds; ++i)
            {
                state = SubBytes(state);
                state = ShiftRows(state);
                state = MixColumns(state);
                state = AddRoundKey(state, exp_key, i);
                //state = NewModifiction(state);
            }
            state = SubBytes(state);
            state = ShiftRows(state);
            state = AddRoundKey(state, exp_key, rounds); 
            state = NewModifiction(state);
           
            return state;
        }

        //New modification to the AES steps
        private byte[,] NewModifiction(byte[,] state)
        {

            for (int i = 0; i < 4; ++i)
            {
                for (int j = 0; j < 4; ++j)
                {
                    state[j, i] ^= ModiSubByte[j, i];          //sub bytes
                    Console.WriteLine("MOd1 " + state[j, i]);
                    state[j, i] ^= ModiShiftRow[j, i];       //shift rows
                    Console.WriteLine("MOD2 " + state[j, i]);
                 //   state[j, i] ^= ModiMixCol[j, i];      //mix columns
                   // Console.WriteLine("MOD3 " + state[j, i]);
                }
            }

            return state;
        }

        //Inverting 
        private byte[,] InvNewModifiction(byte[,] state)
        {

            for (int i = 0; i < 4; ++i)
            {
                for (int j = 0; j < 4; ++j)
                {

                    //  state[j, i] ^= ModiMixCol[j, i];      //mix columns
                    // Console.WriteLine("MOD3 " + state[j, i]);
                    state[j, i] ^= ModiShiftRow[j, i];       //shift rows
                    Console.WriteLine("MOD2 " + state[j, i]);
                    state[j, i] ^= ModiSubByte[j, i];          //sub bytes
                    Console.WriteLine("MOd1 " + state[j, i]);


                }
            }

            return state;
        }

        private byte[] ExpandKey(byte[] key_block, int key_length)
        {
            //expanded key array 
            byte[] exp_key = new byte[4 * key_length * (key_length + 6 + 1)];
            //copy initial key
            for (int i = 0; i < key_block.Length; ++i)
                exp_key[i] = key_block[i];
            //decl
            int bytes = 4 * key_length;
            int my_It = 1;
            byte[] temp_array = new byte[4];

            while (bytes < 4 * key_length * (key_length + 6 + 1))
            {
                for (int i = 0; i < 4; ++i)
                    temp_array[i] = exp_key[bytes - (4 - i)];

                if (bytes % (4 * key_length) == 0)
                    temp_array = ExpandKey_Ext(temp_array, my_It++, key_length, 0);
                else if (key_length > 6 && bytes % (4 * key_length) == 16)
                    temp_array = ExpandKey_Ext(temp_array, 0, key_length, 1);

                for (int i = 0; i < 4; ++i)
                {
                    exp_key[bytes] = (byte)(exp_key[bytes - 4 * key_length] ^ temp_array[i]);
                    bytes++;
                }
            }
            return exp_key;
        }

        private byte[] ExpandKey_Ext(byte[] temp_array, int val, int key_length, int flag)
        {
            byte[] SBox =
            {
                0x63, 0x7C, 0x77, 0x7B, 0xF2, 0x6B, 0x6F, 0xC5, 0x30, 0x01, 0x67, 0x2B, 0xFE, 0xD7, 0xAB, 0x76,
                0xCA, 0x82, 0xC9, 0x7D, 0xFA, 0x59, 0x47, 0xF0, 0xAD, 0xD4, 0xA2, 0xAF, 0x9C, 0xA4, 0x72, 0xC0, 0xB7,
                0xFD, 0x93, 0x26, 0x36, 0x3F, 0xF7, 0xCC, 0x34, 0xA5, 0xE5, 0xF1, 0x71, 0xD8, 0x31, 0x15, 0x04, 0xC7,
                0x23, 0xC3, 0x18, 0x96, 0x05, 0x9A, 0x07, 0x12, 0x80, 0xE2, 0xEB, 0x27, 0xB2, 0x75, 0x09, 0x83, 0x2C,
                0x1A, 0x1B, 0x6E, 0x5A, 0xA0, 0x52, 0x3B, 0xD6, 0xB3, 0x29, 0xE3, 0x2F, 0x84, 0x53, 0xD1, 0x00, 0xED,
                0x20, 0xFC, 0xB1, 0x5B, 0x6A, 0xCB, 0xBE, 0x39, 0x4A, 0x4C, 0x58, 0xCF, 0xD0, 0xEF, 0xAA, 0xFB, 0x43,
                0x4D, 0x33, 0x85, 0x45, 0xF9, 0x02, 0x7F, 0x50, 0x3C, 0x9F, 0xA8, 0x51, 0xA3, 0x40, 0x8F, 0x92, 0x9D,
                0x38, 0xF5, 0xBC, 0xB6, 0xDA, 0x21, 0x10, 0xFF, 0xF3, 0xD2, 0xCD, 0x0C, 0x13, 0xEC, 0x5F, 0x97, 0x44,
                0x17, 0xC4, 0xA7, 0x7E, 0x3D, 0x64, 0x5D, 0x19, 0x73, 0x60, 0x81, 0x4F, 0xDC, 0x22, 0x2A, 0x90, 0x88,
                0x46, 0xEE, 0xB8, 0x14, 0xDE, 0x5E, 0x0B, 0xDB, 0xE0, 0x32, 0x3A, 0x0A, 0x49, 0x06, 0x24, 0x5C, 0xC2,
                0xD3, 0xAC, 0x62, 0x91, 0x95, 0xE4, 0x79, 0xE7, 0xC8, 0x37, 0x6D, 0x8D, 0xD5, 0x4E, 0xA9, 0x6C, 0x56,
                0xF4, 0xEA, 0x65, 0x7A, 0xAE, 0x08, 0xBA, 0x78, 0x25, 0x2E, 0x1C, 0xA6, 0xB4, 0xC6, 0xE8, 0xDD, 0x74,
                0x1F, 0x4B, 0xBD, 0x8B, 0x8A, 0x70, 0x3E, 0xB5, 0x66, 0x48, 0x03, 0xF6, 0x0E, 0x61, 0x35, 0x57, 0xB9,
                0x86, 0xC1, 0x1D, 0x9E, 0xE1, 0xF8, 0x98, 0x11, 0x69, 0xD9, 0x8E, 0x94, 0x9B, 0x1E, 0x87, 0xE9, 0xCE,
                0x55, 0x28, 0xDF, 0x8C, 0xA1, 0x89, 0x0D, 0xBF, 0xE6, 0x42, 0x68, 0x41, 0x99, 0x2D, 0x0F, 0xB0, 0x54,
                0xBB, 0x16
            };

            byte[] rcon =                                    //rcon name change
            {
                0x8d, 0x01, 0x02, 0x04, 0x08, 0x10, 0x20, 0x40, 0x80, 0x1b, 0x36, 0x6c, 0xd8, 0xab, 0x4d, 0x9a,
                0x2f, 0x5e, 0xbc, 0x63, 0xc6, 0x97, 0x35, 0x6a, 0xd4, 0xb3, 0x7d, 0xfa, 0xef, 0xc5, 0x91, 0x39, 0x72,
                0xe4, 0xd3, 0xbd, 0x61, 0xc2, 0x9f, 0x25, 0x4a, 0x94, 0x33, 0x66, 0xcc, 0x83, 0x1d, 0x3a, 0x74, 0xe8,
                0xcb, 0x8d, 0x01, 0x02, 0x04, 0x08, 0x10, 0x20, 0x40, 0x80, 0x1b, 0x36, 0x6c, 0xd8, 0xab, 0x4d, 0x9a,
                0x2f, 0x5e, 0xbc, 0x63, 0xc6, 0x97, 0x35, 0x6a, 0xd4, 0xb3, 0x7d, 0xfa, 0xef, 0xc5, 0x91, 0x39, 0x72,
                0xe4, 0xd3, 0xbd, 0x61, 0xc2, 0x9f, 0x25, 0x4a, 0x94, 0x33, 0x66, 0xcc, 0x83, 0x1d, 0x3a, 0x74, 0xe8,
                0xcb, 0x8d, 0x01, 0x02, 0x04, 0x08, 0x10, 0x20, 0x40, 0x80, 0x1b, 0x36, 0x6c, 0xd8, 0xab, 0x4d, 0x9a,
                0x2f, 0x5e, 0xbc, 0x63, 0xc6, 0x97, 0x35, 0x6a, 0xd4, 0xb3, 0x7d, 0xfa, 0xef, 0xc5, 0x91, 0x39, 0x72,
                0xe4, 0xd3, 0xbd, 0x61, 0xc2, 0x9f, 0x25, 0x4a, 0x94, 0x33, 0x66, 0xcc, 0x83, 0x1d, 0x3a, 0x74, 0xe8,
                0xcb, 0x8d, 0x01, 0x02, 0x04, 0x08, 0x10, 0x20, 0x40, 0x80, 0x1b, 0x36, 0x6c, 0xd8, 0xab, 0x4d, 0x9a,
                0x2f, 0x5e, 0xbc, 0x63, 0xc6, 0x97, 0x35, 0x6a, 0xd4, 0xb3, 0x7d, 0xfa, 0xef, 0xc5, 0x91, 0x39, 0x72,
                0xe4, 0xd3, 0xbd, 0x61, 0xc2, 0x9f, 0x25, 0x4a, 0x94, 0x33, 0x66, 0xcc, 0x83, 0x1d, 0x3a, 0x74, 0xe8,
                0xcb, 0x8d, 0x01, 0x02, 0x04, 0x08, 0x10, 0x20, 0x40, 0x80, 0x1b, 0x36, 0x6c, 0xd8, 0xab, 0x4d, 0x9a,
                0x2f, 0x5e, 0xbc, 0x63, 0xc6, 0x97, 0x35, 0x6a, 0xd4, 0xb3, 0x7d, 0xfa, 0xef, 0xc5, 0x91, 0x39, 0x72,
                0xe4, 0xd3, 0xbd, 0x61, 0xc2, 0x9f, 0x25, 0x4a, 0x94, 0x33, 0x66, 0xcc, 0x83, 0x1d, 0x3a, 0x74, 0xe8,
                0xcb, 0x8d
            };

            if (flag == 0)
            {
                byte t = temp_array[0];

                temp_array[0] = temp_array[1]; temp_array[1] = temp_array[2]; temp_array[2] = temp_array[3];
                temp_array[3] = t;

                for (int i = 0; i < 4; ++i)
                    temp_array[i] = SBox[temp_array[i]];
                temp_array[0] ^= rcon[val];
                return temp_array;
            }
            else
            {
                for (int i = 0; i < 4; ++i)
                    temp_array[i] = SBox[temp_array[i]];
                return temp_array;
            }

        }
        public byte[,] ModiSubByte = new byte[4, 4];
        private byte[,] SubBytes(byte[,] state)
        {


            byte[] SBox =
                {
                0x63, 0x7C, 0x77, 0x7B, 0xF2, 0x6B, 0x6F, 0xC5, 0x30, 0x01, 0x67, 0x2B, 0xFE, 0xD7, 0xAB, 0x76,
                0xCA, 0x82, 0xC9, 0x7D, 0xFA, 0x59, 0x47, 0xF0, 0xAD, 0xD4, 0xA2, 0xAF, 0x9C, 0xA4, 0x72, 0xC0, 0xB7,
                0xFD, 0x93, 0x26, 0x36, 0x3F, 0xF7, 0xCC, 0x34, 0xA5, 0xE5, 0xF1, 0x71, 0xD8, 0x31, 0x15, 0x04, 0xC7,
                0x23, 0xC3, 0x18, 0x96, 0x05, 0x9A, 0x07, 0x12, 0x80, 0xE2, 0xEB, 0x27, 0xB2, 0x75, 0x09, 0x83, 0x2C,
                0x1A, 0x1B, 0x6E, 0x5A, 0xA0, 0x52, 0x3B, 0xD6, 0xB3, 0x29, 0xE3, 0x2F, 0x84, 0x53, 0xD1, 0x00, 0xED,
                0x20, 0xFC, 0xB1, 0x5B, 0x6A, 0xCB, 0xBE, 0x39, 0x4A, 0x4C, 0x58, 0xCF, 0xD0, 0xEF, 0xAA, 0xFB, 0x43,
                0x4D, 0x33, 0x85, 0x45, 0xF9, 0x02, 0x7F, 0x50, 0x3C, 0x9F, 0xA8, 0x51, 0xA3, 0x40, 0x8F, 0x92, 0x9D,
                0x38, 0xF5, 0xBC, 0xB6, 0xDA, 0x21, 0x10, 0xFF, 0xF3, 0xD2, 0xCD, 0x0C, 0x13, 0xEC, 0x5F, 0x97, 0x44,
                0x17, 0xC4, 0xA7, 0x7E, 0x3D, 0x64, 0x5D, 0x19, 0x73, 0x60, 0x81, 0x4F, 0xDC, 0x22, 0x2A, 0x90, 0x88,
                0x46, 0xEE, 0xB8, 0x14, 0xDE, 0x5E, 0x0B, 0xDB, 0xE0, 0x32, 0x3A, 0x0A, 0x49, 0x06, 0x24, 0x5C, 0xC2,
                0xD3, 0xAC, 0x62, 0x91, 0x95, 0xE4, 0x79, 0xE7, 0xC8, 0x37, 0x6D, 0x8D, 0xD5, 0x4E, 0xA9, 0x6C, 0x56,
                0xF4, 0xEA, 0x65, 0x7A, 0xAE, 0x08, 0xBA, 0x78, 0x25, 0x2E, 0x1C, 0xA6, 0xB4, 0xC6, 0xE8, 0xDD, 0x74,
                0x1F, 0x4B, 0xBD, 0x8B, 0x8A, 0x70, 0x3E, 0xB5, 0x66, 0x48, 0x03, 0xF6, 0x0E, 0x61, 0x35, 0x57, 0xB9,
                0x86, 0xC1, 0x1D, 0x9E, 0xE1, 0xF8, 0x98, 0x11, 0x69, 0xD9, 0x8E, 0x94, 0x9B, 0x1E, 0x87, 0xE9, 0xCE,
                0x55, 0x28, 0xDF, 0x8C, 0xA1, 0x89, 0x0D, 0xBF, 0xE6, 0x42, 0x68, 0x41, 0x99, 0x2D, 0x0F, 0xB0, 0x54,
                0xBB, 0x16
            };

            for (int i = 0; i < 4; ++i)
            {
                for (int j = 0; j < 4; ++j)
                {
                    state[j, i] = SBox[state[j, i]];
                    ModiSubByte[j, i] = state[j, i];
                }
            }

            return state;
        }

        private byte[,] ShiftRows(byte[,] state)
        {
            byte[,] temp = new byte[4, 4];
            temp[0, 0] = state[0, 0]; temp[0, 1] = state[1, 1]; temp[1, 1] = state[2, 1];
            temp[2, 1] = state[3, 1]; temp[3, 1] = state[0, 1]; temp[0, 2] = state[2, 2];
            temp[2, 2] = state[0, 2]; temp[1, 2] = state[3, 2]; temp[3, 2] = state[1, 2];
            temp[0, 3] = state[3, 3]; temp[3, 3] = state[2, 3]; temp[2, 3] = state[1, 3];
            temp[1, 3] = state[0, 3]; temp[1, 0] = state[1, 0]; temp[2, 0] = state[2, 0];
            temp[3, 0] = state[3, 0];

            ModiShiftRow[0, 1] = state[0, 1]; ModiShiftRow[1, 1] = state[1, 1]; ModiShiftRow[2, 1] = state[2, 1]; ModiShiftRow[3, 1] = state[3, 1];
            ModiShiftRow[0, 2] = state[0, 2]; ModiShiftRow[1, 2] = state[1, 2]; ModiShiftRow[2, 2] = state[2, 2]; ModiShiftRow[3, 2] = state[3, 2];
            ModiShiftRow[0, 3] = state[0, 3]; ModiShiftRow[1, 3] = state[1, 3]; ModiShiftRow[2, 3] = state[2, 3]; ModiShiftRow[3, 3] = state[3, 3];
            ModiShiftRow[0, 0] = state[0, 0]; ModiShiftRow[1, 0] = state[1, 0]; ModiShiftRow[2, 0] = state[2, 0]; ModiShiftRow[3, 0] = state[3, 0];


            return temp;
        }

        public byte[,] ModiMixCol = new byte[4, 4];
        private byte[,] MixColumns(byte[,] state)
        {
            byte[] mul2 =
            {
                0x00, 0x02, 0x04, 0x06, 0x08, 0x0a, 0x0c, 0x0e, 0x10, 0x12, 0x14, 0x16, 0x18, 0x1a, 0x1c, 0x1e,
                0x20, 0x22, 0x24, 0x26, 0x28, 0x2a, 0x2c, 0x2e, 0x30, 0x32, 0x34, 0x36, 0x38, 0x3a, 0x3c, 0x3e, 0x40,
                0x42, 0x44, 0x46, 0x48, 0x4a, 0x4c, 0x4e, 0x50, 0x52, 0x54, 0x56, 0x58, 0x5a, 0x5c, 0x5e, 0x60, 0x62,
                0x64, 0x66, 0x68, 0x6a, 0x6c, 0x6e, 0x70, 0x72, 0x74, 0x76, 0x78, 0x7a, 0x7c, 0x7e, 0x80, 0x82, 0x84,
                0x86, 0x88, 0x8a, 0x8c, 0x8e, 0x90, 0x92, 0x94, 0x96, 0x98, 0x9a, 0x9c, 0x9e, 0xa0, 0xa2, 0xa4, 0xa6,
                0xa8, 0xaa, 0xac, 0xae, 0xb0, 0xb2, 0xb4, 0xb6, 0xb8, 0xba, 0xbc, 0xbe, 0xc0, 0xc2, 0xc4, 0xc6, 0xc8,
                0xca, 0xcc, 0xce, 0xd0, 0xd2, 0xd4, 0xd6, 0xd8, 0xda, 0xdc, 0xde, 0xe0, 0xe2, 0xe4, 0xe6, 0xe8, 0xea,
                0xec, 0xee, 0xf0, 0xf2, 0xf4, 0xf6, 0xf8, 0xfa, 0xfc, 0xfe, 0x1b, 0x19, 0x1f, 0x1d, 0x13, 0x11, 0x17,
                0x15, 0x0b, 0x09, 0x0f, 0x0d, 0x03, 0x01, 0x07, 0x05, 0x3b, 0x39, 0x3f, 0x3d, 0x33, 0x31, 0x37, 0x35,
                0x2b, 0x29, 0x2f, 0x2d, 0x23, 0x21, 0x27, 0x25, 0x5b, 0x59, 0x5f, 0x5d, 0x53, 0x51, 0x57, 0x55, 0x4b,
                0x49, 0x4f, 0x4d, 0x43, 0x41, 0x47, 0x45, 0x7b, 0x79, 0x7f, 0x7d, 0x73, 0x71, 0x77, 0x75, 0x6b, 0x69,
                0x6f, 0x6d, 0x63, 0x61, 0x67, 0x65, 0x9b, 0x99, 0x9f, 0x9d, 0x93, 0x91, 0x97, 0x95, 0x8b, 0x89, 0x8f,
                0x8d, 0x83, 0x81, 0x87, 0x85, 0xbb, 0xb9, 0xbf, 0xbd, 0xb3, 0xb1, 0xb7, 0xb5, 0xab, 0xa9, 0xaf, 0xad,
                0xa3, 0xa1, 0xa7, 0xa5, 0xdb, 0xd9, 0xdf, 0xdd, 0xd3, 0xd1, 0xd7, 0xd5, 0xcb, 0xc9, 0xcf, 0xcd, 0xc3,
                0xc1, 0xc7, 0xc5, 0xfb, 0xf9, 0xff, 0xfd, 0xf3, 0xf1, 0xf7, 0xf5, 0xeb, 0xe9, 0xef, 0xed, 0xe3, 0xe1,
                0xe7, 0xe5
            };

            byte[] mul3 =
            {
                0x00, 0x03, 0x06, 0x05, 0x0c, 0x0f, 0x0a, 0x09, 0x18, 0x1b, 0x1e, 0x1d, 0x14, 0x17, 0x12, 0x11,
                0x30, 0x33, 0x36, 0x35, 0x3c, 0x3f, 0x3a, 0x39, 0x28, 0x2b, 0x2e, 0x2d, 0x24, 0x27, 0x22, 0x21, 0x60,
                0x63, 0x66, 0x65, 0x6c, 0x6f, 0x6a, 0x69, 0x78, 0x7b, 0x7e, 0x7d, 0x74, 0x77, 0x72, 0x71, 0x50, 0x53,
                0x56, 0x55, 0x5c, 0x5f, 0x5a, 0x59, 0x48, 0x4b, 0x4e, 0x4d, 0x44, 0x47, 0x42, 0x41, 0xc0, 0xc3, 0xc6,
                0xc5, 0xcc, 0xcf, 0xca, 0xc9, 0xd8, 0xdb, 0xde, 0xdd, 0xd4, 0xd7, 0xd2, 0xd1, 0xf0, 0xf3, 0xf6, 0xf5,
                0xfc, 0xff, 0xfa, 0xf9, 0xe8, 0xeb, 0xee, 0xed, 0xe4, 0xe7, 0xe2, 0xe1, 0xa0, 0xa3, 0xa6, 0xa5, 0xac,
                0xaf, 0xaa, 0xa9, 0xb8, 0xbb, 0xbe, 0xbd, 0xb4, 0xb7, 0xb2, 0xb1, 0x90, 0x93, 0x96, 0x95, 0x9c, 0x9f,
                0x9a, 0x99, 0x88, 0x8b, 0x8e, 0x8d, 0x84, 0x87, 0x82, 0x81, 0x9b, 0x98, 0x9d, 0x9e, 0x97, 0x94, 0x91,
                0x92, 0x83, 0x80, 0x85, 0x86, 0x8f, 0x8c, 0x89, 0x8a, 0xab, 0xa8, 0xad, 0xae, 0xa7, 0xa4, 0xa1, 0xa2,
                0xb3, 0xb0, 0xb5, 0xb6, 0xbf, 0xbc, 0xb9, 0xba, 0xfb, 0xf8, 0xfd, 0xfe, 0xf7, 0xf4, 0xf1, 0xf2, 0xe3,
                0xe0, 0xe5, 0xe6, 0xef, 0xec, 0xe9, 0xea, 0xcb, 0xc8, 0xcd, 0xce, 0xc7, 0xc4, 0xc1, 0xc2, 0xd3, 0xd0,
                0xd5, 0xd6, 0xdf, 0xdc, 0xd9, 0xda, 0x5b, 0x58, 0x5d, 0x5e, 0x57, 0x54, 0x51, 0x52, 0x43, 0x40, 0x45,
                0x46, 0x4f, 0x4c, 0x49, 0x4a, 0x6b, 0x68, 0x6d, 0x6e, 0x67, 0x64, 0x61, 0x62, 0x73, 0x70, 0x75, 0x76,
                0x7f, 0x7c, 0x79, 0x7a, 0x3b, 0x38, 0x3d, 0x3e, 0x37, 0x34, 0x31, 0x32, 0x23, 0x20, 0x25, 0x26, 0x2f,
                0x2c, 0x29, 0x2a, 0x0b, 0x08, 0x0d, 0x0e, 0x07, 0x04, 0x01, 0x02, 0x13, 0x10, 0x15, 0x16, 0x1f, 0x1c,
                0x19, 0x1a
            };

            byte[,] temp = new byte[4, 4];



            for (int i = 0; i < 4; i++)
            {

                temp[i, 0] = (byte)(mul2[state[i, 0]] ^ mul3[state[i, 1]] ^ state[i, 2] ^ state[i, 3]);
                temp[i, 1] = (byte)(state[i, 0] ^ mul2[state[i, 1]] ^ mul3[state[i, 2]] ^ state[i, 3]);
                temp[i, 2] = (byte)(state[i, 0] ^ state[i, 1] ^ mul2[state[i, 2]] ^ mul3[state[i, 3]]);
                temp[i, 3] = (byte)(mul3[state[i, 0]] ^ state[i, 1] ^ state[i, 2] ^ mul2[state[i, 3]]);


                ModiMixCol[i, 0] = (byte)(mul2[state[i, 0]] ^ mul3[state[i, 1]] ^ state[i, 2] ^ state[i, 3]);
                ModiMixCol[i, 1] = (byte)(state[i, 0] ^ mul2[state[i, 1]] ^ mul3[state[i, 2]] ^ state[i, 3]);
                ModiMixCol[i, 2] = (byte)(state[i, 0] ^ state[i, 1] ^ mul2[state[i, 2]] ^ mul3[state[i, 3]]);
                ModiMixCol[i, 3] = (byte)(mul3[state[i, 0]] ^ state[i, 1] ^ state[i, 2] ^ mul2[state[i, 3]]);


            }
            /*
            temp[0,0] = (byte) (mul2[state[0,0]] ^ mul3[state[0,1]] ^ state[0,2] ^ state[0,3]);
            temp[0,1] = (byte) (state[0,0] ^ mul2[state[0,1]] ^ mul3[state[0,2]] ^ state[0,3]);
            temp[0,2] = (byte) (state[0,0] ^ state[0,1] ^ mul2[state[0,2]] ^ mul3[state[0,3]]);
            temp[0,3] = (byte) (mul3[state[0,0]] ^ state[0,1] ^ state[0,2] ^ mul2[state[0,3]]);

            temp[1,0] = (byte) (mul2[state[1,0]] ^ mul3[state[1,1]] ^ state[1,2] ^ state[1,3]);
            temp[1,1] = (byte) (state[1,0] ^ mul2[state[1,1]] ^ mul3[state[1,2]] ^ state[1,3]);
            temp[1,2] = (byte) (state[1,0] ^ state[1,1] ^ mul2[state[1,2]] ^ mul3[state[1,3]]);
            temp[1,3] = (byte) (mul3[state[1,0]] ^ state[1,1] ^ state[1,2] ^ mul2[state[1,3]]);

            temp[2,0] = (byte)(mul2[state[2,0]] ^ mul3[state[2,1]] ^ state[2,2] ^ state[2,3]);
            temp[2,1] = (byte)(state[2,0] ^ mul2[state[2,1]] ^ mul3[state[2,2]] ^ state[2,3]);
            temp[2,2] = (byte)(state[2,0] ^ state[2,1] ^ mul2[state[2,2]] ^ mul3[state[2,3]]);
            temp[2,3] = (byte)(mul3[state[2,0]] ^ state[2,1] ^ state[2,2] ^ mul2[state[2,3]]);

            temp[3,0] = (byte)(mul2[state[3,0]] ^ mul3[state[3,1]] ^ state[3,2] ^ state[3,3]);
            temp[3,1] = (byte)(state[3,0] ^ mul2[state[3,1]] ^ mul3[state[3,2]] ^ state[3,3]);
            temp[3,2] = (byte)(state[3,0] ^ state[3,1] ^ mul2[state[3,2]] ^ mul3[state[3,3]]);
            temp[3,3] = (byte)(mul3[state[3,0]] ^ state[3,1] ^ state[3,2] ^ mul2[state[3,3]]);
            */

            return temp;
        }

       public byte[,] ModiAddRound = new byte[4, 4];
        private byte[,] AddRoundKey(byte[,] state, byte[] exp_key, int round)
        {
            for (int i = 0; i < 4; ++i)
            {
                for (int j = 0; j < 4; ++j)
                {
                    state[i, j] ^= exp_key[round * 16 + 4 * i + j];
                    ModiAddRound[i, j]= exp_key[round * 16 + 4 * i + j];
                }
            }

            return state;
        }

        private void btn_decrypt_Click(object sender, EventArgs e)
        {


            //clear the text box content
            tb_decoutput.Text = "";

            string data = tb_decinput.Text;
            string key = tb_deckey.Text;

            //get key length
            int key_length = get_key_length();
            byte[] key_array = key_parsing(key_length, key);

            for (int i = 0; i < data.Length / 32; ++i)
            {
                //encrypted input parsing
                byte[] data_array = new byte[16];
                int it = 0;
                for (int j = 32 * i; j < 32 * (i + 1); j += 2)
                {
                    string temp = "" + data[j] + data[j + 1];
                    int val = int.Parse(temp, System.Globalization.NumberStyles.HexNumber);
                    data_array[it] = (byte)val;
                    it++;
                }

                int newit = 0;
                byte[,] state = new byte[4, 4];
                for (int x = 0; x < 4; x++)
                {
                    for (int y = 0; y < 4; y++)
                    {
                        state[x, y] = data_array[newit];
                        newit++;
                    }
                }

                // ------ end encrypted input parsing



                //Encrypting Function
                state = AES_Decryptor(state, key_array, key_length);


                    for (int x = 0; x < 4; x++)
                    {
                        for (int y = 0; y < 4; y++)
                        {
                            tb_decoutput.Text = tb_decoutput.Text + Convert.ToChar(state[x, y]);
                        }

                    }
               

            }


        }



        private byte[,] AES_Decryptor(byte[,] state, byte[] key_array, int key_length)
        {
            int rounds = key_length + 6;
            byte[] exp_key = ExpandKey(key_array, key_length);
            state = AddRoundKey(state, exp_key, rounds);
            //Decryption Core
            state = InvNewModifiction(state);
            for (int i = rounds - 1; i > 0; --i)
            {
                // state = InvNewModifiction(state);
                state = InvShiftRows(state);
                state = InvSubBytes(state);

                state = AddRoundKey(state, exp_key, i);
                state = InvMixColumns(state);
            }
            state = InvShiftRows(state);
            state = InvSubBytes(state);
            state = AddRoundKey(state, exp_key, 0);




            return state;
        }








        public byte[,] ModiShiftRow = new byte[4, 4];
        private byte[,] InvShiftRows(byte[,] state)
        {
            byte[,] temp = new byte[4, 4];
            temp[0, 1] = state[3, 1]; temp[1, 1] = state[0, 1]; temp[2, 1] = state[1, 1]; temp[3, 1] = state[2, 1];
            temp[2, 2] = state[0, 2]; temp[0, 2] = state[2, 2]; temp[3, 2] = state[1, 2]; temp[1, 2] = state[3, 2];
            temp[3, 3] = state[0, 3]; temp[2, 3] = state[3, 3]; temp[1, 3] = state[2, 3]; temp[0, 3] = state[1, 3];
            temp[0, 0] = state[0, 0]; temp[1, 0] = state[1, 0]; temp[2, 0] = state[2, 0]; temp[3, 0] = state[3, 0];



            return temp;
        }

        private byte[,] InvSubBytes(byte[,] state)
        {
            byte[] SboxInv = {
                0x52, 0x09, 0x6a, 0xd5, 0x30, 0x36, 0xa5, 0x38, 0xbf, 0x40, 0xa3, 0x9e, 0x81, 0xf3, 0xd7, 0xfb,
                0x7c, 0xe3, 0x39, 0x82, 0x9b, 0x2f, 0xff, 0x87, 0x34, 0x8e, 0x43, 0x44, 0xc4, 0xde, 0xe9, 0xcb,
                0x54, 0x7b, 0x94, 0x32, 0xa6, 0xc2, 0x23, 0x3d, 0xee, 0x4c, 0x95, 0x0b, 0x42, 0xfa, 0xc3, 0x4e,
                0x08, 0x2e, 0xa1, 0x66, 0x28, 0xd9, 0x24, 0xb2, 0x76, 0x5b, 0xa2, 0x49, 0x6d, 0x8b, 0xd1, 0x25,
                0x72, 0xf8, 0xf6, 0x64, 0x86, 0x68, 0x98, 0x16, 0xd4, 0xa4, 0x5c, 0xcc, 0x5d, 0x65, 0xb6, 0x92,
                0x6c, 0x70, 0x48, 0x50, 0xfd, 0xed, 0xb9, 0xda, 0x5e, 0x15, 0x46, 0x57, 0xa7, 0x8d, 0x9d, 0x84,
                0x90, 0xd8, 0xab, 0x00, 0x8c, 0xbc, 0xd3, 0x0a, 0xf7, 0xe4, 0x58, 0x05, 0xb8, 0xb3, 0x45, 0x06,
                0xd0, 0x2c, 0x1e, 0x8f, 0xca, 0x3f, 0x0f, 0x02, 0xc1, 0xaf, 0xbd, 0x03, 0x01, 0x13, 0x8a, 0x6b,
                0x3a, 0x91, 0x11, 0x41, 0x4f, 0x67, 0xdc, 0xea, 0x97, 0xf2, 0xcf, 0xce, 0xf0, 0xb4, 0xe6, 0x73,
                0x96, 0xac, 0x74, 0x22, 0xe7, 0xad, 0x35, 0x85, 0xe2, 0xf9, 0x37, 0xe8, 0x1c, 0x75, 0xdf, 0x6e,
                0x47, 0xf1, 0x1a, 0x71, 0x1d, 0x29, 0xc5, 0x89, 0x6f, 0xb7, 0x62, 0x0e, 0xaa, 0x18, 0xbe, 0x1b,
                0xfc, 0x56, 0x3e, 0x4b, 0xc6, 0xd2, 0x79, 0x20, 0x9a, 0xdb, 0xc0, 0xfe, 0x78, 0xcd, 0x5a, 0xf4,
                0x1f, 0xdd, 0xa8, 0x33, 0x88, 0x07, 0xc7, 0x31, 0xb1, 0x12, 0x10, 0x59, 0x27, 0x80, 0xec, 0x5f,
                0x60, 0x51, 0x7f, 0xa9, 0x19, 0xb5, 0x4a, 0x0d, 0x2d, 0xe5, 0x7a, 0x9f, 0x93, 0xc9, 0x9c, 0xef,
                0xa0, 0xe0, 0x3b, 0x4d, 0xae, 0x2a, 0xf5, 0xb0, 0xc8, 0xeb, 0xbb, 0x3c, 0x83, 0x53, 0x99, 0x61,
                0x17, 0x2b, 0x04, 0x7e, 0xba, 0x77, 0xd6, 0x26, 0xe1, 0x69, 0x14, 0x63, 0x55, 0x21, 0x0c, 0x7d
            };

            for (int i = 0; i < 4; ++i)
            {
                for (int j = 0; j < 4; ++j)
                {
                    state[j, i] = SboxInv[state[j, i]];
                }
            }

            return state;
        }

        private byte[,] InvMixColumns(byte[,] state)
        {

            byte[] mul9 = {
                 0x00, 0x09, 0x12, 0x1b, 0x24, 0x2d, 0x36, 0x3f, 0x48, 0x41, 0x5a, 0x53, 0x6c, 0x65, 0x7e, 0x77,
                0x90, 0x99, 0x82, 0x8b, 0xb4, 0xbd, 0xa6, 0xaf, 0xd8, 0xd1, 0xca, 0xc3, 0xfc, 0xf5, 0xee, 0xe7,
                0x3b, 0x32, 0x29, 0x20, 0x1f, 0x16, 0x0d, 0x04, 0x73, 0x7a, 0x61, 0x68, 0x57, 0x5e, 0x45, 0x4c,
                0xab, 0xa2, 0xb9, 0xb0, 0x8f, 0x86, 0x9d, 0x94, 0xe3, 0xea, 0xf1, 0xf8, 0xc7, 0xce, 0xd5, 0xdc,
                0x76, 0x7f, 0x64, 0x6d, 0x52, 0x5b, 0x40, 0x49, 0x3e, 0x37, 0x2c, 0x25, 0x1a, 0x13, 0x08, 0x01,
                0xe6, 0xef, 0xf4, 0xfd, 0xc2, 0xcb, 0xd0, 0xd9, 0xae, 0xa7, 0xbc, 0xb5, 0x8a, 0x83, 0x98, 0x91 ,
                0x4d, 0x44, 0x5f, 0x56, 0x69, 0x60, 0x7b, 0x72, 0x05, 0x0c, 0x17, 0x1e, 0x21, 0x28, 0x33, 0x3a ,
                0xdd, 0xd4, 0xcf, 0xc6, 0xf9, 0xf0, 0xeb, 0xe2, 0x95, 0x9c, 0x87, 0x8e, 0xb1, 0xb8, 0xa3, 0xaa ,
                0xec, 0xe5, 0xfe, 0xf7, 0xc8, 0xc1, 0xda, 0xd3, 0xa4, 0xad, 0xb6, 0xbf, 0x80, 0x89, 0x92, 0x9b ,
                0x7c, 0x75, 0x6e, 0x67, 0x58, 0x51, 0x4a, 0x43, 0x34, 0x3d, 0x26, 0x2f, 0x10, 0x19, 0x02, 0x0b ,
                0xd7, 0xde, 0xc5, 0xcc, 0xf3, 0xfa, 0xe1, 0xe8, 0x9f, 0x96, 0x8d, 0x84, 0xbb, 0xb2, 0xa9, 0xa0 ,
                0x47, 0x4e, 0x55, 0x5c, 0x63, 0x6a, 0x71, 0x78, 0x0f, 0x06, 0x1d, 0x14, 0x2b, 0x22, 0x39, 0x30 ,
                0x9a, 0x93, 0x88, 0x81, 0xbe, 0xb7, 0xac, 0xa5, 0xd2, 0xdb, 0xc0, 0xc9, 0xf6, 0xff, 0xe4, 0xed ,
                0x0a, 0x03, 0x18, 0x11, 0x2e, 0x27, 0x3c, 0x35, 0x42, 0x4b, 0x50, 0x59, 0x66, 0x6f, 0x74, 0x7d ,
                0xa1, 0xa8, 0xb3, 0xba, 0x85, 0x8c, 0x97, 0x9e, 0xe9, 0xe0, 0xfb, 0xf2, 0xcd, 0xc4, 0xdf, 0xd6 ,
                0x31, 0x38, 0x23, 0x2a, 0x15, 0x1c, 0x07, 0x0e, 0x79, 0x70, 0x6b, 0x62, 0x5d, 0x54, 0x4f, 0x46 };

            byte[] mul11 = {
                 0x00, 0x0b, 0x16, 0x1d, 0x2c, 0x27, 0x3a, 0x31, 0x58, 0x53, 0x4e, 0x45, 0x74, 0x7f, 0x62, 0x69 ,
                 0xb0, 0xbb, 0xa6, 0xad, 0x9c, 0x97, 0x8a, 0x81, 0xe8, 0xe3, 0xfe, 0xf5, 0xc4, 0xcf, 0xd2, 0xd9 ,
                 0x7b, 0x70, 0x6d, 0x66, 0x57, 0x5c, 0x41, 0x4a, 0x23, 0x28, 0x35, 0x3e, 0x0f, 0x04, 0x19, 0x12 ,
                 0xcb, 0xc0, 0xdd, 0xd6, 0xe7, 0xec, 0xf1, 0xfa, 0x93, 0x98, 0x85, 0x8e, 0xbf, 0xb4, 0xa9, 0xa2 ,
                 0xf6, 0xfd, 0xe0, 0xeb, 0xda, 0xd1, 0xcc, 0xc7, 0xae, 0xa5, 0xb8, 0xb3, 0x82, 0x89, 0x94, 0x9f ,
                 0x46, 0x4d, 0x50, 0x5b, 0x6a, 0x61, 0x7c, 0x77, 0x1e, 0x15, 0x08, 0x03, 0x32, 0x39, 0x24, 0x2f ,
                 0x8d, 0x86, 0x9b, 0x90, 0xa1, 0xaa, 0xb7, 0xbc, 0xd5, 0xde, 0xc3, 0xc8, 0xf9, 0xf2, 0xef, 0xe4 ,
                 0x3d, 0x36, 0x2b, 0x20, 0x11, 0x1a, 0x07, 0x0c, 0x65, 0x6e, 0x73, 0x78, 0x49, 0x42, 0x5f, 0x54 ,
                 0xf7, 0xfc, 0xe1, 0xea, 0xdb, 0xd0, 0xcd, 0xc6, 0xaf, 0xa4, 0xb9, 0xb2, 0x83, 0x88, 0x95, 0x9e ,
                 0x47, 0x4c, 0x51, 0x5a, 0x6b, 0x60, 0x7d, 0x76, 0x1f, 0x14, 0x09, 0x02, 0x33, 0x38, 0x25, 0x2e ,
                 0x8c, 0x87, 0x9a, 0x91, 0xa0, 0xab, 0xb6, 0xbd, 0xd4, 0xdf, 0xc2, 0xc9, 0xf8, 0xf3, 0xee, 0xe5 ,
                0x3c, 0x37, 0x2a, 0x21, 0x10, 0x1b, 0x06, 0x0d, 0x64, 0x6f, 0x72, 0x79, 0x48, 0x43, 0x5e, 0x55 ,
                 0x01, 0x0a, 0x17, 0x1c, 0x2d, 0x26, 0x3b, 0x30, 0x59, 0x52, 0x4f, 0x44, 0x75, 0x7e, 0x63, 0x68 ,
                 0xb1, 0xba, 0xa7, 0xac, 0x9d, 0x96, 0x8b, 0x80, 0xe9, 0xe2, 0xff, 0xf4, 0xc5, 0xce, 0xd3, 0xd8 ,
                 0x7a, 0x71, 0x6c, 0x67, 0x56, 0x5d, 0x40, 0x4b, 0x22, 0x29, 0x34, 0x3f, 0x0e, 0x05, 0x18, 0x13 ,
                 0xca, 0xc1, 0xdc, 0xd7, 0xe6, 0xed, 0xf0, 0xfb, 0x92, 0x99, 0x84, 0x8f, 0xbe, 0xb5, 0xa8, 0xa3 };

            byte[] mul13 = {
                 0x00, 0x0d, 0x1a, 0x17, 0x34, 0x39, 0x2e, 0x23, 0x68, 0x65, 0x72, 0x7f, 0x5c, 0x51, 0x46, 0x4b ,
                 0xd0, 0xdd, 0xca, 0xc7, 0xe4, 0xe9, 0xfe, 0xf3, 0xb8, 0xb5, 0xa2, 0xaf, 0x8c, 0x81, 0x96, 0x9b ,
                 0xbb, 0xb6, 0xa1, 0xac, 0x8f, 0x82, 0x95, 0x98, 0xd3, 0xde, 0xc9, 0xc4, 0xe7, 0xea, 0xfd, 0xf0 ,
                 0x6b, 0x66, 0x71, 0x7c, 0x5f, 0x52, 0x45, 0x48, 0x03, 0x0e, 0x19, 0x14, 0x37, 0x3a, 0x2d, 0x20 ,
                 0x6d, 0x60, 0x77, 0x7a, 0x59, 0x54, 0x43, 0x4e, 0x05, 0x08, 0x1f, 0x12, 0x31, 0x3c, 0x2b, 0x26 ,
                 0xbd, 0xb0, 0xa7, 0xaa, 0x89, 0x84, 0x93, 0x9e, 0xd5, 0xd8, 0xcf, 0xc2, 0xe1, 0xec, 0xfb, 0xf6 ,
                 0xd6, 0xdb, 0xcc, 0xc1, 0xe2, 0xef, 0xf8, 0xf5, 0xbe, 0xb3, 0xa4, 0xa9, 0x8a, 0x87, 0x90, 0x9d ,
                 0x06, 0x0b, 0x1c, 0x11, 0x32, 0x3f, 0x28, 0x25, 0x6e, 0x63, 0x74, 0x79, 0x5a, 0x57, 0x40, 0x4d ,
                 0xda, 0xd7, 0xc0, 0xcd, 0xee, 0xe3, 0xf4, 0xf9, 0xb2, 0xbf, 0xa8, 0xa5, 0x86, 0x8b, 0x9c, 0x91 ,
                 0x0a, 0x07, 0x10, 0x1d, 0x3e, 0x33, 0x24, 0x29, 0x62, 0x6f, 0x78, 0x75, 0x56, 0x5b, 0x4c, 0x41 ,
                 0x61, 0x6c, 0x7b, 0x76, 0x55, 0x58, 0x4f, 0x42, 0x09, 0x04, 0x13, 0x1e, 0x3d, 0x30, 0x27, 0x2a ,
                 0xb1, 0xbc, 0xab, 0xa6, 0x85, 0x88, 0x9f, 0x92, 0xd9, 0xd4, 0xc3, 0xce, 0xed, 0xe0, 0xf7, 0xfa ,
                 0xb7, 0xba, 0xad, 0xa0, 0x83, 0x8e, 0x99, 0x94, 0xdf, 0xd2, 0xc5, 0xc8, 0xeb, 0xe6, 0xf1, 0xfc ,
                 0x67, 0x6a, 0x7d, 0x70, 0x53, 0x5e, 0x49, 0x44, 0x0f, 0x02, 0x15, 0x18, 0x3b, 0x36, 0x21, 0x2c ,
                 0x0c, 0x01, 0x16, 0x1b, 0x38, 0x35, 0x22, 0x2f, 0x64, 0x69, 0x7e, 0x73, 0x50, 0x5d, 0x4a, 0x47 ,
                0xdc, 0xd1, 0xc6, 0xcb, 0xe8, 0xe5, 0xf2, 0xff, 0xb4, 0xb9, 0xae, 0xa3, 0x80, 0x8d, 0x9a, 0x97 };

            byte[] mul14 = {
                 0x00, 0x0e, 0x1c, 0x12, 0x38, 0x36, 0x24, 0x2a, 0x70, 0x7e, 0x6c, 0x62, 0x48, 0x46, 0x54, 0x5a ,
                 0xe0, 0xee, 0xfc, 0xf2, 0xd8, 0xd6, 0xc4, 0xca, 0x90, 0x9e, 0x8c, 0x82, 0xa8, 0xa6, 0xb4, 0xba ,
                 0xdb, 0xd5, 0xc7, 0xc9, 0xe3, 0xed, 0xff, 0xf1, 0xab, 0xa5, 0xb7, 0xb9, 0x93, 0x9d, 0x8f, 0x81 ,
                 0x3b, 0x35, 0x27, 0x29, 0x03, 0x0d, 0x1f, 0x11, 0x4b, 0x45, 0x57, 0x59, 0x73, 0x7d, 0x6f, 0x61 ,
                 0xad, 0xa3, 0xb1, 0xbf, 0x95, 0x9b, 0x89, 0x87, 0xdd, 0xd3, 0xc1, 0xcf, 0xe5, 0xeb, 0xf9, 0xf7 ,
                 0x4d, 0x43, 0x51, 0x5f, 0x75, 0x7b, 0x69, 0x67, 0x3d, 0x33, 0x21, 0x2f, 0x05, 0x0b, 0x19, 0x17 ,
                 0x76, 0x78, 0x6a, 0x64, 0x4e, 0x40, 0x52, 0x5c, 0x06, 0x08, 0x1a, 0x14, 0x3e, 0x30, 0x22, 0x2c ,
                 0x96, 0x98, 0x8a, 0x84, 0xae, 0xa0, 0xb2, 0xbc, 0xe6, 0xe8, 0xfa, 0xf4, 0xde, 0xd0, 0xc2, 0xcc ,
                 0x41, 0x4f, 0x5d, 0x53, 0x79, 0x77, 0x65, 0x6b, 0x31, 0x3f, 0x2d, 0x23, 0x09, 0x07, 0x15, 0x1b ,
                 0xa1, 0xaf, 0xbd, 0xb3, 0x99, 0x97, 0x85, 0x8b, 0xd1, 0xdf, 0xcd, 0xc3, 0xe9, 0xe7, 0xf5, 0xfb ,
                 0x9a, 0x94, 0x86, 0x88, 0xa2, 0xac, 0xbe, 0xb0, 0xea, 0xe4, 0xf6, 0xf8, 0xd2, 0xdc, 0xce, 0xc0 ,
                 0x7a, 0x74, 0x66, 0x68, 0x42, 0x4c, 0x5e, 0x50, 0x0a, 0x04, 0x16, 0x18, 0x32, 0x3c, 0x2e, 0x20 ,
                 0xec, 0xe2, 0xf0, 0xfe, 0xd4, 0xda, 0xc8, 0xc6, 0x9c, 0x92, 0x80, 0x8e, 0xa4, 0xaa, 0xb8, 0xb6 ,
                 0x0c, 0x02, 0x10, 0x1e, 0x34, 0x3a, 0x28, 0x26, 0x7c, 0x72, 0x60, 0x6e, 0x44, 0x4a, 0x58, 0x56 ,
                 0x37, 0x39, 0x2b, 0x25, 0x0f, 0x01, 0x13, 0x1d, 0x47, 0x49, 0x5b, 0x55, 0x7f, 0x71, 0x63, 0x6d ,
                 0xd7, 0xd9, 0xcb, 0xc5, 0xef, 0xe1, 0xf3, 0xfd, 0xa7, 0xa9, 0xbb, 0xb5, 0x9f, 0x91, 0x83, 0x8d };

            byte[,] temp = new byte[4, 4];
            for (int i = 0; i < 4; i++)
            {


                temp[i, 0] = (byte)(mul14[state[i, 0]] ^ mul11[state[i, 1]] ^ mul13[state[i, 2]] ^ mul9[state[i, 3]]);
                temp[i, 1] = (byte)(mul9[state[i, 0]] ^ mul14[state[i, 1]] ^ mul11[state[i, 2]] ^ mul13[state[i, 3]]);
                temp[i, 2] = (byte)(mul13[state[i, 0]] ^ mul9[state[i, 1]] ^ mul14[state[i, 2]] ^ mul11[state[i, 3]]);
                temp[i, 3] = (byte)(mul11[state[i, 0]] ^ mul13[state[i, 1]] ^ mul9[state[i, 2]] ^ mul14[state[i, 3]]);
            }
            /*
                temp[0, 0] = (byte)(mul14[state[0, 0]] ^ mul11[state[0, 1]] ^ mul13[state[0, 2]] ^ mul9[state[0, 3]]);
            temp[0, 1] = (byte)(mul9[state[0, 0]] ^ mul14[state[0, 1]] ^ mul11[state[0, 2]] ^ mul13[state[0, 3]]);
            temp[0, 2] = (byte)(mul13[state[0, 0]] ^mul9[state[0, 1]] ^ mul14[state[0, 2]] ^ mul11[state[0, 3]]);
            temp[0, 3] = (byte)(mul11[state[0, 0]] ^ mul13[state[0, 1]] ^ mul9[state[0, 2]] ^ mul14[state[0, 3]]);

            temp[1, 0] = (byte)(mul14[state[1, 0]] ^ mul11[state[1, 1]] ^ mul13[state[1, 2]] ^ mul9[state[1, 3]]);
            temp[1, 1] = (byte)(mul9[state[1, 0]] ^ mul14[state[1, 1]] ^ mul11[state[1, 2]] ^ mul13[state[1, 3]]);
            temp[1, 2] = (byte)(mul13[state[1, 0]] ^ mul9[state[1, 1]] ^ mul14[state[1, 2]] ^ mul11[state[1, 3]]);
            temp[1, 3] = (byte)(mul11[state[1, 0]] ^ mul13[state[1, 1]] ^ mul9[state[1, 2]] ^ mul14[state[1, 3]]);

            temp[2, 0] = (byte)(mul14[state[2, 0]] ^ mul11[state[2, 1]] ^ mul13[state[2, 2]] ^ mul9[state[2, 3]]);
            temp[2, 1] = (byte)(mul9[state[2, 0]] ^ mul14[state[2, 1]] ^ mul11[state[2, 2]] ^ mul13[state[2, 3]]);
            temp[2, 2] = (byte)(mul13[state[2, 0]] ^ mul9[state[2, 1]] ^ mul14[state[2, 2]] ^ mul11[state[2, 3]]);
            temp[2, 3] = (byte)(mul11[state[2, 0]] ^ mul13[state[2, 1]] ^ mul9[state[2, 2]] ^ mul14[state[2, 3]]);

            temp[3, 0] = (byte)(mul14[state[3, 0]] ^ mul11[state[3, 1]] ^ mul13[state[3, 2]] ^ mul9[state[3, 3]]);
            temp[3, 1] = (byte)(mul9[state[3, 0]] ^ mul14[state[3, 1]] ^ mul11[state[3, 2]] ^ mul13[state[3, 3]]);
            temp[3, 2] = (byte)(mul13[state[3, 0]] ^ mul9[state[3, 1]] ^ mul14[state[3, 2]] ^ mul11[state[3, 3]]);
            temp[3, 3] = (byte)(mul11[state[3, 0]] ^ mul13[state[3, 1]] ^ mul9[state[3, 2]] ^ mul14[state[3, 3]]);
            */
            return temp;
        }

        private void btn_selectfile_Click(object sender, EventArgs e)
        {

            if (ofd_selectfile.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                tb_selectfile.Text = ofd_selectfile.FileName;
        }
        private void btn_enc_file_Click(object sender, EventArgs e)
        {
           
            string key = tb_file_key.Text;
            //get key length
            int key_length = get_key_length();
            byte[] key_block = key_parsing(key_length, key);

            string file_path = tb_selectfile.Text;  //input file path

            // save the encrypted file to the same folder

            // code for processing the output file name and directory
            char[] reverse = file_path.ToCharArray();
            Array.Reverse(reverse);
            string rev_file_path = new string(reverse);
            string ext_name = "";
            string op_file_path = "";
            string file_name = "";
            int str_it = 0;
            int ext_it = 0;
            for (; ext_it < rev_file_path.Length; ++ext_it)
            {
                if (rev_file_path[ext_it] == '.')
                    break;
            }

            for (int op_it = ext_it - 1; op_it >= 0; --op_it)
                ext_name = ext_name + rev_file_path[op_it];


            for (; str_it < rev_file_path.Length; ++str_it)
            {
                if (rev_file_path[str_it] == '\\')
                    break;
            }

            for (int op_it = str_it - 1; op_it > ext_it; --op_it)
                file_name = file_name + rev_file_path[op_it];


            for (int op_it = rev_file_path.Length - 1; op_it > str_it; op_it--)
                op_file_path = op_file_path + rev_file_path[op_it];

            op_file_path = op_file_path + "\\";
            //----------- end code 


            //open the file for reading
            try { 
            using (FileStream fs = File.Open(file_path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (BufferedStream bs = new BufferedStream(fs))
            using (StreamReader sr = new StreamReader(bs))
            {
                List<string> lines = new List<string>();
                string temp_line = "";
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    while (line.Length % 16 != 0)      //space padding
                        line = line + " ";

                    //input parsing 

                    for (int i = 0; i < line.Length / 16; ++i)
                    {
                        int it = 0;
                        byte[] data_block = new byte[16];
                        for (int j = 16 * i; j < 16 * (i + 1); ++j)
                        {
                            data_block[it] = Convert.ToByte(line[j]);
                            it++;
                        }

                        int newit = 0;
                        byte[,] state = new byte[4, 4];
                        for (int x = 0; x < 4; x++)
                        {
                            for (int y = 0; y < 4; y++)
                            {
                                state[x, y] = data_block[newit];
                                newit++;
                            }
                        }
                        // ------ end input parsing
                        state = AES_Encryptor(state, key_block, key_length);

                        for (int x = 0; x < 4; x++)
                        {
                            for (int y = 0; y < 4; y++)
                            {
                                temp_line = temp_line + state[x, y].ToString("X2");
                            }
                        }

                    }

                    lines.Add(temp_line);
                    temp_line = "";
                }
                //write encrypted data to another file
                System.IO.File.WriteAllLines(op_file_path + file_name + "_enc." + ext_name, lines);

                RadMessageBox.ThemeName = btn_enc_file.ThemeName;
                RadMessageBox.Instance.MinimumSize = new System.Drawing.Size(100, 100);
                    DialogResult ds = RadMessageBox.Show(this, " File Encrypted Successfully  ", "", MessageBoxButtons.OK, RadMessageIcon.Info);
                }
        }
            catch 
            {
               
                RadMessageBox.ThemeName = btn_enc_file.ThemeName;
                RadMessageBox.Instance.MinimumSize = new System.Drawing.Size(100, 100);
                DialogResult ds = RadMessageBox.Show(this, " Pleas! Choose File First ", " Empty File Path Error ", MessageBoxButtons.RetryCancel, RadMessageIcon.Error);
                
            }
            
        }

        private void btn_dec_file_Click(object sender, EventArgs e)
        {
            string key = tb_file_key.Text;
            int key_length = get_key_length();
            byte[] key_block = key_parsing(key_length, key);

            string file_path = tb_selectfile.Text;  //input file path

            // code for processing the output file name and directory
            char[] reverse = file_path.ToCharArray();
            Array.Reverse(reverse);
            string rev_file_path = new string(reverse);
            string ext_name = "";
            string op_file_path = "";
            string file_name = "";
            int str_it = 0;
            int ext_it = 0;
            for (; ext_it < rev_file_path.Length; ++ext_it)
            {
                if (rev_file_path[ext_it] == '.')
                    break;
            }

            for (int op_it = ext_it - 1; op_it >= 0; --op_it)
                ext_name = ext_name + rev_file_path[op_it];


            for (; str_it < rev_file_path.Length; ++str_it)
            {
                if (rev_file_path[str_it] == '\\')
                    break;
            }

            for (int op_it = str_it - 1; op_it > ext_it; --op_it)
                file_name = file_name + rev_file_path[op_it];


            for (int op_it = rev_file_path.Length - 1; op_it > str_it; op_it--)
                op_file_path = op_file_path + rev_file_path[op_it];

            op_file_path = op_file_path + "\\";
            //----------- end code 

            //open the file
            try {
                using (FileStream fs = File.Open(file_path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (BufferedStream bs = new BufferedStream(fs))
                using (StreamReader sr = new StreamReader(bs))
                {
                    List<string> lines = new List<string>();
                    string temp_line = "";
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        while (line.Length % 16 != 0)      //space padding
                            line = line + " ";

                        for (int i = 0; i < line.Length / 32; ++i)
                        {
                            //encrypted input parsing
                            byte[] data_block = new byte[16];
                            int it = 0;
                            for (int j = 32 * i; j < 32 * (i + 1); j += 2)
                            {
                                string temp = "" + line[j] + line[j + 1];
                                int val = int.Parse(temp, System.Globalization.NumberStyles.HexNumber);
                                data_block[it] = (byte)val;
                                it++;
                            }

                            int newit = 0;
                            byte[,] state = new byte[4, 4];
                            for (int x = 0; x < 4; x++)
                            {
                                for (int y = 0; y < 4; y++)
                                {
                                    state[x, y] = data_block[newit];
                                    newit++;
                                }
                            }

                            // ------ end encrypted input parsing


                            state = AES_Decryptor(state, key_block, key_length);

                            for (int x = 0; x < 4; x++)
                            {
                                for (int y = 0; y < 4; y++)
                                {
                                    temp_line = temp_line + Convert.ToChar(state[x, y]);
                                }

                            }
                        }

                        lines.Add(temp_line);
                        temp_line = "";
                    }
                    //write decrypted output to another file
                    System.IO.File.WriteAllLines(op_file_path + file_name + "_dec." + ext_name, lines);

                }

                RadMessageBox.ThemeName = btn_dec_file.ThemeName;
                RadMessageBox.Instance.MinimumSize = new System.Drawing.Size(100, 100);
                DialogResult ds = RadMessageBox.Show(this, "  File Decrypted Successfully  ", "", MessageBoxButtons.OK, RadMessageIcon.Info);
            }
            catch
            {
               
                RadMessageBox.ThemeName = btn_enc_file.ThemeName;
                RadMessageBox.Instance.MinimumSize = new System.Drawing.Size(100, 100);
                DialogResult ds = RadMessageBox.Show(this, " Pleas! Choose File First ", " Empty Path Error ", MessageBoxButtons.RetryCancel, RadMessageIcon.Error);
           
            }
        }

       

        private void main_page_Load(object sender, EventArgs e)
        {

        }
         
       

        private void radProgressBar1_Click(object sender, EventArgs e)
        {

        }

        private void radToggleSwitch1_ValueChanged(object sender, EventArgs e)
        {

            if (radLabel11.Text == "Dark Mood On")
                radLabel11.Text = "Dark Mood Off";
            else radLabel11.Text = "Dark Mood On";


            if (this.ThemeName == crystalDarkTheme1.ThemeName)
                this.ThemeName = crystalTheme1.ThemeName;
            else this.ThemeName = crystalDarkTheme1.ThemeName;

            if (radLabel1.ThemeName == crystalDarkTheme1.ThemeName)
                radLabel1.ThemeName = crystalTheme1.ThemeName;
            else radLabel1.ThemeName = crystalDarkTheme1.ThemeName;

            if (radLabel2.ThemeName == crystalDarkTheme1.ThemeName)
                radLabel2.ThemeName = crystalTheme1.ThemeName;
            else radLabel2.ThemeName = crystalDarkTheme1.ThemeName;

            if (radLabel3.ThemeName == crystalDarkTheme1.ThemeName)
                radLabel3.ThemeName = crystalTheme1.ThemeName;
            else radLabel3.ThemeName = crystalDarkTheme1.ThemeName;

            if (radLabel4.ThemeName == crystalDarkTheme1.ThemeName)
                radLabel4.ThemeName = crystalTheme1.ThemeName;
            else radLabel4.ThemeName = crystalDarkTheme1.ThemeName;

            if (radLabel5.ThemeName == crystalDarkTheme1.ThemeName)
                radLabel5.ThemeName = crystalTheme1.ThemeName;
            else radLabel5.ThemeName = crystalDarkTheme1.ThemeName;

            if (radLabel6.ThemeName == crystalDarkTheme1.ThemeName)
                radLabel6.ThemeName = crystalTheme1.ThemeName;
            else radLabel6.ThemeName = crystalDarkTheme1.ThemeName;

            if (radLabel7.ThemeName == crystalDarkTheme1.ThemeName)
                radLabel7.ThemeName = crystalTheme1.ThemeName;
            else radLabel7.ThemeName = crystalDarkTheme1.ThemeName;

            if (radLabel8.ThemeName == crystalDarkTheme1.ThemeName)
                radLabel8.ThemeName = crystalTheme1.ThemeName;
            else radLabel8.ThemeName = crystalDarkTheme1.ThemeName;

            if (radLabel9.ThemeName == crystalDarkTheme1.ThemeName)
                radLabel9.ThemeName = crystalTheme1.ThemeName;
            else radLabel9.ThemeName = crystalDarkTheme1.ThemeName;

            if (radLabel10.ThemeName == crystalDarkTheme1.ThemeName)
                radLabel10.ThemeName = crystalTheme1.ThemeName;
            else radLabel10.ThemeName = crystalDarkTheme1.ThemeName;

            if (radLabel11.ThemeName == crystalDarkTheme1.ThemeName)
                radLabel11.ThemeName = crystalTheme1.ThemeName;
            else radLabel11.ThemeName = crystalDarkTheme1.ThemeName;

            if (radLabel12.ThemeName == crystalDarkTheme1.ThemeName)
                radLabel12.ThemeName = crystalTheme1.ThemeName;
            else radLabel12.ThemeName = crystalDarkTheme1.ThemeName;

            if (radLabel13.ThemeName == crystalDarkTheme1.ThemeName)
                radLabel13.ThemeName = crystalTheme1.ThemeName;
            else radLabel13.ThemeName = crystalDarkTheme1.ThemeName;

            if (radToggleSwitch1.ThemeName == crystalDarkTheme1.ThemeName)
                radToggleSwitch1.ThemeName = crystalTheme1.ThemeName;
            else radToggleSwitch1.ThemeName = crystalDarkTheme1.ThemeName;

            if (radRadioButton1.ThemeName == crystalDarkTheme1.ThemeName)
                radRadioButton1.ThemeName = crystalTheme1.ThemeName;
            else radRadioButton1.ThemeName = crystalDarkTheme1.ThemeName;

            if (radRadioButton2.ThemeName == crystalDarkTheme1.ThemeName)
                radRadioButton2.ThemeName = crystalTheme1.ThemeName;
            else radRadioButton2.ThemeName = crystalDarkTheme1.ThemeName;

            if (radRadioButton3.ThemeName == crystalDarkTheme1.ThemeName)
                radRadioButton3.ThemeName = crystalTheme1.ThemeName;
            else radRadioButton3.ThemeName = crystalDarkTheme1.ThemeName;

            if (tb_file_key.ThemeName == crystalDarkTheme1.ThemeName)
                tb_file_key.ThemeName = crystalTheme1.ThemeName;
            else tb_file_key.ThemeName = crystalDarkTheme1.ThemeName;

            if (tb_selectfile.ThemeName == crystalDarkTheme1.ThemeName)
                tb_selectfile.ThemeName = crystalTheme1.ThemeName;
            else tb_selectfile.ThemeName = crystalDarkTheme1.ThemeName;

            if (tb_decoutput.ThemeName == crystalDarkTheme1.ThemeName)
                tb_decoutput.ThemeName = crystalTheme1.ThemeName;
            else tb_decoutput.ThemeName = crystalDarkTheme1.ThemeName;

            if (tb_enckey.ThemeName == crystalDarkTheme1.ThemeName)
                tb_enckey.ThemeName = crystalTheme1.ThemeName;
            else tb_enckey.ThemeName = crystalDarkTheme1.ThemeName;


            if (tb_deckey.ThemeName == crystalDarkTheme1.ThemeName)
                tb_deckey.ThemeName = crystalTheme1.ThemeName;
            else tb_deckey.ThemeName = crystalDarkTheme1.ThemeName;

            if (tb_encoutput.ThemeName == crystalDarkTheme1.ThemeName)
                tb_encoutput.ThemeName = crystalTheme1.ThemeName;
            else tb_encoutput.ThemeName = crystalDarkTheme1.ThemeName;

            if (tb_decinput.ThemeName == crystalDarkTheme1.ThemeName)
                tb_decinput.ThemeName = crystalTheme1.ThemeName;
            else tb_decinput.ThemeName = crystalDarkTheme1.ThemeName;

            if (tb_encinput.ThemeName == crystalDarkTheme1.ThemeName)
                tb_encinput.ThemeName = crystalTheme1.ThemeName;
            else tb_encinput.ThemeName = crystalDarkTheme1.ThemeName;

            if (btn_enc_file.ThemeName == crystalDarkTheme1.ThemeName)
                btn_enc_file.ThemeName = crystalTheme1.ThemeName;
            else btn_enc_file.ThemeName = crystalDarkTheme1.ThemeName;

            if (btn_dec_file.ThemeName == crystalDarkTheme1.ThemeName)
                btn_dec_file.ThemeName = crystalTheme1.ThemeName;
            else btn_dec_file.ThemeName = crystalDarkTheme1.ThemeName;

            if (btn_selectfile.ThemeName == crystalDarkTheme1.ThemeName)
                btn_selectfile.ThemeName = crystalTheme1.ThemeName;
            else btn_selectfile.ThemeName = crystalDarkTheme1.ThemeName;

            if (btn_decrypt.ThemeName == crystalDarkTheme1.ThemeName)
                btn_decrypt.ThemeName = crystalTheme1.ThemeName;
            else btn_decrypt.ThemeName = crystalDarkTheme1.ThemeName;

            if (btn_encrypt.ThemeName == crystalDarkTheme1.ThemeName)
                btn_encrypt.ThemeName = crystalTheme1.ThemeName;
            else btn_encrypt.ThemeName = crystalDarkTheme1.ThemeName;


        }

        private void radRadioButton1_ToggleStateChanged(object sender, StateChangedEventArgs args)
        {

        }

  

        private void tb_encinput_TextChanged(object sender, EventArgs e)
        {
            tb_encoutput.NullText = this.tb_encoutput.Text;
        }

        private void tb_enckey_TextChanged(object sender, EventArgs e)
        {
            tb_encoutput.NullText = this.tb_encoutput.Text;
        }
       
        private void main_page_FormClosed(object sender, FormClosedEventArgs e)
        {
             Application.Exit();
        }

        
    }
}
