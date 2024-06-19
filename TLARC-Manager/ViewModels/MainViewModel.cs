using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using DynamicData;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Input;
using TLARC_Manager.Helpers;
using TLARC_Manager.Views;
using System.Reflection;
using Newtonsoft.Json.Linq;
using Microsoft.CodeAnalysis.Emit;
using System.Text;
using Rcl;
using System.Reflection.Metadata;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Newtonsoft.Json;
using TLARC_Manager.DataModels;
namespace TLARC_Manager.ViewModels;

public class MainViewModel : ViewModelBase
{
    public MainWindow? MainWindowRef { get; set; }
    public Bitmap? Icon { get => _icon; set { this.RaiseAndSetIfChanged(ref _icon, value); } }
    private Bitmap? MaxIcon => ImageHelper.LoadFromResource(new System.Uri("avares://TLARC-Manager/Assets/ButtonIcon/maximize-white-512.png"));
    private Bitmap? NormalIcon => ImageHelper.LoadFromResource(new System.Uri("avares://TLARC-Manager/Assets/ButtonIcon/normal-white-512.png"));
    private Bitmap? _icon;

    public event Action DataChange;

    public Dictionary<string, ClassDeclarationSyntax>? ComponentClasses => _componentClasses;
    public Dictionary<string, CompFilesList>? ComponentJsonLists => _componentJsonLists;

    public ICommand CloseWindowCommand { get; }
    public ICommand MaximizeWindowCommand { get; }
    public ICommand MinimizeWindowCommand { get; }
    public ICommand OpenProjectCommand { get; }
    public ICommand CreateProjectCommand { get; }

    public string? ProjectPath { get; set; }

    private Dictionary<string,ClassDeclarationSyntax>? _componentClasses;
    private Dictionary<string, CompFilesList>? _componentJsonLists;

    public MainViewModel()
    {
        CloseWindowCommand = ReactiveCommand.Create(CloseWindow);
        MaximizeWindowCommand = ReactiveCommand.Create(MaximizeWindow);
        MinimizeWindowCommand = ReactiveCommand.Create(MinimizeWindow);
        OpenProjectCommand = ReactiveCommand.Create(OpenProject);
        CreateProjectCommand = ReactiveCommand.Create(CreateProject);
    }

    public MainViewModel(MainWindow window)
    {
        CloseWindowCommand =  ReactiveCommand.Create(CloseWindow);
        MaximizeWindowCommand =  ReactiveCommand.Create(MaximizeWindow);
        MinimizeWindowCommand =  ReactiveCommand.Create(MinimizeWindow);
        OpenProjectCommand = ReactiveCommand.Create(OpenProject);
        CreateProjectCommand = ReactiveCommand.Create(CreateProject);
        MainWindowRef = window;
        if (window.WindowState == WindowState.Normal)
            Icon = MaxIcon;
        else
            Icon = NormalIcon;
        MainWindowRef.SizeChanged += SetMaxIcon;
    }

    private void CloseWindow()
    {
        MainWindowRef?.Close();
    }
    private void MaximizeWindow()
    {
        if ((MainWindowRef ?? throw new()).WindowState == WindowState.Normal)
            (MainWindowRef ?? throw new()).WindowState = WindowState.Maximized;
        else
            (MainWindowRef ?? throw new()).WindowState = WindowState.Normal;
    }
    private void MinimizeWindow()
    {
        (MainWindowRef??throw new()).WindowState = WindowState.Minimized;
    }
    void SetMaxIcon(object? sender,SizeChangedEventArgs e)
    {
        if ((MainWindowRef ?? throw new()).WindowState == WindowState.Normal)
            Icon = MaxIcon;
        else
            Icon = NormalIcon;
    }

