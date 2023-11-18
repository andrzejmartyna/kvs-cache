using kcs_cache.Models;

namespace kcs_cache.Browse;

public class BrowseGeometry
{
    public Rectangle Full { get; init; }
    public Rectangle SummaryRectangle => new(Full.Left + 1, Full.Top + 1,  19, 3);
    public Rectangle SelectionRectangle => new(Full.Left + 21, Full.Top + 1, Full.Width - 22, 2);
    public Rectangle RefreshedRectangle => new(Full.Left + 21, Full.Top + 3, Full.Width - 22, 1);
    public Rectangle BrowsingRectangle => new(Full.Left + 1, Full.Top + 5, Full.Width - 2, Full.Height - 6);
    public HorizontalLine DivideLine => new(Full.Left, Full.Top + 4, Full.Width);
    public HorizontalLine SelectionHeaderLine => new(Full.Left + 1, Full.Top + 4, Full.Width - 2);
    public Rectangle TipsRectangle => new(Full.Left + 1, Full.Bottom, Full.Width - 2, 1);
    
    public BrowseGeometry(Rectangle operationalRectangle)
    {
        Full = operationalRectangle;
    }

    public BrowseGeometry(BrowseGeometry original)
    {
        Full = original.Full;
    }
}

