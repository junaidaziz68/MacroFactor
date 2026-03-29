using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace MacroFactor.Models;

[Table("profiles")]
public class UserProfile : BaseModel
{
    [PrimaryKey("id", false)]
    public string Id { get; set; }

    [Column("target_calories")]
    public int TargetCalories { get; set; }

    [Column("target_protein")]
    public int TargetProtein { get; set; }

    [Column("target_carbs")]
    public int TargetCarbs { get; set; }

    [Column("target_fats")]
    public int TargetFats { get; set; }
}
