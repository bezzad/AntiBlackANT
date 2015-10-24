using System;
using System.Collections.Generic;
using System.Text;

namespace EventArguments
{
    /// <summary>
    /// If occurred Reporter event then reflect by ReflectReports.
    /// All occurrence event saved in this class List.
    /// You can Add or AddRange many ReportEventArgs programmatic to this class objects.
    /// </summary>
    public class ReflectorEventList : List<ReportEventArgs>
    {
        public event EventHandler<ReportEventArgs> ReflectedReports = delegate { };

        public void CallReporter(object sender, ReportEventArgs e)
        {
            base.Add(e);

            ReflectedReports(sender, e);
        }


        // Summary:
        //     Adds an object to the end of the System.Collections.Generic.List<ReportEventArgs>.
        //
        // Parameters:
        //   item:
        //     The object to be added to the end of the System.Collections.Generic.List<ReportEventArgs>.
        //     The value can be null for reference types.
        public new void Add(ReportEventArgs item)
        {
            base.Add(item);

            ReflectedReports(this, item);
        }
    }
}
