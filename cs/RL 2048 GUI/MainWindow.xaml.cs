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
        //private static Conductor<RandomPlay> conductor = new Conductor<RandomPlay>(); //use this one for random chance
        private static readonly Conductor<BranchComparison> conductor = new Conductor<BranchComparison>();
        private static readonly DispatcherTimer timer = new DispatcherTimer();
        public ObservableCollection<List<GridValue>> BestBoard { get; set; } = new ObservableCollection<List<GridValue>>();
        public ObservableCollection<List<GridValue>> SelectedBoard { get; set; } = new ObservableCollection<List<GridValue>>();
        private int selectedBoardIndex = -1;
        public ObservableCollection<IterationStatus> IterationStatuses { get; set; } = new ObservableCollection<IterationStatus>();

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;

            Task.Run(() => conductor.Run(0)); //start the run loop, but let it idle until threads are assigned

            timer.Tick += Timer_Tick;
            timer.Interval = new TimeSpan(0, 0, 0, 0, 500); //500ms
            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (conductor.Boards == 0 && !conductor.ActiveBoards.Any()) return;

            //else
            UpdateBestBoard();
            UpdateIterationStatuses();
            UpdateSelectedBoard();
        }

        private void UpdateIterationStatuses()
        {

            List<IterationStatus> activeBoards;
            lock (conductor.ActiveBoards)
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
            IterationStatuses.AddRange(activeBoards.Where(b => IterationStatuses.All(s => s.Iteration != b.Iteration)));

            //refresh the sort
            SortColumn(dgThreads, nameof(IterationStatus.Closed), ListSortDirection.Ascending);
            SortColumn(dgThreads, nameof(IterationStatus.Closed), ListSortDirection.Descending);
        }

        private void UpdateBestBoard()
        {
            //get the best board info, if it's available
            if (conductor.BestBoard == null) return;

            UpdateBoardDisplay(conductor.BestBoard.Field, BestBoard);
            bestBoardDisplay.ItemsSource = BestBoard;
            lblBBScore.Content = conductor.BestBoard.Field.Score();
            lblBBMoves.Content = conductor.BestBoard.MoveCount;
        }

        private void UpdateSelectedBoard()
        {
            //try to update the selected board
            if (selectedBoardIndex != -1)
            {                
                byte[,] dispState = new byte[4, 4];
                lock (conductor)
                {
                    var currentState = conductor.ActiveBoards.FirstOrDefault(b => b?.Iteration == selectedBoardIndex); //the null check here prevents some unusual null exceptions
                    if (currentState != null)
                        dispState = currentState.Board.Field;
                }
                UpdateBoardDisplay(dispState, SelectedBoard);
                selectedBoardDisplay.ItemsSource = SelectedBoard;
            }
        }

        /// <summary>
        /// Update an ObservableCollection with integers representing a field
        /// </summary>
        /// <param name="disp"></param>
        /// <param name="boundList"></param>
        private void UpdateBoardDisplay(byte[,] disp, ObservableCollection<List<GridValue>> boundList)
        {
            boundList.Clear();
            boundList.AddRange(Enumerable.Range(0, disp.GetLength(0))
                                         .Select(i => Enumerable.Range(0, disp.GetLength(1))
                                                                .Select(j => new GridValue(disp[i, j]))
                                                                .ToList()));
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

        private void SortColumn(DataGrid dg, string colName, ListSortDirection listSortDirection = ListSortDirection.Ascending)
        {
            var col = dg.Columns.FirstOrDefault(c => c.Header.ToString() == colName);
            if (col == null) return;
            col.SortDirection = listSortDirection;
        }
    }

    public class GridValue
    {
        public static string[] CELL_COLORS = { "#9e948a", "#eee4da", "#ede0c8", "#f2b179", "#f59563", "#f67c5f",
                                               "#f65e3b", "#edcf72", "#edcc61", "#edc850", "#edc53f", "#edc22e" };
        public static string[] TEXT_COLORS = { "#9e948a", "#776e65", "#776e65", "#f9f6f2", "#f9f6f2", "#f9f6f2",
                                               "#f9f6f2", "#f9f6f2", "#f9f6f2", "#f9f6f2", "#f9f6f2", "#f9f6f2" };

        public GridValue(byte val) => Value = val;

        private byte _value = 0;
        public byte Value
        {
            get => _value;
            set
            {
                _value = value;
                DispValue = _value == 0 ? 0 : 1 << _value;
            }
        }

        public int DispValue { get; private set; }
        public string CellColor => CELL_COLORS[_value];
        public string TextColor => TEXT_COLORS[_value];
    }
}
