namespace ms_games.Models
{
    public class Game
    {
      public string Id { get; set; } = Guid.NewGuid().ToString();
      public string Name { get; set; }
      public decimal Price { get; set; }
    }
}
