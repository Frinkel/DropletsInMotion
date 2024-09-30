using DropletsInMotion.Application.ExecutionEngine.Models;
using DropletsInMotion.Application.ExecutionEngine.Services;
using DropletsInMotion.Infrastructure.Models.Domain;

namespace DropletsInMotion.Application.Services.Routers;

public class MoveHandler
{
    private readonly TemplateHandler _templateHandler;

    public MoveHandler(TemplateHandler templateHandler)
    {
        _templateHandler = templateHandler;
    }

    public List<BoardAction> MoveDroplet(Droplet droplet, int targetX, int targetY, ref double time)
    {
        var boardActions = new List<BoardAction>();

        // Move along the X-axis
        while (droplet.PositionX != targetX)
        {
            int directionX = targetX > droplet.PositionX ? 1 : -1;
            var template = directionX > 0 ? "moveRight" : "moveLeft";
            boardActions.AddRange(ApplyMove(droplet, template, ref time));
            droplet.PositionX += directionX;
        }

        // Move along the Y-axis
        while (droplet.PositionY != targetY)
        {
            int directionY = targetY > droplet.PositionY ? 1 : -1;
            var template = directionY > 0 ? "moveDown" : "moveUp";
            boardActions.AddRange(ApplyMove(droplet, template, ref time));
            droplet.PositionY += directionY;
        }

        return boardActions;
    }

    private List<BoardAction> ApplyMove(Droplet droplet, string template, ref double time)
    {
        var appliedMoves = _templateHandler.ApplyTemplate(template, droplet, time);
        if (appliedMoves.Any())
        {
            time = appliedMoves.Last().Time;
        }
        return appliedMoves;
    }
}