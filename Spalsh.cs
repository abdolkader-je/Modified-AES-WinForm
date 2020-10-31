using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Telerik.WinControls;

namespace TelerikWinFormsApp4
{
    public partial class Spalsh : Telerik.WinControls.UI.RadForm
    {
        public Spalsh()
        {
            InitializeComponent();
        }

        Timer tmr;
        private void Splash_Shown(object sender, EventArgs e)
        {
            tmr = new Timer();

            //set time interval 1.5 sec

            tmr.Interval = 1575;
           
            //starts the timer

            tmr.Start();

            tmr.Tick += tmr_Tick;
        }
        void tmr_Tick(object sender, EventArgs e)

        {


            //after 1.5 sec stop the timer

            tmr.Stop();

            //display mainform

            main_page mf = new main_page();

            mf.Show();

            //hide this form

            this.Hide();

        }
       
        void timer1_Tick(object sender, EventArgs e)
        {
            if (radProgressBar1.Value1 != 100)
            {
                radProgressBar1.Value1++;
                
               
            }
            else
            {
                timer1.Stop();
            }
        }
       
        private void Spalsh_Load(object sender, EventArgs e)
        {
            timer1.Enabled = true;
            timer1.Start();
            timer1.Interval = 15;
            radProgressBar1.Maximum = 100;
            timer1.Tick += new EventHandler(timer1_Tick);
           
        }

        private void radProgressBar1_Click(object sender, EventArgs e)
        {

        }

        private void radLabel2_Click(object sender, EventArgs e)
        {

        }
    }
}

