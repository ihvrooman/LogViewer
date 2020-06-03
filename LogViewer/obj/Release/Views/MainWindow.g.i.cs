﻿#pragma checksum "..\..\..\Views\MainWindow.xaml" "{8829d00f-11b8-4213-878b-770e8597ac16}" "3A4C967A8A615844D7885D93E70D8EA0582D1BFB0FCC96A281F009ECA9F6A845"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using AppStandards.Converters;
using LogViewer;
using LogViewer.Helpers;
using LogViewer.ViewModels.DesignViewModels;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;


namespace LogViewer.Views {
    
    
    /// <summary>
    /// MainWindow
    /// </summary>
    public partial class MainWindow : System.Windows.Window, System.Windows.Markup.IComponentConnector, System.Windows.Markup.IStyleConnector {
        
        
        #line 175 "..\..\..\Views\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox SearchTextbox;
        
        #line default
        #line hidden
        
        
        #line 181 "..\..\..\Views\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox ExcludeTextbox;
        
        #line default
        #line hidden
        
        
        #line 326 "..\..\..\Views\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ListView LogFilesListView;
        
        #line default
        #line hidden
        
        
        #line 452 "..\..\..\Views\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ListView LogEntriesListView;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/LogViewer;component/views/mainwindow.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\Views\MainWindow.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            
            #line 17 "..\..\..\Views\MainWindow.xaml"
            ((LogViewer.Views.MainWindow)(target)).SourceInitialized += new System.EventHandler(this.Window_SourceInitialized);
            
            #line default
            #line hidden
            
            #line 18 "..\..\..\Views\MainWindow.xaml"
            ((LogViewer.Views.MainWindow)(target)).Loaded += new System.Windows.RoutedEventHandler(this.Window_Loaded);
            
            #line default
            #line hidden
            
            #line 19 "..\..\..\Views\MainWindow.xaml"
            ((LogViewer.Views.MainWindow)(target)).Closing += new System.ComponentModel.CancelEventHandler(this.Window_Closing);
            
            #line default
            #line hidden
            return;
            case 2:
            
            #line 115 "..\..\..\Views\MainWindow.xaml"
            ((System.Windows.Controls.DatePicker)(target)).SelectedDateChanged += new System.EventHandler<System.Windows.Controls.SelectionChangedEventArgs>(this.DateOrUserInfoFilter_Changed);
            
            #line default
            #line hidden
            return;
            case 3:
            
            #line 116 "..\..\..\Views\MainWindow.xaml"
            ((System.Windows.Controls.ComboBox)(target)).SelectionChanged += new System.Windows.Controls.SelectionChangedEventHandler(this.DateOrUserInfoFilter_Changed);
            
            #line default
            #line hidden
            return;
            case 4:
            
            #line 117 "..\..\..\Views\MainWindow.xaml"
            ((System.Windows.Controls.ComboBox)(target)).SelectionChanged += new System.Windows.Controls.SelectionChangedEventHandler(this.DateOrUserInfoFilter_Changed);
            
            #line default
            #line hidden
            return;
            case 5:
            
            #line 118 "..\..\..\Views\MainWindow.xaml"
            ((System.Windows.Controls.ComboBox)(target)).SelectionChanged += new System.Windows.Controls.SelectionChangedEventHandler(this.DateOrUserInfoFilter_Changed);
            
            #line default
            #line hidden
            return;
            case 6:
            
            #line 119 "..\..\..\Views\MainWindow.xaml"
            ((System.Windows.Controls.ComboBox)(target)).SelectionChanged += new System.Windows.Controls.SelectionChangedEventHandler(this.DateOrUserInfoFilter_Changed);
            
            #line default
            #line hidden
            return;
            case 7:
            
            #line 123 "..\..\..\Views\MainWindow.xaml"
            ((System.Windows.Controls.DatePicker)(target)).SelectedDateChanged += new System.EventHandler<System.Windows.Controls.SelectionChangedEventArgs>(this.DateOrUserInfoFilter_Changed);
            
            #line default
            #line hidden
            return;
            case 8:
            
            #line 124 "..\..\..\Views\MainWindow.xaml"
            ((System.Windows.Controls.ComboBox)(target)).SelectionChanged += new System.Windows.Controls.SelectionChangedEventHandler(this.DateOrUserInfoFilter_Changed);
            
            #line default
            #line hidden
            return;
            case 9:
            
            #line 125 "..\..\..\Views\MainWindow.xaml"
            ((System.Windows.Controls.ComboBox)(target)).SelectionChanged += new System.Windows.Controls.SelectionChangedEventHandler(this.DateOrUserInfoFilter_Changed);
            
            #line default
            #line hidden
            return;
            case 10:
            
            #line 126 "..\..\..\Views\MainWindow.xaml"
            ((System.Windows.Controls.ComboBox)(target)).SelectionChanged += new System.Windows.Controls.SelectionChangedEventHandler(this.DateOrUserInfoFilter_Changed);
            
