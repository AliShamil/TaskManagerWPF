using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TaskManagerWPF.Commands;

namespace TaskManagerWPF;

public partial class MainWindow : Window
{
    public ObservableCollection<Process> Processes { get; set; }
    public List<string> BlackList { get; set; }
    public ICommand BlackListCommand { get; set; }

    public MainWindow()
    {
        InitializeComponent();
        DataContext = this;
        Processes = new(Process.GetProcesses());
        BlackList = new();
        BlackListCommand = new RelayCommand(ExecuteBlackListCommand);
        cb_Priority.ItemsSource = Enum.GetValues(typeof(ProcessPriorityClass)).Cast<ProcessPriorityClass>();
        
        var timer = new Timer();
        timer.Interval = 5000;

        timer.Elapsed += Timer_ElapsedRefresh;
        timer.Elapsed += Timer_ElapsedBlackList;
        timer.Start();

    }

    private void ExecuteBlackListCommand(object? parameter)
    {
        if (ProccesList.SelectedItem is null)
            return;

        if (ProccesList.SelectedItem is Process p)
        {
            if (!BlackList.Contains(p.ProcessName))
                BlackList.Add(p.ProcessName);
        }
    }

    private void Timer_ElapsedRefresh(object? sender, ElapsedEventArgs e)
    {
        Dispatcher.Invoke(() =>
        {
            Processes.Clear();
            foreach (var p in Process.GetProcesses())
                Processes.Add(p);
        }
        );

    }

    private void Timer_ElapsedBlackList(object? sender, ElapsedEventArgs e)
    {
        Dispatcher.Invoke(() =>
        {
            foreach (var p in Processes)
            {
                if (BlackList.Any(s => s == p.ProcessName))
                {
                    try
                    {
                        p.Kill();
                        Processes.Remove(p);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message); ;
                    }
                }
            }
        }
        );

    }

    private void Btn_Remove_Click(object sender, RoutedEventArgs e)
    {
        if (ProccesList.SelectedItem is null)
            return;

        if (ProccesList.SelectedItem is Process process)
        {
            try
            {
                process.Kill();
                Processes.Remove(process);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }
    }

    private void Btn_Search_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(SearchTxt.Text))
        {
            MessageBox.Show("Proccess Name must be filled!");
            return;
        }

        try
        {
            ProcessStartInfo startInfo = new(SearchTxt.Text);

            startInfo.WindowStyle = ProcessWindowStyle.Minimized;

            Process.Start(startInfo);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message);
        }

    }

    private void Btn_Add_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txt_BlackList.Text))
        {
            MessageBox.Show("Proccess Name must be filled!");
            return;
        }

        BlackList.Add(txt_BlackList.Text);
    }

    private void Btn_ChangePriority(object sender, RoutedEventArgs e)
    {
        (ProccesList.SelectedItem as Process)!.PriorityClass = (ProcessPriorityClass)cb_Priority.SelectedItem;
    }
}