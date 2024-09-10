namespace DropletsInMotion;

public enum ProgramState
{
    WaitingForClientConnection,
    GettingInitialInformation,
    ReadingInputFiles,
    WaitingForUserInput,
    CompilingProgram,
    Completed
}