            #line default
            #line hidden
            return;
            case 11:
            
            #line 127 "..\..\..\Views\MainWindow.xaml"
            ((System.Windows.Controls.ComboBox)(target)).SelectionChanged += new System.Windows.Controls.SelectionChangedEventHandler(this.DateOrUserInfoFilter_Changed);
            
            #line default
            #line hidden
            return;
            case 12:
            
            #line 135 "..\..\..\Views\MainWindow.xaml"
            ((System.Windows.Controls.CheckBox)(target)).Checked += new System.Windows.RoutedEventHandler(this.LogTypeFilter_Changed);
            
            #line default
            #line hidden
            
            #line 135 "..\..\..\Views\MainWindow.xaml"
            ((System.Windows.Controls.CheckBox)(target)).Unchecked += new System.Windows.RoutedEventHandler(this.LogTypeFilter_Changed);
            
            #line default
            #line hidden
            return;
            case 13:
            
            #line 136 "..\..\..\Views\MainWindow.xaml"
            ((System.Windows.Controls.CheckBox)(target)).Checked += new System.Windows.RoutedEventHandler(this.LogTypeFilter_Changed);
            
            #line default
            #line hidden
            
            #line 136 "..\..\..\Views\MainWindow.xaml"
            ((System.Windows.Controls.CheckBox)(target)).Unchecked += new System.Windows.RoutedEventHandler(this.LogTypeFilter_Changed);
            
            #line default
            #line hidden
            return;
            case 14:
            
            #line 137 "..\..\..\Views\MainWindow.xaml"
            ((System.Windows.Controls.CheckBox)(target)).Checked += new System.Windows.RoutedEventHandler(this.LogTypeFilter_Changed);
            
            #line default
            #line hidden
            
            #line 137 "..\..\..\Views\MainWindow.xaml"
            ((System.Windows.Controls.CheckBox)(target)).Unchecked += new System.Windows.RoutedEventHandler(this.LogTypeFilter_Changed);
            
            #line default
            #line hidden
            return;
            case 15:
            
            #line 138 "..\..\..\Views\MainWindow.xaml"
            ((System.Windows.Controls.CheckBox)(target)).Checked += new System.Windows.RoutedEventHandler(this.LogTypeFilter_Changed);
            
            #line default
            #line hidden
            
            #line 138 "..\..\..\Views\MainWindow.xaml"
            ((System.Windows.Controls.CheckBox)(target)).Unchecked += new System.Windows.RoutedEventHandler(this.LogTypeFilter_Changed);
            
            #line default
            #line hidden
            return;
            case 16:
            
            #line 139 "..\..\..\Views\MainWindow.xaml"
            ((System.Windows.Controls.CheckBox)(target)).Checked += new System.Windows.RoutedEventHandler(this.LogTypeFilter_Changed);
            
            #line default
            #line hidden
            
            #line 139 "..\..\..\Views\MainWindow.xaml"
            ((System.Windows.Controls.CheckBox)(target)).Unchecked += new System.Windows.RoutedEventHandler(this.LogTypeFilter_Changed);
            
            #line default
            #line hidden
            return;
            case 17:
            
            #line 140 "..\..\..\Views\MainWindow.xaml"
            ((System.Windows.Controls.CheckBox)(target)).Checked += new System.Windows.RoutedEventHandler(this.LogTypeFilter_Changed);
            
            #line default
            #line hidden
            
            #line 140 "..\..\..\Views\MainWindow.xaml"
            ((System.Windows.Controls.CheckBox)(target)).Unchecked += new System.Windows.RoutedEventHandler(this.LogTypeFilter_Changed);
            
            #line default
            #line hidden
            return;
            case 18:
            
            #line 141 "..\..\..\Views\MainWindow.xaml"
            ((System.Windows.Controls.CheckBox)(target)).Checked += new System.Windows.RoutedEventHandler(this.LogTypeFilter_Changed);
            
            #line default
            #line hidden
            
            #line 141 "..\..\..\Views\MainWindow.xaml"
            ((System.Windows.Controls.CheckBox)(target)).Unchecked += new System.Windows.RoutedEventHandler(this.LogTypeFilter_Changed);
            
            #line default
            #line hidden
            return;
            case 19:
            
            #line 142 "..\..\..\Views\MainWindow.xaml"
            ((System.Windows.Controls.CheckBox)(target)).Checked += new System.Windows.RoutedEventHandler(this.LogTypeFilter_Changed);
            
            #line default
            #line hidden
            
            #line 142 "..\..\..\Views\MainWindow.xaml"
            ((System.Windows.Controls.CheckBox)(target)).Unchecked += new System.Windows.RoutedEventHandler(this.LogTypeFilter_Changed);
            
            #line default
            #line hidden
            return;
            case 20:
            
