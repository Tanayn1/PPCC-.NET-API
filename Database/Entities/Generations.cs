using System.Text.Json;

namespace Database.Entites.Generations;


public class Generations 
{

    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public JsonElement Generation { get; set; }
}