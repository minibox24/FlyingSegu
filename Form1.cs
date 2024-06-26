﻿using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace FlyingSegu
{
    public partial class Form1 : Form
    {
        private const int WH_MOUSE_LL = 14;
        private const int WM_MOUSEMOVE = 0x0200;

        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int VK_F3 = 0x72;

        private delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int X;
            public int Y;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MSLLHOOKSTRUCT
        {
            public POINT Pt;
            public uint MouseData;
            public uint Flags;
            public uint Time;
            public IntPtr ExtraInfo;
        }

        private LowLevelMouseProc _proc;
        private IntPtr _hookID = IntPtr.Zero;
        private IntPtr _hookID2 = IntPtr.Zero;
        private Point _previousMousePos;
        private bool isFlipped = false;
        private bool isSmall = false;

        private PictureBox pictureBox1;

        public Form1()
        {
            InitializeComponent();
            InitializePictureBox();

            _proc = HookCallback;
            _hookID = SetHook(_proc);
            _hookID2 = SetHook2(_proc);

            Application.ApplicationExit += (sender, e) => {
                UnhookWindowsHookEx(_hookID);
                UnhookWindowsHookEx(_hookID2);
            };
        }

        private void InitializePictureBox()
        {
            pictureBox1 = new PictureBox();
            pictureBox1.Image = Properties.Resources.FlyingSegu;
            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox1.Size = new Size(this.Width, this.Height);
            this.Controls.Add(pictureBox1);
        }

        private IntPtr SetHook(LowLevelMouseProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_MOUSE_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private IntPtr SetHook2(LowLevelMouseProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                if (wParam == WM_KEYDOWN)
                {
                    int vkCode = Marshal.ReadInt32(lParam);
                    if ((Keys)vkCode == Keys.F3)
                    {
                        isSmall = !isSmall;

                        if (isSmall)
                        {
                            this.Width = 300;
                            this.Height = 180;
                        } else
                        {
                            this.Width = 684;
                            this.Height = 373;
                        }


                        pictureBox1.Size = new Size(this.Width, this.Height);
                    }
                }

                if (wParam == WM_MOUSEMOVE)
                {
                    MSLLHOOKSTRUCT hookStruct = (MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));

                    int x = (int)hookStruct.Pt.X;
                    int y = (int)hookStruct.Pt.Y;

                    this.TopMost = true;

                    if (_previousMousePos.X != x)
                    {
                        if (_previousMousePos.X < x != isFlipped && MathF.Abs(_previousMousePos.X - x) > this.Width / 10)
                        {
                            pictureBox1.Image.RotateFlip(RotateFlipType.RotateNoneFlipX);
                            pictureBox1.Invalidate();
                            isFlipped = _previousMousePos.X < x;
                            _previousMousePos = new Point(x, y);
                        }
                        else if (_previousMousePos.X < x == isFlipped)
                            _previousMousePos = new Point((int)x, (int)y);

                    }

                    this.Location = new Point(isFlipped ? x - this.Width : x, y);
                }
            }

            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
