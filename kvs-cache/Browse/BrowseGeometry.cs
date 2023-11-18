using kcs_cache.Models;

namespace kcs_cache.Browse;

public class BrowseGeometry
{
    public Rectangle Full { get; init; }
    public Rectangle SummaryRectangle => new(Full.Left + 2, Full.Top + 1,  19, 3);
    public Rectangle SelectionRectangle => new(Full.Left + 22, Full.Top + 1, Full.Width - 24, 2);
    public Rectangle ReadingProgressRectangle => new(Full.Left + 22, Full.Top + 3, 14, 1);
    public Rectangle RefreshedRectangle => new(Full.Left + 37, Full.Top + 3, Full.Width - 39, 1);
    public Rectangle BrowsingRectangle => new(Full.Left + 2, Full.Top + 5, Full.Width - 4, Full.Height - 6);
    public HorizontalLine DivideLine => new(Full.Left, Full.Top + 4, Full.Width);
    public HorizontalLine SelectionHeaderLine => new(Full.Left + 2, Full.Top + 4, Full.Width - 4);
    public Rectangle TipsRectangle => new(Full.Left + 2, Full.Bottom, Full.Width - 4, 1);
    public HorizontalLine VersionHeaderLine => new(Full.Left + Full.Width / 2, Full.Top, Full.Width - Full.Left - Full.Width / 2 - 2);
    
    public BrowseGeometry(Rectangle operationalRectangle)
    {
        Full = operationalRectangle;
    }

    public BrowseGeometry(BrowseGeometry original)
    {
        Full = original.Full;
    }
}

