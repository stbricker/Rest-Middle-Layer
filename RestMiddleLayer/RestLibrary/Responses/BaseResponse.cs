using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestLibrary.Responses
{
    public class BaseResponse
    {
        public string ErrorMessage { get; set; }

        public bool Successful
        {
            get { return string.IsNullOrWhiteSpace(ErrorMessage); }
        }
    }
}
