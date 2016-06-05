using Caro.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Caro.Models;
using Newtonsoft.Json.Linq;
using Quobject.SocketIoClientDotNet.Client;

namespace Caro
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private int type = 0; //1:nguoi voi nguoi  //2:nguoi voi may //3:nguoi danh online //4:may danh online
        private Quobject.SocketIoClientDotNet.Client.Socket _socket;
        private string dulieu;
        private Button[,] _mangbutton;
        private BoardViewModels board;

        private string _minhnhi;
        public MainWindow()
        {
            InitializeComponent();
            //tao mang button
            _mangbutton = new Button[12, 12];
            //tao giao dien 
            for (int i = 0; i < 12; i++)
            {
                StackPanel pn = new StackPanel();
                pn.Orientation = Orientation.Horizontal;
                stackKhung.Children.Add(pn);
                for (int j = 0; j < 12; j++)
                {
                    string _name = "btn" + i.ToString() + j.ToString();

                    Button btn = new Button();
                    if (i % 2 == 0)
                    {
                        if (j % 2 == 0)
                        { btn.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFDDDDDD")); }
                        else
                        { btn.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FF707070")); }
                    }
                    else
                    {
                        if (j % 2 == 0)
                        { btn.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FF707070")); }
                        else
                        { btn.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFDDDDDD")); }
                    }

                    btn.Name = _name;
                    btn.Width = 40;
                    btn.Height = 40;
                    btn.Tag = string.Format("{0},{1}", i, j);
                    _mangbutton[i, j] = btn;
                    pn.Children.Add(btn);
                }
            }
            SetButtonClick();
            board = new BoardViewModels();
            board.CurrentBoard.OnPlayerWin += CurrentBoard_OnPlayerWin;
        }

        void CurrentBoard_OnPlayerWin(Models.CellValues player)
        {
            MessageBox.Show(player.ToString() + " win!");
        }
        private void SetButtonClick()
        {
            foreach (Button nut in _mangbutton)
            {
                nut.Click += nut_Click;
            }
        }

        void nut_Click(object sender, RoutedEventArgs e)
        {
            string[] s;
            string a = ((Button)sender).Tag.ToString();
            s = a.Split(',');

            Button cell = (Button)sender;

            //neu type = 1 => cho phep danh vap ban co nguoi voi nguoi
            //neu type = 2 => cho phep danh nguoi voi may
            //mac dinh  type = 0 => danh voi may
            if (type == 0 || type == 2)
            {
                if (board.CurrentBoard.Cells[int.Parse(s[0]), int.Parse(s[1])] == CellValues.Player1 || board.CurrentBoard.Cells[int.Parse(s[0]), int.Parse(s[1])] == CellValues.Player2)
                    return;

                //danh voi may

                board.CurrentBoard.Cells[int.Parse(s[0]), int.Parse(s[1])] = CellValues.Player1;
                //cell.Background = Brushes.Yellow;
                ImageBrush myBrush = new ImageBrush();
                myBrush.ImageSource = new BitmapImage(new Uri(BaseUriHelper.GetBaseUri(this), "Images/circle blue.png"));
                cell.Background = myBrush;
                board.CurrentBoard.PlayAt(int.Parse(s[0]), int.Parse(s[1]));

                board.CurrentBoard.ActivePlayer = CellValues.Player2;

                int x = -1;
                int y = -1;
                do
                {
                    Random rd = new Random();
                    x = rd.Next(0, 12);
                    y = rd.Next(0, 12);
                } while (board.CurrentBoard.Cells[x, y] != CellValues.None);
                foreach (Button press in _mangbutton)
                {
                    if (press.Name == ("btn" + x.ToString() + y.ToString()))
                    {
                        //press.Background = Brushes.Green;
                        myBrush = new ImageBrush();
                        myBrush.ImageSource = new BitmapImage(new Uri(BaseUriHelper.GetBaseUri(this), "Images/circle orange.png"));
                        press.Background = myBrush;

                        cell = press;
                    }
                }

                board.CurrentBoard.Cells[x, y] = CellValues.Player2;
                board.CurrentBoard.PlayAt(x, y);
            }
            if (type == 1) // danh nguoi voi nguoi
            {
                //kiem tra
                if (board.CurrentBoard.Cells[int.Parse(s[0]), int.Parse(s[1])] == CellValues.Player1 || board.CurrentBoard.Cells[int.Parse(s[0]), int.Parse(s[1])] == CellValues.Player2)
                    return;

                if (board.CurrentBoard.ActivePlayer == Models.CellValues.Player1)
                {
                    board.CurrentBoard.Cells[int.Parse(s[0]), int.Parse(s[1])] = CellValues.Player1;
                    ImageBrush myBrush = new ImageBrush();
                    myBrush.ImageSource = new BitmapImage(new Uri(BaseUriHelper.GetBaseUri(this), "Images/circle green.png"));
                    //cell.Background = Brushes.Red;
                    cell.Background = myBrush;

                }
                else
                {
                    board.CurrentBoard.Cells[int.Parse(s[0]), int.Parse(s[1])] = CellValues.Player2;
                    //cell.Background = Brushes.Green;
                    ImageBrush myBrush = new ImageBrush();
                    myBrush.ImageSource = new BitmapImage(new Uri(BaseUriHelper.GetBaseUri(this), "Images/circle yellow.png"));
                    cell.Background = myBrush;
                }
                board.CurrentBoard.PlayAt(int.Parse(s[0]), int.Parse(s[1]));
            }
            if (type == 3) //nguoi online
            {
                //gui du lieu len server   
                int r, c;
                r = int.Parse(s[0]);
                c = int.Parse(s[1]);


                _socket.Emit("MyStepIs", JObject.FromObject(new { row = r, col = c }));
            }
        }

        private void frmMain_Loaded(object sender, RoutedEventArgs e)
        {
            btnGui.IsEnabled = false;
            btnChange.IsEnabled = false;
        }

        private void btnGui_Click(object sender, RoutedEventArgs e)
        {
            if (type == 1 || type == 2)
            {
                StackPanel _khunggui;
                Border br = new Border();
                br.BorderThickness = new Thickness(1);
                br.BorderBrush = Brushes.Black;
                _khunggui = new StackPanel();
                Label lb1 = new Label();
                Label lb2 = new Label();
                lb1.Content = txtName.Text + "\t" + DateTime.Now.ToLongTimeString();
                lb2.Content = txtChat.Text;
                _khunggui.Children.Add(br);
                _khunggui.Children.Add(lb1);

                _khunggui.Children.Add(lb2);

                panelChat.Children.Add(_khunggui);
            }
            else
            {
                string t = txtChat.Text;
                _socket.Emit("ChatMessage", t);
            }
        }

        private void btnNguoiNguoi_Click(object sender, RoutedEventArgs e)
        {
            type = 1;
            btnGui.IsEnabled = true;
            btnChange.IsEnabled = false;
        }

        private void btnNguoiMay_Click(object sender, RoutedEventArgs e)
        {
            type = 2;
            btnGui.IsEnabled = true;
            btnChange.IsEnabled = false;
        }

        private void btnNguoiOnline_Click(object sender, RoutedEventArgs e)
        {
            type = 3;
            btnGui.IsEnabled = true;
            btnChange.IsEnabled = true;
            string name = txtName.Text;
            StackPanel _stackpanel;
            _socket = IO.Socket(Caro.Properties.Settings.Default.IP);
            _socket.On(Socket.EVENT_CONNECT, () =>
            {
                Console.WriteLine("connected");
            });
            _socket.On(Socket.EVENT_MESSAGE, (data) =>
            {
                Console.WriteLine(data);
            });
            _socket.On(Socket.EVENT_CONNECT_ERROR, (data) =>
            {
                Console.WriteLine(data);
            });

            _socket.On("ChatMessage", (data) =>
            {
                Console.WriteLine(data);
                if (((Newtonsoft.Json.Linq.JObject)data)["message"].ToString() == "Welcome!")
                {
                    //gui ten len server
                    _socket.Emit("MyNameIs", name);
                    _socket.Emit("ConnectToOtherPlayer");
                }
                string nguoithongbao = "Server";
                string message = data.ToString();
                int vt = message.IndexOf("from");
                if (vt != -1)
                {
                    string[] chuoi = message.Split('"');
                    //lay ten nguoi chat
                    nguoithongbao = chuoi[7];
                    //lay noi dung chat
                    message = chuoi[3];
                }
                else
                {
                    string[] chat = message.Split('"');
                    message = chat[3];
                }
                this.Dispatcher.Invoke((Action)(() =>
                {
                    Border br = new Border();
                    br.BorderThickness = new Thickness(1);
                    br.BorderBrush = Brushes.Black;
                    _stackpanel = new StackPanel();
                    Label lb1 = new Label();
                    Label lb2 = new Label();
                    lb1.Content = nguoithongbao + "\t" + DateTime.Now.ToLongTimeString();
                    lb2.Content = message;
                    _stackpanel.Children.Add(br);
                    _stackpanel.Children.Add(lb1);

                    _stackpanel.Children.Add(lb2);

                    panelChat.Children.Add(_stackpanel);
                }));

            });
            _socket.On(Socket.EVENT_ERROR, (data) =>
            {
                Console.WriteLine(data);
            });
            _socket.On("NextStepIs", (data) =>
            {
                Console.WriteLine("NextStepIs: " + data);
                string chuoi = data.ToString();

                int row = -1;
                int col = -1;
                int count = 0;
                //kiem tra
                if (chuoi[28] == '0' || chuoi[28] == '1' || chuoi[28] == '2' || chuoi[28] == '3' || chuoi[28] == '4' || chuoi[28] == '5' || chuoi[28] == '6' || chuoi[28] == '7' || chuoi[28] == '8' || chuoi[28] == '9')
                    count++;
                if (chuoi[29] == '0' || chuoi[29] == '1' || chuoi[29] == '2' || chuoi[29] == '3' || chuoi[29] == '4' || chuoi[29] == '5' || chuoi[29] == '6' || chuoi[29] == '7' || chuoi[29] == '8' || chuoi[29] == '9')
                    count++;
                if (chuoi[42] == '0' || chuoi[42] == '1' || chuoi[42] == '2' || chuoi[42] == '3' || chuoi[42] == '4' || chuoi[42] == '5' || chuoi[42] == '6' || chuoi[42] == '7' || chuoi[42] == '8' || chuoi[42] == '9')
                    count++;
                if (chuoi[43] == '0' || chuoi[43] == '1' || chuoi[43] == '2' || chuoi[43] == '3' || chuoi[43] == '4' || chuoi[43] == '5' || chuoi[43] == '6' || chuoi[43] == '7' || chuoi[43] == '8' || chuoi[43] == '9')
                    count++;
                if (count == 4)
                {
                    row = int.Parse(chuoi[28].ToString()) * 10 + int.Parse(chuoi[29].ToString());
                    col = int.Parse(chuoi[42].ToString()) * 10 + int.Parse(chuoi[43].ToString());
                }
                else
                {
                    if ((chuoi[28] == '0' || chuoi[28] == '1' || chuoi[28] == '2' || chuoi[28] == '3' || chuoi[28] == '4' || chuoi[28] == '5' || chuoi[28] == '6' || chuoi[28] == '7' || chuoi[28] == '8' || chuoi[28] == '9')
                        && (chuoi[29] == '0' || chuoi[29] == '1' || chuoi[29] == '2' || chuoi[29] == '3' || chuoi[29] == '4' || chuoi[29] == '5' || chuoi[29] == '6' || chuoi[29] == '7' || chuoi[29] == '8' || chuoi[29] == '9'))
                    {
                        row = int.Parse(chuoi[28].ToString()) * 10 + int.Parse(chuoi[29].ToString());
                        col = int.Parse(chuoi[42].ToString());
                    }
                    else
                    {
                        if ((chuoi[41] == '0' || chuoi[41] == '1' || chuoi[41] == '2' || chuoi[41] == '3' || chuoi[41] == '4' || chuoi[41] == '5' || chuoi[41] == '6' || chuoi[41] == '7' || chuoi[41] == '8' || chuoi[41] == '9')
                        && (chuoi[42] == '0' || chuoi[42] == '1' || chuoi[42] == '2' || chuoi[42] == '3' || chuoi[42] == '4' || chuoi[42] == '5' || chuoi[42] == '6' || chuoi[42] == '7' || chuoi[42] == '8' || chuoi[42] == '9'))
                        {
                            row = int.Parse(chuoi[28].ToString());
                            col = int.Parse(chuoi[41].ToString()) * 10 + int.Parse(chuoi[42].ToString());
                        }
                        else
                        {
                            row = int.Parse(chuoi[28].ToString());
                            col = int.Parse(chuoi[41].ToString());
                        }
                    }

                }

                if (board.CurrentBoard.ActivePlayer == CellValues.Player1)
                {
                    board.CurrentBoard.Cells[row, col] = CellValues.Player2;
                    //board.CurrentBoard.ActivePlayer = CellValues.Player2;
                }
                else
                {
                    board.CurrentBoard.Cells[row, col] = CellValues.Player1;
                    //board.CurrentBoard.ActivePlayer = CellValues.Player1;
                }


                this.Dispatcher.Invoke((Action)(() =>
                {
                    foreach (Button press in _mangbutton)
                    {
                        if (press.Name == ("btn" + row.ToString() + col.ToString()))
                        {
                            ImageBrush myBrush = new ImageBrush();
                            if (board.CurrentBoard.ActivePlayer == CellValues.Player1)
                                myBrush.ImageSource = new BitmapImage(new Uri(BaseUriHelper.GetBaseUri(this), "Images/circle orange.png"));
                            else
                                myBrush.ImageSource = new BitmapImage(new Uri(BaseUriHelper.GetBaseUri(this), "Images/circle blue.png"));
                            press.Background = myBrush;
                        }
                    }
                }));
                //danh vao ban co
                board.CurrentBoard.PlayAt(row, col);
            });
        }

        private void btnMayOnline_Click(object sender, RoutedEventArgs e)
        {
            type = 4;
            btnGui.IsEnabled = true;
            btnChange.IsEnabled = true;
            txtName.Text = "Robot";
            string name = txtName.Text;
            _socket = IO.Socket(Caro.Properties.Settings.Default.IP);
            _socket.On(Socket.EVENT_CONNECT, () =>
            {
                Console.WriteLine("connected");
            });
            _socket.On(Socket.EVENT_MESSAGE, (data) =>
            {
                Console.WriteLine(data);
            });
            _socket.On(Socket.EVENT_CONNECT_ERROR, (data) =>
            {
                Console.WriteLine(data);
            });

            _socket.On("ChatMessage", (data) =>
            {
                StackPanel _stackpanel;
                Console.WriteLine(data);
                if (((Newtonsoft.Json.Linq.JObject)data)["message"].ToString() == "Welcome!")
                {
                    //gui ten len server
                    _socket.Emit("MyNameIs", name);
                    _socket.Emit("ConnectToOtherPlayer");
                }
                string nguoithongbao = "Server";
                string message = data.ToString();

                int vt = message.IndexOf("from");
                int vt1 = message.IndexOf("You are the second player!");
                int vt2 = message.IndexOf("You are the first player!");
                if (vt != -1)
                {
                    string[] chuoi = message.Split('"');
                    //lay ten nguoi chat
                    nguoithongbao = chuoi[7];
                    //lay noi dung chat
                    message = chuoi[3];
                }
                else
                {
                    if (vt1 != -1)
                    {
                        _minhnhi = "second";
                    }
                    if (vt2 != -1)
                    {
                        _minhnhi = "first";
                    }
                    string[] chat = message.Split('"');
                    message = chat[3];
                }

                this.Dispatcher.Invoke((Action)(() =>
                {
                    Border br = new Border();
                    br.BorderThickness = new Thickness(1);
                    br.BorderBrush = Brushes.Black;
                    _stackpanel = new StackPanel();
                    Label lb1 = new Label();
                    Label lb2 = new Label();
                    lb1.Content = nguoithongbao + "\t" + DateTime.Now.ToLongTimeString();
                    lb2.Content = message;
                    _stackpanel.Children.Add(br);
                    _stackpanel.Children.Add(lb1);

                    _stackpanel.Children.Add(lb2);

                    panelChat.Children.Add(_stackpanel);
                }));
                if (_minhnhi == "first")
                {
                    Random rd = new Random();
                    int x = 0, y = 0;
                    //kiem tra trung
                    do
                    {
                        x = rd.Next(0, 12);
                        y = rd.Next(0, 12);
                    } while (board.CurrentBoard.Cells[x, y] != CellValues.None);
                    _socket.Emit("MyStepIs", JObject.FromObject(new { row = x, col = y }));
                }

            });
            _socket.On(Socket.EVENT_ERROR, (data) =>
            {
                Console.WriteLine(data);
            });
            _socket.On("NextStepIs", (data) =>
            {
                Console.WriteLine("NextStepIs: " + data);
                string chuoi = data.ToString();
                int vt = chuoi.IndexOf("1");
                int vt1 = chuoi.IndexOf("0");
                //neu may la nguoi danh dau tien
                //if (vt1 != 0 && vt < 10)
                //{
                //    Random rd = new Random();
                //    int x = 0, y = 0;
                //    //kiem tra trung
                //    do
                //    {
                //        x = rd.Next(0, 12);
                //        y = rd.Next(0, 12);
                //    } while (board.CurrentBoard.Cells[x, y] != CellValues.None);
                //    _socket.Emit("MyStepIs", JObject.FromObject(new { row = x, col = y }));
                //    return;
                //}

                int row = -1;
                int col = -1;
                int count = 0;
                //kiem tra
                if (chuoi[28] == '0' || chuoi[28] == '1' || chuoi[28] == '2' || chuoi[28] == '3' || chuoi[28] == '4' || chuoi[28] == '5' || chuoi[28] == '6' || chuoi[28] == '7' || chuoi[28] == '8' || chuoi[28] == '9')
                    count++;
                if (chuoi[29] == '0' || chuoi[29] == '1' || chuoi[29] == '2' || chuoi[29] == '3' || chuoi[29] == '4' || chuoi[29] == '5' || chuoi[29] == '6' || chuoi[29] == '7' || chuoi[29] == '8' || chuoi[29] == '9')
                    count++;
                if (chuoi[42] == '0' || chuoi[42] == '1' || chuoi[42] == '2' || chuoi[42] == '3' || chuoi[42] == '4' || chuoi[42] == '5' || chuoi[42] == '6' || chuoi[42] == '7' || chuoi[42] == '8' || chuoi[42] == '9')
                    count++;
                if (chuoi[43] == '0' || chuoi[43] == '1' || chuoi[43] == '2' || chuoi[43] == '3' || chuoi[43] == '4' || chuoi[43] == '5' || chuoi[43] == '6' || chuoi[43] == '7' || chuoi[43] == '8' || chuoi[43] == '9')
                    count++;
                if (count == 4)
                {
                    row = int.Parse(chuoi[28].ToString()) * 10 + int.Parse(chuoi[29].ToString());
                    col = int.Parse(chuoi[42].ToString()) * 10 + int.Parse(chuoi[43].ToString());
                }
                else
                {
                    if ((chuoi[28] == '0' || chuoi[28] == '1' || chuoi[28] == '2' || chuoi[28] == '3' || chuoi[28] == '4' || chuoi[28] == '5' || chuoi[28] == '6' || chuoi[28] == '7' || chuoi[28] == '8' || chuoi[28] == '9')
                        && (chuoi[29] == '0' || chuoi[29] == '1' || chuoi[29] == '2' || chuoi[29] == '3' || chuoi[29] == '4' || chuoi[29] == '5' || chuoi[29] == '6' || chuoi[29] == '7' || chuoi[29] == '8' || chuoi[29] == '9'))
                    {
                        row = int.Parse(chuoi[28].ToString()) * 10 + int.Parse(chuoi[29].ToString());
                        col = int.Parse(chuoi[42].ToString());
                    }
                    else
                    {
                        if ((chuoi[41] == '0' || chuoi[41] == '1' || chuoi[41] == '2' || chuoi[41] == '3' || chuoi[41] == '4' || chuoi[41] == '5' || chuoi[41] == '6' || chuoi[41] == '7' || chuoi[41] == '8' || chuoi[41] == '9')
                        && (chuoi[42] == '0' || chuoi[42] == '1' || chuoi[42] == '2' || chuoi[42] == '3' || chuoi[42] == '4' || chuoi[42] == '5' || chuoi[42] == '6' || chuoi[42] == '7' || chuoi[42] == '8' || chuoi[42] == '9'))
                        {
                            row = int.Parse(chuoi[28].ToString());
                            col = int.Parse(chuoi[41].ToString()) * 10 + int.Parse(chuoi[42].ToString());
                        }
                        else
                        {
                            row = int.Parse(chuoi[28].ToString());
                            col = int.Parse(chuoi[41].ToString());
                        }
                    }
                }

                if (board.CurrentBoard.ActivePlayer == CellValues.Player1)
                {
                    board.CurrentBoard.Cells[row, col] = CellValues.Player2;
                }
                else
                    board.CurrentBoard.Cells[row, col] = CellValues.Player1;

                this.Dispatcher.Invoke((Action)(() =>
                {
                    foreach (Button press in _mangbutton)
                    {
                        if (press.Name == ("btn" + row.ToString() + col.ToString()))
                        {
                            ImageBrush myBrush = new ImageBrush();
                            if (board.CurrentBoard.ActivePlayer == CellValues.Player1)
                                myBrush.ImageSource = new BitmapImage(new Uri(BaseUriHelper.GetBaseUri(this), "Images/circle orange.png"));
                            else
                                myBrush.ImageSource = new BitmapImage(new Uri(BaseUriHelper.GetBaseUri(this), "Images/circle blue.png"));
                            press.Background = myBrush;
                        }
                    }
                }));
                //danh vao ban co
                board.CurrentBoard.PlayAt(row, col);
                //may danh
                //if (vt != -1 && vt < 10)
                //{
                    Random rd = new Random();
                    int x = 0, y = 0;
                    //kiem tra trung
                    do
                    {
                        x = rd.Next(0, 12);
                        y = rd.Next(0, 12);
                    } while (board.CurrentBoard.Cells[x, y] != CellValues.None);
                    _socket.Emit("MyStepIs", JObject.FromObject(new { row = x, col = y }));
                //}
            });
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            _socket.Emit("MyNameIs", txtName.Text);
        }
    }
}
