using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuroSpeech.Positron;
public static class GridExtensions
{

    public static T SetGrid<T>(
        this T @this,
        int? column = null,
        int? row = null,
        int? columnSpan = null,
        int? rowSpan = null)
        where T : Element
    {
        if (column != null)
        {
            Grid.SetColumn(@this, column.Value);
        }
        if (row != null)
        {
            Grid.SetRow(@this, row.Value);
        }
        if (columnSpan != null)
        {
            Grid.SetColumnSpan(@this, columnSpan.Value);
        }
        if (rowSpan != null)
        {
            Grid.SetRowSpan(@this, rowSpan.Value);
        }
        return @this;
    }

}