    async void  OpenProject()
    {
        var topLevel = TopLevel.GetTopLevel(MainWindowRef);
        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open Text File",
            FileTypeFilter = [new FilePickerFileType("csproj") {
                Patterns = ["*.csproj" ],
                AppleUniformTypeIdentifiers = [ "" ],
                MimeTypes = [ "csproj /*" ]
            }],
            AllowMultiple = false
        });

        if (files.Count >= 1)
        {
            _componentClasses = [];
            _componentJsonLists = [];
            var process = new ProcessBarWindows();
            process.Show();
            Uri fileRoot = files[0].Path;
            List<DirectoryInfo> directories = [new DirectoryInfo(fileRoot.AbsolutePath).Parent.GetDirectories().First(x => x.Name == "src")];
            List<FileInfo> csFiles = [];
            List<string> csharps = [];

            while (directories.Count > 0)
            {
                var d = directories[0];
                directories.RemoveAt(0);
                directories.AddRange(d.GetDirectories());
                csFiles.AddRange(d.GetFiles().Where(F => F.Extension == ".cs"));
            }
            process.SetProcess(10f);
            foreach (var i in csFiles)
                csharps.Add(File.ReadAllText(i.FullName, Encoding.UTF8));

            process.SetProcess(30f);
            List<SyntaxTree> syntaxTrees = [];
            int k = 0;
            foreach (var code in csharps)
                syntaxTrees.Add(CSharpSyntaxTree.ParseText(code + '\n').WithFilePath(csFiles[(int)k++].FullName));
            var compilation = CSharpCompilation.Create(null).AddSyntaxTrees(syntaxTrees);

            foreach (var syntaxTree in syntaxTrees)
            {
                var model = compilation.GetSemanticModel(syntaxTree);
                var faceClass = syntaxTree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().FirstOrDefault();
                try
                {
                    var faceSymbol = model.GetDeclaredSymbol(faceClass);
                    var baseType = faceSymbol.BaseType;
                    var @namespace = faceClass.AncestorsAndSelf().OfType<NamespaceDeclarationSyntax>().FirstOrDefault();
                    while (baseType != null)
                    {
                        if (baseType.Name == "Component")
                        {
                            Debug.WriteLine(@namespace.Name.ToString() + '.' + faceSymbol.Name + '\t' + syntaxTree.FilePath);
                            _componentClasses.TryAdd(@namespace.Name.ToString() + '.' + faceSymbol.Name, faceClass);
                            break;
                        }
                        else
                            baseType = baseType.BaseType;
                    }
                }
                catch (Exception)
                {
                    continue;
                }
            }
            process.SetProcess(50);

            var root = new DirectoryInfo(fileRoot.AbsolutePath).Parent.GetDirectories().First(x => x.Name == "declarations");
            directories = [root];
            List<FileInfo> JsonFiles = [];

            while (directories.Count > 0)
            {
                var d = directories[0];
                directories.RemoveAt(0);
                directories.AddRange(d.GetDirectories());
                JsonFiles.AddRange(d.GetFiles().Where(F => F.Extension == ".json"));
            }
            foreach (var jsonFile in JsonFiles)
            {
                _componentJsonLists.TryAdd(jsonFile.FullName.Substring(root.FullName.Length + 1, jsonFile.FullName.LastIndexOf('.') - root.FullName.Length - 1),
                    JsonConvert.DeserializeObject<CompFilesList>(File.ReadAllText(jsonFile.FullName, Encoding.UTF8)));
            }
            process.SetProcess(60);

            var GridPannel = MainWindowRef.FindControl<Grid>("MainGrid");
            GridPannel.Children.Clear();
            GridPannel.Children.Add(new Views.ToolBarView() { });
            Grid.SetColumn(GridPannel.Children[0], 0);
            Grid.SetRow(GridPannel.Children[0], 0);
            Grid.SetColumnSpan(GridPannel.Children[0], 10);
            Grid.SetRowSpan(GridPannel.Children[0], 1);
            GridPannel.Children.Add(new Views.DependenciesAndTimeLine()
            {
                DataContext = this
            });
            DataChange += (GridPannel.Children[1] as DependenciesAndTimeLine).DataChangeHook;
            Grid.SetColumn(GridPannel.Children[1], 0);
            Grid.SetRow(GridPannel.Children[1], 1);
            Grid.SetColumnSpan(GridPannel.Children[1], 4);
            Grid.SetRowSpan(GridPannel.Children[1], 7);
            GridPannel.Children.Add(new Views.CodeEditor() { });
            Grid.SetColumn(GridPannel.Children[2], 4);
            Grid.SetRow(GridPannel.Children[2], 1);
            Grid.SetColumnSpan(GridPannel.Children[2], 4);
            Grid.SetRowSpan(GridPannel.Children[2], 7);
            GridPannel.Children.Add(new Views.ErrorReport() { });
            Grid.SetColumn(GridPannel.Children[3], 0);
            Grid.SetRow(GridPannel.Children[3], 8);
            Grid.SetColumnSpan(GridPannel.Children[3], 8);
            Grid.SetRowSpan(GridPannel.Children[3], 2);
            GridPannel.Children.Add(new Views.ProjectManager() { });
            Grid.SetColumn(GridPannel.Children[4], 8);
            Grid.SetRow(GridPannel.Children[4], 1);
            Grid.SetColumnSpan(GridPannel.Children[4], 2);
            Grid.SetRowSpan(GridPannel.Children[4], 9);
            MainWindowRef.WindowState = WindowState.Maximized;

            DataChange();
            process.Close();
        }
    }
    void CreateProject()
    {

    }
}
