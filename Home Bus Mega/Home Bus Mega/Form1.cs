using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MyLibrary;

namespace Home_Bus_Mega
{
    public partial class MainForm : Form
    {
        /*
          59/27 = 4/x
          */
        //setting
        public const string FONT_NAME = "Microsoft Sans Serif";

        //dont touch variable
        public double old_window_width = 1280;
        public double old_window_height = 720;
        public double magnification_ratio;
        Process_loc_stat process_Loc_Stat;
        Mapa mapa;

        //user variable
        string using_map;

        public MainForm()
        {
            InitializeComponent();
            label_headtitle.Parent = header_bg_label;

            process_Loc_Stat = new Process_loc_stat();
            mapa = new Mapa(this);


            //load map
            using_map = process_Loc_Stat.Read_Data("map");
            mapa.Process_mapa_main(using_map, 640);

            //resize
            FitTheScreen.Resize_Child(this, 1280);

        }



        //switch mode buttons

        private void switch_mode(int mode)
        {
            Controls.Remove(Controls["panel_eta"]);
            mapa.mode = mode;
        }
        private void button_to_mode_1_Click(object sender, EventArgs e)
        {
            switch_mode(1);
        }

        private void button_to_mode_2_Click(object sender, EventArgs e)
        {
            switch_mode(2);
        }
    }
}
