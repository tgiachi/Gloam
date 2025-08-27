using Gloam.Core.Types;

namespace Gloam.Core.Interfaces;

public interface IRenderSurface
{
    int Width { get; }       // colonne (Cells) o px (Pixels)
    int Height { get; }      // righe (Cells) o px (Pixels)
    RenderUnit Unit { get; } // Cells per console, Pixels per GPU

    // opzionale (per backend grafici a tiles): dimensione cella
    int CellWidth { get; }  // 1 per console
    int CellHeight { get; } // 1 per console

    event Action<int, int>? Resized; // (width, height)
}
