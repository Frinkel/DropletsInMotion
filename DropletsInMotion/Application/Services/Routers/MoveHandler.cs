using DropletsInMotion.Application.Execution.Models;
using DropletsInMotion.Application.ExecutionEngine.Models;
using DropletsInMotion.Application.Models;
using DropletsInMotion.Application.Services.Routers.Models;
using DropletsInMotion.Infrastructure.Models.Platform;
using DropletsInMotion.Infrastructure.Repositories;
using Microsoft.Extensions.FileSystemGlobbing.Internal.PathSegments;
using static DropletsInMotion.Application.Services.Routers.Models.Types;

namespace DropletsInMotion.Application.Services.Routers;

public class MoveHandler
{
    private readonly ITemplateService _templateHandler;
    private readonly ITemplateRepository _templateRepository;
    private readonly IPlatformRepository _platformRepository;

    public MoveHandler(ITemplateService templateHandler, ITemplateRepository templateRepository, IPlatformRepository platformRepository)
    {
        _templateHandler = templateHandler;
        _templateRepository = templateRepository;
        _platformRepository = platformRepository;
    }

    public List<BoardAction> MoveDroplet(Droplet droplet, int targetX, int targetY, ref double time, double scaleFactor)
    {
        var boardActions = new List<BoardAction>();

        double currentTime = time;

        // Move along the X-axis
        while (droplet.PositionX != targetX)
        {
            int directionX = targetX > droplet.PositionX ? 1 : -1;
            var template = directionX > 0 ? "moveRight" : "moveLeft";
            boardActions.AddRange(ApplyMove(droplet, template, ref time, scaleFactor));
            droplet.PositionX += directionX;
        }

        // Move along the Y-axis
        while (droplet.PositionY != targetY)
        {
            int directionY = targetY > droplet.PositionY ? 1 : -1;
            var template = directionY > 0 ? "moveDown" : "moveUp";
            boardActions.AddRange(ApplyMove(droplet, template, ref time, scaleFactor));
            droplet.PositionY += directionY;
        }


        boardActions = boardActions.OrderBy(b => b.Time).ToList();

        return boardActions;
    }

    private List<BoardAction> ApplyMove(Droplet droplet, string template, ref double time, double scaleFactor)
    {
        var appliedMoves = _templateHandler.ApplyTemplateScaled(template, droplet, time, scaleFactor);
        if (appliedMoves.Any())
        {
            var totalTime = appliedMoves.Last().Time - time;
            time += (totalTime / scaleFactor);
        }

        appliedMoves.ForEach(x => Console.WriteLine(x));

        return appliedMoves;
    }

    public List<BoardAction> Unravel(Agent agentInitial, double time)
    {
        UnravelTemplate? unravelTemplate = _templateRepository?.UnravelTemplates?.Find(t =>
            t.FinalPositions.First().Value == (agentInitial.GetAgentSize() - 2, -1)
            && t.MinSize <= agentInitial.Volume && agentInitial.Volume < t.MaxSize) ?? null;

        if (unravelTemplate == null)
        {
            return new List<BoardAction>();
        }
        
        return unravelTemplate.Apply(_platformRepository.Board[agentInitial.PositionX - unravelTemplate.InitialPositions.First().Value.x][agentInitial.PositionY - unravelTemplate.InitialPositions.First().Value.y].Id, time, 1);
    }

    public List<BoardAction> Ravel(Agent agent, double time)
    {
        RavelTemplate? ravelTemplate = _templateRepository?.RavelTemplates?.Find(t =>
            t.InitialPositions.First().Value == (-1, agent.GetAgentSize() - 2)
            && t.MinSize <= agent.Volume && agent.Volume < t.MaxSize) ?? null;

        if (ravelTemplate == null)
        {
            return new List<BoardAction>();
        }


        return ravelTemplate.Apply(_platformRepository.Board[agent.PositionX - ravelTemplate.FinalPositions.First().Value.x][agent.PositionY - ravelTemplate.FinalPositions.First().Value.y].Id, time - ravelTemplate.Duration, 1);
    }

    //public List<BoardAction> Unravel(Agent agent, Types.RouteAction action, double time)
    //{
    //    UnravelTemplate? unravelTemplate = _templateRepository?.UnravelTemplates?.Find(t => t.Direction == action.Name && t.MinSize <= agent.Volume && agent.Volume < t.MaxSize) ?? null;

    //    if (unravelTemplate == null)
    //    {
    //        return new List<BoardAction>();
    //    }

    //    return unravelTemplate.Apply(_platformRepository.Board[agent.PositionX][agent.PositionY].Id, time + 0.5, 1);
    //}

    //public List<BoardAction> Ravel(Agent agent, Types.RouteAction action, double time)
    //{
    //    RavelTemplate? ravelTemplate = _templateRepository?.RavelTemplates?.Find(t => t.Direction == action.Name && t.MinSize <= agent.Volume && agent.Volume < t.MaxSize) ?? null;

    //    if (ravelTemplate == null)
    //    {
    //        return new List<BoardAction>();
    //    }

    //    return ravelTemplate.Apply(_platformRepository.Board[agent.PositionX][agent.PositionY].Id, time - ravelTemplate.Duration, 1);
    //}

}