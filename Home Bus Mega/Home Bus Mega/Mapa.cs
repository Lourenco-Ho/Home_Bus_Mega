﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MyLibrary;

//contents
//-- setting
//-- static value
//-- internal value
//-- function
//-- buttons function
//-- common function
//-- custom component
//-- bus icon

namespace Home_Bus_Mega
{
    class Mapa
    {
        //setting
        //const string ENG_FONT_NAME = "Microsoft Sans Serif";
        const string ENG_FONT_NAME = "Arial";

        //static value
        public string name, image;
        public int width, height;
        public List<Button> buttons = new List<Button>();
        public int mode = 1;


        //internal value
        string FOLDER_PATH = @"map/";
        private Form form1;
        static string image_folder = @"image/";
        static string bus_icon_image_folder = image_folder + @"bus_icon/",
            btn_red_image = image_folder + "map_button_red.png",
            btn_green_image = image_folder + "map_button_green.png",
            point_button_bg = image_folder + "point_button_bg.png",
            stop_icon_image = image_folder + "stop_icon.png";
        PictureBox map_picturebox;

        //--disable label but not be gray
        private const int GWL_STYLE = -16;
        private const int WS_DISABLED = 0x8000000;
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int wndproc);
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);




        //function
        public Mapa(Form context)
        {
            form1 = context;
        }

        public void Process_mapa_main(string map_name, int target_width)
        {
            string file_path = $"{FOLDER_PATH}{map_name}.mapa/{map_name}.mapa_main";

            string[] fileRead = File.ReadAllLines(file_path);
            int file_length = fileRead.Length;

            //start
            name = fileRead[0];
            image = $"{FOLDER_PATH}{map_name}.mapa/{map_name}.png";

            string[] size = fileRead[1].Split(" "); //讀得地圖大小
            width = int.Parse(size[0]); height = int.Parse(size[1]);

            double ratio = (double)(target_width) / (double)(width); //放大至指定闊度
            width = (int)(width * ratio); height = (int)(height * ratio);

            map_picturebox = Map_Image(); //spawn map image
            form1.Controls.Add(map_picturebox);

            int line_index = 3;
            //find the tag
            line_index = find_the_tag(line_index, "#button", fileRead.Skip(line_index).ToArray()) + 1;

            //start spawn button
            while (line_index < file_length)
            {
                string stop_num = fileRead[line_index];
                string[] coor = fileRead[line_index + 1].Split(" ");
                float font_size = float.Parse(fileRead[line_index + 2]);
                int button_size = int.Parse(fileRead[line_index + 3]);
                Button map_button = Map_Button(stop_num,
                    (int)(int.Parse(coor[0]) * ratio), //x coor
                    (int)(int.Parse(coor[1]) * ratio), //y coor
                    (float)(font_size * ratio),
                    (int)(button_size * ratio));
                map_picturebox.Controls.Add(map_button);
                buttons.Add(map_button);

                if (fileRead[line_index + 4] == "*end")
                {
                    break;
                }
                else
                {
                    line_index += 5;
                }
                
            }

            //find the next tag
            line_index = find_the_tag(line_index, "#stop list", fileRead.Skip(line_index).ToArray()) + 1;

            while (line_index < file_length)
            {
                string stop_num = fileRead[line_index];
                string chi_content = fileRead[line_index + 1];
                string eng_content = fileRead[line_index + 2];
                Button mode_1_button = Mode_1_Button(stop_num, chi_content, eng_content);
                form1.Controls["panel_mode_1A"].Controls["flowLayoutPanel"].Controls.Add(mode_1_button);

                if (fileRead[line_index + 3] == "*end")
                {
                    break;
                }
                else
                {
                    line_index += 4;
                }

            }


        }


        public void Process_Stops(FlowLayoutPanel flowLayoutPanel, string stop_num)
        {
            string file_path = $"{FOLDER_PATH}{name}.mapa/{stop_num}.stops";

            string[] fileRead = File.ReadAllLines(file_path);
            int file_length = fileRead.Length;

            int line_index = 0;
            while (line_index < file_length)
            {
                line_index = find_the_tag(line_index, "#stop", fileRead.Skip(line_index).ToArray()) + 1;
                if(line_index >= file_length) // if out-of file_length
                {
                    break;
                }

                //stop info
                string company_code = fileRead[line_index];
                string stop_id = fileRead[line_index + 1];
                string stop_name_chi = fileRead[line_index + 2];
                string stop_name_eng = fileRead[line_index + 3];
                Panel panel_bus_stop = Panel_bus_stop(company_code, stop_id, stop_name_chi, stop_name_eng);
                flowLayoutPanel.Controls.Add(panel_bus_stop);

                line_index = find_the_tag(line_index, "#route", fileRead.Skip(line_index).ToArray()) + 1;
                while(line_index < file_length)
                {
                    if(company_code == "CTB")
                    {
                        string route_name = fileRead[line_index];
                        string direction = fileRead[line_index + 1];
                        string dest_name_chi = fileRead[line_index + 2];
                        string dest_name_eng = fileRead[line_index + 3];

                        Debug.WriteLine(route_name);
                        if (fileRead[line_index + 4] == "*end stop")
                        {
                            break;
                        }
                        else
                        {
                            line_index += 5;
                        }
                    }
                    else if (company_code == "KMB")
                    {
                        // remember the value
                        IntValue value_file_line_index = new IntValue
                        {
                            Value = line_index,
                            Name = "value_file_line_index"
                        };
                        StringValue value_route_name = new StringValue
                        {
                            Value = fileRead[line_index],
                            Name = "value_route_name"
                        };
                        StringValue value_direction = new StringValue
                        {
                            Value = fileRead[line_index + 1],
                            Name = "value_direction"
                        };
                        IntValue value_service_type = new IntValue
                        {
                            Value = int.Parse(fileRead[line_index + 2]),
                            Name = "value_service_type"
                        };
                        string dest_name_chi = fileRead[line_index + 3];
                        string dest_name_eng = fileRead[line_index + 4];

                        PictureBox icon = Label_bus_icon(Bus_Route.is_LWB_route(company_code)? "LWB":"KMB", value_route_name.Value);
                        Panel panel_route_box = Panel_route_box(icon, value_route_name.Value, dest_name_chi, dest_name_eng);
                        panel_bus_stop.Controls.Add(panel_route_box);

                        panel_route_box.Controls.Add(value_route_name);
                        panel_route_box.Controls.Add(value_direction);
                        panel_route_box.Controls.Add(value_service_type);

                        if (fileRead[line_index + 5] == "*end stop")
                        {
                            break;
                        }
                        else
                        {
                            line_index += 6;
                        }
                    }
                }

                FitTheScreen.Resize(panel_bus_stop, 1280);
                FitTheScreen.Resize_Child(panel_bus_stop, 1280);
            }
        }





        //buttons function
        private void Mode_1_Button_Clicked(object sender, EventArgs e, string stop_num)
        {
            if (mode == 1)
            {
                mode = 0; //disable buttons
                string chi_name = form1.Controls["panel_mode_1A"].Controls["flowLayoutPanel"].Controls[stop_num].Controls["label_chi"].Text;
                string eng_name = form1.Controls["panel_mode_1A"].Controls["flowLayoutPanel"].Controls[stop_num].Controls["label_eng"].Text;
                //----------- panel
                Panel eta_panel = ETA_Panel(stop_num, chi_name, eng_name);

                form1.Controls.Add(eta_panel);
                eta_panel.BringToFront();
                FitTheScreen.Resize(eta_panel, 1280);
                FitTheScreen.Resize_Child(eta_panel, 1280);

                //------------ content
                Process_Stops((FlowLayoutPanel)eta_panel.Controls["flowLayoutPanel"] ,stop_num);
            }
        }





        //common function

        public int find_the_tag(int start_line, string the_tag, string[]fileRead)
        {
            int line_number = start_line;
            while (line_number - start_line < fileRead.Length)
            {
                if (fileRead[line_number - start_line] == the_tag)
                {
                    break;
                }

                line_number++;
            }
            return line_number;
        }






        //custom component
        private PictureBox Map_Image()
        {
            //Spawn image
            PictureBox pictureBox = new PictureBox();
            pictureBox.Size = new Size(width, height);
            pictureBox.Location = new Point(0, 0);
            pictureBox.Image = Image.FromFile(image);
            pictureBox.SizeMode = PictureBoxSizeMode.StretchImage;

            return pictureBox;
        }

        private Button Map_Button(string stop_num, int x_coor, int y_coor, float font_size, int button_size)
        {
            Button button = new Button();

            button.BackColor = Color.Transparent;
            button.BackgroundImage = Image.FromFile(btn_red_image);
            button.BackgroundImageLayout = ImageLayout.Stretch;
            button.Cursor = Cursors.Hand;
            button.FlatAppearance.BorderSize = 0;
            button.FlatStyle = FlatStyle.Flat;
            button.Font = new Font(ENG_FONT_NAME, font_size, FontStyle.Regular, GraphicsUnit.Point);
            button.ForeColor = Color.Transparent;
            button.Location = new Point(x_coor, y_coor); //**
            button.Margin = new Padding(0);
            button.Size = new Size(button_size, button_size); //**
            button.Text = stop_num; //**
            button.TextImageRelation = TextImageRelation.TextAboveImage;
            button.UseCompatibleTextRendering = true;
            button.UseVisualStyleBackColor = false;
            button.Click += (sender, e) => Mode_1_Button_Clicked(sender, e, stop_num);

            return button;
        }

        private Button Mode_1_Button(string stop_num, string chi_content, string eng_content)
        {
            // 
            // label_name
            // 
            Label label_name = new Label();
            label_name.Font = new Font("Microsoft Sans Serif", 27.75F, FontStyle.Regular, GraphicsUnit.Point);
            label_name.ForeColor = App_Colors.Text_white;
            label_name.Location = new Point(5, 5);
            label_name.Name = "label2";
            label_name.Size = new Size(65, 63);
            label_name.TabIndex = 5;
            label_name.Text = stop_num; //**
            label_name.TextAlign = ContentAlignment.MiddleCenter;
            SetWindowLong(label_name.Handle, GWL_STYLE, WS_DISABLED + GetWindowLong(label_name.Handle, GWL_STYLE));
            // 
            // label_name_bg
            // 
            PictureBox label_name_bg = new PictureBox();
            label_name_bg.BackColor = Color.Transparent;
            label_name_bg.Controls.Add(label_name);
            label_name_bg.Image = Image.FromFile(btn_red_image);
            label_name_bg.Location = new Point(10, 10);
            label_name_bg.Name = "pictureBox2";
            label_name_bg.Size = new Size(70, 70);
            label_name_bg.SizeMode = PictureBoxSizeMode.StretchImage;
            label_name_bg.TabIndex = 5;
            label_name_bg.TabStop = false;
            SetWindowLong(label_name_bg.Handle, GWL_STYLE, WS_DISABLED + GetWindowLong(label_name_bg.Handle, GWL_STYLE));
            // 
            // label_chi
            // 
            Label label_chi = new Label();
            label_chi.AutoSize = true;
            label_chi.BackColor = Color.Transparent;
            label_chi.ForeColor = App_Colors.Text_black;
            label_chi.Font = new Font("微軟正黑體", 30F, FontStyle.Bold, GraphicsUnit.Point);
            label_chi.Location = new Point(90, 0);
            label_chi.Name = "label_chi";
            label_chi.Size = new Size(442, 50);
            label_chi.TabIndex = 5;
            label_chi.Text = chi_content; //**
            SetWindowLong(label_chi.Handle, GWL_STYLE, WS_DISABLED + GetWindowLong(label_chi.Handle, GWL_STYLE));
            // 
            // label_eng
            // 
            Label label_eng = new Label();
            label_eng.AutoSize = true;
            label_eng.BackColor = Color.Transparent;
            label_eng.Font = new Font(ENG_FONT_NAME, 14.25F, FontStyle.Bold, GraphicsUnit.Point);
            label_eng.Location = new Point(90, 50);
            label_eng.Name = "label_eng";
            label_eng.Size = new Size(272, 48);
            label_eng.TabIndex = 6;
            label_eng.Text = eng_content; //**
            SetWindowLong(label_eng.Handle, GWL_STYLE, WS_DISABLED + GetWindowLong(label_eng.Handle, GWL_STYLE));
            // button
            // 
            Button button = new Button();
            button.BackColor = Color.Transparent;
            button.BackgroundImage = Image.FromFile(point_button_bg);
            button.BackgroundImageLayout = ImageLayout.Stretch;
            button.Controls.Add(label_name_bg);
            button.Controls.Add(label_chi);
            button.Controls.Add(label_eng);
            button.Cursor = Cursors.Hand;
            button.FlatAppearance.BorderSize = 0;
            button.FlatStyle = FlatStyle.Flat;
            button.Location = new Point(0, 0);
            button.Name = stop_num; //**
            button.Size = new Size(623, 95);
            button.TabIndex = 0;
            button.UseVisualStyleBackColor = false;
            button.Click += (sender, e) => Mode_1_Button_Clicked(sender, e, stop_num);

            return button;
        }


        private Panel ETA_Panel(string number, string chi_name, string eng_name)
        {
            // 
            // label_chi
            // 
            Label label_chi = new Label();
            label_chi.AutoSize = true;
            label_chi.BackColor = App_Colors.Green;
            label_chi.FlatStyle = FlatStyle.Flat;
            label_chi.Font = new Font("微軟正黑體", 30F, FontStyle.Bold, GraphicsUnit.Point);
            label_chi.ForeColor = App_Colors.Text_white;
            label_chi.Location = new Point(90, 0);
            label_chi.Margin = new Padding(0);
            label_chi.Name = "label_chi";
            label_chi.Size = new Size(222, 50);
            label_chi.TabIndex = 0;
            label_chi.Text = chi_name; //**
            // 
            // label_eng
            // 
            Label label_eng = new Label();
            label_eng.AutoSize = true;
            label_eng.BackColor = App_Colors.Green;
            label_eng.FlatStyle = FlatStyle.Flat;
            label_eng.Font = new Font(ENG_FONT_NAME, 14.25F, FontStyle.Bold, GraphicsUnit.Point);
            label_eng.ForeColor = App_Colors.Text_white;
            label_eng.Location = new Point(90, 50);
            label_eng.Margin = new Padding(0);
            label_eng.Name = "label_eng";
            label_eng.Size = new Size(49, 24);
            label_eng.TabIndex = 2;
            label_eng.Text = eng_name; //**
            label_eng.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // label_chi_2
            // 
            Label label_chi_2 = new Label();
            label_chi_2.AutoSize = true;
            label_chi_2.BackColor = App_Colors.Green;
            label_chi_2.FlatStyle = FlatStyle.Flat;
            label_chi_2.Font = new Font("微軟正黑體", 30F, FontStyle.Bold, GraphicsUnit.Point);
            label_chi_2.ForeColor = App_Colors.Text_white;
            label_chi_2.Location = new Point(538, 0);
            label_chi_2.Margin = new Padding(0);
            label_chi_2.Name = "label_chi_2";
            label_chi_2.Size = new Size(102, 50);
            label_chi_2.TabIndex = 8;
            label_chi_2.Text = "上車";
            //
            // label_eng_2
            // 
            Label label_eng_2 = new Label();
            label_eng_2.AutoSize = true;
            label_eng_2.BackColor = App_Colors.Green;
            label_eng_2.FlatStyle = FlatStyle.Flat;
            label_eng_2.Font = new Font(ENG_FONT_NAME, 15F, FontStyle.Bold, GraphicsUnit.Point);
            label_eng_2.ForeColor = App_Colors.Text_white;
            label_eng_2.Location = new Point(559, 50);
            label_eng_2.Margin = new Padding(0);
            label_eng_2.Name = "label_eng_2";
            label_eng_2.Size = new Size(81, 25);
            label_eng_2.TabIndex = 9;
            label_eng_2.Text = "aboard";
            // 
            // label_icon_name
            // 
            Label label_icon_name = new Label();
            label_icon_name.Font = new Font("Microsoft Sans Serif", 27.75F, FontStyle.Regular, GraphicsUnit.Point);
            label_icon_name.ForeColor = App_Colors.Text_white;
            label_icon_name.Location = new Point(5, 5);
            label_icon_name.Name = "label_icon_name";
            label_icon_name.Size = new Size(65, 63);
            label_icon_name.TabIndex = 5;
            label_icon_name.Text = number; //**
            label_icon_name.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // label_icon_bg
            // 
            PictureBox label_icon_bg = new PictureBox();
            label_icon_bg.BackColor = Color.Transparent;
            label_icon_bg.Controls.Add(label_icon_name);
            label_icon_bg.Image = Image.FromFile(btn_red_image);
            label_icon_bg.Location = new Point(4, 4);
            label_icon_bg.Name = "label_icon_bg";
            label_icon_bg.Size = new Size(70, 70);
            label_icon_bg.SizeMode = PictureBoxSizeMode.StretchImage;
            label_icon_bg.TabIndex = 2;
            label_icon_bg.TabStop = false;
            // 
            // pictureBox2
            // 
            PictureBox label_headtitle_bg = new PictureBox();
            label_headtitle_bg.BackColor = App_Colors.Green;
            label_headtitle_bg.Controls.Add(label_icon_bg);
            label_headtitle_bg.Controls.Add(label_chi);
            label_headtitle_bg.Controls.Add(label_eng);
            label_headtitle_bg.Controls.Add(label_chi_2);
            label_headtitle_bg.Controls.Add(label_eng_2);
            label_headtitle_bg.Location = new Point(0, 0);
            label_headtitle_bg.Margin = new Padding(0);
            label_headtitle_bg.Name = "label_headtitle_bg";
            label_headtitle_bg.Size = new Size(640, 79);
            label_headtitle_bg.TabIndex = 6;
            label_headtitle_bg.TabStop = false;
            // 
            // flowLayoutPanel
            // 
            FlowLayoutPanel flowLayoutPanel = new FlowLayoutPanel();
            flowLayoutPanel.AutoScroll = true;
            flowLayoutPanel.FlowDirection = FlowDirection.TopDown;
            flowLayoutPanel.Location = new Point(0, 79);
            flowLayoutPanel.Name = "flowLayoutPanel";
            flowLayoutPanel.Size = new Size(640, 571);
            flowLayoutPanel.TabIndex = 7;
            flowLayoutPanel.WrapContents = false;
            flowLayoutPanel.BackColor = App_Colors.Eta_bg;
            // 
            // panel1
            // 
            Panel panel_eta = new Panel();
            panel_eta.Controls.Add(label_headtitle_bg);
            panel_eta.Controls.Add(flowLayoutPanel);
            panel_eta.Location = new Point(640, 0);
            panel_eta.Name = "panel_eta";
            panel_eta.Size = new Size(640, 650);
            panel_eta.TabIndex = 6;

            return panel_eta;
        }

        private Panel Panel_bus_stop(string company_code, string stop_id, string stop_name_chi, string stop_name_eng)
        {
            // 
            // chi_name
            // 
            Label label_chi_name = new Label();
            label_chi_name.Font = new Font("微軟正黑體", 23.25F, FontStyle.Bold, GraphicsUnit.Point, 136);
            label_chi_name.Location = new Point(70, 10);
            label_chi_name.Name = "label_chi_name";
            label_chi_name.Size = new Size(550, 39);
            label_chi_name.Text = stop_name_chi; //**
            // 
            // icon
            // 
            PictureBox icon = new PictureBox();
            icon.Location = new Point(10, 16);
            icon.Name = "icon";
            icon.Size = new Size(50, 50);
            icon.TabStop = false;
            icon.BackgroundImage = Image.FromFile(stop_icon_image);
            icon.BackgroundImageLayout = ImageLayout.Stretch;
            // 
            // eng_name
            // 
            Label label_eng_name = new Label();
            label_eng_name.Font = new Font(ENG_FONT_NAME, 14.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label_eng_name.Location = new Point(70, 50);
            label_eng_name.Name = "eng_name";
            label_eng_name.Size = new Size(550, 24);
            label_eng_name.Text = stop_name_eng; //**
            // 
            // stop_box
            // 
            Panel stop_box = new Panel();
            stop_box.BackColor = App_Colors.Stop_box_bg;
            stop_box.Controls.Add(label_eng_name);
            stop_box.Controls.Add(icon);
            stop_box.Controls.Add(label_chi_name);
            stop_box.Location = new Point(0, 0);
            stop_box.Name = stop_id; //**
            stop_box.Size = new Size(623, 84);
            //
            // panel
            //
            Panel panel = new Panel();
            panel.Controls.Add(stop_box);
            panel.Location = new Point(0, 0);
            panel.Name = company_code; //**
            //panel.Size = stop_box.Size;
            panel.Size = new Size(stop_box.Width, stop_box.Height);
            panel.ControlAdded += Panel_ControlAdded;

            return panel;
        }

        private void Panel_ControlAdded(object sender, ControlEventArgs e)
        {
            Panel panel = sender as Panel;
            e.Control.Location = new Point(0, 84 + 74 * (panel.Controls.Count - 2));
            panel.Size = new(panel.Width, panel.Height + 74);
        }

        private Panel Panel_route_box(PictureBox icon, string route_name, string dest_name_chi, string dest_name_eng, string service_type = null)
        {
            // 
            // chi_name
            // 
            Label label_chi_name = new Label();
            label_chi_name.Font = new Font("微軟正黑體", 23.25F, FontStyle.Bold, GraphicsUnit.Point, 136);
            label_chi_name.Location = new Point(118, 10);
            label_chi_name.Name = "label_chi_name";
            label_chi_name.Size = new Size(550, 39);
            label_chi_name.Text = dest_name_chi; //**
            label_chi_name.ForeColor = App_Colors.Text_blue;
            label_chi_name.BackColor = Color.Transparent;
            // 
            // eng_name
            // 
            Label label_eng_name = new Label();
            label_eng_name.Font = new Font(ENG_FONT_NAME, 14.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label_eng_name.Location = new Point(118, 50);
            label_eng_name.Name = "eng_name";
            label_eng_name.Size = new Size(550, 24);
            label_eng_name.Text = dest_name_eng; //**
            label_eng_name.ForeColor = App_Colors.Text_blue;
            label_eng_name.BackColor = Color.Transparent;
            // 
            // stop_box
            // 
            Panel panel = new Panel();
            panel.BackColor = App_Colors.Eta_bg;
            panel.Controls.Add(label_eng_name);
            panel.Controls.Add(icon);
            panel.Controls.Add(label_chi_name);
            panel.Location = new Point(0, 0);
            panel.Name = "hello"; //**
            panel.Size = new Size(623, 74);

            
            return panel;
        }

        


        //bus icon
        private PictureBox Label_bus_icon(string company_code, string route_name)
        {
            Image icon_image = Image.FromFile($"{bus_icon_image_folder}{company_code.ToLower()}.png");

            Label label_route = new Label();
            label_route.Font = new Font(ENG_FONT_NAME, 16F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label_route.Name = "route_name";
            label_route.Size = new Size(97, 24);
            label_route.Text = route_name; //**
            label_route.TextAlign = ContentAlignment.MiddleCenter;
            label_route.BackColor = Color.Transparent;
            label_route.AutoSize = false;

            PictureBox icon = new PictureBox();
            icon.Controls.Add(label_route);
            icon.Location = new Point(10, 13);
            icon.Name = "icon";
            icon.Size = new Size(97, 61);
            icon.TabStop = false;
            icon.BackgroundImage = icon_image;
            icon.BackgroundImageLayout = ImageLayout.Stretch;

            if (company_code == "KMB")
            {
                label_route.Location = new Point(0, 29);
                label_route.ForeColor = App_Colors.Text_Bus;
            }
            else if (company_code == "LWB")
            {
                label_route.Location = new Point(0, 28);
                label_route.ForeColor = App_Colors.Text_Bus_LWB;
            }
            else if (company_code == "CTB")
            {
                label_route.Location = new Point(0, 27);
                label_route.ForeColor = App_Colors.Text_Bus;
            }
            else if (company_code == "CTBA")
            {
                label_route.Location = new Point(8, 27);
                label_route.ForeColor = App_Colors.Text_Bus_CTBA;
            }
            else if (company_code == "NLB")
            {
                label_route.Location = new Point(0, 32);
                label_route.ForeColor = App_Colors.Text_Bus;
            }
            else if (company_code == "GMB")
            {
                label_route.Location = new Point(0, 21);
                label_route.ForeColor = App_Colors.Text_Bus;
            }
            else if (company_code == "LRT")
            {
                label_route.Location = new Point(0, 29);
                label_route.ForeColor = App_Colors.Text_Bus;
            }
            else if (company_code == "MTRB")
            {
                label_route.Location = new Point(0, 32);
                label_route.ForeColor = App_Colors.Text_Bus_MTRB;
            }

            return icon;
        }
    }
}
