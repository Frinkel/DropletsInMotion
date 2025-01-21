using DropletsInMotion.Infrastructure.Exceptions;
using DropletsInMotion.Infrastructure.Models.Commands;
using DropletsInMotion.Infrastructure.Models.Commands.DeviceCommands;
using DropletsInMotion.Infrastructure.Models.Commands.DropletCommands;
using DropletsInMotion.Infrastructure.Repositories;

namespace DropletsInMotion.Translation.Services;

public class TypeChecker : ITypeChecker
{
    private HashSet<string> _variables;
    private HashSet<string> _droplets;
    private IPlatformRepository _platformRepository;

    public TypeChecker(IPlatformRepository platformRepository)
    {
        _variables = new HashSet<string>();
        _droplets = new HashSet<string>();
        _platformRepository = platformRepository;
    }

    public void ResetTypeEnviroments()
    {
        _variables = new HashSet<string>();
        _droplets = new HashSet<string>();
    }

    public void TypeCheck(List<ICommand> commands)
    {
        foreach (var command in commands)
        {
            switch (command)
            {
                case AssignCommand assignCommand:
                    Assign(assignCommand);
                    break;

                case DropletDeclaration dropletDeclaration:
                    DefineDroplet(dropletDeclaration);
                    break;

                case Move moveCommand:
                    Move(moveCommand);
                    break;

                case Dispense dispenseCommand:
                    Dispense(dispenseCommand);
                    break;

                case Mix mixCommand:
                    Mix(mixCommand);
                    break;

                case SplitByRatio splitByRatioCommand:
                    SplitByRatio(splitByRatioCommand);
                    break;

                case SplitByVolume splitByVolumeCommand:
                    SplitByVolume(splitByVolumeCommand);
                    break;

                case Wait waitCommand:
                    Wait(waitCommand);
                    break;

                case WaitForUserInput waitForUserInputCommand:
                    WaitForUserInput(waitForUserInputCommand);
                    break;

                case Store storeCommand:
                    Store(storeCommand);
                    break;

                case Merge mergeCommand:
                    Merge(mergeCommand);
                    break;

                case WhileCommand whileCommand:
                    WhileLoop(whileCommand);
                    break;

                case IfCommand ifCommand:
                    IfStatement(ifCommand);
                    break;

                case PrintCommand printCommand:
                    Print(printCommand);
                    break;

                case SensorCommand sensorCommand:
                    Sensor(sensorCommand);
                    break;

                case ActuatorCommand actuatorCommand:
                    Actuator(actuatorCommand);
                    break;

                case Waste wasteCommand:
                    Waste(wasteCommand);
                    break;

                case DeclareDispenserCommand declareDispenserCommand:
                    DeclareDispenser(declareDispenserCommand);
                    break;

                default:
                    throw new CommandException($"Typechecker: Unknown command: {command.GetType().Name}", command);
            }
        }
    }

    private void Assign(AssignCommand assignCommand)
    {
        var variableName = assignCommand.VariableName;

        VariablesExist(assignCommand.ValueExpression.GetVariables(), assignCommand);

        if (_droplets.Contains(variableName))
        {
            throw new CommandException($"Variable '{variableName}' conflicts with a droplet name in command '{assignCommand}'.", assignCommand);
        }
        _variables.Add(variableName);
    }

    private void DefineDroplet(DropletDeclaration dropletDeclaration)
    {
        var inputDroplets = dropletDeclaration.GetInputDroplets();
        var outputDroplets = dropletDeclaration.GetOutputDroplets();
        var variables = dropletDeclaration.GetInputVariables();
        var nonDropletVariables = variables.Where(v => !inputDroplets.Contains(v)).ToList();
        VariablesExist(nonDropletVariables, dropletDeclaration);

        ValidateOutputDroplets(outputDroplets, inputDroplets, dropletDeclaration);

        UpdateDeclaredDroplets(inputDroplets, outputDroplets, dropletDeclaration);
    }

    private void Move(Move moveCommand)
    {
        var droplets = moveCommand.GetInputDroplets();
        ValidateInputDroplets(droplets, moveCommand);

        var variables = moveCommand.GetInputVariables();
        var nonDropletVariables = variables.Where(v => !droplets.Contains(v)).ToList();
        VariablesExist(nonDropletVariables, moveCommand);

    }

    private void Dispense(Dispense dispenseCommand)
    {
        var inputDroplets = dispenseCommand.GetInputDroplets();
        var outputDroplets = dispenseCommand.GetOutputDroplets();
        var variables = dispenseCommand.GetInputVariables();
        var nonDropletVariables = variables.Where(v => !inputDroplets.Contains(v)).ToList();
        VariablesExist(nonDropletVariables, dispenseCommand);

        ValidateOutputDroplets(outputDroplets, inputDroplets, dispenseCommand);

        UpdateDeclaredDroplets(inputDroplets, outputDroplets, dispenseCommand);
    }

