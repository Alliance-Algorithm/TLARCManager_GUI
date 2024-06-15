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
namespace TLARC_Manager.ViewModels;

public class MainViewModel : ViewModelBase
{
    public MainWindow? MainWindowRef { get; set; }
    public string Greeting => "Welcome to Avalonia!";
    public Bitmap? Icon { get => _icon; set { this.RaiseAndSetIfChanged(ref _icon, value); } }
    private Bitmap? MaxIcon => ImageHelper.LoadFromResource(new System.Uri("avares://TLARC-Manager/Assets/ButtonIcon/maximize-white-512.png"));
    private Bitmap? NormalIcon => ImageHelper.LoadFromResource(new System.Uri("avares://TLARC-Manager/Assets/ButtonIcon/normal-white-512.png"));
    private Bitmap? _icon;
    public ICommand CloseWindowCommand { get; }
    public ICommand MaximizeWindowCommand { get; }
    public ICommand MinimizeWindowCommand { get; }
    public ICommand OpenProjectCommand { get; }
    public ICommand CreateProjectCommand { get; }

    public string ProjectPath { get; set; }

    private Dictionary<string,ClassDeclarationSyntax> ComponentClasses;

    public MainViewModel()
    {
        CloseWindowCommand = ReactiveCommand.Create(CloseWindow);
        MaximizeWindowCommand = ReactiveCommand.Create(MaximizeWindow);
        MinimizeWindowCommand = ReactiveCommand.Create(MinimizeWindow);
    }

    public MainViewModel(MainWindow? window)
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
    {// 从当前控件获取 TopLevel。或者，您也可以使用 Window 引用。
        var topLevel = TopLevel.GetTopLevel(MainWindowRef);
        // 启动异步操作以打开对话框。
        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open Text File",
            FileTypeFilter = [new FilePickerFileType("csproj") {
                Patterns = new[] { "*.csproj" },
                AppleUniformTypeIdentifiers = new[] { "" },
                MimeTypes = new[] { "csproj /*" }
            }],
            AllowMultiple = false
        });

        if (files.Count >= 1)
        {
            ComponentClasses = [];
            var process = new ProcessBarWindows();
            process.Show();
            Uri fileRoot = files[0].Path;
            List<DirectoryInfo> directories =[new DirectoryInfo(fileRoot.AbsolutePath).Parent.GetDirectories().First(x => x.Name == "src")];
            List<FileInfo> csFiles = new();
            List<FileInfo> dllFiles = new();
            List<string> csharps = new();

            while ( directories.Count > 0) {
                var d = directories[0];
                
                directories.RemoveAt(0);
                directories.AddRange(d.GetDirectories());
                foreach (var f in d.GetFiles())
                    if (f.Extension == ".cs")
                        csFiles.Add(f);
            }
            process.SetProcess(30f);
            foreach (var i in csFiles)
                csharps.Add(File.ReadAllText(i.FullName,Encoding.UTF8));
            
            process.SetProcess(10f);
            List<SyntaxTree> syntaxTrees = [];
            int k = 0;
            foreach (var code in csharps)
                syntaxTrees .Add(CSharpSyntaxTree.ParseText(code + '\n').WithFilePath(csFiles[(int)k++].FullName));
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
                            Debug.WriteLine(@namespace.Name.ToString()+'.'+ faceSymbol.Name + '\t' + syntaxTree.FilePath);
                            ComponentClasses.TryAdd(@namespace.Name.ToString() + '.' + faceSymbol.Name, faceClass);
                            break;
                        }
                        else
                            baseType = baseType.BaseType;
                    }
                }
                catch (Exception e)
                {
                    continue;
                }
            }
            process.SetProcess(30);

            process.Close();
        }
    }
    void CreateProject()
    {

    }
}
