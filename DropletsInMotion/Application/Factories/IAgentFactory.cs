using DropletsInMotion.Application.Models;

namespace DropletsInMotion.Application.Factories;

public interface IAgentFactory
{
    Agent CreateAgent(string dropletName, int positionX, int positionY, double volume);
    Agent CreateAgent(string dropletName, int positionX, int positionY, double volume, int substanceId);
    Agent CreateAgent(string dropletName, int positionX, int positionY, double volume, int substanceId, LinkedList<(int x, int y)> snakeBody);
}