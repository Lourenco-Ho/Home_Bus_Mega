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

        //user variable
        string using_map;

        public MainForm()
        {
            InitializeComponent();
            label_headtitle.Parent = header_bg_label;

            process_Loc_Stat = new Process_loc_stat();
            mapa = new Mapa(this);
            ScrollingInPanel.Add_Module(flowLayoutPanel);


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
                            Async_GettingETA_KMB(big_stop_box, stop_id);
                        } else if (company_code == "CTB" || company_code == "CTBA")
                        {
                            Async_GettingETA_CTB(big_stop_box, stop_id);
                        } else if (company_code == "GMB")
                        {
                            Async_GettingETA_GMB(big_stop_box, stop_id);
                        } else if (company_code == "NLB")
                        {
                            Async_GettingETA_NLB(big_stop_box, stop_id);
                        } else if (company_code == "LRT")
                        {
                            Async_GettingETA_LRT(big_stop_box, stop_id);
                        } else if (company_code == "MTRB")
                        {
                            Async_GettingETA_MTRB(big_stop_box);
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

        public void Async_GettingETA_KMB(Panel big_stop_box, string stop_id)
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
                    bool is_found_route = false;
                    bool[] is_found_eta = { false, false };
                    string route_name = ((StringValue)route_box.Controls["value_route_name"]).Value;
                    string direction = ((StringValue)route_box.Controls["value_direction"]).Value == "inbound" ? "I" : "O";
                    int service_type = ((IntValue)route_box.Controls["value_service_type"]).Value;

                    foreach (JsonElement jsonobj_route in json_data.EnumerateArray())
                    {
                        if (jsonobj_route.GetProperty("route").ToString() == route_name &&
                            jsonobj_route.GetProperty("dir").ToString() == direction &&
                            jsonobj_route.GetProperty("service_type").GetInt32() == service_type &&
                            jsonobj_route.TryGetProperty("eta", out JsonElement ageElement))
                        {
                            if (ageElement.ValueKind != JsonValueKind.Null) //if have ETA
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
                                    route_box.Controls["eta box"].Controls["eta 1"].Visible = true;
                                    route_box.Controls["eta box"].Controls["eta 1 chi"].Visible = true;
                                    route_box.Controls["eta box"].Controls["eta 1 eng"].Visible = true;
                                    is_found_eta[0] = true;
                                }
                                else if (jsonobj_route.GetProperty("eta_seq").GetInt32() == 2)
                                {
                                    route_box.Controls["eta box"].Controls["eta 2"].Text = timeDifference.ToString();
                                    route_box.Controls["eta box"].Controls["eta 2"].Visible = true;
                                    route_box.Controls["eta box"].Controls["eta 2 chi"].Visible = true;
                                    route_box.Controls["eta box"].Controls["eta 2 eng"].Visible = true;
                                    is_found_eta[1] = true;
                                }

                                is_found_route = true;
                            }
                            else //找到路線但沒有ETA
                            {
                                is_found_route = true;

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
                        }
                        else if (is_found_route) //如果找到符合之後, 再找不到
                        {
                            break;
                        }
                    }

                    if (!is_found_route)
                    {
                        route_box.Controls["eta box"].Controls["eta 1"].Text = "89";

                        //此站沒有此路線
                        Panel sp_eta_box = mapa.Panel_ETA_Box_No_Route();
                        FitTheScreen.Resize_Child(sp_eta_box, 1280);
                        FitTheScreen.Resize(sp_eta_box, 1280);
                        route_box.Controls.Add(sp_eta_box);
                        sp_eta_box.BringToFront();
                    }

                    if (!is_found_eta[0])
                    {
                        route_box.Controls["eta box"].Controls["eta 1"].Text = "?";
                        route_box.Controls["eta box"].Controls["eta 1"].Visible = false;
                        route_box.Controls["eta box"].Controls["eta 1 chi"].Visible = false;
                        route_box.Controls["eta box"].Controls["eta 1 eng"].Visible = false;
                    }

                    if (!is_found_eta[1])
                    {
                        route_box.Controls["eta box"].Controls["eta 2"].Text = "?";
                        route_box.Controls["eta box"].Controls["eta 2"].Visible = false;
                        route_box.Controls["eta box"].Controls["eta 2 chi"].Visible = false;
                        route_box.Controls["eta box"].Controls["eta 2 eng"].Visible = false;
                    }
                }
            }
        }


        public void Async_GettingETA_CTB(Panel big_stop_box, string stop_id)
        {
            foreach (Control route_box in big_stop_box.Controls)
            {
                if (route_box.Name == "route box")
                {
                    bool is_found_route = false;
                    bool[] is_found_eta = { false, false };
                    string route_name = ((StringValue)route_box.Controls["value_route_name"]).Value;
                    string direction = ((StringValue)route_box.Controls["value_direction"]).Value == "inbound" ? "I" : "O";
                    int stop_seq = ((IntValue)route_box.Controls["value_stop_seq"]).Value;

                    string url = $"{API_Link.CTB_eta_url}{stop_id}/{route_name}";
                    Debug.WriteLine(url);

                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                    string json_string_response = new StreamReader(response.GetResponseStream()).ReadToEnd();

                    JsonElement json_data = (JsonDocument.Parse(json_string_response)).RootElement.GetProperty("data");
                    Debug.WriteLine(json_data);

                    foreach (JsonElement jsonobj_route in json_data.EnumerateArray())
                    {
                        if (jsonobj_route.GetProperty("dir").ToString() == direction &&
                            jsonobj_route.GetProperty("seq").GetInt32() == stop_seq &&
                            jsonobj_route.TryGetProperty("eta", out JsonElement ageElement))
                        {
                            if (ageElement.ValueKind != JsonValueKind.Null && jsonobj_route.GetProperty("eta").GetString() != "") //if have ETA
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
                                if (!is_found_eta[0])
                                {
                                    route_box.Controls["eta box"].Controls["eta 1"].Text = timeDifference.ToString();
                                    route_box.Controls["eta box"].Controls["eta 1"].Visible = true;
                                    route_box.Controls["eta box"].Controls["eta 1 chi"].Visible = true;
                                    route_box.Controls["eta box"].Controls["eta 1 eng"].Visible = true;
                                    is_found_eta[0] = true;
                                }
                                else if (!is_found_eta[1])
                                {
                                    route_box.Controls["eta box"].Controls["eta 2"].Text = timeDifference.ToString();
                                    route_box.Controls["eta box"].Controls["eta 2"].Visible = true;
                                    route_box.Controls["eta box"].Controls["eta 2 chi"].Visible = true;
                                    route_box.Controls["eta box"].Controls["eta 2 eng"].Visible = true;
                                    is_found_eta[1] = true;
                                }

                                is_found_route = true;
                            }
                        }
                        else if (is_found_route) //如果找到符合之後, 再找不到
                        {
                            break;
                        }
                    }

                    if (!is_found_route)
                    {
                        //remark
                        Panel sp_eta_box = mapa.Panel_ETA_Box_Remark("非服務時間", "Non-service hours");
                        FitTheScreen.Resize_Child(sp_eta_box, 1280);
                        FitTheScreen.Resize(sp_eta_box, 1280);
                        route_box.Controls.Add(sp_eta_box);
                        sp_eta_box.BringToFront();
                    }

                    if (!is_found_eta[0])
                    {
                        route_box.Controls["eta box"].Controls["eta 1"].Text = "?";
                        route_box.Controls["eta box"].Controls["eta 1"].Visible = false;
                        route_box.Controls["eta box"].Controls["eta 1 chi"].Visible = false;
                        route_box.Controls["eta box"].Controls["eta 1 eng"].Visible = false;
                    }

                    if (!is_found_eta[1])
                    {
                        route_box.Controls["eta box"].Controls["eta 2"].Text = "?";
                        route_box.Controls["eta box"].Controls["eta 2"].Visible = false;
                        route_box.Controls["eta box"].Controls["eta 2 chi"].Visible = false;
                        route_box.Controls["eta box"].Controls["eta 2 eng"].Visible = false;
                    }
                }
            }
        }

        public void Async_GettingETA_LRT(Panel big_stop_box, string stop_id)
        {
            string url = API_Link.LRT_eta_url + stop_id;
            Debug.WriteLine(url);

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            string json_string_response = new StreamReader(response.GetResponseStream()).ReadToEnd();

            JsonElement json_data = (JsonDocument.Parse(json_string_response)).RootElement.GetProperty("platform_list");
            Debug.WriteLine(json_data);

            foreach (Control route_box in big_stop_box.Controls)
            {
                if (route_box.Name == "route box")
                {
                    bool is_found_eta = false;
                    string route_name = ((StringValue)route_box.Controls["value_route_name"]).Value;
                    string platform_id = ((StringValue)route_box.Controls["value_platform_id"]).Value;

                    //find the correct platform's jsonobject
                    foreach (JsonElement jsonobj_platform in json_data.EnumerateArray())
                    {
                        if (jsonobj_platform.GetProperty("platform_id").ToString() == platform_id)
                        {
                            //find the correct route's jsonobject
                            foreach (JsonElement jsonobj_route in jsonobj_platform.GetProperty("route_list").EnumerateArray())
                            {
                                if (jsonobj_route.GetProperty("route_no").ToString() == route_name)
                                {
                                    //Debug.WriteLine(jsonobj_route.GetProperty("route_no"));

                                    //顯示ETA
                                    string eta_string = jsonobj_route.GetProperty("time_en").ToString().Split(" min")[0];
                                    if (eta_string == "Arriving" || eta_string == "Departing" || eta_string == "-")
                                    {
                                        eta_string = "0";
                                    }
                                    Debug.WriteLine(jsonobj_route.GetProperty("route_no") + " :" + eta_string);

                                    route_box.Controls["eta box"].Controls["eta 1"].Text = eta_string;
                                    route_box.Controls["eta box"].Controls["eta 1"].Visible = true;
                                    route_box.Controls["eta box"].Controls["eta 1 chi"].Visible = true;
                                    route_box.Controls["eta box"].Controls["eta 1 eng"].Visible = true;
                                    is_found_eta = true;

                                    //顯示列車長度
                                    if(jsonobj_route.GetProperty("train_length").GetInt32() == 1)
                                    {
                                        route_box.Controls["eta box"].Controls["label_car_1"].Visible = true;
                                        route_box.Controls["eta box"].Controls["label_car_2"].Visible = false;
                                    }
                                    else
                                    {
                                        route_box.Controls["eta box"].Controls["label_car_1"].Visible = true;
                                        route_box.Controls["eta box"].Controls["label_car_2"].Visible = true;
                                    }

                                    break;
                                }
                            }
                        }
                    }

                    if (!is_found_eta)
                    {
                        //隱藏分鐘顥示
                        route_box.Controls["eta box"].Controls["eta 1"].Text = "?";
                        route_box.Controls["eta box"].Controls["eta 1"].Visible = false;
                        route_box.Controls["eta box"].Controls["eta 1 chi"].Visible = false;
                        route_box.Controls["eta box"].Controls["eta 1 eng"].Visible = false;
                        route_box.Controls["eta box"].Controls["label_car_1"].Visible = false;
                        route_box.Controls["eta box"].Controls["label_car_2"].Visible = false;

                        //顯示未找到ETA
                        Panel sp_eta_box = mapa.Panel_ETA_Box_Remark("系統未提供ETA", "ETA is not found in the server");
                        FitTheScreen.Resize_Child(sp_eta_box, 1280);
                        FitTheScreen.Resize(sp_eta_box, 1280);
                        route_box.Controls.Add(sp_eta_box);
                        sp_eta_box.BringToFront();
                    }

                }
            }
        }

        public void Async_GettingETA_MTRB(Panel big_stop_box)
        {
            foreach (Control route_box in big_stop_box.Controls)
            {
                if (route_box.Name == "route box")
                {
                    bool[] is_found_eta = { false, false };
                    string route_name = ((StringValue)route_box.Controls["value_route_name"]).Value;
                    string stop_id = ((StringValue)route_box.Controls["value_stop_id"]).Value;

                    string request_json = "{\"language\": \"zh\", \"routeName\": \"" + route_name +"\"}";
                    JsonDocument json_data = JsonDocument.Parse(Http_Helper.Post(API_Link.MTRB_eta_url, request_json));
                    JsonElement json_busStop = json_data.RootElement.GetProperty("busStop");
                    Debug.WriteLine(route_name);
                    Debug.WriteLine(json_busStop);

                    foreach (JsonElement jsonobj_stop in json_busStop.EnumerateArray())
                    {
                        if(jsonobj_stop.GetProperty("busStopId").GetString() == $"{route_name}-{stop_id}")
                        {
                            int eta_seq = 1;
                            foreach (JsonElement jsonobj_bus in jsonobj_stop.GetProperty("bus").EnumerateArray())
                            {
                                string eta_string = jsonobj_bus.GetProperty("arrivalTimeText").GetString().Split(" 分鐘")[0];
                                if (eta_string == "即將到站/已離開")
                                {
                                    eta_string = "0";
                                }

                                //eta_seq;
                                if (eta_seq == 1)
                                {
                                    route_box.Controls["eta box"].Controls["eta 1"].Text = eta_string;
                                    route_box.Controls["eta box"].Controls["eta 1"].Visible = true;
                                    route_box.Controls["eta box"].Controls["eta 1 chi"].Visible = true;
                                    route_box.Controls["eta box"].Controls["eta 1 eng"].Visible = true;
                                    is_found_eta[0] = true;

                                    eta_seq++;
                                }
                                else if (eta_seq == 2)
                                {
                                    route_box.Controls["eta box"].Controls["eta 2"].Text = eta_string;
                                    route_box.Controls["eta box"].Controls["eta 2"].Visible = true;
                                    route_box.Controls["eta box"].Controls["eta 2 chi"].Visible = true;
                                    route_box.Controls["eta box"].Controls["eta 2 eng"].Visible = true;
                                    is_found_eta[1] = true;
                                    break;
                                }

                                
                                
                            }
                        }
                    }

                    if (!is_found_eta[0] && !is_found_eta[1])
                    {
                        //remark
                        Panel sp_eta_box = mapa.Panel_ETA_Box_Remark("非服務時間", "Non-service hours");
                        FitTheScreen.Resize_Child(sp_eta_box, 1280);
                        FitTheScreen.Resize(sp_eta_box, 1280);
                        route_box.Controls.Add(sp_eta_box);
                        sp_eta_box.BringToFront();
                    }

                    if (!is_found_eta[0])
                    {
                        route_box.Controls["eta box"].Controls["eta 1"].Text = "?";
                        route_box.Controls["eta box"].Controls["eta 1"].Visible = false;
                        route_box.Controls["eta box"].Controls["eta 1 chi"].Visible = false;
                        route_box.Controls["eta box"].Controls["eta 1 eng"].Visible = false;
                    }

                    if (!is_found_eta[1])
                    {
                        route_box.Controls["eta box"].Controls["eta 2"].Text = "?";
                        route_box.Controls["eta box"].Controls["eta 2"].Visible = false;
                        route_box.Controls["eta box"].Controls["eta 2 chi"].Visible = false;
                        route_box.Controls["eta box"].Controls["eta 2 eng"].Visible = false;
                    }
                }
            }
        }

        public void Async_GettingETA_GMB(Panel big_stop_box, string stop_id)
        {
            string url = API_Link.GMB_eta_url + stop_id;
            string json_string_response;
            Debug.WriteLine(url);

            // 获取用户主目录
            string userFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            // 构造 Python 可执行文件路径
            string pythonExePath = Path.Combine(userFolder, @"AppData\Local\Microsoft\WindowsApps\python.exe");
            // Python 脚本路径
            string pythonFilePath = "GMB_api.py"; // 替换为实际路径

            ProcessStartInfo start = new ProcessStartInfo
            {
                FileName = pythonExePath,
                Arguments = $"{pythonFilePath} \"{url}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };

            using (Process process = Process.Start(start))
            {
                using (StreamReader reader = process.StandardOutput)
                {
                    //ReadFile();
                    json_string_response = File.ReadAllText("GMB_api_result.txt");
                    File.Delete("GMB_api_result.txt");
                    Debug.WriteLine(json_string_response);
                }
            }

            JsonElement json_data = (JsonDocument.Parse(json_string_response)).RootElement.GetProperty("data");
            Debug.WriteLine(json_data);

            foreach (Control route_box in big_stop_box.Controls)
            {
                if (route_box.Name == "route box")
                {
                    bool is_found_route = false;
                    bool[] is_found_eta = { false, false };
                    string route_name = ((StringValue)route_box.Controls["value_route_name"]).Value;
                    string route_id = ((StringValue)route_box.Controls["value_route_id"]).Value;
                    int route_seq = ((IntValue)route_box.Controls["value_route_seq"]).Value;
                    Debug.WriteLine($"{route_name}, {route_id}, {route_seq}");

                    foreach (JsonElement jsonobj_route in json_data.EnumerateArray())
                    {
                        if (jsonobj_route.GetProperty("route_id").ToString() == route_id &&
                            jsonobj_route.GetProperty("route_seq").GetInt32() == route_seq)
                        {
                            foreach (JsonElement jsonobj_eta in jsonobj_route.GetProperty("eta").EnumerateArray()) //if have ETA
                            {
                                int timeDifference = jsonobj_eta.GetProperty("diff").GetInt32();

                                //eta_seq
                                if (jsonobj_eta.GetProperty("eta_seq").GetInt32() == 1)
                                {
                                    route_box.Controls["eta box"].Controls["eta 1"].Text = timeDifference.ToString();
                                    route_box.Controls["eta box"].Controls["eta 1"].Visible = true;
                                    route_box.Controls["eta box"].Controls["eta 1 chi"].Visible = true;
                                    route_box.Controls["eta box"].Controls["eta 1 eng"].Visible = true;
                                    is_found_eta[0] = true;
                                }
                                else if (jsonobj_eta.GetProperty("eta_seq").GetInt32() == 2)
                                {
                                    route_box.Controls["eta box"].Controls["eta 2"].Text = timeDifference.ToString();
                                    route_box.Controls["eta box"].Controls["eta 2"].Visible = true;
                                    route_box.Controls["eta box"].Controls["eta 2 chi"].Visible = true;
                                    route_box.Controls["eta box"].Controls["eta 2 eng"].Visible = true;
                                    is_found_eta[1] = true;
                                }

                                is_found_route = true;
                            }

                        }
                        else if (is_found_route) //如果找到符合之後, 再找不到
                        {
                            break;
                        }
                    }


                    if (!is_found_eta[0])
                    {
                        route_box.Controls["eta box"].Controls["eta 1"].Text = "?";
                        route_box.Controls["eta box"].Controls["eta 1"].Visible = false;
                        route_box.Controls["eta box"].Controls["eta 1 chi"].Visible = false;
                        route_box.Controls["eta box"].Controls["eta 1 eng"].Visible = false;
                    }

                    if (!is_found_eta[1])
                    {
                        route_box.Controls["eta box"].Controls["eta 2"].Text = "?";
                        route_box.Controls["eta box"].Controls["eta 2"].Visible = false;
                        route_box.Controls["eta box"].Controls["eta 2 chi"].Visible = false;
                        route_box.Controls["eta box"].Controls["eta 2 eng"].Visible = false;
                    }
                }
            }
        }

        public void Async_GettingETA_NLB(Panel big_stop_box, string stop_id)
        {
            foreach (Control route_box in big_stop_box.Controls)
            {
                if (route_box.Name == "route box")
                {
                    bool[] is_found_eta = { false, false };
                    string route_id = ((StringValue)route_box.Controls["value_route_id"]).Value;

                    string url = string.Format(API_Link.NLB_eta_url, route_id, stop_id);
                    Debug.WriteLine(url);

                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                    string json_string_response = new StreamReader(response.GetResponseStream()).ReadToEnd();

                    JsonElement json_data = (JsonDocument.Parse(json_string_response)).RootElement.GetProperty("estimatedArrivals");

                    foreach (JsonElement jsonobj_eta in json_data.EnumerateArray())
                    {
                        DateTime eta_DateTime = DateTime.Parse(jsonobj_eta.GetProperty("estimatedArrivalTime").GetString());
                        DateTime current_DateTime = DateTime.Now;
                        int timeDifference = (int)Math.Round((eta_DateTime - current_DateTime).TotalMinutes);
                        timeDifference = (timeDifference < 0) ? 0 : timeDifference;
                        Debug.WriteLine(timeDifference);

                        //eta_seq
                        if (!is_found_eta[0]) //eta_seq = 1
                        {
                            route_box.Controls["eta box"].Controls["eta 1"].Text = timeDifference.ToString();
                            route_box.Controls["eta box"].Controls["eta 1"].Visible = true;
                            route_box.Controls["eta box"].Controls["eta 1 chi"].Visible = true;
                            route_box.Controls["eta box"].Controls["eta 1 eng"].Visible = true;
                            is_found_eta[0] = true;
                        }
                        else if (!is_found_eta[1]) //eta_seq = 2
                        {
                            route_box.Controls["eta box"].Controls["eta 2"].Text = timeDifference.ToString();
                            route_box.Controls["eta box"].Controls["eta 2"].Visible = true;
                            route_box.Controls["eta box"].Controls["eta 2 chi"].Visible = true;
                            route_box.Controls["eta box"].Controls["eta 2 eng"].Visible = true;
                            is_found_eta[1] = true;
                        }
                        
                    }

                    if (!is_found_eta[0] && !is_found_eta[1])
                    {
                        //remark
                        Panel sp_eta_box = mapa.Panel_ETA_Box_Remark("非服務時間", "Non-service hours");
                        FitTheScreen.Resize_Child(sp_eta_box, 1280);
                        FitTheScreen.Resize(sp_eta_box, 1280);
                        route_box.Controls.Add(sp_eta_box);
                        sp_eta_box.BringToFront();
                    }

                    if (!is_found_eta[0])
                    {
                        route_box.Controls["eta box"].Controls["eta 1"].Text = "?";
                        route_box.Controls["eta box"].Controls["eta 1"].Visible = false;
                        route_box.Controls["eta box"].Controls["eta 1 chi"].Visible = false;
                        route_box.Controls["eta box"].Controls["eta 1 eng"].Visible = false;
                    }

                    if (!is_found_eta[1])
                    {
                        route_box.Controls["eta box"].Controls["eta 2"].Text = "?";
                        route_box.Controls["eta box"].Controls["eta 2"].Visible = false;
                        route_box.Controls["eta box"].Controls["eta 2 chi"].Visible = false;
                        route_box.Controls["eta box"].Controls["eta 2 eng"].Visible = false;
                    }
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
