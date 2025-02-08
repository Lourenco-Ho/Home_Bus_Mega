using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text.Json;
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

        private CancellationTokenSource cancellationTokenSource;

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

            this.ControlAdded += MainForm_ControlAdded;

        }

        private async void MainForm_ControlAdded(object sender, ControlEventArgs e)
        {
            if (e.Control is Panel)
            {
                if ( ((Panel)e.Control).Name == "panel_eta")
                {
                    await Async_GettingETA((FlowLayoutPanel)e.Control.Controls["flowLayoutPanel"]);
                }
            }
        }

        public async Task Async_GettingETA(FlowLayoutPanel flowLayoutPanel)
        {
            while(mapa.mode == 0)
            {
                foreach(Panel big_stop_box in flowLayoutPanel.Controls)
                {
                    if (big_stop_box.Name == "big stop box")
                    {
                        string company_code = ((StringValue)big_stop_box.Controls["value_company_code"]).Value;
                        string stop_id = ((StringValue)big_stop_box.Controls["value_stop_id"]).Value;
                        Debug.WriteLine(company_code);

                        if (company_code == "KMB")
                        {
                            string url = API_Link.KMB_stop_eta_url + stop_id;
                            Debug.WriteLine(url);

                            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                            string json_string_response = new StreamReader(response.GetResponseStream()).ReadToEnd();

                            JsonElement json_data = (JsonDocument.Parse(json_string_response)).RootElement.GetProperty("data");
                            Debug.WriteLine(json_data);

                            foreach (Control route_box in big_stop_box.Controls)
                            {
                                if (route_box.Name == "route box")
                                {
                                    bool is_found = false;
                                    string route_name = ((StringValue)route_box.Controls["value_route_name"]).Value;
                                    string direction = ((StringValue)route_box.Controls["value_direction"]).Value == "inbound" ? "I":"O";
                                    int service_type = ((IntValue)route_box.Controls["value_service_type"]).Value;

                                    foreach (JsonElement jsonobj_route in json_data.EnumerateArray())
                                    {
                                        if (jsonobj_route.GetProperty("route").ToString() == route_name &&
                                            jsonobj_route.GetProperty("dir").ToString() == direction &&
                                            jsonobj_route.GetProperty("service_type").GetInt32() == service_type &&
                                            jsonobj_route.TryGetProperty("eta", out JsonElement ageElement))
                                        {
                                            if(ageElement.ValueKind != JsonValueKind.Null) //if have ETA
                                            {
                                                Debug.WriteLine(jsonobj_route.GetProperty("route"));
                                                string eta_timestamp = jsonobj_route.GetProperty("eta").ToString();

                                                DateTimeOffset eta_DateTime = DateTimeOffset.Parse(eta_timestamp);
                                                Debug.WriteLine(eta_DateTime);

                                                DateTimeOffset current_DateTime = DateTimeOffset.UtcNow; // get the time now（UTC）
                                                int timeDifference = (int)Math.Round((eta_DateTime - current_DateTime).TotalMinutes); // 計算時間差
                                                timeDifference = (timeDifference < 0) ? 0 : timeDifference;
                                                Debug.WriteLine(timeDifference);

                                                //eta_seq
                                                if (jsonobj_route.GetProperty("eta_seq").GetInt32() == 1)
                                                {
                                                    route_box.Controls["eta box"].Controls["eta 1"].Text = timeDifference.ToString();
                                                }
                                                else if (jsonobj_route.GetProperty("eta_seq").GetInt32() == 2)
                                                {
                                                    route_box.Controls["eta box"].Controls["eta 2"].Text = timeDifference.ToString();
                                                }

                                                is_found = true;
                                            }
                                            else //找到路線但沒有ETA
                                            {
                                                is_found = true;

                                                //remark
                                                string rmk_tc = jsonobj_route.GetProperty("rmk_tc").ToString();
                                                rmk_tc = rmk_tc.Length == 0 ? "非服務時間" : rmk_tc;
                                                string rmk_en = jsonobj_route.GetProperty("rmk_en").ToString();
                                                rmk_en = rmk_en.Length == 0 ? "Non-service hours" : rmk_en;

                                                Panel sp_eta_box = mapa.Panel_ETA_Box_Remark(rmk_tc, rmk_en);
                                                FitTheScreen.Resize_Child(sp_eta_box, 1280);
                                                FitTheScreen.Resize(sp_eta_box, 1280);
                                                route_box.Controls.Add(sp_eta_box);
                                                sp_eta_box.BringToFront();
                                            }
                                        }else if(is_found) //如果找到符合之後, 再找不到
                                        {
                                            break;
                                        }
                                    }

                                    if (!is_found)
                                    {
                                        route_box.Controls["eta box"].Controls["eta 1"].Text = "89";

                                        //此站沒有此路線
                                        Panel sp_eta_box = mapa.Panel_ETA_Box_No_Route();
                                        FitTheScreen.Resize_Child(sp_eta_box, 1280);
                                        FitTheScreen.Resize(sp_eta_box, 1280);
                                        route_box.Controls.Add(sp_eta_box);
                                        sp_eta_box.BringToFront();
                                    }
                                }
                            }

                            
                            

                        }
                    }
                }
                if (mapa.is_eta_start)
                {
                    await Task.Delay(10000);
                    Debug.WriteLine("File read complete!!!");
                }
                else
                {
                    await Task.Delay(100);
                    Debug.WriteLine("Waiting file read complete");
                }
            }
        }


        //switch mode buttons

        private void switch_mode(int mode)
        {
            Controls.Remove(Controls["panel_eta"]);
            mapa.mode = mode;
            mapa.is_eta_start = false;
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
