using System;
using System.Collections.Generic;
using ReactiveUI;
namespace TLARC_Manager.ViewModels;

public class ProcessBarViewModel : ReactiveObject
{
    public float Value { get => _value; set { this.RaiseAndSetIfChanged(ref _value, value); } }
    public string ValueStr  => _value.ToString(); 
    float _value = 0;
}