    private void DeclareDispenser(DeclareDispenserCommand declareDispenserCommand)
    {
        var dispenserName = declareDispenserCommand.DispenserIdentifier;

        if (_variables.Contains(dispenserName) || _droplets.Contains(dispenserName))
        {
            throw new CommandException($"Dispenser name '{dispenserName}' is already in use.", declareDispenserCommand);
        }

        _variables.Add(dispenserName);
    }


    private void Mix(Mix mixCommand)
    {
        var droplets = mixCommand.GetInputDroplets();
        ValidateInputDroplets(droplets, mixCommand);

        var variables = mixCommand.GetInputVariables();
        var nonDropletVariables = variables.Where(v => !droplets.Contains(v)).ToList();
        VariablesExist(nonDropletVariables, mixCommand);
    }

    private void SplitByRatio(SplitByRatio splitCommand)
    {
        var inputDroplets = splitCommand.GetInputDroplets();
        var outputDroplets = splitCommand.GetOutputDroplets();
        ValidateInputDroplets(inputDroplets, splitCommand);

        var variables = splitCommand.GetInputVariables();
        var nonDropletVariables = variables.Where(v => !inputDroplets.Contains(v)).ToList();
        VariablesExist(nonDropletVariables, splitCommand);

        ValidateOutputDroplets(outputDroplets, inputDroplets, splitCommand);

        UpdateDeclaredDroplets(inputDroplets, outputDroplets, splitCommand);
    }

    private void SplitByVolume(SplitByVolume splitCommand)
    {
        var inputDroplets = splitCommand.GetInputDroplets();
        var outputDroplets = splitCommand.GetOutputDroplets();
        ValidateInputDroplets(inputDroplets, splitCommand);

        var variables = splitCommand.GetInputVariables();
        var nonDropletVariables = variables.Where(v => !inputDroplets.Contains(v)).ToList();
        VariablesExist(nonDropletVariables, splitCommand);

        ValidateOutputDroplets(outputDroplets, inputDroplets, splitCommand);

        UpdateDeclaredDroplets(inputDroplets, outputDroplets, splitCommand);
    }

    private void Wait(Wait waitCommand)
    {
        var variables = waitCommand.GetInputVariables();
        VariablesExist(variables, waitCommand);
    }

    private void WaitForUserInput(WaitForUserInput waitForUserInputCommand)
    {
    }

    private void Store(Store storeCommand)
    {
        var droplets = storeCommand.GetInputDroplets();
        ValidateInputDroplets(droplets, storeCommand);

        var variables = storeCommand.GetInputVariables();
        var nonDropletVariables = variables.Where(v => !droplets.Contains(v)).ToList();
        VariablesExist(nonDropletVariables, storeCommand);
    }

    private void Merge(Merge mergeCommand)
    {
        var inputDroplets = mergeCommand.GetInputDroplets();
        var outputDroplets = mergeCommand.GetOutputDroplets();
        ValidateInputDroplets(inputDroplets, mergeCommand);

        var variables = mergeCommand.GetInputVariables();
        var nonDropletVariables = variables.Where(v => !inputDroplets.Contains(v)).ToList();
        VariablesExist(nonDropletVariables, mergeCommand);

        ValidateOutputDroplets(outputDroplets, inputDroplets, mergeCommand);

        UpdateDeclaredDroplets(inputDroplets, outputDroplets, mergeCommand);
    }

    private void WhileLoop(WhileCommand whileCommand)
    {
        VariablesExist(whileCommand.Condition.GetVariables(), whileCommand);

        var dropletStateBeforeWhile = new HashSet<string>(_droplets);

        TypeCheck(whileCommand.Commands);

        var dropletStateAfterWhile = new HashSet<string>(_droplets);
        if (!dropletStateBeforeWhile.SetEquals(dropletStateAfterWhile))
        {
            throw new CommandException(
                $"Error: The state of droplets is altered in the while loop {whileCommand}" +
                $"Droplets before the while loop: {string.Join(", ", dropletStateBeforeWhile)}, " +
                $"Droplets after the while loop: {string.Join(", ", dropletStateAfterWhile)}", whileCommand);
        }

    }

