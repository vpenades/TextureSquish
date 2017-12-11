using System;

namespace Epsylon.TextureSquish
{
    abstract class ColourFit
    {
        public ColourFit(ColourSet colours, CompressionOptions flags)
        {
            m_colours = colours;
            m_flags = flags;
        }

        public void Compress(BlockWindow block)
        {
            if (block.IsDtx1)
            {
                if (m_colours.IsTransparent) Compress3(block);
                else                         Compress4(block);
            }
            else Compress4(block);
        }

        protected abstract void Compress3(BlockWindow block);
        protected abstract void Compress4(BlockWindow block);

        protected ColourSet m_colours;
        protected CompressionOptions m_flags;
    }

}


