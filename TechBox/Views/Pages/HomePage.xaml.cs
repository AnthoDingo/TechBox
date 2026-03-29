// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System.Diagnostics;
using TechBox.ViewModels.Pages;
using Wpf.Ui.Controls;

namespace TechBox.Views.Pages
{
    public partial class HomePage : INavigableView<HomeViewModel>
    {
        public HomeViewModel ViewModel { get; }

        public HomePage(HomeViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;

            InitializeComponent();
        }

        private void AutoSuggestBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            ViewModel.GetComputer(args.SelectedItem.ToString());
        }
    }
}
