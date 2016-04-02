using System;
using System.Collections.Generic;

namespace Zw.XmlLanguageEditor.ViewModels.Behaviors
{
    /// <summary>
    /// Defines a complete set of configured columns a ListView should use through the ConfigToDynamicGridViewConverter.
    /// </summary>
    /// <remarks>
    /// Credits to: https://github.com/9swampy/DynamicPropertyPropertiesListGridViewExample
    /// </remarks>
    public class ColumnConfig
    {

        public List<Column> Columns { get; private set; }

        public ColumnConfig()
        {
            this.Columns = new List<Column>();
        }

    }
}
