using System;
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
        public bool is_eta_start = false;


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
                IntValue value_file_stop_line_index = new IntValue
                {
                    Value = line_index,
                    Name = "value_file_line_index"
                };
                StringValue value_company_code = new StringValue
                {
                    Value = fileRead[line_index],
                    Name = "value_company_code"
                };
                StringValue value_stop_id = new StringValue {
                    Value = fileRead[line_index + 1],
                    Name = "value_stop_id"
                };
                string stop_name_chi, stop_name_eng;
                if (value_company_code.Value == "LRT")
                {
                    string[] stop_name = MTR_database.Get_Station_Name_By_Station_ID(value_stop_id.Value);
                    stop_name_chi = stop_name[0];
                    stop_name_eng = stop_name[1];
                }
                else
                {
                    stop_name_chi = fileRead[line_index + 2];
                    stop_name_eng = fileRead[line_index + 3];
                }
                Panel panel_bus_stop = Panel_bus_stop(value_company_code.Value, value_stop_id.Value, stop_name_chi, stop_name_eng);
                panel_bus_stop.Controls.Add(value_file_stop_line_index);
                panel_bus_stop.Controls.Add(value_company_code);
                panel_bus_stop.Controls.Add(value_stop_id);
                flowLayoutPanel.Controls.Add(panel_bus_stop);

                line_index = find_the_tag(line_index, "#route", fileRead.Skip(line_index).ToArray()) + 1;
                while(line_index < file_length)
                {
                    if (value_company_code.Value == "CTB")
                    {
                        // remember the value
                        IntValue value_file_route_line_index = new IntValue
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
                        IntValue value_stop_seq = new IntValue
                        {
                            Value = int.Parse(fileRead[line_index + 2]),
                            Name = "value_stop_seq"
                        };
                        string dest_name_chi = fileRead[line_index + 3];
                        string dest_name_eng = fileRead[line_index + 4];

                        PictureBox icon = Label_bus_icon(Bus_Route.is_CTBA_route(value_company_code.Value) ? "CTBA" : "CTB", value_route_name.Value);
                        Panel panel_route_box = Panel_route_box(icon, dest_name_chi, dest_name_eng);
                        panel_bus_stop.Controls.Add(panel_route_box);

                        panel_route_box.Controls.Add(value_file_route_line_index);
                        panel_route_box.Controls.Add(value_route_name);
                        panel_route_box.Controls.Add(value_direction);
                        panel_route_box.Controls.Add(value_stop_seq);

                        if (fileRead[line_index + 5] == "*end stop")
                        {
                            break;
                        }
                        else
                        {
                            line_index += 6;
                        }
                    }
                    else if (value_company_code.Value == "KMB")
                    {
                        // remember the value
                        IntValue value_file_route_line_index = new IntValue
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

                        PictureBox icon = Label_bus_icon(Bus_Route.is_LWB_route(value_route_name.Value) ? "LWB" : "KMB", value_route_name.Value);
                        Panel panel_route_box = Panel_route_box(icon, dest_name_chi, dest_name_eng);
                        panel_bus_stop.Controls.Add(panel_route_box);

                        panel_route_box.Controls.Add(value_file_route_line_index);
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
                    else if (value_company_code.Value == "LRT")
                    {
                        // remember the value
                        IntValue value_file_route_line_index = new IntValue
                        {
                            Value = line_index,
                            Name = "value_file_line_index"
                        };
                        StringValue value_platform_id = new StringValue
                        {
                            Value = fileRead[line_index],
                            Name = "value_platform_id"
                        };
                        StringValue value_route_name = new StringValue
                        {
                            Value = fileRead[line_index + 1],
                            Name = "value_route_name"
                        };
                        string[] dest_name = MTR_database.Get_Station_Name_By_Station_Code(fileRead[line_index + 2]);
                        string dest_name_chi = dest_name[0];
                        string dest_name_eng = dest_name[1];

                        PictureBox icon = Label_bus_icon(value_company_code.Value, value_route_name.Value);
                        Panel panel_route_box = Panel_route_box_LRT(icon, value_platform_id.Value, dest_name_chi, dest_name_eng);
                        panel_bus_stop.Controls.Add(panel_route_box);

                        panel_route_box.Controls.Add(value_file_route_line_index);
                        panel_route_box.Controls.Add(value_platform_id);
                        panel_route_box.Controls.Add(value_route_name);

                        if (fileRead[line_index + 3] == "*end stop")
                        {
                            break;
                        }
                        else
                        {
                            line_index += 4;
                        }
                    }
                    else if (value_company_code.Value == "MTRB")
                    {
                        // remember the value
                        IntValue value_file_route_line_index = new IntValue
                        {
                            Value = line_index,
                            Name = "value_file_line_index"
                        };
                        StringValue value_route_name = new StringValue
                        {
                            Value = fileRead[line_index],
                            Name = "value_route_name"
                        };
                        StringValue value_mtrb_stop_id = new StringValue
                        {
                            Value = fileRead[line_index + 1],
                            Name = "value_stop_id"
                        };
                        string dest_name_chi = fileRead[line_index + 2];
                        string dest_name_eng = fileRead[line_index + 3];

                        PictureBox icon = Label_bus_icon(value_company_code.Value, value_route_name.Value);
                        Panel panel_route_box = Panel_route_box(icon, dest_name_chi, dest_name_eng);
                        panel_bus_stop.Controls.Add(panel_route_box);

                        panel_route_box.Controls.Add(value_file_route_line_index);
                        panel_route_box.Controls.Add(value_mtrb_stop_id);
                        panel_route_box.Controls.Add(value_route_name);

                        if (fileRead[line_index + 4] == "*end stop")
                        {
                            break;
                        }
                        else
                        {
                            line_index += 5;
                        }
                    }
                    else if (value_company_code.Value == "GMB")
                    {
                        // remember the value
                        string[] route_info = fileRead[line_index].Split(" , ");
                        IntValue value_file_route_line_index = new IntValue
                        {
                            Value = line_index,
                            Name = "value_file_line_index"
                        };
                        StringValue value_route_name = new StringValue
                        {
                            Value = route_info[0],
                            Name = "value_route_name"
                        };
                        StringValue value_region = new StringValue
                        {
                            Value = route_info[1],
                            Name = "value_region"
                        };
                        StringValue value_route_id = new StringValue
                        {
                            Value = route_info[2],
                            Name = "value_route_id"
                        };
                        IntValue value_route_seq = new IntValue
                        {
                            Value = int.Parse(route_info[3]),
                            Name = "value_route_seq"
                        };
                        string description_tc = fileRead[line_index + 1];
                        string description_en = fileRead[line_index + 2];
                        string dest_name_chi = fileRead[line_index + 3];
                        string dest_name_eng = fileRead[line_index + 4];

                        PictureBox icon = Label_bus_icon(value_company_code.Value, value_route_name.Value);
                        Panel panel_route_box = Panel_route_box(icon, dest_name_chi, dest_name_eng);
                        panel_bus_stop.Controls.Add(panel_route_box);

                        panel_route_box.Controls.Add(value_file_route_line_index);
                        panel_route_box.Controls.Add(value_route_name);
                        panel_route_box.Controls.Add(value_region);
                        panel_route_box.Controls.Add(value_route_id);
                        panel_route_box.Controls.Add(value_route_seq);

                        if (fileRead[line_index + 5] == "*end stop")
                        {
                            break;
                        }
                        else
                        {
                            line_index += 6;
                        }
                    } else if (value_company_code.Value == "NLB")
                    {
                        // remember the value
                        string[] route_info = fileRead[line_index].Split(" , ");
                        IntValue value_file_route_line_index = new IntValue
                        {
                            Value = line_index,
                            Name = "value_file_line_index"
                        };
                        StringValue value_route_name = new StringValue
                        {
                            Value = route_info[0],
                            Name = "value_route_name"
                        };
                        StringValue value_route_id = new StringValue
                        {
                            Value = route_info[1],
                            Name = "value_route_id"
                        };
                        string dest_name_chi = fileRead[line_index + 1];
                        string dest_name_eng = fileRead[line_index + 2];

                        PictureBox icon = Label_bus_icon(value_company_code.Value, value_route_name.Value);
                        Panel panel_route_box = Panel_route_box(icon, dest_name_chi, dest_name_eng);
                        panel_bus_stop.Controls.Add(panel_route_box);

                        panel_route_box.Controls.Add(value_file_route_line_index);
                        panel_route_box.Controls.Add(value_route_name);
                        panel_route_box.Controls.Add(value_route_id);

                        if (fileRead[line_index + 3] == "*end stop")
                        {
                            break;
                        }
                        else
                        {
                            line_index += 4;
                        }
                    }
                }

                FitTheScreen.Resize(panel_bus_stop, 1280);
                FitTheScreen.Resize_Child(panel_bus_stop, 1280);
                is_eta_start = true;
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
            Label label_chi = new Label
            {
                AutoSize = true,
                BackColor = App_Colors.Green,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("微軟正黑體", 30F, FontStyle.Bold, GraphicsUnit.Point),
                ForeColor = App_Colors.Text_white,
                Location = new Point(90, 0),
                Margin = new Padding(0),
                Name = "label_chi",
                Size = new Size(222, 50),
                TabIndex = 0,
                Text = chi_name //**
            };
            // 
            // label_eng
            // 
            Label label_eng = new Label
            {
                AutoSize = true,
                BackColor = App_Colors.Green,
                FlatStyle = FlatStyle.Flat,
                Font = new Font(ENG_FONT_NAME, 14.25F, FontStyle.Bold, GraphicsUnit.Point),
                ForeColor = App_Colors.Text_white,
                Location = new Point(90, 50),
                Margin = new Padding(0),
                Name = "label_eng",
                Size = new Size(49, 24),
                TabIndex = 2,
                Text = eng_name, //**
                TextAlign = ContentAlignment.MiddleCenter
            };
            // 
            // label_chi_2
            // 
            Label label_chi_2 = new Label {
                AutoSize = true,
                BackColor = App_Colors.Green,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("微軟正黑體", 30F, FontStyle.Bold, GraphicsUnit.Point),
                ForeColor = App_Colors.Text_white,
                Location = new Point(538, 0),
                Margin = new Padding(0),
                Name = "label_chi_2",
                Size = new Size(102, 50),
                TabIndex = 8,
                Text = "上車"
            };
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
            ScrollingInPanel.Add_Module(flowLayoutPanel);
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
            //label_eng_name.AutoSize = false;
            // 
            // stop_box
            // 
            Panel stop_box = new Panel();
            stop_box.BackColor = App_Colors.Stop_box_bg;
            stop_box.Controls.Add(label_eng_name);
            stop_box.Controls.Add(icon);
            stop_box.Controls.Add(label_chi_name);
            stop_box.Location = new Point(0, 0);
            stop_box.Name = "stop box";
            stop_box.Size = new Size(623, 84);
            //
            // panel
            //
            Panel panel = new Panel
            {
                Location = new Point(0, 0),
                Name = "big stop box", //**
                Size = new Size(stop_box.Width, stop_box.Height)
            };
            panel.Controls.Add(stop_box);
            panel.ControlAdded += Panel_ControlAdded;

            return panel;
        }

        private void Panel_ControlAdded(object sender, ControlEventArgs e)
        {
            Panel panel = sender as Panel;
            e.Control.Location = new Point(0, panel.Height);
            if (e.Control is Panel)
            {
                panel.Size = new(panel.Width, panel.Height + e.Control.Height);
            }
        }

        private Panel Panel_route_box(PictureBox icon, string dest_name_chi, string dest_name_eng)
        {
            // 
            // chi_name
            // 
            Label label_chi_name = new Label
            {
                Font = new Font("微軟正黑體", 23.25F, FontStyle.Bold, GraphicsUnit.Point, 136),
                Location = new Point(118, 10),
                Name = "label_chi_name",
                Size = new Size(315, 39),
                Text = dest_name_chi, //**
                ForeColor = App_Colors.Text_blue,
                BackColor = Color.Transparent,
            };
            // 
            // eng_name
            // 
            Label label_eng_name = new Label
            {
                Font = new Font(ENG_FONT_NAME, 14.25F, FontStyle.Regular, GraphicsUnit.Point, 0),
                Location = new Point(118, 50),
                Name = "eng_name",
                Size = new Size(315, 24),
                Text = dest_name_eng, //**
                ForeColor = App_Colors.Text_blue,
                BackColor = Color.Transparent,
            };
            FitTheScreen.Resize_Label_Height(label_eng_name);
            //
            //ETA box
            //
            Panel eta_panel = new Panel
            {
                Location = new Point(433, 0), //623 - 140 - 50
                Name = "eta box", //**
                Size = new Size(152, 70),
                BackColor = Color.Transparent
            };

            Label label_eta_min_1 = new Label
            {
                Font = new Font("Arial Narrow", 42.25F, FontStyle.Bold, GraphicsUnit.Point, 136),
                TextAlign = ContentAlignment.TopRight,
                Location = new Point(-15, 0),
                Name = "eta 1",
                Size = new Size(80, 70),
                Text = "88", //**
                ForeColor = App_Colors.Text_blue,
                BackColor = Color.Transparent,
            };
            eta_panel.Controls.Add(label_eta_min_1);

            Label label_eta_min_1_chi = new Label
            {
                Font = new Font("微軟正黑體", 9.25F, FontStyle.Bold, GraphicsUnit.Point, 136),
                TextAlign = ContentAlignment.TopLeft,
                Location = new Point(47, 28),
                Name = "eta 1 chi",
                Size = new Size(25, 21),
                Text = "分", //**
                ForeColor = App_Colors.Text_blue,
                BackColor = Color.Transparent,
            };

            Label label_eta_min_1_eng = new Label
            {
                Font = new Font("Arial Narrow", 9.25F, FontStyle.Bold, GraphicsUnit.Point, 136),
                TextAlign = ContentAlignment.TopLeft,
                Location = new Point(47, 43),
                Name = "eta 1 eng",
                Size = new Size(25, 21),
                Text = "min", //**
                ForeColor = App_Colors.Text_blue,
                BackColor = Color.Transparent,
            };
            eta_panel.Controls.Add(label_eta_min_1_chi);
            eta_panel.Controls.Add(label_eta_min_1_eng);
            label_eta_min_1_chi.BringToFront();
            label_eta_min_1_eng.BringToFront();

            Label label_eta_min_2 = new Label
            {
                Font = new Font("Arial Narrow", 42.25F, FontStyle.Bold, GraphicsUnit.Point, 136),
                TextAlign = ContentAlignment.TopRight,
                Location = new Point(65, 0),
                Name = "eta 2",
                Size = new Size(80, 70),
                Text = "88", //**
                ForeColor = App_Colors.Text_blue,
                BackColor = Color.Transparent,
            };
            eta_panel.Controls.Add(label_eta_min_2);

            Label label_eta_min_2_chi = new Label
            {
                Font = new Font("微軟正黑體", 9.25F, FontStyle.Bold, GraphicsUnit.Point, 136),
                TextAlign = ContentAlignment.TopLeft,
                Location = new Point(127, 28),
                Name = "eta 2 chi",
                Size = new Size(25, 21),
                Text = "分", //**
                ForeColor = App_Colors.Text_blue,
                BackColor = Color.Transparent,
            };

            Label label_eta_min_2_eng = new Label
            {
                Font = new Font("Arial Narrow", 9.25F, FontStyle.Bold, GraphicsUnit.Point, 136),
                TextAlign = ContentAlignment.TopLeft,
                Location = new Point(127, 43),
                Name = "eta 2 eng",
                Size = new Size(25, 21),
                Text = "min", //**
                ForeColor = App_Colors.Text_blue,
                BackColor = Color.Transparent,
            };
            eta_panel.Controls.Add(label_eta_min_2_chi);
            eta_panel.Controls.Add(label_eta_min_2_eng);
            label_eta_min_2_chi.BringToFront();
            label_eta_min_2_eng.BringToFront();
            // 
            // stop_box
            // 
            Panel panel = new Panel {
                Location = new Point(0, 0),
                Name = "route box", //**
                Size = new Size(623, 50 + (label_eng_name.Height < 24 ? 24:label_eng_name.Height)),
                BackColor = App_Colors.Eta_bg
            };
            panel.Controls.Add(label_eng_name);
            panel.Controls.Add(icon);
            panel.Controls.Add(label_chi_name);
            panel.Controls.Add(eta_panel);

            
            return panel;
        }

        private Panel Panel_route_box_LRT(PictureBox icon, string platform_id, string dest_name_chi, string dest_name_eng)
        {
            // 
            // chi_name
            // 
            Label label_chi_name = new Label
            {
                Font = new Font("微軟正黑體", 23.25F, FontStyle.Bold, GraphicsUnit.Point, 136),
                Location = new Point(118, 10),
                Name = "label_chi_name",
                Size = new Size(315 - 80, 39),
                Text = dest_name_chi, //**
                ForeColor = App_Colors.Text_blue,
                BackColor = Color.Transparent,
            };
            // 
            // eng_name
            // 
            Label label_eng_name = new Label
            {
                Font = new Font(ENG_FONT_NAME, 14.25F, FontStyle.Regular, GraphicsUnit.Point, 0),
                Location = new Point(118, 50),
                Name = "eng_name",
                Size = new Size(315 - 80, 24),
                Text = dest_name_eng, //**
                ForeColor = App_Colors.Text_blue,
                BackColor = Color.Transparent,
            };
            FitTheScreen.Resize_Label_Height(label_eng_name);
            PictureBox label_platform_id_bg = new PictureBox
            {
                Location = new Point(433 - 80, 5),
                Name = "label_platform_id_bg",
                Size = new Size(65, 65),
                BackgroundImage = Image.FromFile($"{image_folder}LRT_platform_icon.png"),
                BackgroundImageLayout = ImageLayout.Stretch,
            };
            Label label_platform_id = new Label
            {
                Font = new Font("Arial Narrow", 42.25F, FontStyle.Bold, GraphicsUnit.Point, 136),
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(0, 0),
                Name = "label_platform_id",
                Size = new Size(70, 70),
                Text = platform_id, //**
                ForeColor = App_Colors.Text_dark_blue,
                BackColor = Color.Transparent
            };
            label_platform_id_bg.Controls.Add(label_platform_id);
            //
            //ETA box
            //
            Panel eta_panel = new Panel
            {
                Location = new Point(433, 0), //623 - 140 - 50
                Name = "eta box", //**
                Size = new Size(152, 70),
                BackColor = Color.Transparent
            };

            Label label_eta_min_1 = new Label
            {
                Font = new Font("Arial Narrow", 42.25F, FontStyle.Bold, GraphicsUnit.Point, 136),
                TextAlign = ContentAlignment.TopRight,
                Location = new Point(-15, 0),
                Name = "eta 1",
                Size = new Size(80, 70),
                Text = "88", //**
                ForeColor = App_Colors.Text_blue,
                BackColor = Color.Transparent,
            };
            eta_panel.Controls.Add(label_eta_min_1);

            Label label_eta_min_1_chi = new Label
            {
                Font = new Font("微軟正黑體", 9.25F, FontStyle.Bold, GraphicsUnit.Point, 136),
                TextAlign = ContentAlignment.TopLeft,
                Location = new Point(47, 28),
                Name = "eta 1 chi",
                Size = new Size(25, 21),
                Text = "分", //**
                ForeColor = App_Colors.Text_blue,
                BackColor = Color.Transparent,
            };

            Label label_eta_min_1_eng = new Label
            {
                Font = new Font("Arial Narrow", 9.25F, FontStyle.Bold, GraphicsUnit.Point, 136),
                TextAlign = ContentAlignment.TopLeft,
                Location = new Point(47, 43),
                Name = "eta 1 eng",
                Size = new Size(25, 21),
                Text = "min", //**
                ForeColor = App_Colors.Text_blue,
                BackColor = Color.Transparent,
            };
            eta_panel.Controls.Add(label_eta_min_1_chi);
            eta_panel.Controls.Add(label_eta_min_1_eng);
            label_eta_min_1_chi.BringToFront();
            label_eta_min_1_eng.BringToFront();

            PictureBox label_car_1 = new PictureBox
            {
                Location = new Point(70, 0),
                Name = "label_car_1",
                Size = new Size(35, 70),
                BackgroundImage = Image.FromFile($"{image_folder}LRV.png"),
                BackgroundImageLayout = ImageLayout.Stretch,
            };
            PictureBox label_car_2 = new PictureBox
            {
                Location = new Point(105, 0),
                Name = "label_car_2",
                Size = new Size(35, 70),
                BackgroundImage = Image.FromFile($"{image_folder}LRV.png"),
                BackgroundImageLayout = ImageLayout.Stretch,
            };
            eta_panel.Controls.Add(label_car_1);
            eta_panel.Controls.Add(label_car_2);
            label_car_1.BringToFront();
            label_car_2.BringToFront();
            // 
            // stop_box
            // 
            Panel panel = new Panel
            {
                Location = new Point(0, 0),
                Name = "route box", //**
                Size = new Size(623, 50 + (label_eng_name.Height < 24 ? 24 : label_eng_name.Height)),
                BackColor = App_Colors.Eta_bg
            };
            panel.Controls.Add(label_eng_name);
            panel.Controls.Add(icon);
            panel.Controls.Add(label_chi_name);
            panel.Controls.Add(label_platform_id_bg);
            panel.Controls.Add(eta_panel);


            return panel;
        }

        public Panel Panel_ETA_Box_No_Route()
        {
            //
            //ETA box
            //
            Panel eta_panel = new Panel
            {
                Location = new Point(433, 0), //623 - 140 - 50
                Name = "eta box sp", //**
                Size = new Size(152, 70),
                BackColor = Color.Transparent
            };

            Label label_chi = new Label
            {
                Font = new Font("微軟正黑體", 9.25F, FontStyle.Bold, GraphicsUnit.Point, 136),
                TextAlign = ContentAlignment.TopLeft,
                Location = new Point(0, 28),
                Name = "eta 1 chi",
                Size = new Size(25, 21),
                Text = "此站沒有此路線", //**
                AutoSize = true,
                ForeColor = Color.Red,
                BackColor = Color.Transparent,
            };

            Label label_eng = new Label
            {
                Font = new Font("Arial Narrow", 9.25F, FontStyle.Bold, GraphicsUnit.Point, 136),
                TextAlign = ContentAlignment.TopLeft,
                Location = new Point(0, 43),
                Name = "eta 1 eng",
                Size = new Size(25, 21),
                Text = "No this route at this stop", //**
                AutoSize = true,
                ForeColor = Color.Red,
                BackColor = Color.Transparent,
            };
            eta_panel.Controls.Add(label_chi);
            eta_panel.Controls.Add(label_eng);

            return eta_panel;
        }

        public Panel Panel_ETA_Box_Remark(string chi_content, string eng_content)
        {
            //
            //ETA box
            //
            Panel eta_panel = new Panel
            {
                Location = new Point(433, 0), //623 - 140 - 50
                Name = "eta box sp", //**
                Size = new Size(152, 70),
                BackColor = Color.Transparent
            };

            Label label_chi = new Label
            {
                Font = new Font("微軟正黑體", 9.25F, FontStyle.Bold, GraphicsUnit.Point, 136),
                TextAlign = ContentAlignment.TopLeft,
                Location = new Point(0, 28),
                Name = "eta 1 chi",
                Size = new Size(25, 21),
                Text = chi_content, //**
                AutoSize = true,
                ForeColor = App_Colors.Text_blue,
                BackColor = Color.Transparent,
            };

            Label label_eng = new Label
            {
                Font = new Font("Arial Narrow", 9.25F, FontStyle.Bold, GraphicsUnit.Point, 136),
                TextAlign = ContentAlignment.TopLeft,
                Location = new Point(0, 43),
                Name = "eta 1 eng",
                Size = new Size(150, 21),
                Text = eng_content, //**
                ForeColor = App_Colors.Text_blue,
                BackColor = Color.Transparent,
            };
            FitTheScreen.Resize_Label_Height(label_eng);
            eta_panel.Controls.Add(label_chi);
            eta_panel.Controls.Add(label_eng);

            return eta_panel;
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
