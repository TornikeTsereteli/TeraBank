using Domain.Interfaces;

namespace Application.Services;

public class MoneySentBackStrategy : IMoneySentBackStrategy
{
    public async Task SentBack(decimal amount)
    {
        Console.WriteLine($"{amount} get sent back to the client account");
    }
}