    private void IfStatement(IfCommand ifCommand)
    {
        VariablesExist(ifCommand.Condition.GetVariables(), ifCommand);

        var dropletStateBeforeIf = new HashSet<string>(_droplets);

        var dropletStateAfterIf = new HashSet<string>(_droplets); 
        TypeCheck(ifCommand.IfBlockCommands);
        var dropletStateAfterThen = new HashSet<string>(_droplets); 

        _droplets = dropletStateAfterIf; 
        if (ifCommand.ElseBlockCommands.Count > 0)
        {
            TypeCheck(ifCommand.ElseBlockCommands);
            var dropletStateAfterElse = new HashSet<string>(_droplets); 

            if (!dropletStateAfterThen.SetEquals(dropletStateAfterElse))
            {
                throw new CommandException(
                    $"Error: The droplets in the 'then' and 'else' blocks do not match for the command {ifCommand}\n" +
                    $"Then block droplets: {string.Join(", ", dropletStateAfterThen)}, " +
                    $"Else block droplets: {string.Join(", ", dropletStateAfterElse)}", ifCommand);
            }
        } else
        {
            if (!dropletStateAfterThen.SetEquals(dropletStateBeforeIf))
            {
                throw new CommandException(
                    $"Error: The droplets in the 'then' blocks does not match the implicit else case {ifCommand}\n" +
                    $"Then block droplets: {string.Join(", ", dropletStateAfterThen)}, " +
                    $"Else block droplets: {string.Join(", ", dropletStateBeforeIf)}", ifCommand);
            }
        }

        _droplets = dropletStateAfterThen; 
    }

    private void Print(PrintCommand printCommand)
    {
        var variables = printCommand.GetInputVariables();
        VariablesExist(variables, printCommand);
    }

    private void Sensor(SensorCommand sensorCommand)
    {
        var variableName = sensorCommand.VariableName;

        if (_droplets.Contains(variableName))
        {
            throw new CommandException($"Error: Variable '{variableName}' conflicts with a droplet name in command '{sensorCommand}'.", sensorCommand);
        }
        _variables.Add(variableName);
    }

    private void Actuator(ActuatorCommand actuatorCommand)
    {
        //var variables = actuatorCommand.GetInputVariables();
        //VariablesExist(variables, actuatorCommand);
    }

    private void Waste(Waste wasteCommand)
    {
        var inputDroplets = wasteCommand.GetInputDroplets();
        var outputDroplets = wasteCommand.GetOutputDroplets();
        ValidateInputDroplets(inputDroplets, wasteCommand);

        var variables = wasteCommand.GetInputVariables();
        var nonDropletVariables = variables.Where(v => !inputDroplets.Contains(v)).ToList();
        VariablesExist(nonDropletVariables, wasteCommand);

        UpdateDeclaredDroplets(inputDroplets, outputDroplets, wasteCommand);
    }

    private void ValidateInputDroplets(List<string> inputDroplets, ICommand command)
    {
        if (inputDroplets.Count == 1)
        {
            var inputDroplet = inputDroplets[0];
            if (!_droplets.Contains(inputDroplet))
            {
                throw new CommandException($"Input droplet '{inputDroplet}' does not exist.", command);
            }
            return;
        }

        if (inputDroplets.Count == 2 && inputDroplets[0] == inputDroplets[1])
        {
            throw new CommandException($"The input droplets '{inputDroplets[0]}' and '{inputDroplets[1]}' cannot have the same name in command '{command}'.", command);
        }

        foreach (var inputDroplet in inputDroplets)
        {
            if (!_droplets.Contains(inputDroplet))
            {
                throw new CommandException($"Droplet '{inputDroplet}' does not exist when the command is executed. ", command);
            }
        }
    }

    private void ValidateOutputDroplets(List<string> outputDroplets, List<string> inputDroplets, ICommand command)
    {
        if (outputDroplets.Count == 1)
        {
            var outputDroplet = outputDroplets[0];
            if (!inputDroplets.Contains(outputDroplet) && _droplets.Contains(outputDroplet))
            {
                throw new CommandException($"Output droplet '{outputDroplet}' already exists in command '{command}'.", command);
            }
            return;
        }

        if (outputDroplets.Count == 2 && outputDroplets[0] == outputDroplets[1])
        {
            throw new CommandException($"The output droplets '{outputDroplets[0]}' and '{outputDroplets[1]}' cannot have the same name in command '{command}'.", command);
        }

        foreach (var outputDroplet in outputDroplets)
        {
            if (!inputDroplets.Contains(outputDroplet) && _droplets.Contains(outputDroplet))
            {
                throw new CommandException($"Output droplet '{outputDroplet}' already exists in command '{command}'.", command);
            }
        }
    }

    private void UpdateDeclaredDroplets(List<string> inputDroplets, List<string> outputDroplets, ICommand command)
    {
        foreach (var inputDroplet in inputDroplets)
        {
            _droplets.Remove(inputDroplet);
        }

        foreach (var outputDroplet in outputDroplets)
        {
            if (_variables.Contains(outputDroplet))
            {
                throw new CommandException($"Droplet '{outputDroplet}' conflicts with a variable name in command '{command}'.", command);
            }
            if (!_droplets.Contains(outputDroplet))
            {
                _droplets.Add(outputDroplet);
            }
        }
    }

    private void VariablesExist(List<string> variables, ICommand command)
    {
        foreach (var variable in variables)
        {
            if (!_variables.Contains(variable))
            {
                throw new CommandException($"Variable '{variable}' is not defined in command '{command}'.", command);
            }
        }
    }
}