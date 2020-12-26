using System;
using System.Collections.Generic;
using System.Text;

namespace VendingMachine.Models
{
    public class VendingMachineException : Exception
    {
        public VendingMachineException(string message) : base(message) 
        { }
    }
}
