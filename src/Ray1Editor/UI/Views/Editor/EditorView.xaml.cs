﻿using System.Windows;
using System.Windows.Controls;

namespace Ray1Editor;

/// <summary>
/// Interaction logic for EditorView.xaml
/// </summary>
public partial class EditorView : UserControl
{
    public EditorView(EditorViewModel viewModel)
    {
        InitializeComponent();
        ViewModel = viewModel;
        viewModel.PropertyChanged += (_, e) =>
        {
            if (!ViewModel.IsSelectingObjFromList && e.PropertyName == nameof(EditorViewModel.SelectedObject) && ViewModel.SelectedObject != null)
                EditorTabControl.SelectedIndex = 3;
        };

        EditorTabsColumnDef.Width = new GridLength(R1EServices.App.UserData.UI_EditorTabsWidth);
    }

    public EditorViewModel ViewModel
    {
        get => DataContext as EditorViewModel;
        set => DataContext = value;
    }

    private void EditorView_OnUnloaded(object sender, RoutedEventArgs e)
    {
        R1EServices.App.UserData.UI_EditorTabsWidth = EditorTabsColumnDef.Width.Value;
    }
}