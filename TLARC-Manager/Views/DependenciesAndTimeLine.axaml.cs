using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using DynamicData;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using System.Windows.Input;
using TLARC_Manager.DataModels;
using TLARC_Manager.ViewModels;

namespace TLARC_Manager.Views;

public partial class DependenciesAndTimeLine : UserControl
{
    public Action DataChangeHook { get => DataChanged; }
    private Vector2 _position;
    private Grid _grid;
    public DependenciesAndTimeLine()
    {
        InitializeComponent();
        _position = new ();
        _grid = this.FindControl<Grid>("MainGrid")??throw new Exception();
    }
    static uint PoolDim = 0;

    private void DataChanged()
    {
        var data = DataContext as MainViewModel;
        _grid.Children.Clear();
        _grid.Margin = new Avalonia.Thickness(Math.Min(-_position.X, 0), Math.Min(-_position.Y, 0), Math.Min(_position.X, 0), Math.Min(_position.Y, 0));
        var components = new Dictionary<uint, ComponentCell>();
        var key_value = new Dictionary<uint, int>();
        int k = 0;
        _grid.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(20)));
        foreach (var lists in data.ComponentJsonLists)
        {
            foreach (var c in lists.Value.list)
            {
                components.Add(c.this_id, c);
                key_value.Add(c.this_id, k);
                _grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
                _grid.Children.Add(new TextBlock() { Text = c.this_id.ToString(),HorizontalAlignment=Avalonia.Layout.HorizontalAlignment.Left,VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center });
                Grid.SetRow(_grid.Children.Last(), k++);
                Grid.SetColumn (_grid.Children.Last(), 0);
                _grid.RowDefinitions.Add(new RowDefinition(new GridLength(10)));
                k++;
            }
        }

        var l = components.Values.ToArray();
        for (int j = 0; j < l.Length; j++)
        {
            try
            {
                foreach (var i in l[j].RecieveID)
                {
                    if (i.Value == 0)
                    {
                        continue;
                    }
                    l[j].Forward.Add(components[i.Value]);
                }
            }
            catch (Exception e)
            {
                Environment.Exit(-1);
            }
        }




        for (int j = 0; j < l.Length; j++)
        {
            if (l[j].Dim != 0)
                continue;
            Hashtable colored = [];
            FindPath(ref l[j], ref colored);
            PoolDim = Math.Max(l[j].Dim, PoolDim);
        }
        PoolDim += 1;

        for (int i = 0; i < PoolDim; i++)
        {
            _grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
            foreach (var c in components.Values.ToArray())
                if (c.Ealy == i)
                {
                    _grid.Children.Add(new Button() { Content = c.Ealy.ToString() + "|" + c.Component.type + "|" + c.Dim.ToString(), BorderThickness = new(1),BorderBrush = Brush.Parse("#83da83") });
                    Grid.SetRow(_grid.Children.Last(),key_value[c.ID]);
                    Grid.SetColumn(_grid.Children.Last(), i  + 1);
                    Grid.SetColumnSpan(_grid.Children.Last(), (int)(c.Dim - c.Ealy + 1));
                }
        }
    }
    void FindPath(ref ComponentCell cell, ref Hashtable colored)
    {
        try
        {
            if (colored.ContainsKey(cell.ID))
                throw new Exception("There is a loop,path is:");
            colored.Add(cell.ID, null);
            uint max = 0;
            for (var i = 0; i < cell.Forward.Count; i++)
            {
                ComponentCell c = cell.Forward[i];
                if (c.Dim == 0)
                    FindPath(ref c, ref colored);
                max = Math.Max(c.Dim, max);
            }
            cell.Dim = max + 1;
            cell.Ealy = max + 1;
            if (max == 0)
                return;
            for (var i = 0; i < cell.Forward.Count; i++)
            {
                if (cell.Forward[i].Dim == cell.Forward[i].Ealy)
                    cell.Forward[i].Dim = max;
                cell.Forward[i].Dim =Math.Min(max, cell.Forward[i].Dim);
            }
            return;
        }
        catch (Exception e)
        {
            throw new Exception(e.Message + "<-" + cell.ID.ToString());
        }
    }
}

