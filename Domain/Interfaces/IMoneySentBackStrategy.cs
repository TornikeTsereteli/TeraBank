namespace Domain.Interfaces;

public interface IMoneySentBackStrategy
{

    Task SentBack(decimal amount);
}