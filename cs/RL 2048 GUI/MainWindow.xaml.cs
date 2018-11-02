using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Threading;

using Xceed.Wpf.Toolkit;

using _2048_c_sharp;
using _2048_c_sharp.Auto;

namespace _2048_c_sharp.GUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static DBTraining db { get; set; } = new DBTraining();
        private static Conductor<BranchComparison> conductor = new Conductor<BranchComparison>(db);
        private static DispatcherTimer timer = new DispatcherTimer();
        public ObservableCollection<List<int>> BestBoard = new ObservableCollection<List<int>>();
        public ObservableCollection<List<int>> SelectedBoard = new ObservableCollection<List<int>>();
        private int selectedBoardIndex = -1;
        public ObservableCollection<IterationStatus> IterationStatuses = new ObservableCollection<IterationStatus>();

        public MainWindow()
        {
            timer.Tick += Timer_Tick;
            timer.Interval = new TimeSpan(0, 0, 0, 0, 500); //500ms

            InitializeComponent();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (conductor.Stop && !conductor.ActiveBoards.Any())
            {
                Done();
                return;
            }

            //get the best board info, if it's available
            if (conductor.BestBoard != null)
            {
                var disp = conductor.BestBoard.Field.AsDisplayValues();
                UpdateBoardDisplay(disp, BestBoard);
                bestBoardDisplay.ItemsSource = BestBoard;
                lblBBScore.Content = disp.Cast<int>().Sum().ToString();
                lblBBMoves.Content = conductor.BestBoard.MoveCount;
            }

            //get the ongoing task data
            IterationStatuses = new ObservableCollection<IterationStatus>(conductor.ActiveBoards
                                                                                   .Where(ab => ab != null)
                                                                                   .Select(ab => ab.GetStatus()));
            dgThreads.ItemsSource = IterationStatuses;

            //try to update the selected board
            if (selectedBoardIndex != -1)
            {
                var currentState = conductor.ActiveBoards.FirstOrDefault(b => b.Iteration == selectedBoardIndex);
                int[,] dispState = new int[4, 4];
                if (currentState != null)
                    dispState = currentState.Board.Field.AsDisplayValues();
                UpdateBoardDisplay(dispState, SelectedBoard);
                selectedBoardDisplay.ItemsSource = SelectedBoard;
            }
        }

        private void Done()
        {
            btnStart.Content = "Start";
            btnStart.IsEnabled = true;
            timer.Stop();
        }

        private void UpdateBoardDisplay(int[,] disp, ObservableCollection<List<int>> boundList)
            => boundList = new ObservableCollection<List<int>>(Enumerable.Range(0, disp.GetLength(0))
                                                                         .Select(i => Enumerable.Range(0, disp.GetLength(1))
                                                                                                .Select(j => disp[i, j])
                                                                                                .ToList()));

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var caption = (string)(sender as Button).Content;
            if (caption == "Start")
            {
                lblBBScore.Content = "0";
                lblBBMoves.Content = "0";
                int boardCount = iudConcurrentBoardCount.Value.Value;
                Task.Run(() => conductor.Run(boardCount));
                (sender as Button).Content = "Stop";
                timer.Start();
            }
            else
            {
                conductor.Stop = true;
                (sender as Button).IsEnabled = false;
            }
        }

        private void IntegerUpDown_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            conductor.Boards = (sender as IntegerUpDown).Value.Value;
        }

        private void DgThreads_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var newItems = e.AddedItems;
            if (newItems.Count <= 0) return;

            var newSelectedItem = (IterationStatus)newItems[0];
            selectedBoardIndex = newSelectedItem.Iteration;            
        }
    }
}
