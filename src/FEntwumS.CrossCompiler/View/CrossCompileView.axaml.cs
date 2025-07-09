using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using FEntwumS.CrossCompiler.ViewModel;

namespace FEntwumS.CrossCompiler.View
{
    public partial class CrossCompileViewModel : UserControl
    {
        
        public CrossCompileViewModel() 
        {
            InitializeComponent();
        }
        /*
        protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
        {
            (DataContext as gccFrontendViewModel)?.Detach();
            base.OnDetachedFromVisualTree(e);
        }
        

        private void newSelectionForTargetSystem(object? sender, SelectionChangedEventArgs e)
        {
            if (!targetSystem.SelectionBoxItem.Equals(null))
            {
                (DataContext as ViewModel.CrossCompileViewModel).viewTargetSystem = targetSystem.SelectionBoxItem.ToString();
            }
            
        }*/

        private void clickCrossCompiling(object? sender, RoutedEventArgs e)
        {
            if (DebugBox.IsChecked.GetValueOrDefault() || UploadBox.IsChecked.GetValueOrDefault())
            {
                Compiling.Flyout.Hide();
                (DataContext as ViewModel.CrossCompileViewModel).DoCrossCompiling.Execute(null);
            }
            else
            {
                Compiling.Flyout.ShowAt(Compiling);
            }
        }

        private void checkedUpload(object? sender, RoutedEventArgs e)
        {
            DebugBox.IsChecked = false;
            (DataContext as ViewModel.CrossCompileViewModel).debugSelectRoutine((bool)DebugBox.IsChecked);
        }
        

        private void checkedDebug(object? sender, RoutedEventArgs e)
        {
            UploadBox.IsChecked = false;
            (DataContext as ViewModel.CrossCompileViewModel).debugSelectRoutine((bool)DebugBox.IsChecked);
           
        }
    }
}