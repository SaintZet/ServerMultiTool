using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ServerMultiTool.ViewModels.Data;

public enum MilestoneIndicator
{
    Wait,
    Success,
    Error,
}

public class MilestoneContainer : ObservableCollection<MilestoneItem>
{
    public async Task StartExecute()
    {
        foreach (var item in this) 
            await item.Execute();
    }
    
    public void ResetAllIndicators()
    {
        foreach (var item in this) 
            item.IsCompleted = MilestoneIndicator.Wait;
    }
}

public class MilestoneItem  : ObservableObject
{
    public MilestoneItem(string description, Func<Task<bool>> action)
    {
        Description = description;
        Action = action;
    }

    public string Description { get; }
    private Func<Task<bool>> Action { get; }

    private MilestoneIndicator _isCompleted = MilestoneIndicator.Wait;
    public MilestoneIndicator IsCompleted
    {
        get => _isCompleted;
        set => SetProperty(ref _isCompleted, value);
    }

    public async Task Execute()
    {
        var success = await Action.Invoke();
        IsCompleted = success ? MilestoneIndicator.Success : MilestoneIndicator.Error ;
    }
}