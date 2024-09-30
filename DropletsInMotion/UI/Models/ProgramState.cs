namespace DropletsInMotion.UI.Models;

public enum ProgramState
{
    WaitingForClientConnection,
    GettingInitialInformation,
    ReadingInputFiles,
    WaitingForUserInput,
    CompilingProgram,
    Completed
}