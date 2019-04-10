using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Sockets;
using System.IO;

namespace TwitchChatReader
{
    public partial class Form1 : Form
    {
        TcpClient tcpClient;
        StreamReader reader;
        StreamWriter writer;
        string Name = "";
        string OAuth = "";
        string JoinChan = "";
        string ChanPrefix;
        string ChatCommID;
        //initial sleep time, set it low enough so when you start a stage it runs it fast enough
        float SleepTime = 30000f;
        int[] CommandSelectAmm = new int[3];
        bool DispText_ = true;
        //required to execute our commands
        ParseCommands parser = new ParseCommands();
        int[] CurComms = new int[3];

        //add one to this each time you add a new command
        int AmmComms = 11;
        
        public Form1()
        {
            if (File.Exists("Config.cfg"))
            {
                string[] Data = File.ReadAllLines("Config.cfg");
                Name = Data[0];
                OAuth = Data[1];
                JoinChan = Data[2];
            }
            else
            {
                InputBox("Connect", "Please input your twitch name.", ref Name);
                InputBox("Connect", "Please input your twitch OAuth.", ref OAuth);
                InputBox("Connect", "Please input the twitch channel to join.", ref JoinChan);
                string[] WriteData = new string[3];
                WriteData[0] = Name;
                WriteData[1] = OAuth;
                WriteData[2] = JoinChan;
                File.WriteAllLines("Config.cfg", WriteData);
            }

            //sets up our parser
            parser.Init();            

            //fancy twitch chat reading stuff
            ChatCommID = "PRIVMSG";
            ChanPrefix = $":{Name}!{Name}@{Name}.tmi.twitch.tv {ChatCommID} #{JoinChan} :";

            InitializeComponent();
            Reconnect();
        }

        private void Reconnect()
        {
            tcpClient = new TcpClient("irc.twitch.tv", 6667);
            reader = new StreamReader(tcpClient.GetStream());
            writer = new StreamWriter(tcpClient.GetStream());

            var username = Name;
            var password = OAuth;

            writer.WriteLine(string.Format("PASS {0}{1}NICK {2}{1}USER {2} 8 * : {2}", password, Environment.NewLine, username));
            writer.Flush();
            writer.WriteLine($"JOIN #{JoinChan}");
            writer.Flush();
        }

        private void CheckChat_Tick(object sender, EventArgs e)
        {
            if (!tcpClient.Connected)
            {
                Reconnect();
            }

            if (SleepTime < 20000 && SleepTime > 0)
                DispText();

            TryReceive();
        }

        private void TryReceive()
        {
            if (tcpClient.Available > 0 || reader.Peek() >= 0)
            {

                var Message = reader.ReadLine();

                CommandList.Text += Message + Environment.NewLine;

                //command reading stuff happens here
                var iCollon = Message.IndexOf(":", 1);
                if (iCollon > 0)
                {
                    var Command = Message.Substring(1, iCollon);
                    if (SleepTime > -10000 && SleepTime < 20000)
                    {
                        var ChatMessage = Message.Substring(Command.Length + 1);
                        switch (ChatMessage)
                        {
                            case ("1"):
                                {
                                    CommandSelectAmm[0] += 1;
                                    break;
                                }
                            case ("2"):
                                {
                                    CommandSelectAmm[1] += 1;
                                    break;
                                }
                            case ("3"):
                                {
                                    CommandSelectAmm[2] += 1;
                                    break;
                                }
                        }
                    }
                }

            }

            if (SleepTime < -10000 && parser.CheckInStage())
            {
                if (!DispText_)
                {
                    //displays text and executes commands
                    parser.RemoveStatEffects();
                    //if too short extend
                    SleepTime = 60000;
                    DispText_ = true;
                    int maxValue = CommandSelectAmm.Max();
                    int maxIndex = CommandSelectAmm.ToList().IndexOf(maxValue);


                    parser.ExecCom(CurComms[maxIndex]);

                    Array.Clear(CurComms, 0, 3);
                    Array.Clear(CommandSelectAmm, 0, 3);
                }
            }

            if (parser.CheckInStage())
            {
                SleepTime -= CheckChat.Interval;
            }            
        }

        private void DispText()
        {
            if (DispText_ && parser.CheckInStage())
            {
                //handles command selection
                DispText_ = false;

                Random RND = new Random();

                CurComms[0] = RND.Next(0, AmmComms);
                CurComms[1] = RND.Next(0, AmmComms);
                CurComms[2] = RND.Next(0, AmmComms);

                string ToDispText = string.Format("1 {0}*2 {1}*3 {2}*", parser.GetCommName(CurComms[0]),
                    parser.GetCommName(CurComms[1]), parser.GetCommName(CurComms[2]));
                parser.DispText(ToDispText);
            }
        }


        //Simple inputbox code, don't remove
        public static DialogResult InputBox(string title, string promptText, ref string value)
        {
            Form form = new Form();
            Label label = new Label();
            TextBox textBox = new TextBox();
            Button buttonOk = new Button();
            Button buttonCancel = new Button();

            form.Text = title;
            label.Text = promptText;
            textBox.Text = value;

            buttonOk.Text = "OK";
            buttonCancel.Text = "Cancel";
            buttonOk.DialogResult = DialogResult.OK;
            buttonCancel.DialogResult = DialogResult.Cancel;

            label.SetBounds(9, 20, 372, 13);
            textBox.SetBounds(12, 36, 372, 20);
            buttonOk.SetBounds(228, 72, 75, 23);
            buttonCancel.SetBounds(309, 72, 75, 23);

            label.AutoSize = true;
            textBox.Anchor = textBox.Anchor | AnchorStyles.Right;
            buttonOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;

            form.ClientSize = new Size(396, 107);
            form.Controls.AddRange(new Control[] { label, textBox, buttonOk, buttonCancel });
            form.ClientSize = new Size(Math.Max(300, label.Right + 10), form.ClientSize.Height);
            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.StartPosition = FormStartPosition.CenterScreen;
            form.MinimizeBox = false;
            form.MaximizeBox = false;
            form.AcceptButton = buttonOk;
            form.CancelButton = buttonCancel;

            DialogResult dialogResult = form.ShowDialog();
            value = textBox.Text;
            return dialogResult;
        }
    }
}
