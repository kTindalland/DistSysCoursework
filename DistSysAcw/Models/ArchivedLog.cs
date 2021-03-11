using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DistSysAcw.Models
{
    public class ArchivedLog
    {
        public ArchivedLog()
        {

        }

        [Key]
        public int Id { get; set; }
        public string ApiKey { get; set; }
        public string Username { get; set; }
        public string LogString { get; set; }
        public DateTime LogDateTime { get; set; }
    }
}
