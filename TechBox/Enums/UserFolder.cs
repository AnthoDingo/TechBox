using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechBox.Attributes;

namespace TechBox.Enums
{
    public enum UserFolder
    {
        [StringValue("Desktop")]
        Desktop,

        [StringValue("Pictures")]
        Pictures,

        [StringValue("Music")]
        Music,

        [StringValue("Downloads")]
        Downloads,

        [StringValue("Videos")]
        Videos,

        [StringValue(@"AppData\Local\Google")]
        AppDataGoogle,

        [StringValue(@"AppData\Roaming\Microsoft\Signatures")]
        OutlookSignatures,

        [StringValue(@"AppData\Roaming\SAP\Common")]
        SAPCommon
    }
}