            #line 143 "..\..\..\Views\MainWindow.xaml"
            ((System.Windows.Controls.CheckBox)(target)).Checked += new System.Windows.RoutedEventHandler(this.LogTypeFilter_Changed);
            
            #line default
            #line hidden
            
            #line 143 "..\..\..\Views\MainWindow.xaml"
            ((System.Windows.Controls.CheckBox)(target)).Unchecked += new System.Windows.RoutedEventHandler(this.LogTypeFilter_Changed);
            
            #line default
            #line hidden
            return;
            case 21:
            
            #line 144 "..\..\..\Views\MainWindow.xaml"
            ((System.Windows.Controls.CheckBox)(target)).Checked += new System.Windows.RoutedEventHandler(this.LogTypeFilter_Changed);
            
            #line default
            #line hidden
            
            #line 144 "..\..\..\Views\MainWindow.xaml"
            ((System.Windows.Controls.CheckBox)(target)).Unchecked += new System.Windows.RoutedEventHandler(this.LogTypeFilter_Changed);
            
            #line default
            #line hidden
            return;
            case 22:
            
            #line 162 "..\..\..\Views\MainWindow.xaml"
            ((System.Windows.Controls.ComboBox)(target)).SelectionChanged += new System.Windows.Controls.SelectionChangedEventHandler(this.DateOrUserInfoFilter_Changed);
            
            #line default
            #line hidden
            return;
            case 23:
            
            #line 166 "..\..\..\Views\MainWindow.xaml"
            ((System.Windows.Controls.ComboBox)(target)).SelectionChanged += new System.Windows.Controls.SelectionChangedEventHandler(this.DateOrUserInfoFilter_Changed);
            
            #line default
            #line hidden
            return;
            case 24:
            this.SearchTextbox = ((System.Windows.Controls.TextBox)(target));
            
            #line 175 "..\..\..\Views\MainWindow.xaml"
            this.SearchTextbox.GotFocus += new System.Windows.RoutedEventHandler(this.SearchTextbox_GotFocus);
            
            #line default
            #line hidden
            
            #line 175 "..\..\..\Views\MainWindow.xaml"
            this.SearchTextbox.LostFocus += new System.Windows.RoutedEventHandler(this.SearchTextbox_LostFocus);
            
            #line default
            #line hidden
            
            #line 175 "..\..\..\Views\MainWindow.xaml"
            this.SearchTextbox.TextChanged += new System.Windows.Controls.TextChangedEventHandler(this.SearchOrExcludeFilter_Changed);
            
            #line default
            #line hidden
            return;
            case 25:
            this.ExcludeTextbox = ((System.Windows.Controls.TextBox)(target));
            
            #line 181 "..\..\..\Views\MainWindow.xaml"
            this.ExcludeTextbox.GotFocus += new System.Windows.RoutedEventHandler(this.ExcludeTextbox_GotFocus);
            
            #line default
            #line hidden
            
            #line 181 "..\..\..\Views\MainWindow.xaml"
            this.ExcludeTextbox.LostFocus += new System.Windows.RoutedEventHandler(this.ExcludeTextbox_LostFocus);
            
            #line default
            #line hidden
            
            #line 181 "..\..\..\Views\MainWindow.xaml"
            this.ExcludeTextbox.TextChanged += new System.Windows.Controls.TextChangedEventHandler(this.SearchOrExcludeFilter_Changed);
            
            #line default
            #line hidden
            return;
            case 26:
            this.LogFilesListView = ((System.Windows.Controls.ListView)(target));
            return;
            case 29:
            this.LogEntriesListView = ((System.Windows.Controls.ListView)(target));
            return;
            }
            this._contentLoaded = true;
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        void System.Windows.Markup.IStyleConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 27:
            
            #line 372 "..\..\..\Views\MainWindow.xaml"
            ((System.Windows.Controls.CheckBox)(target)).Checked += new System.Windows.RoutedEventHandler(this.LogFileOrDatabase_KeepCurrentChanged);
            
            #line default
            #line hidden
            
            #line 372 "..\..\..\Views\MainWindow.xaml"
            ((System.Windows.Controls.CheckBox)(target)).Unchecked += new System.Windows.RoutedEventHandler(this.LogFileOrDatabase_KeepCurrentChanged);
            
            #line default
            #line hidden
            break;
            case 28:
            
            #line 420 "..\..\..\Views\MainWindow.xaml"
            ((System.Windows.Controls.CheckBox)(target)).Checked += new System.Windows.RoutedEventHandler(this.LogFileOrDatabase_KeepCurrentChanged);
            
            #line default
            #line hidden
            
            #line 420 "..\..\..\Views\MainWindow.xaml"
            ((System.Windows.Controls.CheckBox)(target)).Unchecked += new System.Windows.RoutedEventHandler(this.LogFileOrDatabase_KeepCurrentChanged);
            
            #line default
            #line hidden
            break;
            }
        }
    }
}

