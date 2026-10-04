namespace IdentityMail.Web.Entites.Enums
{
    public enum CategoryTheme
    {
        PurpleTheme = 1,
        BlueTheme = 2,
        YellowTheme = 3,
        RedTheme = 4,
        WhiteTheme = 5
    }

    public record ThemePreset(
        string CardBg,
        string IconBox,
        string Badge,
        string WatermarkPos,
        string DefaultWatermarkIcon
    );

    public static class CategoryThemeExtensions
    {
        public static ThemePreset GetPreset(this CategoryTheme theme) => theme switch
        {
            CategoryTheme.PurpleTheme => new(
                "bg-primary-container text-on-primary-container",
                "bg-brutal-yellow text-brutal-black",
                "bg-brutal-black text-white",
                "-right-4 -top-4 opacity-20 group-hover:opacity-40",
                "inbox"
            ),
            CategoryTheme.BlueTheme => new(
                "bg-brutal-blue text-white",
                "bg-white text-brutal-black",
                "bg-brutal-black text-white",
                "-right-4 -bottom-4 opacity-20 group-hover:opacity-40",
                "group"
            ),
            CategoryTheme.YellowTheme => new(
                "bg-brutal-yellow text-brutal-black",
                "bg-brutal-black text-brutal-yellow",
                "bg-white text-brutal-black border-2 border-brutal-black",
                "-left-4 -bottom-4 opacity-10 group-hover:opacity-30",
                "campaign"
            ),
            CategoryTheme.RedTheme => new(
                "bg-brutal-red text-white",
                "bg-brutal-black text-white",
                "bg-white text-brutal-black border-2 border-brutal-black",
                "right-0 top-0 opacity-20 group-hover:opacity-40",
                "security"
            ),
            CategoryTheme.WhiteTheme or _ => new(
                "bg-surface text-brutal-black",
                "bg-primary text-white",
                "bg-brutal-yellow text-brutal-black border-2 border-brutal-black",
                "-right-2 -bottom-2 opacity-5 group-hover:opacity-10",
                "work"
            )
        };
    }
}
