using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

using Xceed.Wpf.Toolkit;

using _2048_c_sharp.Auto;
using _2048_c_sharp.Utilities;

namespace _2048_c_sharp.GUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static DBTraining db { get; set; } = new DBTraining();
        //private static Conductor<RLOne> conductor = new Conductor<RLOne>(db);
        private static Conductor<BranchComparison> conductor = new Conductor<BranchComparison>(db);
        private static DispatcherTimer timer = new DispatcherTimer();
        public ObservableCollection<List<int>> BestBoard { get; set; } = new ObservableCollection<List<int>>();
        public ObservableCollection<List<int>> SelectedBoard { get; set; } = new ObservableCollection<List<int>>();
        private int selectedBoardIndex = -1;
        public ObservableCollection<IterationStatus> IterationStatuses { get; set; } = new ObservableCollection<IterationStatus>();

        public MainWindow()
        {
            timer.Tick += Timer_Tick;
            timer.Interval = new TimeSpan(0, 0, 0, 0, 500); //500ms

            InitializeComponent();
            this.DataContext = this;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (conductor.Stop && !conductor.ActiveBoards.Any())
            {
                Done();
                return;
            }
            //else
            UpdateBestBoard();
            UpdateIterationStatuses();
            UpdateSelectedBoard();
        }

        private void UpdateIterationStatuses()
        {
            //get the ongoing task data
            List<IterationStatus> activeBoards;
            lock (conductor)
            {
                activeBoards = conductor.ActiveBoards.Where(ab => ab != null)
                                                     .Select(ab => ab.GetStatus()).ToList();
            }
            
            //update any boards we already know of
            foreach (var board in IterationStatuses.Where(s => !s.Closed))
            {
                var matchingBoard = activeBoards.FirstOrDefault(b => b.Iteration == board.Iteration);

                //check for removed boards
                if (matchingBoard == null)
                {
                    board.Close();
                    continue;
                }

                //update the rest
                board.Update(matchingBoard);
            }

            //add any new boards
            IterationStatuses.AddRange(activeBoards.Where(b => !IterationStatuses.Any(s => s.Iteration == b.Iteration)));            
        }

        private void UpdateBestBoard()
        {
            //get the best board info, if it's available
            if (conductor.BestBoard != null)
            {
                var disp = conductor.BestBoard.Field.AsDisplayValues();
                UpdateBoardDisplay(disp, BestBoard);
                bestBoardDisplay.ItemsSource = BestBoard;
                lblBBScore.Content = disp.Cast<int>().Sum().ToString();
                lblBBMoves.Content = conductor.BestBoard.MoveCount;
            }
        }

        private void UpdateSelectedBoard()
        {
            //try to update the selected board
            if (selectedBoardIndex != -1)
            {                
                int[,] dispState = new int[4, 4];
                lock (conductor)
                {
                    var currentState = conductor.ActiveBoards.FirstOrDefault(b => b?.Iteration == selectedBoardIndex); //the null check here prevents some unusual null exceptions
                    if (currentState != null)
                        dispState = currentState.Board.Field.AsDisplayValues();
                }
                UpdateBoardDisplay(dispState, SelectedBoard);
                selectedBoardDisplay.ItemsSource = SelectedBoard;
            }
        }

        private void Done()
        {
            btnStart.Content = "Start";
            btnStart.IsEnabled = true;
            timer.Stop();
            IterationStatuses.Clear();
            dgThreads.Items.Refresh();
        }

        private void UpdateBoardDisplay(int[,] disp, ObservableCollection<List<int>> boundList)
        {
            boundList.Clear();
            boundList.AddRange(Enumerable.Range(0, disp.GetLength(0))
                                         .Select(i => Enumerable.Range(0, disp.GetLength(1))
                                                                .Select(j => disp[i, j])
                                                                .ToList()));
        }

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

        private void DgThreads_AutoGeneratedColumns(object sender, EventArgs e)
        {                        
            AddSortColumn(dgThreads, nameof(IterationStatus.Closed));
            AddSortColumn(dgThreads, nameof(IterationStatus.Iteration));
        }

        private void AddSortColumn(DataGrid dg, string colName, ListSortDirection listSortDirection = ListSortDirection.Ascending)
        {
            var col = dg.Columns.FirstOrDefault(c => c.Header.ToString() == colName);
            if (col == null) return;
            col.SortDirection = listSortDirection;
            dg.Items.SortDescriptions.Add(new SortDescription(col.SortMemberPath, listSortDirection));            
        }
    }
}
