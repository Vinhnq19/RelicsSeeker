using System.Collections.Generic;
using UnityEngine;
//Undo/Redo
public interface ICommand
{
    void Execute();
    void Undo();
}
public class CommandSystem
{
    //Sử dụng Stack để lưu trữ lịch sử các lệnh đã thực hiện
    private Stack<ICommand> commandHistory = new();
    public void ExecuteCommand(ICommand command)
    {
        command.Execute();
        commandHistory.Push(command);
    }
    public void Undo()
    {
        if (commandHistory.Count > 0)
        {
            ICommand lastCommand = commandHistory.Pop();
            lastCommand.Undo();
        }
    }
}
