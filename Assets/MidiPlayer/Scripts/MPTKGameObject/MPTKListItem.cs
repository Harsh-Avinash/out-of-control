using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MidiPlayerTK
{
    /// <summary>
    /// A list of string with index: midi, preset, bank, drum, ...
    /// </summary>
    public class MPTKListItem
    {
        /// <summary>
        /// Index in the list: 
        ///! @li @c patch num if patch list, 
        ///! @li @c bank number if bank list, 
        ///! @li @c index in list for midi.
        /// </summary>
        public int Index;
        /// <summary>
        /// Label
        /// </summary>
        public string Label;
    }

}
