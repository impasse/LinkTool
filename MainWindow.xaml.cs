using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using System.Threading;
using System.Security.Principal;
using System.Runtime.InteropServices;

using FolderBrowserDialog = System.Windows.Forms.FolderBrowserDialog;

namespace LinkTool
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private ContextMenu contextMenu; 
        public MainWindow()
        {
            InitializeComponent();
            checkPermission();
            contextMenu = new ContextMenu();
            contextMenu.Items.Add("Delete");
        }

        private void checkPermission()
        {
            AppDomain ad = Thread.GetDomain();
            ad.SetPrincipalPolicy(PrincipalPolicy.WindowsPrincipal);
            WindowsPrincipal user = (WindowsPrincipal)Thread.CurrentPrincipal;
            if (!user.IsInRole(WindowsBuiltInRole.Administrator))
            {
                Close();
                MessageBox.Show("需要以管理员身份启动");
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new FolderBrowserDialog
            {
                RootFolder = Environment.SpecialFolder.MyComputer,
            };
            dialog.ShowDialog();
            Target.Text = dialog.SelectedPath;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var dialog = new FolderBrowserDialog
            {
                RootFolder = Environment.SpecialFolder.MyComputer,
            };
            dialog.ShowDialog();
            Link.Text = dialog.SelectedPath;
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            var target = Target.Text;
            var link = Link.Text;
            if (target == "Target" || link == "Link")
            {
                MessageBox.Show(this, "Source or Target 未选择");
            }
            if (!Directory.EnumerateFileSystemEntries(link).Any())
            {
                Directory.Delete(link);
                if (NativeMethods.CreateSymbolicLink(link, target, NativeMethods.SymbolicLink.File))
                {
                    MessageBox.Show(this, "完成");
                }
                else
                {
                    MessageBox.Show(this, $"失败，错误码: {Marshal.GetLastWin32Error()}");
                }
                
            }
            else
            {
                var result = MessageBox.Show(this, "目录非空，是否清空？", "确认？", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    Directory.Delete(link, true);
                    if (NativeMethods.CreateSymbolicLink(link, target, NativeMethods.SymbolicLink.Directory))
                    {
                        MessageBox.Show(this, "完成");
                    }
                    else
                    {
                        MessageBox.Show(this, $"失败，错误码: {Marshal.GetLastWin32Error()}");
                    }
                }
            }
            RefreshList();
        }

        private void RefreshList()
        {
            var path = OneDrivePath.Text;
            if (!Directory.Exists(path))
            {
                return;
            }
            Queue<string> queue = new Queue<string>(new[] { path });
            List<string> result = new List<string>();
            LinkList.Items.Clear();
            while (queue.Count != 0)
            {
                var cur = queue.Dequeue();
                var target = NativeMethods.GetFinalPathName(cur).Replace(@"\\?\", "");
                if (cur != target)
                {
                    LinkList.Items.Add($"{cur} => {target}");
                }
                foreach (string dir in Directory.EnumerateDirectories(cur))
                {
                    queue.Enqueue(dir);
                }
            }
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            var dialog = new FolderBrowserDialog
            {
                RootFolder = Environment.SpecialFolder.MyComputer,
            };
            dialog.ShowDialog();
            OneDrivePath.Text = dialog.SelectedPath;
            RefreshList();
        }

        private void RemoveListItem(object sender, RoutedEventArgs e)
        {
            if (LinkList.SelectedIndex == -1)
            {
                return;
            }
            var target = (LinkList.SelectedItem as string).Split(new string[] { " => " }, StringSplitOptions.None)[0];
            Directory.Delete(target);
            MessageBox.Show(this, "完成");
        }
    }
}
