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
using System.Runtime.InteropServices;
using System.Threading;
using System.IO.Ports;
using System.Diagnostics;

namespace SerialToKey
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        private static extern bool AllocConsole();

        [DllImport("user32.dll")]
        static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);
        [DllImport("user32.dll")]
        static extern uint MapVirtualKeyEx(uint uCode, uint uMapType, IntPtr dwhkl);


        public const int KEYEVENTF_EXTENDEDKEY = 0x0001; //Key down flag
        public const int KEYEVENTF_KEYUP = 0x0002; //Key up flag

        public Boolean running = true;
        Thread thread;

        public MainWindow()
        {
            InitializeComponent();
        }

        void StartListen()
        {
            string com = "COM3";
            int baudRate = 9600;
            Application.Current.Dispatcher.Invoke(
            System.Windows.Threading.DispatcherPriority.Normal, (Action)delegate
            {
                com = textBox.Text.Trim();
                try
                {
                    baudRate = int.Parse(comboBox.Text);
                }
                catch
                {
                    textBlock.Text += ("Please select baud rate\n");
                }
            });
            SerialPort port;
            try
            {
                port = new SerialPort(com, baudRate);
                port.Open();
            }
            catch (Exception)
            {
                Application.Current.Dispatcher.Invoke(
                System.Windows.Threading.DispatcherPriority.Normal, (Action)delegate
                {
                    textBlock.Text += ("Coudn't open COM\n");
                });
                return;
            }

            Application.Current.Dispatcher.Invoke(
            System.Windows.Threading.DispatcherPriority.Normal, (Action)delegate
            {
                textBlock.Text += ("Listening" + "\n");
            });

            while (running)
            {
                string message = port.ReadLine().Trim();
                string[] data = message.Split('|');

                string what = data[0].Trim();
                try
                {
                    byte keyCode = (byte)int.Parse(data[1]);
                    if (what.Equals("p"))
                    {
                        keybd_event(keyCode, (byte)MapVirtualKeyEx(keyCode, 0, IntPtr.Zero), KEYEVENTF_EXTENDEDKEY, UIntPtr.Zero);
                    }

                    if (what.Equals("r"))
                    {
                        keybd_event(keyCode, (byte)MapVirtualKeyEx(keyCode, 0, IntPtr.Zero), KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, UIntPtr.Zero);
                    }

                }
                catch (Exception e)
                {
                    Application.Current.Dispatcher.Invoke(
                    System.Windows.Threading.DispatcherPriority.Normal, (Action)delegate
                    {
                        textBlock.Text += ("Unable to parse Exception caught : " + e.GetType() + "\n");
                    });
                }
            }
            Application.Current.Dispatcher.Invoke(
           System.Windows.Threading.DispatcherPriority.Normal, (Action)delegate
           {
               textBlock.Text += ("Stopped" + "\n");
           });
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            running = true;
            thread = new Thread(new ThreadStart(StartListen));
            thread.Start();
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            if (thread != null && thread.IsAlive)
            {

                textBlock.Text += ("Stopping..." + "\n");
                running = false;
                thread.Join();
            }
            else
            {
                textBlock.Text += ("Already Stoped..." + "\n");
            }
        }
    }





}
