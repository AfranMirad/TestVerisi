using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TestVerisiOlustur.Models
{
    public class SepetUrun
    {
        public int Id { get; set; }
        public int SepetId { get; set; }
        public float Tutar{ get; set; }
        public string Aciklama { get; set; }
    }
}
