using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.Input;
using DynamicData;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Input;
using DataEditor.Models;
using DataEditor.Models.Enums;
using DataEditor.Support;
using System.Threading.Tasks;

namespace DataEditor.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase, INotifyPropertyChanged
    {
        public MainWindowViewModel()
        {
            init();

            KeyDownModeCommand = ReactiveCommand.Create<object>(HandleModeKeyDown);
            CopyModeCommand = new RelayCommand<object>(CopyRowsMode);
            PasteModeCommand = new RelayCommand<object>(PasteRowsMode);
            AddNewRowModeCommand = new RelayCommand(AddNewRowMode);
            DeleteRowModeCommand = new RelayCommand<object>(DeleteRowMode);

            KeyDownStepCommand = ReactiveCommand.Create<object>(HandleStepKeyDown);
            CopyStepCommand = new RelayCommand<object>(CopyRowsStep);
            PasteStepCommand = new RelayCommand<object>(PasteRowsStep);
            AddNewRowStepCommand = new RelayCommand(AddNewRowStep);
            DeleteRowStepCommand = new RelayCommand<object>(DeleteRowStep);
        }

        public void init()
        {
            _modelMode = DatabaseHelper.LoadModes();
            foreach (var mode in _modelMode)
            {
                mode.PropertyChanged += OnModePropertyChanged;
            }

            _modelStep = DatabaseHelper.LoadSteps();
            foreach (var step in _modelStep)
            {
                step.PropertyChanged += OnStepPropertyChanged;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public async Task<bool> Notification(
            string txt = "",
            string header = "Warning",
            string btn = "OK")
        {
            var msgWnd = new NotificationWindow(txt, header, btn);
            bool result = await msgWnd.ShowDialog<bool>(AppData.Instance.Wnd);
            return result;
        }

        #region Mode
        private void Mode_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            DatabaseHelper.SaveModeChanges(sender as Mode, DBOperation.Modify);
        }

        public ICommand KeyDownModeCommand { get; }
        public ICommand CopyModeCommand { get; }
        public ICommand PasteModeCommand { get; }
        public ICommand AddNewRowModeCommand { get; }
        public ICommand DeleteRowModeCommand { get; }

        public ObservableCollection<Mode> _modelMode;
        public ObservableCollection<Mode> ModelMode
        {
            get => _modelMode;
            set
            {
                if (_modelMode != value)
                {
                    if (_modelMode != null)
                    {
                        _modelMode.CollectionChanged -= OnModelModeCollectionChanged;
                        UnsubscribeFromModelModePropertyChanged(_modelMode);
                    }
                    _modelMode = value;
                    if (_modelMode != null)
                    {
                        _modelMode.CollectionChanged += OnModelModeCollectionChanged;
                        SubscribeToModelModePropertyChanged(_modelMode);
                    }
                    OnPropertyChanged();
                }
            }
        }

        private void OnModelModeCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (Mode mode in e.NewItems)
                {
                    mode.PropertyChanged += OnModePropertyChanged;
                }
            }

            if (e.OldItems != null)
            {
                foreach (Mode mode in e.OldItems)
                {
                    mode.PropertyChanged -= OnModePropertyChanged;
                }
            }
        }

        private void OnModePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            DatabaseHelper.SaveModeChanges(sender as Mode, DBOperation.Modify);
        }

        private void SubscribeToModelModePropertyChanged(ObservableCollection<Mode> model)
        {
            foreach (var mode in model)
            {
                mode.PropertyChanged += OnModePropertyChanged;
            }
        }

        private void UnsubscribeFromModelModePropertyChanged(ObservableCollection<Mode> model)
        {
            foreach (var mode in model)
            {
                mode.PropertyChanged -= OnModePropertyChanged;
            }
        }

        private void AddNewRowMode()
        {
            ModelMode.Add(new Mode());
            ModelMode[ModelMode.Count - 1].PropertyChanged += OnModePropertyChanged;
            DatabaseHelper.SaveModeChanges(ModelMode[ModelMode.Count - 1], DBOperation.New);
        }
        private async void DeleteRowMode(object parameter)
        {
            if (parameter == null)
            {
                return;
            }
            if (parameter is Mode mode)
            {
                for (int i = 0; i < ModelStep.Count; i++)
                {
                    if (ModelStep[i].ModeId == mode.ID)
                    {
                        await Notification("Can't delete this record, because at least the record with ID "+ ModelStep[i].ID+" in Steps table has connection to it. Please edit it first.");
                        return;
                    }
                }
                mode.PropertyChanged -= OnModePropertyChanged;
                DatabaseHelper.SaveModeChanges(mode, DBOperation.Delete);
                ModelMode.Remove(mode);
            }
        }

        private void HandleModeKeyDown(object parameter)
        {
            if (parameter is KeyEventArgs e)
            {
                if (e.Key == Key.C && e.KeyModifiers.HasFlag(KeyModifiers.Control))
                {
                    if (AppData.Instance.Wnd.FindControl<StackPanel>("ModesPanel").IsVisible)
                    {
                        CopyModeCommand.Execute(null);
                    }
                }
                else if (e.Key == Key.V && e.KeyModifiers.HasFlag(KeyModifiers.Control))
                {
                    if(AppData.Instance.Wnd.FindControl<StackPanel>("ModesPanel").IsVisible)
                    {
                        PasteModeCommand.Execute(null);
                    }
                }
            }
        }

        private async void CopyRowsMode(object parameter)
        {
            var mainWindow = AppData.Instance.Wnd as MainWindow;
            if (mainWindow == null)
            {
                return;
            }
            var dataGrid = mainWindow.FindControl<DataGrid>("ModesPanelGrid");
            if (dataGrid.SelectedItems != null && dataGrid.SelectedItems.Cast<object>().Any())
            {
                var clipboardText = new StringBuilder();
                foreach (var item in dataGrid.SelectedItems)
                {
                    if (item is Mode mode)
                    {
                        clipboardText.AppendLine($"{mode.ID}\t{mode.Name}\t{mode.MaxBottleNumber}\t{mode.MaxUsedTips}");
                    }
                }
                var topLevel = TopLevel.GetTopLevel(dataGrid);
                if (topLevel != null)
                {
                    var textToCopy = clipboardText.ToString().Replace("\"", string.Empty);
                    await topLevel.Clipboard.SetTextAsync(textToCopy);
                }
            }
        }

        private async void PasteRowsMode(object parameter)
        {
            var mainWindow = AppData.Instance.Wnd as MainWindow;
            if (mainWindow == null)
            {
                return;
            }
            var dataGrid = mainWindow.FindControl<DataGrid>("ModesPanelGrid");
            var topLevel = TopLevel.GetTopLevel(dataGrid);
            if (topLevel != null)
            {
                string clipboardText = await topLevel.Clipboard.GetTextAsync();
                if (!string.IsNullOrEmpty(clipboardText))
                {
                    var rows = clipboardText.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                    var selectedIndex = dataGrid.SelectedIndex;

                    var itemsSource = dataGrid.ItemsSource as IList<Mode>;
                    if (itemsSource == null)
                    {
                        return;
                    }
                    if (selectedIndex < 0)
                    {
                        selectedIndex = itemsSource.Count;
                    }
                    foreach (var row in rows)
                    {
                        var columns = row.Split('\t');
                        if ((columns.Length >= 4) && 
                            int.TryParse(columns[0], out int id) &&
                            int.TryParse(columns[2], out int maxBottleNumber) &&
                            int.TryParse(columns[3], out int maxUsedTips))
                        {
                            string name = !string.IsNullOrEmpty(columns[1]) ? columns[1] : "";
                            
                            if (selectedIndex < ModelMode.Count)
                            {
                                ModelMode[selectedIndex].Name = name;
                                ModelMode[selectedIndex].MaxBottleNumber = maxBottleNumber;
                                ModelMode[selectedIndex].MaxUsedTips = maxUsedTips;
                                DatabaseHelper.SaveModeChanges(ModelMode[selectedIndex], DBOperation.Modify);
                            }
                            else
                            {
                                Mode newMode = new Mode(id, name, maxBottleNumber, maxUsedTips);
                                newMode.PropertyChanged += OnModePropertyChanged;
                                ModelMode.Add(newMode);
                                DatabaseHelper.SaveModeChanges(ModelMode[ModelMode.Count-1], DBOperation.New);
                            }
                            selectedIndex++;
                        }
                    }
                }
            }
        }

        #endregion

        #region Step
        private void Step_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            DatabaseHelper.SaveStepChanges(sender as Step, DBOperation.Modify);
        }

        public ICommand KeyDownStepCommand { get; }
        public ICommand CopyStepCommand { get; }
        public ICommand PasteStepCommand { get; }
        public ICommand AddNewRowStepCommand { get; }
        public ICommand DeleteRowStepCommand { get; }

        public ObservableCollection<Step> _modelStep;
        public ObservableCollection<Step> ModelStep
        {
            get => _modelStep;
            set
            {
                if (_modelStep != value)
                {
                    if (_modelStep != null)
                    {
                        _modelStep.CollectionChanged -= OnModelStepCollectionChanged;
                        UnsubscribeFromModelStepPropertyChanged(_modelStep);
                    }
                    _modelStep = value;
                    if (_modelStep != null)
                    {
                        _modelStep.CollectionChanged += OnModelStepCollectionChanged;
                        SubscribeToModelStepPropertyChanged(_modelStep);
                    }
                    OnPropertyChanged();
                }
            }
        }

        private void OnModelStepCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (Step step in e.NewItems)
                {
                    step.PropertyChanged += OnStepPropertyChanged;
                }
            }

            if (e.OldItems != null)
            {
                foreach (Step step in e.OldItems)
                {
                    step.PropertyChanged -= OnStepPropertyChanged;
                }
            }
        }

        private async void OnStepPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            int modeId = (sender as Step).GetOldModeID();
            bool res = false;
            for (int i = 0; i < ModelMode.Count; i++) 
            { 
                if (ModelMode[i].ID == (sender as Step).ModeId)
                {
                    res = true;
                    break;
                }
            }
            if (!res)
            {
                await Notification("There are no records in the table Modes with the Id " + (sender as Step).ModeId + ". \nPlease create this record first.");
                (sender as Step).ModeId = modeId;
                return;
            }
            else
            {

                DatabaseHelper.SaveStepChanges(sender as Step, DBOperation.Modify);
            }
        }

        private void SubscribeToModelStepPropertyChanged(ObservableCollection<Step> model)
        {
            foreach (var step in model)
            {
                step.PropertyChanged += OnStepPropertyChanged;
            }
        }

        private void UnsubscribeFromModelStepPropertyChanged(ObservableCollection<Step> model)
        {
            foreach (var step in model)
            {
                step.PropertyChanged -= OnStepPropertyChanged;
            }
        }
               

        private async void AddNewRowStep()
        {
            if (ModelMode.Count==0)
            {
                await Notification("There are no records in the table Modes to fill ModeId field. \nPlease create this record first.");
                return;
            }
            ModelStep.Add(new Step(0, ModelMode[0].ID,0,"",0,"",0));
            ModelStep[ModelStep.Count - 1].PropertyChanged += OnStepPropertyChanged;
            DatabaseHelper.SaveStepChanges(ModelStep[ModelStep.Count - 1], DBOperation.New);
        }
        private void DeleteRowStep(object parameter)
        {
            if (parameter == null)
            {
                return;
            }
            if (parameter is Step step)
            {
                step.PropertyChanged -= OnStepPropertyChanged;
                DatabaseHelper.SaveStepChanges(step, DBOperation.Delete);
                ModelStep.Remove(step);
            }
        }

        private void HandleStepKeyDown(object parameter)
        {
            if (parameter is KeyEventArgs e)
            {
                if (e.Key == Key.C && e.KeyModifiers.HasFlag(KeyModifiers.Control))
                {
                    if (AppData.Instance.Wnd.FindControl<StackPanel>("StepsPanel").IsVisible)
                    {
                        CopyStepCommand.Execute(null);
                    }
                }
                else if (e.Key == Key.V && e.KeyModifiers.HasFlag(KeyModifiers.Control))
                {
                    if (AppData.Instance.Wnd.FindControl<StackPanel>("StepsPanel").IsVisible)
                    {
                        PasteStepCommand.Execute(null);
                    }
                }
            }
        }

        private async void CopyRowsStep(object parameter)
        {
            var mainWindow = AppData.Instance.Wnd as MainWindow;
            if (mainWindow == null)
            {
                return;
            }
            var dataGrid = mainWindow.FindControl<DataGrid>("StepsPanelGrid");
            if (dataGrid.SelectedItems != null && dataGrid.SelectedItems.Cast<object>().Any())
            {
                var clipboardText = new StringBuilder();
                foreach (var item in dataGrid.SelectedItems)
                {
                    if (item is Step step)
                    {
                        clipboardText.AppendLine($"{step.ID}\t{step.ModeId}\t{step.Timer}\t{step.Destination}\t{step.Speed}\t{step.Type}\t{step.Volume}");
                    }
                }
                var topLevel = TopLevel.GetTopLevel(dataGrid);
                if (topLevel != null)
                {
                    var textToCopy = clipboardText.ToString().Replace("\"", string.Empty);
                    await topLevel.Clipboard.SetTextAsync(textToCopy);
                }
            }
        }

        private async void PasteRowsStep(object parameter)
        {
            var mainWindow = AppData.Instance.Wnd as MainWindow;
            if (mainWindow == null)
            {
                return;
            }

            if (ModelMode.Count == 0)
            {
                await Notification("It's impossible to add any records into Steps table because there are no records in Modes table for ModeId link. Please create any record in Modes table first.");
                return;
            }

                var dataGrid = mainWindow.FindControl<DataGrid>("StepsPanelGrid");
            var topLevel = TopLevel.GetTopLevel(dataGrid);
            if (topLevel != null)
            {
                string clipboardText = await topLevel.Clipboard.GetTextAsync();
                if (!string.IsNullOrEmpty(clipboardText))
                {
                    var rows = clipboardText.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                    var selectedIndex = dataGrid.SelectedIndex;

                    var itemsSource = dataGrid.ItemsSource as IList<Step>;
                    if (itemsSource == null)
                    {
                        return;
                    }
                    if (selectedIndex < 0)
                    {
                        selectedIndex = itemsSource.Count;
                    }
                    foreach (var row in rows)
                    {
                        var columns = row.Split('\t');
                        if ((columns.Length >= 7) &&
                            int.TryParse(columns[0], out int id) &&
                            int.TryParse(columns[1], out int modeId) &&
                            int.TryParse(columns[2], out int timer) &&
                            int.TryParse(columns[4], out int speed) &&
                            int.TryParse(columns[6], out int volume))
                        {                            
                            bool res = false;
                            for (int i = 0; i < ModelMode.Count; i++) 
                            { 
                                if (ModelMode[i].ID == modeId)
                                {
                                    res = true;
                                    break;
                                }
                            }
                            if (!res)
                            {
                                await Notification("There are no records with the Id " + modeId + " in the Mode table. ModeId value will be replaced by the ID of the first record from the Mode table.");
                                modeId = ModelMode[0].ID;
                            }
                            string destination = !string.IsNullOrEmpty(columns[3]) ? columns[3] : "";
                            string type = !string.IsNullOrEmpty(columns[5]) ? columns[5] : "";

                            if (selectedIndex < ModelStep.Count)
                            {
                                ModelStep[selectedIndex].ModeId = modeId;
                                ModelStep[selectedIndex].Timer = timer;
                                ModelStep[selectedIndex].Destination = destination;
                                ModelStep[selectedIndex].Speed = speed;
                                ModelStep[selectedIndex].Type = type;
                                ModelStep[selectedIndex].Volume = volume;
                                DatabaseHelper.SaveStepChanges(ModelStep[selectedIndex], DBOperation.Modify);
                            }
                            else
                            {
                                Step newStep = new Step(id, modeId, timer, destination, speed, type, volume);
                                newStep.PropertyChanged += OnStepPropertyChanged;
                                ModelStep.Add(newStep);
                                DatabaseHelper.SaveStepChanges(ModelStep[ModelStep.Count - 1], DBOperation.New);
                            }
                            selectedIndex++;
                        }
                    }
                }
            }
        }

        #endregion
    }
}